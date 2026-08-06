using Avalonia;
using Avalonia.Controls;

namespace ParallaxTravelCardsList.Controls;

/// <summary>
/// Lays the three artwork layers of a travel card out so they slide past each other as the card is
/// dragged. Port of <c>travel_card_renderer.dart</c>'s <c>_buildCityImageStack</c>.
/// </summary>
/// <remarks>
/// Every layer is shifted by the same global parallax of <c>offset * maxParallax * 2</c> and then
/// pulled back by <c>offset * maxParallax * factor</c>. The rear layer has the smallest factor, so
/// it is pulled back least and ends up travelling furthest, which is what separates the layers.
/// </remarks>
public sealed class ParallaxScenery : Panel
{
    /// <summary>
    /// Defines the WidthFactor attached property.
    /// </summary>
    public static readonly AttachedProperty<double> WidthFactorProperty =
        AvaloniaProperty.RegisterAttached<ParallaxScenery, Control, double>("WidthFactor", 1d);

    /// <summary>
    /// Defines the MaxOffsetFactor attached property.
    /// </summary>
    public static readonly AttachedProperty<double> MaxOffsetFactorProperty =
        AvaloniaProperty.RegisterAttached<ParallaxScenery, Control, double>("MaxOffsetFactor");

    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<ParallaxScenery, double>(nameof(Offset));

    /// <summary>
    /// Defines the <see cref="CardWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CardWidthProperty =
        AvaloniaProperty.Register<ParallaxScenery, double>(nameof(CardWidth));

    /// <summary>
    /// Defines the <see cref="CardHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CardHeightProperty =
        AvaloniaProperty.Register<ParallaxScenery, double>(nameof(CardHeight));

    /// <summary>
    /// Defines the <see cref="MaxParallax"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxParallaxProperty =
        AvaloniaProperty.Register<ParallaxScenery, double>(nameof(MaxParallax), 30d);

    /// <summary>
    /// Defines the <see cref="BaselineFactor"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BaselineFactorProperty =
        AvaloniaProperty.Register<ParallaxScenery, double>(nameof(BaselineFactor), 0.45d);

    // The Flutter original uses two different paddings: 28 to size the container, but 24 to work
    // out where a layer's centre sits. Both are kept so the artwork lands on the same pixel.
    private const double ContainerPadding = 28d;
    private const double CenteringPadding = 24d;

    static ParallaxScenery()
    {
        AffectsMeasure<ParallaxScenery>(CardWidthProperty, CardHeightProperty);
        AffectsArrange<ParallaxScenery>(OffsetProperty, MaxParallaxProperty, BaselineFactorProperty);

        // Flutter's inner Stack clips, which is what trims the tallest artwork at the top.
        ClipToBoundsProperty.OverrideDefaultValue<ParallaxScenery>(true);
    }

    /// <summary>
    /// Gets or sets the normalised drag offset, from -1 to 1.
    /// </summary>
    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the card this scene belongs to.
    /// </summary>
    public double CardWidth
    {
        get => GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the card this scene belongs to.
    /// </summary>
    public double CardHeight
    {
        get => GetValue(CardHeightProperty);
        set => SetValue(CardHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the travel, in pixels, of the frontmost layer at full offset.
    /// </summary>
    public double MaxParallax
    {
        get => GetValue(MaxParallaxProperty);
        set => SetValue(MaxParallaxProperty, value);
    }

    /// <summary>
    /// Gets or sets where the layers rest, as a fraction of <see cref="CardHeight"/> measured up
    /// from the bottom of the scene.
    /// </summary>
    public double BaselineFactor
    {
        get => GetValue(BaselineFactorProperty);
        set => SetValue(BaselineFactorProperty, value);
    }

    /// <summary>
    /// Gets the width of a layer as a fraction of the scene width.
    /// </summary>
    public static double GetWidthFactor(Control control) => control.GetValue(WidthFactorProperty);

    /// <summary>
    /// Sets the width of a layer as a fraction of the scene width.
    /// </summary>
    public static void SetWidthFactor(Control control, double value) => control.SetValue(WidthFactorProperty, value);

    /// <summary>
    /// Gets how far a layer is pulled back, as a fraction of <see cref="MaxParallax"/>.
    /// </summary>
    public static double GetMaxOffsetFactor(Control control) => control.GetValue(MaxOffsetFactorProperty);

    /// <summary>
    /// Sets how far a layer is pulled back, as a fraction of <see cref="MaxParallax"/>.
    /// </summary>
    public static void SetMaxOffsetFactor(Control control, double value) =>
        control.SetValue(MaxOffsetFactorProperty, value);

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = new Size(Math.Max(0d, CardWidth - ContainerPadding), Math.Max(0d, CardHeight));

        foreach (var child in Children)
        {
            // Each layer keeps its aspect ratio, so only the width is constrained.
            child.Measure(new Size(size.Width * GetWidthFactor(child), double.PositiveInfinity));
        }

        return size;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var offset = Offset;
        var maxParallax = MaxParallax;
        var globalOffset = offset * maxParallax * 2d;
        var centeringWidth = CardWidth - CenteringPadding;
        var baseline = finalSize.Height - (CardHeight * BaselineFactor);

        foreach (var child in Children)
        {
            var width = finalSize.Width * GetWidthFactor(child);
            var height = child.DesiredSize.Height;
            var maxOffset = maxParallax * GetMaxOffsetFactor(child);
            var x = (centeringWidth / 2d) - (width / 2d) - (offset * maxOffset) + globalOffset;

            child.Arrange(new Rect(x, baseline - height, width, height));
        }

        return finalSize;
    }
}
