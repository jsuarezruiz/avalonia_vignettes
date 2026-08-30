using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlantForms.Controls;

namespace PlantForms.Views;

/// <summary>
/// The first page: what is being bought. Port of <c>plant_form_summary.dart</c>.
/// </summary>
public partial class SummaryPage : FormPage
{
    public SummaryPage() => InitializeComponent();

    /// <summary>
    /// Control themes resolve by exact type, so the page borrows its base's.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(FormPage);

    private void OnNextClick(object? sender, RoutedEventArgs e) => RequestNext();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
