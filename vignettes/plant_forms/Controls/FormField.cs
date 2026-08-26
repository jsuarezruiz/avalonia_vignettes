using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using PlantForms.Models;

namespace PlantForms.Controls;

/// <summary>
/// A text field with the form's decoration: a bold label above, the placeholder promoted to a
/// caption once something is typed, and the reason it is not yet valid in the corner. Port of
/// <c>text_input.dart</c>.
/// </summary>
[TemplatePart(PartInput, typeof(TextBox))]
[PseudoClasses(":invalid")]
public class FormField : TemplatedControl
{
    private const string PartInput = "PART_Input";

    public static readonly StyledProperty<string> FieldKeyProperty =
        AvaloniaProperty.Register<FormField, string>(nameof(FieldKey), string.Empty);

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<FormField, string>(nameof(Label), string.Empty);

    public static readonly StyledProperty<string> HelperProperty =
        AvaloniaProperty.Register<FormField, string>(nameof(Helper), string.Empty);

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<FormField, string>(nameof(Value), string.Empty, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<FormField, bool>(nameof(IsRequired));

    public static readonly StyledProperty<double> BoxHeightProperty =
        AvaloniaProperty.Register<FormField, double>(nameof(BoxHeight), BoxHeightFor(1));

    public static readonly StyledProperty<bool> IsMultilineProperty =
        AvaloniaProperty.Register<FormField, bool>(nameof(IsMultiline));

    public static readonly StyledProperty<object?> AccessoryProperty =
        AvaloniaProperty.Register<FormField, object?>(nameof(Accessory));

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<FormField>();

    public static readonly StyledProperty<InputType> InputTypeProperty =
        AvaloniaProperty.Register<FormField, InputType>(nameof(InputType));

    public static readonly DirectProperty<FormField, string> CaptionProperty =
        AvaloniaProperty.RegisterDirect<FormField, string>(nameof(Caption), o => o.Caption);

    public static readonly DirectProperty<FormField, string> ErrorTextProperty =
        AvaloniaProperty.RegisterDirect<FormField, string>(nameof(ErrorText), o => o.ErrorText);

    public static readonly DirectProperty<FormField, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<FormField, bool>(nameof(IsValid), o => o.IsValid);

    public static readonly RoutedEvent<RoutedEventArgs> ValidatedEvent =
        RoutedEvent.Register<FormField, RoutedEventArgs>(nameof(Validated), RoutingStrategies.Bubble);

    private string _caption = string.Empty;
    private string _errorText = string.Empty;
    private bool _isValid;
    private bool _hasBeenEdited;

    /// <summary>
    /// Raised whenever the field's validity is worked out again.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Validated
    {
        add => AddHandler(ValidatedEvent, value);
        remove => RemoveHandler(ValidatedEvent, value);
    }

    /// <summary>
    /// Gets or sets the name the field's value is stored under.
    /// </summary>
    public string FieldKey
    {
        get => GetValue(FieldKeyProperty);
        set => SetValue(FieldKeyProperty, value);
    }

    /// <summary>
    /// Gets or sets the bold heading above the field, if it has one.
    /// </summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder, which becomes the caption once the field is filled.
    /// </summary>
    public string Helper
    {
        get => GetValue(HelperProperty);
        set => SetValue(HelperProperty, value);
    }

    /// <summary>
    /// Gets or sets what has been typed.
    /// </summary>
    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the field has to be filled in.
    /// </summary>
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets how tall the box is.
    /// </summary>
    public double BoxHeight
    {
        get => GetValue(BoxHeightProperty);
        set => SetValue(BoxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the field takes more than one line.
    /// </summary>
    public bool IsMultiline
    {
        get => GetValue(IsMultilineProperty);
        set => SetValue(IsMultilineProperty, value);
    }

    /// <summary>
    /// Gets or sets what is drawn at the right hand end of the box.
    /// </summary>
    public object? Accessory
    {
        get => GetValue(AccessoryProperty);
        set => SetValue(AccessoryProperty, value);
    }

    /// <summary>
    /// Gets or sets where the text sits in the box.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets what the field holds, which decides how it is checked.
    /// </summary>
    public InputType InputType
    {
        get => GetValue(InputTypeProperty);
        set => SetValue(InputTypeProperty, value);
    }

    /// <summary>
    /// Gets the small caption in the field's top left corner.
    /// </summary>
    public string Caption
    {
        get => _caption;
        private set => SetAndRaise(CaptionProperty, ref _caption, value);
    }

    /// <summary>
    /// Gets what is wrong with the field, printed in its top right corner.
    /// </summary>
    public string ErrorText
    {
        get => _errorText;
        private set => SetAndRaise(ErrorTextProperty, ref _errorText, value);
    }

    /// <summary>
    /// Gets whether the field is filled in acceptably.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        private set => SetAndRaise(IsValidProperty, ref _isValid, value);
    }

    /// <summary>
    /// The height Material gives an outlined field of <paramref name="lines"/> lines: its content
    /// padding is 24 above and 16 below, and a line of the form's 16 point type is 19.2 high.
    /// </summary>
    public static double BoxHeightFor(int lines) => 24d + (lines * 19.2d) + 16d;

    /// <summary>
    /// Checks the field and reports the result, whatever the user has done so far.
    /// </summary>
    public void Validate() => Validate(force: true);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Validate(force: Value.Length > 0);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            _hasBeenEdited = true;

            Validate(force: false);
        }
        else if (change.Property == IsRequiredProperty || change.Property == HelperProperty)
        {
            UpdateCaption();
        }
    }

    /// <summary>
    /// Checks the value the way the original's validator does.
    /// </summary>
    /// <param name="isValid">Whether the value passes.</param>
    /// <param name="error">What to print in the corner when it does not.</param>
    protected virtual void Check(out bool isValid, out string error)
    {
        var value = Value;

        if (IsRequired && value.Length == 0)
        {
            isValid = false;
            error = "Required";
        }
        else if (value.Length == 0 || InputValidator.Validate(InputType, value))
        {
            isValid = true;
            error = string.Empty;
        }
        else
        {
            isValid = false;
            error = "Not Valid";
        }
    }

    /// <summary>
    /// Works out the small caption in the top left corner.
    /// </summary>
    protected virtual string BuildCaption()
    {
        if (Value.Length > 0 && Label.Length == 0)
        {
            return Helper;
        }

        return !IsRequired && Value.Length == 0 ? "Optional" : string.Empty;
    }

    private void Validate(bool force)
    {
        Check(out var isValid, out var error);

        IsValid = isValid;

        // Material only shows what is wrong once the user has touched the field.
        ErrorText = _hasBeenEdited || force ? error : string.Empty;

        PseudoClasses.Set(":invalid", ErrorText.Length > 0);

        UpdateCaption();
        RaiseEvent(new RoutedEventArgs(ValidatedEvent));
    }

    private void UpdateCaption() => Caption = BuildCaption().ToUpperInvariant();
}
