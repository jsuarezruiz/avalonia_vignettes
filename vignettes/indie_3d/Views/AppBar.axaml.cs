using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Indie3D.Views;

/// <summary>
/// The menu and the label across the top. Port of <c>indie_app_bar.dart</c>.
/// </summary>
public partial class AppBar : UserControl
{
    public AppBar() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
