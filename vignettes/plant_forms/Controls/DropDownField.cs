using Avalonia;
using Avalonia.Interactivity;

namespace PlantForms.Controls;

/// <summary>
/// A field that picks from a list, which it opens as a page of its own. Port of
/// <c>dropdown_menu.dart</c>.
/// </summary>
public sealed class DropDownField : FormField
{
    public static readonly StyledProperty<IReadOnlyList<string>> OptionsProperty =
        AvaloniaProperty.Register<DropDownField, IReadOnlyList<string>>(nameof(Options), []);

    public static readonly RoutedEvent<RoutedEventArgs> OpenRequestedEvent =
        RoutedEvent.Register<DropDownField, RoutedEventArgs>(nameof(OpenRequested), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the field is tapped and its options should be shown.
    /// </summary>
    public event EventHandler<RoutedEventArgs> OpenRequested
    {
        add => AddHandler(OpenRequestedEvent, value);
        remove => RemoveHandler(OpenRequestedEvent, value);
    }

    /// <summary>
    /// Gets or sets what can be picked.
    /// </summary>
    public IReadOnlyList<string> Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Tapped += (_, _) => RaiseEvent(new RoutedEventArgs(OpenRequestedEvent));
    }

    /// <summary>
    /// A choice counts as made as soon as there is one, as the original's does.
    /// </summary>
    protected override void Check(out bool isValid, out string error)
    {
        isValid = Value.Length > 0;
        error = string.Empty;
    }

    /// <summary>
    /// The label is the hint, which the chosen value replaces, so there is no caption.
    /// </summary>
    protected override string BuildCaption() => string.Empty;
}
