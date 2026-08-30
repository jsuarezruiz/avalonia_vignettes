using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PlantForms.Views;

/// <summary>
/// Hosts the vignette at the size of the phone it was designed for.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
