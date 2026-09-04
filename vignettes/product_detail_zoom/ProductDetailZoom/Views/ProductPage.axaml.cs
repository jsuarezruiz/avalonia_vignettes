using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using AvaloniaVignettes.Shared.Controls;
using ProductDetailZoom.Controls;

namespace ProductDetailZoom.Views;

/// <summary>
/// The page the demo opens on: the speaker, its write-up, and the button that zooms in. Port of
/// <c>demo.dart</c>.
/// </summary>
public partial class ProductPage : ContentPage, IHeroPage
{
    public static readonly DirectProperty<ProductPage, double> FrameWidthProperty =
        AvaloniaProperty.RegisterDirect<ProductPage, double>(nameof(FrameWidth), o => o.FrameWidth);

    public static readonly DirectProperty<ProductPage, double> FrameHeightProperty =
        AvaloniaProperty.RegisterDirect<ProductPage, double>(nameof(FrameHeight), o => o.FrameHeight);

    public static readonly DirectProperty<ProductPage, double> CopyHeightProperty =
        AvaloniaProperty.RegisterDirect<ProductPage, double>(nameof(CopyHeight), o => o.CopyHeight);

    public static readonly StyledProperty<double> CopySlideProperty =
        AvaloniaProperty.Register<ProductPage, double>(nameof(CopySlide));

    private const double CopyHeightFactor = 0.43d;

    // The write-up's slide. Kept and moved rather than replaced: assigning a new transform each
    // frame makes the render layer rebuild its state every tick, which is enough to make a slide
    // this slow visibly step.
    private readonly Avalonia.Media.TranslateTransform _slide = new();

    private double _frameWidth;
    private double _frameHeight;
    private double _copyHeight;

    public ProductPage()
    {
        InitializeComponent();

        DataContext = this;
        Description.RenderTransform = _slide;
    }

    /// <summary>
    /// Raised when the zoom button is pressed.
    /// </summary>
    public event EventHandler? ZoomRequested;

    /// <summary>
    /// Gets the sheet the speaker is drawn from.
    /// </summary>
    public Bitmap SpriteSheet => ProductAssets.SpriteSheet;

    /// <summary>
    /// Gets the bag shown in the app bar.
    /// </summary>
    public Bitmap BagImage => ProductAssets.Bag;

    /// <summary>
    /// Gets the width the speaker is drawn at.
    /// </summary>
    public double FrameWidth
    {
        get => _frameWidth;
        private set => SetAndRaise(FrameWidthProperty, ref _frameWidth, value);
    }

    /// <summary>
    /// Gets the height the speaker is drawn at.
    /// </summary>
    public double FrameHeight
    {
        get => _frameHeight;
        private set => SetAndRaise(FrameHeightProperty, ref _frameHeight, value);
    }

    /// <summary>
    /// Gets the height of the block of copy at the bottom.
    /// </summary>
    public double CopyHeight
    {
        get => _copyHeight;
        private set => SetAndRaise(CopyHeightProperty, ref _copyHeight, value);
    }

    /// <summary>
    /// Gets the speaker, which is the end of the flight that lives on this page.
    /// </summary>
    public Control Hero => Speaker;

    /// <summary>
    /// Gets the button that starts the zoom, so its fade can be driven from outside.
    /// </summary>
    public PulsingButton ZoomButton => Zoom;

    /// <summary>
    /// Gets or sets how far the write-up has slid aside, as a fraction of its own width. An animation
    /// can only be run against a visual, so this is animated on the page and mirrored onto the
    /// transform rather than being animated on the transform directly.
    /// </summary>
    public double CopySlide
    {
        get => GetValue(CopySlideProperty);
        set => SetValue(CopySlideProperty, value);
    }

    /// <summary>
    /// Gets the write-up, which slides aside as the zoom begins. Only this moves, the buttons
    /// below it stay put, as they do in the original.
    /// </summary>
    public Control Copy => Description;

    static ProductPage() =>
        CopySlideProperty.Changed.AddClassHandler<ProductPage>((x, e) =>
            x._slide.X = e.GetNewValue<double>() * x.Description.Bounds.Width);

    protected override Size ArrangeOverride(Size finalSize)
    {
        var (width, height) = ProductAssets.FrameFor(finalSize.Width, finalSize.Height);

        FrameWidth = width;
        FrameHeight = height;
        CopyHeight = finalSize.Height * CopyHeightFactor;

        return base.ArrangeOverride(finalSize);
    }

    private void OnZoomClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ZoomRequested?.Invoke(this, EventArgs.Empty);
}
