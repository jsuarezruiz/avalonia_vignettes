using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TicketFold.Views;

/// <summary>
/// Hosts the vignette at the size of the phone it was designed for.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
