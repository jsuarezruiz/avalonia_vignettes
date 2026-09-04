using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PlantForms.Controls;

namespace PlantForms.Views;

/// <summary>
/// The page a dropdown opens to pick from, with the choice taking effect only once it is done with.
/// Port of <c>DropdownOptions</c>.
/// </summary>
public partial class OptionsOverlay : UserControl
{
    public static readonly DirectProperty<OptionsOverlay, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<OptionsOverlay, string>(nameof(Title), o => o.Title);

    public static readonly DirectProperty<OptionsOverlay, string> SelectedProperty =
        AvaloniaProperty.RegisterDirect<OptionsOverlay, string>(nameof(Selected), o => o.Selected, (o, value) => o.Selected = value);

    private string _title = string.Empty;
    private string _selected = string.Empty;
    private DropDownField? _field;

    public OptionsOverlay()
    {
        InitializeComponent();

        DataContext = this;
    }

    /// <summary>
    /// Gets what is being picked.
    /// </summary>
    public string Title
    {
        get => _title;
        private set => SetAndRaise(TitleProperty, ref _title, value);
    }

    /// <summary>
    /// Gets or sets the option currently ticked.
    /// </summary>
    public string Selected
    {
        get => _selected;
        set => SetAndRaise(SelectedProperty, ref _selected, value);
    }

    /// <summary>
    /// Opens the list for <paramref name="field"/>.
    /// </summary>
    public void Show(DropDownField field)
    {
        _field = field;

        Title = field.Label;
        Selected = field.Value;
        Options.ItemsSource = field.Options;
        IsVisible = true;
    }

    private void OnDoneClick(object? sender, RoutedEventArgs e)
    {
        if (_field is not null && Selected.Length > 0)
        {
            _field.Value = Selected;
        }

        IsVisible = false;
    }
}
