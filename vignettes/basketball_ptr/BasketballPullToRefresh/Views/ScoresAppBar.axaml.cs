using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BasketballPullToRefresh.Views;

/// <summary>
/// The gradient bar across the top of the scores. Port of <c>scores_app_bar.dart</c>.
/// </summary>
public partial class ScoresAppBar : UserControl
{
    public ScoresAppBar() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
