using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;

namespace GooeyEdge.Controls;

/// <summary>
/// One full-bleed page of the carousel. Port of <c>content_card.dart</c>.
/// </summary>
/// <remarks>
/// The background is permanently, very slowly breathing: it is scaled a little past the card and
/// drifted on two out-of-phase sine waves, so the artwork never sits still even when nothing is
/// being swiped.
/// </remarks>
public sealed class ContentCard : TemplatedControl
{
    public static readonly StyledProperty<string?> PaletteProperty =
        AvaloniaProperty.Register<ContentCard, string?>(nameof(Palette));

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<ContentCard, IBrush?>(nameof(AccentBrush));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ContentCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<ContentCard, string?>(nameof(Subtitle));

    public static readonly DirectProperty<ContentCard, Bitmap?> BackgroundImageProperty =
        AvaloniaProperty.RegisterDirect<ContentCard, Bitmap?>(nameof(BackgroundImage), o => o.BackgroundImage);

    public static readonly DirectProperty<ContentCard, Bitmap?> IllustrationImageProperty =
        AvaloniaProperty.RegisterDirect<ContentCard, Bitmap?>(nameof(IllustrationImage), o => o.IllustrationImage);

    public static readonly DirectProperty<ContentCard, Bitmap?> SliderImageProperty =
        AvaloniaProperty.RegisterDirect<ContentCard, Bitmap?>(nameof(SliderImage), o => o.SliderImage);

    public static readonly DirectProperty<ContentCard, ITransform> BackgroundTransformProperty =
        AvaloniaProperty.RegisterDirect<ContentCard, ITransform>(nameof(BackgroundTransform), o => o.BackgroundTransform);

    private const string ImageRoot = "avares://GooeyEdge/Assets/Images";

    private const double DriftPeriodMilliseconds = 2000d;

    private readonly MatrixTransform _backgroundTransform = new(Matrix.Identity);

    private FrameTicker? _ticker;
    private Bitmap? _backgroundImage;
    private Bitmap? _illustrationImage;
    private Bitmap? _sliderImage;

    static ContentCard() =>
        PaletteProperty.Changed.AddClassHandler<ContentCard>((x, _) => x.LoadImages());

    /// <summary>
    /// Gets or sets the palette name used to pick the artwork, such as <c>Red</c>.
    /// </summary>
    public string? Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill of the call-to-action button.
    /// </summary>
    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the headline.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the supporting copy.
    /// </summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Gets the full-bleed background artwork.
    /// </summary>
    public Bitmap? BackgroundImage
    {
        get => _backgroundImage;
        private set => SetAndRaise(BackgroundImageProperty, ref _backgroundImage, value);
    }

    /// <summary>
    /// Gets the illustration shown above the copy.
    /// </summary>
    public Bitmap? IllustrationImage
    {
        get => _illustrationImage;
        private set => SetAndRaise(IllustrationImageProperty, ref _illustrationImage, value);
    }

    /// <summary>
    /// Gets the page indicator dots.
    /// </summary>
    public Bitmap? SliderImage
    {
        get => _sliderImage;
        private set => SetAndRaise(SliderImageProperty, ref _sliderImage, value);
    }

    /// <summary>
    /// Gets the drift applied to the background. The instance is stable; its matrix is what changes
    /// each frame, which is enough to keep the render in step.
    /// </summary>
    public ITransform BackgroundTransform => _backgroundTransform;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private void OnTick(TimeSpan elapsed)
    {
        if (Bounds is not { Width: > 0d, Height: > 0d })
        {
            return;
        }

        var time = elapsed.TotalMilliseconds / DriftPeriodMilliseconds;

        var scaleX = 1.2d + (Math.Sin(time) * 0.05d);
        var scaleY = 1.2d + (Math.Cos(time) * 0.07d);
        var offsetY = 20d + (Math.Cos(time) * 20d);

        // The scale is anchored at the top-left, so the card is re-centred by hand.
        var translateX = -(scaleX - 1d) / 2d * Bounds.Width;
        var translateY = (-(scaleY - 1d) / 2d * Bounds.Height) + offsetY;

        _backgroundTransform.Matrix =
            Matrix.CreateScale(scaleX, scaleY) * Matrix.CreateTranslation(translateX, translateY);
    }

    private void LoadImages()
    {
        if (Palette is not { Length: > 0 } palette)
        {
            BackgroundImage = IllustrationImage = SliderImage = null;
            return;
        }

        BackgroundImage = Load($"Bg-{palette}");
        IllustrationImage = Load($"Illustration-{palette}");
        SliderImage = Load($"Slider-{palette}");

        static Bitmap Load(string name) => new(AssetLoader.Open(new Uri($"{ImageRoot}/{name}.png")));
    }
}
