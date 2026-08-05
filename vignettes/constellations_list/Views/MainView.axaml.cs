using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaVignettes.Shared.Animation;
using ConstellationsList.Controls;
using ConstellationsList.Models;

namespace ConstellationsList.Views;

/// <summary>
/// The vignette's shell: the star field, the page on top of it, and the card that flies between
/// pages. Port of <c>demo.dart</c>, together with the nested navigator and fade route it uses.
/// </summary>
/// <remarks>
/// The star field is the constant here, it never rebuilds, it is only ever handed a new speed. The
/// list feeds it scroll velocity, and choosing a constellation runs a fixed sequence that hauls the
/// field backwards, throws it forwards past the viewer, then brings it to rest, which reads as
/// flying to the star being looked at.
/// <para>
/// Nothing is co-ordinated by shared state: the detail page is simply told how long to wait before
/// revealing itself, so its content lands as the flight ends.
/// </para>
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>
    /// How fast the field drifts when nothing is happening.
    /// </summary>
    private const double IdleSpeed = 0.2d;

    /// <summary>
    /// The fastest the list's own scrolling can drive the field.
    /// </summary>
    private const double MaxSpeed = 10d;

    /// <summary>
    /// How long after the last scroll report the field drops back to its drift. Flutter keeps
    /// sending scroll notifications as a list settles and ends with a delta of zero, which is what
    /// returns it to idle there; Avalonia only reports while the offset is actually changing, so the
    /// last delta would otherwise stand forever and the stars would fly on for good.
    /// </summary>
    private static readonly TimeSpan ScrollRest = TimeSpan.FromMilliseconds(120);

    private static readonly TimeSpan FlightDuration = TimeSpan.FromMilliseconds(3000);
    private static readonly TimeSpan PageFade = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The detail page waits out the flight, then a beat more, before revealing itself.
    /// </summary>
    private static readonly TimeSpan ContentDelay = FlightDuration + TimeSpan.FromMilliseconds(1000);

    private readonly AnimationController _starFlight;
    private readonly AnimationController _cardFlight;
    private readonly ListPage _list = new();
    private readonly DispatcherTimer _scrollRest = new() { Interval = ScrollRest };

    private ConstellationTitleCard? _flyingCard;
    private Control? _flightSource;
    private Control? _flightTarget;
    private Rect _flightFrom;
    private Rect _flightTo;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();

        _starFlight = new AnimationController(this, OnStarFlightChanged) { Duration = FlightDuration };
        _cardFlight = new AnimationController(this, OnCardFlightChanged) { Duration = PageFade };

        _scrollRest.Tick += OnScrollRested;
        _list.Scrolled += OnListScrolled;
        _list.EntryTapped += OnEntryTapped;

        Pages.Content = _list;
    }

    /// <summary>
    /// The speed the field runs at part way through the flight. It is pulled back before being
    /// thrown forward, and the three legs take a fifth, a third and a half of the run.
    /// </summary>
    private static double FlightSpeedAt(double progress) => progress switch
    {
        < 0.2d => Lerp(IdleSpeed, -2d, FlutterEasings.EaseOut.Ease(progress / 0.2d)),
        < 0.5d => Lerp(-2d, 20d, FlutterEasings.EaseOut.Ease((progress - 0.2d) / 0.3d)),
        _ => Lerp(20d, 0d, FlutterEasings.EaseOut.Ease((progress - 0.5d) / 0.5d)),
    };

    private static double Lerp(double from, double to, double t) => from + ((to - from) * t);

    private void OnListScrolled(object? sender, double delta)
    {
        if (_starFlight.IsAnimating)
        {
            return;
        }

        // A still list drifts; a moving one drives the field at its own velocity.
        Stars.Speed = delta == 0d ? IdleSpeed : Math.Clamp(delta, -MaxSpeed, MaxSpeed);

        // Restarted on every report, so it only fires once the list has actually stopped.
        _scrollRest.Stop();
        _scrollRest.Start();
    }

    private void OnScrollRested(object? sender, EventArgs e)
    {
        _scrollRest.Stop();

        if (!_starFlight.IsAnimating)
        {
            Stars.Speed = IdleSpeed;
        }
    }

    private void OnEntryTapped(object? sender, ConstellationTitleCard card)
    {
        if (card.Constellation is not { } constellation)
        {
            return;
        }

        var detail = new DetailPage
        {
            Constellation = constellation,
            IsRedMode = card.IsRedMode,
        };

        detail.ReturnRequested += (_, _) => ReturnToList(detail);

        // Where the card is now, read before the page is swapped: once the list is on its way out
        // the card can no longer report a position, and the flight would set off from the corner.
        var origin = RectOf(card);

        Pages.Content = detail;
        detail.Reveal(ContentDelay);

        _starFlight.SetValue(0d);
        _starFlight.Forward();

        BeginFlight(origin, card, detail.TitleCardControl, constellation, card.IsRedMode);
    }

    private void ReturnToList(DetailPage detail)
    {
        var origin = RectOf(detail.TitleCardControl);

        Pages.Content = _list;

        // Reverse the flight if it is still running; otherwise just settle back to the drift.
        if (_starFlight.IsAnimating)
        {
            _starFlight.Reverse();
        }
        else
        {
            Stars.Speed = IdleSpeed;
        }

        if (detail.Constellation is { } constellation && _flightSource is { } source)
        {
            BeginFlight(origin, detail.TitleCardControl, source, constellation, detail.IsRedMode);
        }
    }

    /// <summary>
    /// Hides both ends straight away, then starts the flight once layout has caught up.
    /// </summary>
    /// <remarks>
    /// The slot being flown to belongs to a page that has only just been swapped in, so its position
    /// is not known until the next layout pass. Reading it immediately gives a stale rectangle and
    /// the card lands somewhere other than where the real one is, which shows up as a jump the
    /// moment the real one is restored.
    /// </remarks>
    private void BeginFlight(
        Rect origin,
        Control from,
        Control to,
        Constellation constellation,
        bool isRedMode)
    {
        if (origin.Width <= 0d)
        {
            return;
        }

        from.Opacity = 0d;
        to.Opacity = 0d;

        Dispatcher.UIThread.Post(
            () => StartCardFlight(origin, from, to, constellation, isRedMode),
            DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Where a control sits in this view's coordinates, right now.
    /// </summary>
    private Rect RectOf(Control control) =>
        new(control.TranslatePoint(default, this) ?? default, control.Bounds.Size);

    /// <summary>
    /// Sends a copy of the card from one page's slot to the other's, hiding both ends while it is in
    /// the air. This stands in for Flutter's <c>Hero</c>, which the nested navigator gives it for free.
    /// </summary>
    private void StartCardFlight(
        Rect origin,
        Control from,
        Control to,
        Constellation constellation,
        bool isRedMode)
    {
        _flightFrom = origin;

        // The far end has not been laid out yet, so its slot is worked out rather than measured: the
        // detail page docks its card to the top, centred, below a 24 inset.
        // The far end has had a layout pass by now, so its slot can be measured. If it still has no
        // size, fall back to where the detail page docks its card: centred, below a 24 inset.
        _flightTo = to.Bounds.Width > 0d
            ? RectOf(to)
            : new Rect((Bounds.Width - _flightFrom.Width) / 2d, 24d, _flightFrom.Width, _flightFrom.Height);

        // Left to size itself: both ends draw the same text at the same size, and pinning the copy
        // to the departure width made it land at a different width from the card it hands over to.
        _flyingCard ??= new ConstellationTitleCard();
        _flyingCard.Constellation = constellation;
        _flyingCard.IsRedMode = isRedMode;

        if (!FlightLayer.Children.Contains(_flyingCard))
        {
            FlightLayer.Children.Add(_flyingCard);
        }

        _flightSource = from;
        _flightTarget = to;

        _cardFlight.SetValue(0d);
        _cardFlight.Forward();
    }

    private void OnCardFlightChanged(double progress)
    {
        if (_flyingCard is not { } card)
        {
            return;
        }

        // Easing the two axes differently bows the path, which is what Flutter's arc tween does
        // rather than sliding the card along a straight line.
        var x = Lerp(_flightFrom.X, _flightTo.X, FlutterEasings.EaseIn.Ease(progress));
        var y = Lerp(_flightFrom.Y, _flightTo.Y, FlutterEasings.EaseOut.Ease(progress));

        Canvas.SetLeft(card, x);
        Canvas.SetTop(card, y);

        if (progress < 1d)
        {
            return;
        }

        FlightLayer.Children.Remove(card);

        // Both ends are restored, not just the one arrived at. Leaving the far end hidden is what
        // made the card pop back into existence on the last frame of the return.
        if (_flightTarget is { } target)
        {
            target.Opacity = 1d;
        }

        if (_flightSource is { } source)
        {
            source.Opacity = 1d;
        }
    }

    private void OnStarFlightChanged(double progress) => Stars.Speed = FlightSpeedAt(progress);

    /// <summary>
    /// Shows a detail page directly. Used only by the screenshot harness.
    /// </summary>
    internal void ShowDetailForCapture(DetailPage page) => Pages.Content = page;
}
