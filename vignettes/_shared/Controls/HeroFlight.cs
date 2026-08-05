using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Animation;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// A page that owns one end of a shared-element flight.
/// </summary>
public interface IHeroPage
{
    /// <summary>
    /// Gets the control this page hands over to, and receives back.
    /// </summary>
    Control Hero { get; }
}

/// <summary>
/// Flies a copy of a control between the two pages of a navigation, which is what Flutter's
/// <c>Hero</c> does and Avalonia has no equivalent for.
/// </summary>
/// <remarks>
/// A page transition only ever gets the two pages, so it can fade or slide them but cannot hand a
/// control from one to the other. This layer sits above the navigation, hides both ends for the
/// duration, and moves a stand-in between their two slots along the arc Material uses.
/// <para>
/// <see cref="ContentProgress"/> is the stand-in's own progress: it counts up on the way in and back
/// down on the way out, so content that is a function of it, as the scenery is, opens as it flies
/// and closes as it returns.
/// </para>
/// </remarks>
public sealed class HeroFlight : Canvas
{
    /// <summary>
    /// Defines the <see cref="Content"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<HeroFlight, Control?>(nameof(Content));

    /// <summary>
    /// Defines the <see cref="Progress"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<HeroFlight, double>(nameof(Progress));

    /// <summary>
    /// Defines the <see cref="ContentProgress"/> property.
    /// </summary>
    public static readonly DirectProperty<HeroFlight, double> ContentProgressProperty =
        AvaloniaProperty.RegisterDirect<HeroFlight, double>(nameof(ContentProgress), o => o.ContentProgress);

    private MaterialRectArcTween? _path;
    private Control? _from;
    private Control? _to;
    private double _contentProgress;
    private double _current;
    private Size _measured;
    private bool _isForward = true;

    static HeroFlight()
    {
        ContentProperty.Changed.AddClassHandler<HeroFlight>((x, e) => x.OnContentChanged(e));
        ProgressProperty.Changed.AddClassHandler<HeroFlight>((x, e) =>
        {
            x._current = e.GetNewValue<double>();
            x.Place();
        });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroFlight"/> class.
    /// </summary>
    public HeroFlight() => IsHitTestVisible = false;

    /// <summary>
    /// Gets or sets the stand-in that flies between the two slots.
    /// </summary>
    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets how far along its path the stand-in is, from 0 to 1.
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// Gets how far open the stand-in should be: the same as <see cref="Progress"/> going in, and
    /// its opposite coming back.
    /// </summary>
    public double ContentProgress
    {
        get => _contentProgress;
        private set => SetAndRaise(ContentProgressProperty, ref _contentProgress, value);
    }

    /// <summary>
    /// Runs a flight between the hero slots of two pages, for as long as the navigation that
    /// triggered it lasts.
    /// </summary>
    /// <param name="from">The page being left.</param>
    /// <param name="to">The page being entered.</param>
    /// <param name="forward">Whether this is a push rather than a pop.</param>
    /// <param name="duration">How long the navigation takes.</param>
    /// <param name="cancellationToken">Cancels the flight if the navigation is interrupted.</param>
    public async Task RunAsync(
        Visual? from,
        Visual? to,
        bool forward,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        // The page being entered has only just been added, so it has no layout yet, and no
        // realised content either, which is what the search below needs.
        (to as Layoutable)?.UpdateLayout();

        if (Content is null || HeroIn(from) is not { } departure || HeroIn(to) is not { } arrival)
        {
            return;
        }

        _from = departure;
        _to = arrival;
        _isForward = forward;
        _path = new MaterialRectArcTween(RectOf(_from), RectOf(_to));

        // Both ends are hidden rather than removed, the way a Hero leaves a placeholder of its own
        // size behind: taking either out of its page would let the rest of it jump.
        _from.Opacity = 0d;
        _to.Opacity = 0d;

        // The animation holds its final value after a run, which outranks a plain write, so the
        // start of the next flight is set here rather than by resetting the animated property,
        // otherwise a second departure would be drawn at the previous arrival for a frame.
        _current = 0d;
        Progress = 0d;

        Content.IsVisible = true;
        Place();

        try
        {
            await Flight(duration).RunAsync(this, cancellationToken);
        }
        finally
        {
            Land();
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);

        // A Canvas measures its children unconstrained, which would leave the stand-in laid out for
        // a width it is never given: text that wraps at the slot's width would be measured on one
        // line and drawn on two, and the second line would be cut off for the whole flight. It is
        // measured at the slot it is about to be arranged into instead.
        if (Content is { IsVisible: true } content && _path is not null)
        {
            _measured = _path.Lerp(_current).Size;

            content.Measure(_measured);
        }

        return size;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Canvas arranges its children at their desired size; the stand-in is given the slot it is
        // currently interpolated to instead.
        base.ArrangeOverride(finalSize);

        if (Content is { IsVisible: true } content && _path is not null)
        {
            content.Arrange(_path.Lerp(_current));
        }

        return finalSize;
    }

    /// <summary>
    /// The flight runs on Material's standard easing, in both directions.
    /// </summary>
    /// <remarks>
    /// The final value is held rather than released, so the stand-in stays where it landed for the
    /// frame in which the real control takes over. Releasing it would snap the stand-in back to the
    /// start of its path first, which reads as a flicker.
    /// </remarks>
    private static Avalonia.Animation.Animation Flight(TimeSpan duration) => new()
    {
        Duration = duration,
        Easing = FlutterEasings.FastOutSlowIn,
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(ProgressProperty, 0d) } },
            new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(ProgressProperty, 1d) } },
        },
    };

    private void OnContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is Control old)
        {
            Children.Remove(old);
        }

        if (e.NewValue is Control added)
        {
            added.IsVisible = false;
            Children.Add(added);
        }
    }

    /// <summary>
    /// Finds the hero a page hands over. A transition is given whatever the navigation hosts its
    /// pages in rather than the page itself, so the page is looked for underneath it.
    /// </summary>
    private static Control? HeroIn(Visual? visual) => visual switch
    {
        null => null,
        IHeroPage page => page.Hero,
        _ => visual.GetVisualDescendants().OfType<IHeroPage>().FirstOrDefault()?.Hero,
    };

    /// <summary>
    /// Where a control sits in this layer's coordinates, right now.
    /// </summary>
    private Rect RectOf(Visual control) =>
        new(control.TranslatePoint(default, this) ?? default, control.Bounds.Size);

    private void Place()
    {
        if (_path is null)
        {
            return;
        }

        ContentProgress = _isForward ? _current : 1d - _current;

        // Only a change of size needs a fresh measure. Where the two slots are the same size, a
        // shared element that transforms in place rather than travelling, re-measuring every frame
        // would put a full layout pass between each one for nothing.
        if (_path is not null && _path.Lerp(_current).Size != _measured)
        {
            InvalidateMeasure();
        }
        else
        {
            InvalidateArrange();
        }
    }

    /// <summary>
    /// Hands the control back. Both ends are restored, not just the one arrived at, or the far one
    /// stays hidden the next time it is needed.
    /// </summary>
    private void Land()
    {
        if (Content is { } content)
        {
            content.IsVisible = false;
        }

        if (_from is not null)
        {
            _from.Opacity = 1d;
        }

        if (_to is not null)
        {
            _to.Opacity = 1d;
        }

        _from = null;
        _to = null;
        _path = null;
        _measured = default;
    }
}
