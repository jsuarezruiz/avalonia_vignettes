using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;
using ConstellationsList.Models;

namespace ConstellationsList.Views;

/// <summary>
/// One constellation, full size. Port of <c>constellation_detail_view.dart</c>.
/// </summary>
/// <remarks>
/// The chart and its lettering are held back until the flight through the stars is nearly over, then
/// arrive on overlapping slices of one clock, the chart swelling from nothing, the lettering fading
/// in a little later. Nothing here knows about the star field; it is choreographed purely by the
/// delay it is handed.
/// </remarks>
public partial class DetailPage : UserControl
{
    public static readonly StyledProperty<Constellation?> ConstellationProperty =
        AvaloniaProperty.Register<DetailPage, Constellation?>(nameof(Constellation));

    public static readonly DirectProperty<DetailPage, Bitmap?> ChartImageProperty =
        AvaloniaProperty.RegisterDirect<DetailPage, Bitmap?>(nameof(ChartImage), o => o.ChartImage);

    public static readonly DirectProperty<DetailPage, Bitmap?> LabelImageProperty =
        AvaloniaProperty.RegisterDirect<DetailPage, Bitmap?>(nameof(LabelImage), o => o.LabelImage);

    public static readonly StyledProperty<bool> IsRedModeProperty =
        AvaloniaProperty.Register<DetailPage, bool>(nameof(IsRedMode));

    private static readonly Easing ChartEasing = new IntervalEasing(0.4d, 0.8d, FlutterEasings.EaseOutQuad);

    private static readonly Easing LabelEasing = new IntervalEasing(0.6d, 1d, FlutterEasings.EaseOutQuad);

    private readonly ScaleTransform _chartScale = new(0d, 0d);

    private Bitmap? _chartImage;
    private Bitmap? _labelImage;
    private readonly AnimationController _reveal;

    public DetailPage()
    {
        InitializeComponent();

        DataContext = this;
        Chart.RenderTransform = _chartScale;

        _reveal = new AnimationController(this, OnRevealProgressChanged);
    }

    static DetailPage() =>
        ConstellationProperty.Changed.AddClassHandler<DetailPage>((x, _) => x.LoadArtwork());

    /// <summary>
    /// Raised when the return button is pressed.
    /// </summary>
    public event EventHandler? ReturnRequested;

    /// <summary>
    /// Gets or sets the constellation being shown.
    /// </summary>
    public Constellation? Constellation
    {
        get => GetValue(ConstellationProperty);
        set => SetValue(ConstellationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the title is the red, outlined kind.
    /// </summary>
    public bool IsRedMode
    {
        get => GetValue(IsRedModeProperty);
        set => SetValue(IsRedModeProperty, value);
    }

    /// <summary>
    /// Gets the star chart.
    /// </summary>
    public Bitmap? ChartImage
    {
        get => _chartImage;
        private set => SetAndRaise(ChartImageProperty, ref _chartImage, value);
    }

    /// <summary>
    /// Gets the lettering drawn over the chart.
    /// </summary>
    public Bitmap? LabelImage
    {
        get => _labelImage;
        private set => SetAndRaise(LabelImageProperty, ref _labelImage, value);
    }

    /// <summary>
    /// Gets the card the title is drawn in, so a caller can fly one to it.
    /// </summary>
    public Control TitleCardControl => TitleCard;

    /// <summary>
    /// Starts the reveal, spread over <paramref name="duration"/>.
    /// </summary>
    public void Reveal(TimeSpan duration)
    {
        _reveal.Duration = duration;
        _reveal.SetValue(0d);
        _reveal.Forward();
    }

    // Loads the artwork here rather than binding the file locations straight at the images: a
    // binding hands the target a Uri, which is not converted to an image at runtime,
    // and the pictures simply never appear.
    private void LoadArtwork()
    {
        if (Constellation is not { } constellation)
        {
            ChartImage = LabelImage = null;
            return;
        }

        ChartImage = new Bitmap(AssetLoader.Open(constellation.ChartUri));
        LabelImage = new Bitmap(AssetLoader.Open(constellation.LabelUri));
    }

    private void OnRevealProgressChanged(double progress)
    {
        _chartScale.ScaleX = _chartScale.ScaleY = ChartEasing.Ease(progress);

        Label.Opacity = LabelEasing.Ease(progress);
    }

    private void OnReturnClicked(object? sender, RoutedEventArgs e) =>
        ReturnRequested?.Invoke(this, EventArgs.Empty);
}
