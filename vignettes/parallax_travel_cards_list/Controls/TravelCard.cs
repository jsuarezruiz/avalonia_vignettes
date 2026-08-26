using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ParallaxTravelCardsList.Controls;

/// <summary>
/// A single destination card: a pastel panel, three layers of artwork breaking out over its top
/// edge, and the destination copy. Port of <c>travel_card_renderer.dart</c>.
/// </summary>
public sealed class TravelCard : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TravelCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<TravelCard, string?>(nameof(Description));

    public static readonly StyledProperty<IBrush?> CardBackgroundProperty =
        AvaloniaProperty.Register<TravelCard, IBrush?>(nameof(CardBackground));

    public static readonly StyledProperty<Bitmap?> BackImageProperty =
        AvaloniaProperty.Register<TravelCard, Bitmap?>(nameof(BackImage));

    public static readonly StyledProperty<Bitmap?> MiddleImageProperty =
        AvaloniaProperty.Register<TravelCard, Bitmap?>(nameof(MiddleImage));

    public static readonly StyledProperty<Bitmap?> FrontImageProperty =
        AvaloniaProperty.Register<TravelCard, Bitmap?>(nameof(FrontImage));

    public static readonly StyledProperty<double> ParallaxOffsetProperty =
        AvaloniaProperty.Register<TravelCard, double>(nameof(ParallaxOffset));

    public static readonly StyledProperty<double> CardWidthProperty =
        AvaloniaProperty.Register<TravelCard, double>(nameof(CardWidth), 250d);

    public static readonly StyledProperty<double> CardHeightProperty =
        AvaloniaProperty.Register<TravelCard, double>(nameof(CardHeight));

    public static readonly DirectProperty<TravelCard, double> ArtworkReserveProperty =
        AvaloniaProperty.RegisterDirect<TravelCard, double>(nameof(ArtworkReserve), o => o.ArtworkReserve);

    public static readonly DirectProperty<TravelCard, BoxShadows> CardShadowProperty =
        AvaloniaProperty.RegisterDirect<TravelCard, BoxShadows>(nameof(CardShadow), o => o.CardShadow);

    private const double ArtworkReserveFactor = 0.57d;

    // Flutter's Colors.black12.
    private static readonly Color ShadowColor = Color.FromArgb(0x1F, 0, 0, 0);

    private double _artworkReserve;
    private BoxShadows _cardShadow;

    static TravelCard()
    {
        ParallaxOffsetProperty.Changed.AddClassHandler<TravelCard>((x, _) => x.UpdateShadow());
        CardHeightProperty.Changed.AddClassHandler<TravelCard>((x, _) => x.UpdateArtworkReserve());
    }

    public TravelCard() => UpdateShadow();

    /// <summary>
    /// Gets or sets the destination headline.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the supporting copy.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the pastel fill behind the artwork.
    /// </summary>
    public IBrush? CardBackground
    {
        get => GetValue(CardBackgroundProperty);
        set => SetValue(CardBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the rearmost artwork layer.
    /// </summary>
    public Bitmap? BackImage
    {
        get => GetValue(BackImageProperty);
        set => SetValue(BackImageProperty, value);
    }

    /// <summary>
    /// Gets or sets the middle artwork layer.
    /// </summary>
    public Bitmap? MiddleImage
    {
        get => GetValue(MiddleImageProperty);
        set => SetValue(MiddleImageProperty, value);
    }

    /// <summary>
    /// Gets or sets the frontmost artwork layer.
    /// </summary>
    public Bitmap? FrontImage
    {
        get => GetValue(FrontImageProperty);
        set => SetValue(FrontImageProperty, value);
    }

    /// <summary>
    /// Gets or sets the normalised drag offset, from -1 to 1. It spreads the artwork layers apart
    /// and deepens the card's shadow.
    /// </summary>
    public double ParallaxOffset
    {
        get => GetValue(ParallaxOffsetProperty);
        set => SetValue(ParallaxOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the card.
    /// </summary>
    public double CardWidth
    {
        get => GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height the artwork and copy are laid out against.
    /// </summary>
    public double CardHeight
    {
        get => GetValue(CardHeightProperty);
        set => SetValue(CardHeightProperty, value);
    }

    /// <summary>
    /// Gets the vertical space the copy leaves clear for the artwork.
    /// </summary>
    public double ArtworkReserve
    {
        get => _artworkReserve;
        private set => SetAndRaise(ArtworkReserveProperty, ref _artworkReserve, value);
    }

    /// <summary>
    /// Gets the pair of shadows under the card. Both grow with the drag offset, so a card lifts off
    /// the page as it tilts.
    /// </summary>
    public BoxShadows CardShadow
    {
        get => _cardShadow;
        private set => SetAndRaise(CardShadowProperty, ref _cardShadow, value);
    }

    private void UpdateArtworkReserve() => ArtworkReserve = CardHeight * ArtworkReserveFactor;

    private void UpdateShadow()
    {
        var magnitude = Math.Abs(ParallaxOffset);

        CardShadow = new BoxShadows(
            Shadow(4d * magnitude),
            [Shadow(10d + (6d * magnitude))]);

        static BoxShadow Shadow(double blur) => new()
        {
            OffsetX = 0d,
            OffsetY = 0d,
            Blur = blur,
            Spread = 0d,
            Color = ShadowColor,
        };
    }
}
