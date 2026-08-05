using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GooeyEdge.Views;

/// <summary>
/// The vignette's screen. Port of <c>demo.dart</c>'s <c>GooeyEdgeDemo</c>.
/// </summary>
public partial class MainView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
