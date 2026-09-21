using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// How a <see cref="FlexPanel"/> distributes the space left over along its main axis.
/// Mirrors Flutter's <c>MainAxisAlignment</c>.
/// </summary>
public enum MainAxisAlignment
{
    /// <summary>
    /// Packs the children at the start of the axis.
    /// </summary>
    Start,

    /// <summary>
    /// Packs the children at the end of the axis.
    /// </summary>
    End,

    /// <summary>
    /// Packs the children in the middle of the axis.
    /// </summary>
    Center,

    /// <summary>
    /// Spreads the children out, with no space before the first or after the last.
    /// </summary>
    SpaceBetween,

    /// <summary>
    /// Spreads the children out, with half-size gaps before the first and after the last.
    /// </summary>
    SpaceAround,

    /// <summary>
    /// Spreads the children out, with equal gaps everywhere including the ends.
    /// </summary>
    SpaceEvenly,
}

/// <summary>
/// A stack panel that also offers Flutter's space-distribution modes, which
/// <see cref="StackPanel"/> has no equivalent for.
/// </summary>
public sealed class FlexPanel : Panel
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<FlexPanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<MainAxisAlignment> MainAxisAlignmentProperty =
        AvaloniaProperty.Register<FlexPanel, MainAxisAlignment>(nameof(MainAxisAlignment));

    /// <summary>
    /// Identifies the Flexible attached property, the equivalent of Flutter's <c>Flexible</c>: the
    /// child is offered whatever the others leave rather than as much room as it likes, so a scroll
    /// view put here starts scrolling instead of growing past the panel.
    /// </summary>
    public static readonly AttachedProperty<bool> FlexibleProperty =
        AvaloniaProperty.RegisterAttached<FlexPanel, Control, bool>("Flexible");

    static FlexPanel()
    {
        AffectsMeasure<FlexPanel>(OrientationProperty);
        AffectsArrange<FlexPanel>(MainAxisAlignmentProperty);
    }

    /// <summary>
    /// Gets or sets the axis children are laid out along.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets how leftover space along the main axis is distributed.
    /// </summary>
    public MainAxisAlignment MainAxisAlignment
    {
        get => GetValue(MainAxisAlignmentProperty);
        set => SetValue(MainAxisAlignmentProperty, value);
    }

    private bool IsVertical => Orientation == Orientation.Vertical;

    /// <summary>
    /// Gets whether <paramref name="control"/> takes what the other children leave.
    /// </summary>
    public static bool GetFlexible(Control control) => control.GetValue(FlexibleProperty);

    /// <summary>
    /// Sets whether <paramref name="control"/> takes what the other children leave.
    /// </summary>
    /// <remarks>
    /// Required by the XAML compiler for the <c>Flexible</c> attached property; do not remove
    /// even though no C# code calls it directly.
    /// </remarks>
    public static void SetFlexible(Control control, bool value) => control.SetValue(FlexibleProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        var main = 0d;
        var cross = 0d;

        // The children that ask for what they need go first, so what is left over is known by the
        // time the flexible ones are measured.
        foreach (var child in Children)
        {
            if (!GetFlexible(child))
            {
                Measure(child, double.PositiveInfinity);
            }
        }

        var remaining = Math.Max(0d, (IsVertical ? availableSize.Height : availableSize.Width) - main);

        foreach (var child in Children)
        {
            if (GetFlexible(child))
            {
                Measure(child, remaining);
            }
        }

        return IsVertical ? new Size(cross, main) : new Size(main, cross);

        void Measure(Control child, double extent)
        {
            child.Measure(IsVertical
                ? new Size(availableSize.Width, extent)
                : new Size(extent, availableSize.Height));

            main += IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
            cross = Math.Max(cross, IsVertical ? child.DesiredSize.Width : child.DesiredSize.Height);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var extent = IsVertical ? finalSize.Height : finalSize.Width;
        var used = 0d;

        foreach (var child in Children)
        {
            used += IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
        }

        var free = Math.Max(0d, extent - used);
        var count = Children.Count;
        var (leading, between) = MainAxisAlignment switch
        {
            MainAxisAlignment.End => (free, 0d),
            MainAxisAlignment.Center => (free / 2d, 0d),
            MainAxisAlignment.SpaceBetween when count > 1 => (0d, free / (count - 1)),
            MainAxisAlignment.SpaceAround when count > 0 => (free / count / 2d, free / count),
            MainAxisAlignment.SpaceEvenly when count > 0 => (free / (count + 1), free / (count + 1)),
            _ => (0d, 0d),
        };

        var position = leading;

        foreach (var child in Children)
        {
            var size = IsVertical ? child.DesiredSize.Height : child.DesiredSize.Width;

            child.Arrange(IsVertical
                ? new Rect(0d, position, finalSize.Width, size)
                : new Rect(position, 0d, size, finalSize.Height));

            position += size + between;
        }

        return finalSize;
    }
}
