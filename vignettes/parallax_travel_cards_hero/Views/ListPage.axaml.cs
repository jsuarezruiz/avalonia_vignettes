using Avalonia;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Controls;
using ParallaxTravelCardsHero.Controls;

namespace ParallaxTravelCardsHero.Views;

/// <summary>
/// The page the demo opens on: a heading, the city card, and the hotels for that city. Port of
/// <c>demo.dart</c>'s <c>HeroCardDemo</c>.
/// </summary>
public partial class ListPage : ContentPage, IHeroPage
{
    public static readonly StyledProperty<double> ScreenWidthProperty =
        AvaloniaProperty.Register<ListPage, double>(nameof(ScreenWidth));

    public static readonly StyledProperty<double> ScreenHeightProperty =
        AvaloniaProperty.Register<ListPage, double>(nameof(ScreenHeight));

    public static readonly DirectProperty<ListPage, double> CardWidthProperty =
        AvaloniaProperty.RegisterDirect<ListPage, double>(nameof(CardWidth), o => o.CardWidth);

    public static readonly DirectProperty<ListPage, double> CardHeightProperty =
        AvaloniaProperty.RegisterDirect<ListPage, double>(nameof(CardHeight), o => o.CardHeight);

    private const double MaxCardWidth = 300d;

    private const double CardHeightFactor = 0.44d;

    private double _cardWidth;
    private double _cardHeight;

    public ListPage() => InitializeComponent();

    static ListPage()
    {
        // The card is a Container with only maximum constraints wrapped around a Stack that fills
        // whatever it is given, so those maximums are the size it settles at.
        ScreenWidthProperty.Changed.AddClassHandler<ListPage>((x, e) =>
            x.CardWidth = Math.Min(MaxCardWidth, e.GetNewValue<double>()));

        ScreenHeightProperty.Changed.AddClassHandler<ListPage>((x, e) =>
            x.CardHeight = e.GetNewValue<double>() * CardHeightFactor);
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
    /// Gets the card's width.
    /// </summary>
    public double CardWidth
    {
        get => _cardWidth;
        private set => SetAndRaise(CardWidthProperty, ref _cardWidth, value);
    }

    /// <summary>
    /// Gets the card's height.
    /// </summary>
    public double CardHeight
    {
        get => _cardHeight;
        private set => SetAndRaise(CardHeightProperty, ref _cardHeight, value);
    }

    /// <summary>
    /// Gets the card, which is the end of the flight that lives on this page.
    /// </summary>
    public Control Hero => Card;
}
