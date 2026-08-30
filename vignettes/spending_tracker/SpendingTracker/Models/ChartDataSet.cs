namespace SpendingTracker.Models;

/// <summary>
/// One series of the chart, a value per month. Port of <c>chart_data_set.dart</c>.
/// </summary>
/// <param name="values">The series' values.</param>
public sealed class ChartDataSet(params double[] values)
{
    /// <summary>
    /// Gets the series' values.
    /// </summary>
    public IReadOnlyList<double> Values => values;

    /// <summary>
    /// Gets the values between two indices, the equivalent of Dart's <c>sublist</c>.
    /// </summary>
    public ReadOnlySpan<double> Slice(int start, int end) => values.AsSpan(start, end - start);
}
