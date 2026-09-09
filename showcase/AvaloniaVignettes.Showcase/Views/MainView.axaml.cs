using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using AvaloniaVignettes.Showcase.Models;

namespace AvaloniaVignettes.Showcase.Views;

public partial class MainView : UserControl
{
    private readonly List<IResourceProvider> _sampleResources = [];
    private readonly List<IStyle> _sampleStyles = [];

    public MainView()
    {
        InitializeComponent();
        ResolveNamedControls();
        DataContext = this;

        SampleList.SelectedIndex = 0;
    }

    public IReadOnlyList<VignetteSample> Samples => VignetteCatalog.All;

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void ResolveNamedControls()
    {
        SampleList = this.FindControl<ListBox>(nameof(SampleList))!;
        ViewCodeButton = this.FindControl<HyperlinkButton>(nameof(ViewCodeButton))!;
        SelectedCategory = this.FindControl<TextBlock>(nameof(SelectedCategory))!;
        SelectedTitle = this.FindControl<TextBlock>(nameof(SelectedTitle))!;
        SelectedDescription = this.FindControl<TextBlock>(nameof(SelectedDescription))!;
        SampleTheme = this.FindControl<ThemeVariantScope>(nameof(SampleTheme))!;
        SampleHost = this.FindControl<ContentControl>(nameof(SampleHost))!;
        SelectedInstruction = this.FindControl<TextBlock>(nameof(SelectedInstruction))!;
    }

    private void OnSampleSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SampleList.SelectedItem is VignetteSample sample)
        {
            Show(sample);
        }
    }

    private void Show(VignetteSample sample)
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("The showcase requires a running Avalonia application.");

        // Detach the old view first so animation tickers and visual subscriptions can stop.
        SampleHost.Content = null;

        foreach (var style in _sampleStyles)
        {
            SampleHost.Styles.Remove(style);
        }

        foreach (var resources in _sampleResources)
        {
            application.Resources.MergedDictionaries.Remove(resources);
        }

        _sampleStyles.Clear();
        _sampleResources.Clear();

        foreach (var uri in sample.ResourceUris)
        {
            var resources = new ResourceInclude(uri) { Source = uri };
            application.Resources.MergedDictionaries.Add(resources);
            _sampleResources.Add(resources);
        }

        foreach (var uri in sample.StyleUris)
        {
            var style = new StyleInclude(uri) { Source = uri };
            SampleHost.Styles.Add(style);
            _sampleStyles.Add(style);
        }

        SampleTheme.RequestedThemeVariant = sample.Theme;
        SampleHost.Content = sample.CreateView();

        ViewCodeButton.NavigateUri = sample.CodeUri;
        SelectedCategory.Text = $"{sample.Number}  /  {sample.Category}";
        SelectedTitle.Text = sample.Title;
        SelectedDescription.Text = sample.Description;
        SelectedInstruction.Text = sample.Instruction;
    }
}
