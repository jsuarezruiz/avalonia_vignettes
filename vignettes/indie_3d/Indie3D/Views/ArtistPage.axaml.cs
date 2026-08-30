using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Indie3D.Models;

namespace Indie3D.Views;

/// <summary>
/// One artist's page. Port of <c>page.dart</c>.
/// </summary>
public partial class ArtistPage : UserControl
{
    public static readonly StyledProperty<ShapeScene?> SceneProperty =
        AvaloniaProperty.Register<ArtistPage, ShapeScene?>(nameof(Scene));

    public static readonly DirectProperty<ArtistPage, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<ArtistPage, bool>(nameof(IsLoading), o => o.IsLoading);

    public static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<ArtistPage, int>(nameof(PageIndex));

    public static readonly StyledProperty<string> TopTitleProperty =
        AvaloniaProperty.Register<ArtistPage, string>(nameof(TopTitle), string.Empty);

    public static readonly StyledProperty<string> BottomTitleProperty =
        AvaloniaProperty.Register<ArtistPage, string>(nameof(BottomTitle), string.Empty);

    public static readonly StyledProperty<IBrush?> PageBrushProperty =
        AvaloniaProperty.Register<ArtistPage, IBrush?>(nameof(PageBrush));

    public static readonly StyledProperty<IImage?> ArtworkProperty =
        AvaloniaProperty.Register<ArtistPage, IImage?>(nameof(Artwork));

    public static readonly StyledProperty<double> BottomTitleScaleProperty =
        AvaloniaProperty.Register<ArtistPage, double>(nameof(BottomTitleScale), 1d);

    public static readonly StyledProperty<double> BehindOpacityProperty =
        AvaloniaProperty.Register<ArtistPage, double>(nameof(BehindOpacity), 0.85d);

    public ArtistPage()
    {
        InitializeComponent();

        DataContext = this;
    }

    /// <summary>
    /// Gets or sets the shapes drifting over the page.
    /// </summary>
    public ShapeScene? Scene
    {
        get => GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    /// <summary>
    /// Gets or sets which page this is.
    /// </summary>
    public int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the artist's first name.
    /// </summary>
    public string TopTitle
    {
        get => GetValue(TopTitleProperty);
        set => SetValue(TopTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the artist's last name.
    /// </summary>
    public string BottomTitle
    {
        get => GetValue(BottomTitleProperty);
        set => SetValue(BottomTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the page's colour.
    /// </summary>
    public IBrush? PageBrush
    {
        get => GetValue(PageBrushProperty);
        set => SetValue(PageBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the artist's photograph.
    /// </summary>
    public IImage? Artwork
    {
        get => GetValue(ArtworkProperty);
        set => SetValue(ArtworkProperty, value);
    }

    /// <summary>
    /// Gets or sets the extra scaling the last name carries on this page.
    /// </summary>
    public double BottomTitleScale
    {
        get => GetValue(BottomTitleScaleProperty);
        set => SetValue(BottomTitleScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets how strongly the shapes behind the artist are laid on.
    /// </summary>
    public double BehindOpacity
    {
        get => GetValue(BehindOpacityProperty);
        set => SetValue(BehindOpacityProperty, value);
    }

    /// <summary>
    /// Gets whether the shapes are still being read in.
    /// </summary>
    public bool IsLoading => Scene is null;

    /// <summary>
    /// Gets which layer of shapes goes behind the artist.
    /// </summary>
    public int BehindLayer => PageIndex * 2;

    /// <summary>
    /// Gets which layer of shapes goes in front.
    /// </summary>
    public int FrontLayer => (PageIndex * 2) + 1;

    /// <summary>
    /// Gets the title strip, so the demo can wipe it in.
    /// </summary>
    public Controls.ClippedTitle Title => Titles;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SceneProperty)
        {
            var wasLoading = change.GetOldValue<ShapeScene?>() is null;
            var isLoading = change.GetNewValue<ShapeScene?>() is null;

            if (wasLoading != isLoading)
            {
                RaisePropertyChanged(IsLoadingProperty, wasLoading, isLoading);
            }
        }
    }

    /// <summary>
    /// Redraws the shapes, which move every frame.
    /// </summary>
    public void Invalidate()
    {
        Behind.InvalidateVisual();
        InFront.InvalidateVisual();
    }
}
