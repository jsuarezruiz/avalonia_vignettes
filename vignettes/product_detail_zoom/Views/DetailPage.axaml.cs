using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using AvaloniaVignettes.Shared.Controls;
using ProductDetailZoom.Controls;

namespace ProductDetailZoom.Views;

/// <summary>
/// The zoomed page: the speaker at the end of its spin, the callouts over it, and the button that
/// returns. Port of <c>details.dart</c>.
/// </summary>
public partial class DetailPage : ContentPage, IHeroPage
{
    /// <summary>
    /// Defines the <see cref="FrameWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<DetailPage, double> FrameWidthProperty =
        AvaloniaProperty.RegisterDirect<DetailPage, double>(nameof(FrameWidth), o => o.FrameWidth);

    /// <summary>
    /// Defines the <see cref="FrameHeight"/> property.
    /// </summary>
    public static readonly DirectProperty<DetailPage, double> FrameHeightProperty =
        AvaloniaProperty.RegisterDirect<DetailPage, double>(nameof(FrameHeight), o => o.FrameHeight);

    /// <summary>
    /// Where the return button sits, as a fraction from the centre to the right edge.
    /// </summary>
    private const double ShrinkAlignment = 0.6d;

    private double _frameWidth;
    private double _frameHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailPage"/> class.
    /// </summary>
    public DetailPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Raised when the return button is pressed.
    /// </summary>
    public event EventHandler? ShrinkRequested;

    /// <summary>
    /// Gets the sheet the speaker is drawn from.
    /// </summary>
    public Bitmap SpriteSheet => ProductAssets.SpriteSheet;

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
    /// Gets the speaker, which is the end of the flight that lives on this page.
    /// </summary>
    public Control Hero => Speaker;

    /// <summary>
    /// Gets the button that returns, so its delayed fade can be driven from outside.
    /// </summary>
    public PulsingButton ShrinkButton => Shrink;

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var (width, height) = ProductAssets.FrameFor(finalSize.Width, finalSize.Height);

        FrameWidth = width;
        FrameHeight = height;

        var result = base.ArrangeOverride(finalSize);

        // Alignment(.6, 0): six tenths of the way from the middle to the right edge.
        Shrink.RenderTransform = new Avalonia.Media.TranslateTransform(
            ShrinkAlignment * (finalSize.Width - Shrink.Bounds.Width) / 2d,
            0d);

        return result;
    }

    private void OnShrinkPressed(object? sender, EventArgs e) => ShrinkRequested?.Invoke(this, EventArgs.Empty);
}
