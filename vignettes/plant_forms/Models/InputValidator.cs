using System.Globalization;
using System.Text.RegularExpressions;

namespace PlantForms.Models;

/// <summary>
/// Checks a field's value. Port of <c>input_validator.dart</c>.
/// </summary>
public static partial class InputValidator
{
    /// <summary>
    /// Checks a text field of the given <paramref name="type"/>.
    /// </summary>
    public static bool Validate(InputType type, string value) => type switch
    {
        InputType.Email => EmailPattern().IsMatch(value),
        InputType.Telephone => PhonePattern().IsMatch(value),
        _ => true,
    };

    /// <summary>
    /// Checks one part of a card, whose rules depend on the network.
    /// </summary>
    public static bool Validate(CreditCardInputType type, string value, CreditCardNetwork network) => type switch
    {
        CreditCardInputType.Number => value.Replace(" ", string.Empty).Length == DigitsFor(network),
        CreditCardInputType.SecurityCode => value.Length == (network == CreditCardNetwork.Amex ? 4 : 3),
        CreditCardInputType.ExpirationDate => ValidateExpiry(value),
        _ => false,
    };

    /// <summary>
    /// Works out which network a card number belongs to, from its first digits.
    /// </summary>
    public static CreditCardNetwork NetworkFor(string value)
    {
        var digits = value.Replace(" ", string.Empty);

        if (digits.StartsWith('4'))
        {
            return CreditCardNetwork.Visa;
        }

        if (digits.Length < 2)
        {
            return CreditCardNetwork.Unknown;
        }

        return digits[..2] switch
        {
            "34" or "37" => CreditCardNetwork.Amex,
            "51" or "52" or "53" or "54" or "55" => CreditCardNetwork.Mastercard,
            _ => CreditCardNetwork.Unknown,
        };
    }

    /// <summary>
    /// How many digits a card of the given network carries.
    /// </summary>
    public static int DigitsFor(CreditCardNetwork network) => network == CreditCardNetwork.Amex ? 15 : 16;

    private static bool ValidateExpiry(string value)
    {
        var parts = value.Split('/');

        if (value.Length <= 3 || parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], CultureInfo.InvariantCulture, out var month) ||
            !int.TryParse(parts[1], CultureInfo.InvariantCulture, out var year))
        {
            return false;
        }

        return month <= 12 && year + 2000 >= DateTime.Now.Year;
    }

    [GeneratedRegex(@"(^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$)")]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"(^(1\s?)?(\(\d{3}\)|\d{3})[\s\-]?\d{3}[\s\-]?\d{4}$)")]
    private static partial Regex PhonePattern();
}
