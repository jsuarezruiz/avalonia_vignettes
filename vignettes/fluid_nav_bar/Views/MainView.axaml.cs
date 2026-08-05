using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluidNavBar.Views;

/// <summary>
/// The three pages and the bar that picks between them. Port of <c>demo.dart</c>.
/// </summary>
public partial class MainView : UserControl
{
    /// <summary>
    /// Defines the <see cref="TileSize"/> property.
    /// </summary>
    public static readonly DirectProperty<MainView, double> TileSizeProperty =
        AvaloniaProperty.RegisterDirect<MainView, double>(nameof(TileSize), o => o.TileSize);

    /// <summary>
    /// The grid page is inset by 8 on each side.
    /// </summary>
    private const double GridPadding = 8d;

    /// <summary>
    /// Portrait shows two columns; the original counts three when it is wider than tall.
    /// </summary>
    private const int GridColumns = 2;

    private double _tileSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Gets one entry per row on the two list pages, as the original's 20.
    /// </summary>
    public IReadOnlyList<int> Rows { get; } = [.. Enumerable.Range(0, 20)];

    /// <summary>
    /// Gets one entry per tile on the grid page, as the original's 30.
    /// </summary>
    public IReadOnlyList<int> Tiles { get; } = [.. Enumerable.Range(0, 30)];

    /// <summary>
    /// Gets the side of one grid tile. A Flutter <c>GridView.count</c> lays its cells out square
    /// unless told otherwise, and a <see cref="UniformGrid"/> instead takes its row height from the
    /// tallest cell, so the side is worked out here and given to each tile.
    /// </summary>
    public double TileSize
    {
        get => _tileSize;
        private set => SetAndRaise(TileSizeProperty, ref _tileSize, value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        TileSize = Math.Max(0d, (finalSize.Width - (GridPadding * 2d)) / GridColumns);

        return base.ArrangeOverride(finalSize);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
