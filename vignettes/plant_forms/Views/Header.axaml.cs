using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PlantForms.Views;

/// <summary>
/// The scene behind the form cards. Port of <c>header.dart</c>.
/// </summary>
public partial class Header : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Header"/> class.
    /// </summary>
    public Header() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
