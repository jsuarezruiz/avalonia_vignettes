using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using ParallaxTravelCardsHero.Models;

namespace ParallaxTravelCardsHero.Controls;

/// <summary>
/// The city card, and the scene it opens into. Port of <c>city_scenery.dart</c>'s
/// <c>CityScenery</c>.
/// </summary>
/// <remarks>
/// The same control serves as the closed card, as the open scene on the details page, and as the
/// copy that flies between them; the only difference is <see cref="AnimationValue"/>. Everything it
/// shows is a function of that one number, which is what lets the flight simply feed it a progress
/// value and get every intermediate state for free.
/// </remarks>
public sealed class CityScenery : TemplatedControl
{
    public static readonly StyledProperty<double> AnimationValueProperty =
        AvaloniaProperty.Register<CityScenery, double>(nameof(AnimationValue));

    public static readonly StyledProperty<double> ScreenWidthProperty =
        AvaloniaProperty.Register<CityScenery, double>(nameof(ScreenWidth));

    public static readonly StyledProperty<double> ScreenHeightProperty =
        AvaloniaProperty.Register<CityScenery, double>(nameof(ScreenHeight));

    public static readonly StyledProperty<City?> CityProperty =
        AvaloniaProperty.Register<CityScenery, City?>(nameof(City));

    public static readonly DirectProperty<CityScenery, IBrush?> SceneryBrushProperty =
        AvaloniaProperty.RegisterDirect<CityScenery, IBrush?>(nameof(SceneryBrush), o => o.SceneryBrush);

    public static readonly DirectProperty<CityScenery, CornerRadius> CardCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<CityScenery, CornerRadius>(nameof(CardCornerRadius), o => o.CardCornerRadius);

    public static readonly DirectProperty<CityScenery, double> CardInfoOpacityProperty =
        AvaloniaProperty.RegisterDirect<CityScenery, double>(nameof(CardInfoOpacity), o => o.CardInfoOpacity);

    public static readonly DirectProperty<CityScenery, double> SkylineReserveProperty =
        AvaloniaProperty.RegisterDirect<CityScenery, double>(nameof(SkylineReserve), o => o.SkylineReserve);

    private const double ClosedCornerRadius = 10d;

    private const double SkylineReserveFactor = 0.22d;

    private static readonly Color OpenGradientStart = Color.FromRgb(0xFD, 0xE9, 0xC8);

    private static readonly Color OpenGradientEnd = Color.FromRgb(0xFD, 0xF8, 0xF1);

    private static readonly Easing CardInfoFade = new IntervalEasing(0d, 0.22d);

    private readonly GradientStop _gradientTop = new(Colors.Transparent, 0d);
    private readonly GradientStop _gradientBottom = new(Colors.Transparent, 1d);

    private IBrush? _sceneryBrush;
    private CornerRadius _cardCornerRadius = new(ClosedCornerRadius);
    private double _cardInfoOpacity = 1d;
    private double _skylineReserve;

    static CityScenery()
    {
        AnimationValueProperty.Changed.AddClassHandler<CityScenery>((x, _) => x.Refresh());
        CityProperty.Changed.AddClassHandler<CityScenery>((x, _) => x.Refresh());
        ScreenHeightProperty.Changed.AddClassHandler<CityScenery>((x, e) =>
            x.SkylineReserve = e.GetNewValue<double>() * SkylineReserveFactor);
    }

    /// <summary>
    /// Gets or sets how far the card has opened, from 0 for closed to 1 for the full scene.
    /// </summary>
    public double AnimationValue
    {
        get => GetValue(AnimationValueProperty);
        set => SetValue(AnimationValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the screen width, the equivalent of <c>MediaQuery.size.width</c>.
    /// </summary>
    public double ScreenWidth
    {
        get => GetValue(ScreenWidthProperty);
        set => SetValue(ScreenWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the screen height, the equivalent of <c>MediaQuery.size.height</c>.
    /// </summary>
    public double ScreenHeight
    {
        get => GetValue(ScreenHeightProperty);
        set => SetValue(ScreenHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the destination this card is for.
    /// </summary>
    public City? City
    {
        get => GetValue(CityProperty);
        set => SetValue(CityProperty, value);
    }

    /// <summary>
    /// Gets the gradient filling the card, which warms and lightens as it opens.
    /// </summary>
    public IBrush? SceneryBrush
    {
        get => _sceneryBrush;
        private set => SetAndRaise(SceneryBrushProperty, ref _sceneryBrush, value);
    }

    /// <summary>
    /// Gets the card's corner radius, which squares off as it opens into a full page.
    /// </summary>
    public CornerRadius CardCornerRadius
    {
        get => _cardCornerRadius;
        private set => SetAndRaise(CardCornerRadiusProperty, ref _cardCornerRadius, value);
    }

    /// <summary>
    /// Gets the opacity of the card copy, which is gone by the time the card is a fifth open.
    /// </summary>
    public double CardInfoOpacity
    {
        get => _cardInfoOpacity;
        private set => SetAndRaise(CardInfoOpacityProperty, ref _cardInfoOpacity, value);
    }

    /// <summary>
    /// Gets the space above the card copy that the skyline is drawn into.
    /// </summary>
    public double SkylineReserve
    {
        get => _skylineReserve;
        private set => SetAndRaise(SkylineReserveProperty, ref _skylineReserve, value);
    }

    private void Refresh()
    {
        if (City is not { } city)
        {
            return;
        }

        var value = AnimationValue;

        // The top of the gradient warms ahead of the bottom, so the sky reads as lit before the
        // ground does. The stops are mutated rather than replaced, because this runs every frame of
        // the flight.
        _gradientTop.Color = Lerp(city.Color, OpenGradientStart, FlutterEasings.EaseOut.Ease(value));
        _gradientBottom.Color = Lerp(city.Color, OpenGradientEnd, value);

        SceneryBrush ??= new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5d, 0d, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5d, 1d, RelativeUnit.Relative),
            GradientStops = { _gradientTop, _gradientBottom },
        };

        CardCornerRadius = new CornerRadius(ClosedCornerRadius * (1d - value));
        CardInfoOpacity = 1d - CardInfoFade.Ease(value);
    }

    private static Color Lerp(Color from, Color to, double progress)
    {
        return Color.FromArgb(
            Channel(from.A, to.A),
            Channel(from.R, to.R),
            Channel(from.G, to.G),
            Channel(from.B, to.B));

        byte Channel(byte start, byte end) => (byte)Math.Round(start + ((end - start) * progress));
    }
}
