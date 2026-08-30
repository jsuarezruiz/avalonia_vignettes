using System.Globalization;

namespace SpendingTracker;

/// <summary>
/// Writes out a figure the way the vignette shows it. Port of <c>globals.dart</c>.
/// </summary>
internal static class Money
{
    /// <summary>
    /// Groups a figure into thousands, as <c>numberToPriceString</c> does.
    /// </summary>
    public static string Format(int value) => value.ToString("N0", CultureInfo.InvariantCulture);
}
