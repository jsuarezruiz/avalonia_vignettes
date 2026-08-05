using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BasketballPullToRefresh.Views;

/// <summary>
/// The gradient bar across the top of the scores. Port of <c>scores_app_bar.dart</c>.
/// </summary>
public partial class ScoresAppBar : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScoresAppBar"/> class.
    /// </summary>
    public ScoresAppBar() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
