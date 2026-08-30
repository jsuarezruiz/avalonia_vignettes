using System.Text;

namespace PlantForms.Models;

/// <summary>
/// Formats digits into a mask, where <c>0</c> is a digit and anything else is punctuation the mask
/// supplies. Stands in for the original's <c>MaskedTextController</c>.
/// </summary>
public static class InputMask
{
    /// <summary>
    /// The mask a card field carries until enough has been typed to choose another.
    /// </summary>
    public const string Initial = "00";

    /// <summary>
    /// Gets the mask for one part of a card of the given network.
    /// </summary>
    public static string For(CreditCardInputType type, CreditCardNetwork network) => type switch
    {
        CreditCardInputType.Number when network == CreditCardNetwork.Amex => "0000 000000 00000",
        CreditCardInputType.Number when network == CreditCardNetwork.Unknown => Initial,
        CreditCardInputType.Number => "0000 0000 0000 0000",
        CreditCardInputType.ExpirationDate => "00/00",
        CreditCardInputType.SecurityCode when network == CreditCardNetwork.Amex => "0000",
        _ => "000",
    };

    /// <summary>
    /// Lays the digits of <paramref name="value"/> into <paramref name="mask"/>.
    /// </summary>
    public static string Apply(string mask, string value)
    {
        var digits = value.Where(char.IsDigit).ToArray();
        var result = new StringBuilder(mask.Length);
        var next = 0;

        foreach (var slot in mask)
        {
            if (next >= digits.Length)
            {
                break;
            }

            if (slot == '0')
            {
                result.Append(digits[next++]);
            }
            else
            {
                result.Append(slot);
            }
        }

        return result.ToString();
    }
}
