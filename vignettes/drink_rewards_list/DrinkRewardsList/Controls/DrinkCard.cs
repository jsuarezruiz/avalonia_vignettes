using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;
using DrinkRewardsList.Models;

namespace DrinkRewardsList.Controls;

/// <summary>
/// One reward on the list: a bar showing the drink and its price, which opens into a card that
/// fills with the drink as you go. Port of <c>drink_card.dart</c>.
/// </summary>
/// <remarks>
/// Opening runs two clocks at once. One springs the card's height open on an elastic curve, and
/// overshoots, that wobble is the whole character of the thing. The other is a three second clock
/// the fill rides on: the liquid rises over a slice near the start, the points count down over an
/// overlapping slice, and the surface keeps sloshing long after both have finished.
/// </remarks>
public sealed class DrinkCard : TemplatedControl
{
    public static readonly StyledProperty<Drink?> DrinkProperty =
        AvaloniaProperty.Register<DrinkCard, Drink?>(nameof(Drink));

    public static readonly StyledProperty<int> EarnedPointsProperty =
        AvaloniaProperty.Register<DrinkCard, int>(nameof(EarnedPoints), 100);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<DrinkCard, bool>(nameof(IsOpen));

    public static readonly DirectProperty<DrinkCard, string?> PointsRemainingLabelProperty =
        AvaloniaProperty.RegisterDirect<DrinkCard, string?>(
            nameof(PointsRemainingLabel), o => o.PointsRemainingLabel);

    public static readonly DirectProperty<DrinkCard, Bitmap?> ArtworkProperty =
        AvaloniaProperty.RegisterDirect<DrinkCard, Bitmap?>(nameof(Artwork), o => o.Artwork);

    /// <summary>
    /// Raised when the card is tapped.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> TappedCardEvent =
        RoutedEvent.Register<DrinkCard, RoutedEventArgs>(nameof(TappedCard), RoutingStrategies.Bubble);

    /// <summary>
    /// The height of a closed card. The list scrolls by this, so it is public.
    /// </summary>
    public const double NominalHeightClosed = 96d;

    /// <summary>
    /// The height of an open card.
    /// </summary>
    public const double NominalHeightOpen = 290d;

    private const double LiquidDrop = 1.2d;

    private static readonly TimeSpan OpenDuration = TimeSpan.FromMilliseconds(1500);
    private static readonly TimeSpan CloseDuration = TimeSpan.FromMilliseconds(1200);
    private static readonly TimeSpan FillDuration = TimeSpan.FromMilliseconds(3000);

    private static readonly Easing OpenEasing = new ElasticOutEasing { Period = 0.4d };

    private static readonly Easing CloseEasing = new ElasticOutEasing { Period = 0.9d };

    private static readonly Easing FillEasing = new IntervalEasing(0.12d, 0.45d, FlutterEasings.EaseOut);

    private static readonly Easing PointsEasing = new IntervalEasing(0.1d, 0.5d, FlutterEasings.EaseOutQuart);

    private readonly AnimationController _height;
    private readonly AnimationController _fill;

    private LiquidBackground? _liquid;
    private Bitmap? _artwork;
    private string? _pointsRemainingLabel;
    private double _heightFrom = NominalHeightClosed;

    static DrinkCard()
    {
        IsOpenProperty.Changed.AddClassHandler<DrinkCard>((x, e) => x.OnIsOpenChanged(e));
        DrinkProperty.Changed.AddClassHandler<DrinkCard>((x, _) => x.OnDrinkChanged());
        EarnedPointsProperty.Changed.AddClassHandler<DrinkCard>((x, _) => x.UpdatePoints());
    }

    public DrinkCard()
    {
        Height = NominalHeightClosed;

        _height = new AnimationController(this, OnHeightProgressChanged) { Duration = OpenDuration };
        _fill = new AnimationController(this, OnFillProgressChanged) { Duration = FillDuration };

        Tapped += (_, _) => RaiseEvent(new RoutedEventArgs(TappedCardEvent, this));
    }

