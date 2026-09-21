using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Media;

namespace BubbleTabBar.Controls;

/// <summary>
/// One tab of the <see cref="NavBar"/>: a pill that swells to fit its label when picked and shrinks
/// back to its icon when not. Port of <c>navbar_button.dart</c>.
/// </summary>
/// <remarks>
/// The label is always there. What changes is how much of the pill there is to see it through, so
/// the text is revealed by the pill growing rather than by fading in, which is why the button is
/// built around a <see cref="ClippedView"/> instead of simply hiding the label.
/// <para>
/// The icon turns a half circle about its vertical axis as the tab is taken, and unwinds when it is
/// given up. It ends face on but mirrored, which is what the original does.
/// </para>
/// <para>
/// The outline goes in <see cref="TabItem.Icon"/>, the property a tab already has for exactly this,
/// rather than in one of its own.
/// </para>
/// </remarks>
public sealed class NavBarButton : TabItem
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NavBarButton, string?>(nameof(Title));

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<NavBarButton, IBrush?>(nameof(AccentBrush));

    public static readonly StyledProperty<double> ExpandedWidthProperty =
        AvaloniaProperty.Register<NavBarButton, double>(nameof(ExpandedWidth), 110d);

    public static readonly DirectProperty<NavBarButton, ITransform> IconTransformProperty =
        AvaloniaProperty.RegisterDirect<NavBarButton, ITransform>(nameof(IconTransform), o => o.IconTransform);

    private const double IconTurnDegrees = 180d;

    private static readonly TimeSpan IconTurnDuration = TimeSpan.FromMilliseconds(350);

    private readonly MatrixTransform _iconTransform = new(Matrix.Identity);
    private readonly AnimationController _iconTurn;

    static NavBarButton() =>
        IsSelectedProperty.Changed.AddClassHandler<NavBarButton>((x, e) => x.OnIsSelectedChanged(e));

    public NavBarButton() =>
        _iconTurn = new AnimationController(this, OnIconTurnChanged) { Duration = IconTurnDuration };

    /// <summary>
    /// Gets or sets the label shown beside the icon.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour the pill takes when this tab is picked.
    /// </summary>
    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets how wide the pill grows to when picked.
    /// </summary>
    public double ExpandedWidth
    {
        get => GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    /// <summary>
    /// Gets the turn applied to the icon. The instance is stable; its matrix is what changes.
    /// </summary>
    public ITransform IconTransform => _iconTransform;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _iconTurn.Stop();
    }

    private void OnIsSelectedChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            _iconTurn.Forward();
        }
        else
        {
            _iconTurn.Reverse();
        }
    }

    private void OnIconTurnChanged(double progress) =>
        _iconTransform.Matrix = Rotation3D.AroundY(IconTurnDegrees * progress);
}
