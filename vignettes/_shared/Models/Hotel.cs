using System.Globalization;

namespace AvaloniaVignettes.Shared.Models;

/// <summary>
/// A hotel recommendation shared by the two travel-card vignettes.
/// </summary>
/// <param name="Name">The hotel name.</param>
/// <param name="Rating">The star rating, from 0 to 5.</param>
/// <param name="Reviews">The number of reviews.</param>
/// <param name="Price">The nightly price in dollars.</param>
public sealed record Hotel(string Name, double Rating, int Reviews, int Price)
{
    /// <summary>
    /// Gets the rating with one decimal place, matching Dart's presentation of a double.
    /// </summary>
    public string RatingLabel => Rating.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the review count in the parentheses the designs call for.
    /// </summary>
    public string ReviewsLabel => $"({Reviews})";

    /// <summary>
    /// Gets the price with its currency prefix.
    /// </summary>
    public string PriceLabel => $"${Price}";

    /// <summary>
    /// Gets one entry per whole star, clamped to the documented rating range.
    /// </summary>
    public IReadOnlyList<int> Stars { get; } =
        Enumerable.Range(0, Math.Clamp((int)Rating, 0, 5)).ToArray();
}
