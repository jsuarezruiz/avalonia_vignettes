using Avalonia;
using Avalonia.Controls;

namespace BubbleTabBar.Controls;

/// <summary>
/// Lays children out in a fixed number of columns of square cells, the way Flutter's
/// <c>GridView.count</c> does.
/// </summary>
/// <remarks>
/// Avalonia's <see cref="UniformGrid"/> divides the available height between its rows as well as
/// the width between its columns, which cannot work inside a scroll viewer where the height is
/// unbounded. Here the cell size comes from the width alone and the height follows from how many
/// rows that leaves.
/// </remarks>
public sealed class SquareGridPanel : Panel
{
    /// <summary>
    /// Defines the <see cref="Columns"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<SquareGridPanel, int>(nameof(Columns), 2);

    static SquareGridPanel() => AffectsMeasure<SquareGridPanel>(ColumnsProperty);

    /// <summary>
    /// Gets or sets how many cells sit side by side.
    /// </summary>
    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = Math.Max(1, Columns);
        var cell = double.IsInfinity(availableSize.Width) ? 0d : availableSize.Width / columns;

        foreach (var child in Children)
        {
            child.Measure(new Size(cell, cell));
        }

        var rows = (Children.Count + columns - 1) / columns;

        return new Size(cell * columns, cell * rows);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = Math.Max(1, Columns);
        var cell = finalSize.Width / columns;

        for (var i = 0; i < Children.Count; i++)
        {
            Children[i].Arrange(new Rect(i % columns * cell, i / columns * cell, cell, cell));
        }

        return finalSize;
    }
}
