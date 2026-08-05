using System.Globalization;

namespace ParallaxTravelCardsHero.Models;

/// <summary>
/// A hotel recommendation shown under the city card. Port of <c>demo_data.dart</c>'s
/// <c>HotelData</c>.
/// </summary>
/// <param name="Name">The hotel name.</param>
/// <param name="Rating">The star rating, from 0 to 5.</param>
/// <param name="Reviews">The number of reviews.</param>
/// <param name="Price">The nightly price in dollars.</param>
public sealed record Hotel(string Name, double Rating, int Reviews, int Price)
{
    /// <summary>
    /// Gets the rating formatted the way Dart prints a double, for example <c>5.0</c>.
    /// </summary>
    public string RatingLabel => Rating.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the review count in the parentheses the design calls for.
    /// </summary>
    public string ReviewsLabel => $"({Reviews})";

    /// <summary>
    /// Gets the price with its currency prefix.
    /// </summary>
    public string PriceLabel => $"${Price}";

    /// <summary>
    /// Gets one entry per whole star, so the rating can be data-bound to a row of icons.
    /// </summary>
    public IReadOnlyList<int> Stars { get; } = Enumerable.Range(0, (int)Math.Round(Rating)).ToArray();
}
