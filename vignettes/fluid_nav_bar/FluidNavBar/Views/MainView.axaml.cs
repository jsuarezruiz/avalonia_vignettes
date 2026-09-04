using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluidNavBar.Views;

/// <summary>
/// The three pages and the bar that picks between them. Port of <c>demo.dart</c>.
/// </summary>
public partial class MainView : UserControl
{
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

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
