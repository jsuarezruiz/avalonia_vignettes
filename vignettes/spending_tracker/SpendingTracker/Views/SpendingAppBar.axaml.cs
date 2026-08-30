using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpendingTracker.Views;

/// <summary>
/// The bar across the top. Port of <c>spending_app_bar.dart</c>.
/// </summary>
public partial class SpendingAppBar : UserControl
{
    public SpendingAppBar() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
