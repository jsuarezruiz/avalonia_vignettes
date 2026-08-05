using Avalonia.Controls;

namespace BubbleTabBar.Views;

/// <summary>
/// The vignette's single screen: a bottom tab bar with a page behind each tab. Port of
/// <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// The pages themselves are filler, so rather than five near-identical view files they are declared
/// inline against two counted item sources, nine for the lists, twenty for the grids, which is
/// what the original's <c>itemCount</c> and <c>List.generate</c> come to.
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Gets the nine rows the list pages show.
    /// </summary>
    public IReadOnlyList<int> CardItems { get; } = [.. Enumerable.Range(0, 9)];

    /// <summary>
    /// Gets the twenty cells the grid pages show.
    /// </summary>
    public IReadOnlyList<int> GridItems { get; } = [.. Enumerable.Range(0, 20)];
}
