using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Controls;
using PlantForms.Models;

namespace PlantForms.Controls;

/// <summary>
/// A card field, which punctuates what is typed into it and works out the network from the first
/// digits. Port of <c>credit_card_input.dart</c>.
/// </summary>
public sealed class CreditCardField : FormField
{
    public static readonly StyledProperty<CreditCardInputType> CardInputTypeProperty =
        AvaloniaProperty.Register<CreditCardField, CreditCardInputType>(nameof(CardInputType));

    public static readonly StyledProperty<CreditCardNetwork> NetworkProperty =
        AvaloniaProperty.Register<CreditCardField, CreditCardNetwork>(
            nameof(Network),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly DirectProperty<CreditCardField, string> NetworkLabelProperty =
        AvaloniaProperty.RegisterDirect<CreditCardField, string>(nameof(NetworkLabel), o => o.NetworkLabel);

    private string _networkLabel = string.Empty;
    private bool _isFormatting;

    /// <summary>
    /// Gets or sets which part of the card this field holds.
    /// </summary>
    public CreditCardInputType CardInputType
    {
        get => GetValue(CardInputTypeProperty);
        set => SetValue(CardInputTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the network. The number field works it out; the security code field is told it,
    /// because American Express asks for a digit more than the others.
    /// </summary>
    public CreditCardNetwork Network
    {
        get => GetValue(NetworkProperty);
        set => SetValue(NetworkProperty, value);
    }

    /// <summary>
    /// Gets the network's name, shown beside the number, or empty while it is unknown.
    /// </summary>
    public string NetworkLabel
    {
        get => _networkLabel;
        private set => SetAndRaise(NetworkLabelProperty, ref _networkLabel, value);
    }

    protected override Type StyleKeyOverride => typeof(FormField);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ValueProperty && !_isFormatting)
        {
            Format();
        }

        base.OnPropertyChanged(change);

        if (change.Property == NetworkProperty || change.Property == CardInputTypeProperty)
        {
            NetworkLabel = Network switch
            {
                CreditCardNetwork.Visa => "VISA",
                CreditCardNetwork.Mastercard => "MC",
                CreditCardNetwork.Amex => "AMEX",
                _ => string.Empty,
            };

            UpdateAccessory();
        }
    }

    protected override void Check(out bool isValid, out string error)
    {
        if (Value.Length == 0)
        {
            isValid = false;
            error = "Required";
        }
        else if (InputValidator.Validate(CardInputType, Value, Network))
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

    protected override string BuildCaption() =>
        Value.Length > 0 && Label.Length == 0 ? Helper : string.Empty;

    // The number field carries a card mark: the network's name once it is known, and Material's
    // card outline until then, which is the icon the original falls back to.
    private void UpdateAccessory()
    {
        if (CardInputType != CreditCardInputType.Number)
        {
            return;
        }

        Accessory = NetworkLabel.Length > 0
            ? new TextBlock { Text = NetworkLabel, Classes = { "cardNetwork" } }
            : new MaterialIcon
            {
                Data = this.FindResource("CreditCardIcon") as Geometry,
                IconSize = 28d,
                Foreground = this.FindResource("DarkGrayBrush") as IBrush,
            };
    }

    private void Format()
    {
        // The number field cannot know its own mask until two digits have named the network.
        if (CardInputType == CreditCardInputType.Number)
        {
            Network = InputValidator.NetworkFor(Value);
        }

        var formatted = InputMask.Apply(InputMask.For(CardInputType, Network), Value);

        if (formatted == Value)
        {
            return;
        }

        _isFormatting = true;
        Value = formatted;
        _isFormatting = false;
    }
}