    /// <summary>
    /// Occurs when the card is tapped.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? TappedCard
    {
        add => AddHandler(TappedCardEvent, value);
        remove => RemoveHandler(TappedCardEvent, value);
    }

    /// <summary>
    /// Gets or sets the drink this card offers.
    /// </summary>
    public Drink? Drink
    {
        get => GetValue(DrinkProperty);
        set => SetValue(DrinkProperty, value);
    }

    /// <summary>
    /// Gets or sets how many points the customer has.
    /// </summary>
    public int EarnedPoints
    {
        get => GetValue(EarnedPointsProperty);
        set => SetValue(EarnedPointsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the card is open.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets the drink's artwork.
    /// </summary>
    public Bitmap? Artwork
    {
        get => _artwork;
        private set => SetAndRaise(ArtworkProperty, ref _artwork, value);
    }

    /// <summary>
    /// Gets how many points are still needed, counting down as the card fills.
    /// </summary>
    public string? PointsRemainingLabel
    {
        get => _pointsRemainingLabel;
        private set => SetAndRaise(PointsRemainingLabelProperty, ref _pointsRemainingLabel, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _liquid = e.NameScope.Find<LiquidBackground>("PART_Liquid");

        OnFillProgressChanged(_fill.Value);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _height.Stop();
        _fill.Stop();
    }

    private void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var isOpen = e.GetNewValue<bool>();

        PseudoClasses.Set(":open", isOpen);

        if (isOpen)
        {
            // A fresh roll each time, so opening the same card twice never looks the same.
            _liquid?.Restart();

            _fill.SetValue(0d);
            _fill.Forward();
        }

        // The card springs open further than it settles, and takes longer doing it than it does
        // closing again.
        _height.Duration = isOpen ? OpenDuration : CloseDuration;
        _heightFrom = Bounds.Height > 0d ? Height : NominalHeightClosed;

        _height.SetValue(0d);
        _height.Forward();
    }

    private void OnHeightProgressChanged(double progress)
    {
        var easing = IsOpen ? OpenEasing : CloseEasing;
        var target = IsOpen ? NominalHeightOpen : NominalHeightClosed;

        SetCurrentValue(HeightProperty, _heightFrom + ((target - _heightFrom) * easing.Ease(progress)));
    }

    private void OnFillProgressChanged(double progress)
    {
        UpdatePoints(PointsEasing.Ease(progress));

        if (_liquid is not { } liquid)
        {
            return;
        }

        liquid.FarSurface.Update(progress);
        liquid.NearSurface.Update(progress);

        // The liquid starts a card and a bit below the top and rises to however full this drink is.
        liquid.Level = (NominalHeightOpen * LiquidDrop)
            - (NominalHeightOpen * FillEasing.Ease(progress) * MaxFillLevel * LiquidDrop);

        liquid.InvalidateVisual();
    }

    private double MaxFillLevel =>
        Drink is { RequiredPoints: > 0 } drink ? Math.Min(1d, (double)EarnedPoints / drink.RequiredPoints) : 0d;

    private void OnDrinkChanged()
    {
        Artwork = Drink is { } drink
            ? new Bitmap(AssetLoader.Open(new Uri($"avares://DrinkRewardsList/Assets/Images/{drink.Image}")))
            : null;

        UpdatePoints();
    }

    private void UpdatePoints() => UpdatePoints(PointsEasing.Ease(_fill.Value));

    private void UpdatePoints(double countdown)
    {
        if (Drink is not { } drink)
        {
            PointsRemainingLabel = null;
            return;
        }

        var required = drink.RequiredPoints;
        var remaining = required - (countdown * Math.Min(EarnedPoints, required));

        PointsRemainingLabel = Math.Round(remaining).ToString("0");

        // The copy swaps to a congratulation once the countdown reaches zero, which only happens
        // for a drink the customer can already afford.
        PseudoClasses.Set(":redeemable", remaining <= 0d);
        PseudoClasses.Set(":affordable", EarnedPoints >= required);
    }
}
