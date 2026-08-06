namespace PlantForms.Models;

/// <summary>
/// What a text field holds, which decides how it is validated. Port of <c>InputType</c>.
/// </summary>
public enum InputType
{
    /// <summary>
    /// Anything.
    /// </summary>
    Text,

    /// <summary>
    /// An email address.
    /// </summary>
    Email,

    /// <summary>
    /// A number.
    /// </summary>
    Number,

    /// <summary>
    /// A phone number.
    /// </summary>
    Telephone,
}

/// <summary>
/// Which part of a card a field holds. Port of <c>CreditCardInputType</c>.
/// </summary>
public enum CreditCardInputType
{
    /// <summary>
    /// The card number.
    /// </summary>
    Number,

    /// <summary>
    /// The expiry date.
    /// </summary>
    ExpirationDate,

    /// <summary>
    /// The security code.
    /// </summary>
    SecurityCode,
}

/// <summary>
/// The card networks the form recognises. Port of <c>CreditCardNetwork</c>.
/// </summary>
public enum CreditCardNetwork
{
    /// <summary>
    /// Not recognised.
    /// </summary>
    Unknown,

    /// <summary>
    /// Visa.
    /// </summary>
    Visa,

    /// <summary>
    /// Mastercard.
    /// </summary>
    Mastercard,

    /// <summary>
    /// American Express.
    /// </summary>
    Amex,
}
