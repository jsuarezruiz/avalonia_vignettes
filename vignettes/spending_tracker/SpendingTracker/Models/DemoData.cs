namespace SpendingTracker.Models;

/// <summary>
/// The two series the demo plots, in thousands. Port of <c>demo_data.dart</c>.
/// </summary>
public static class DemoData
{
    /// <summary>
    /// Gets the income series.
    /// </summary>
    public static ChartDataSet Income { get; } = new(
        7.0d, 7.2d, 7.3d, 7.0d, 6.5d, 6.8d, 7.1d, 6.8d, 7.0d, 7.0d, 7.0d,
        7.1d, 6.8d, 6.8d, 7.0d, 7.0d, 7.0d, 7.1d, 7.1d, 7.2d, 7.2d);

    /// <summary>
    /// Gets the expense series.
    /// </summary>
    public static ChartDataSet Expense { get; } = new(
        5.8d, 5.0d, 5.0d, 4.5d, 4.6d, 4.7d, 5.6d, 5.2d, 4.6d, 4.7d, 4.6d,
        4.7d, 4.6d, 4.5d, 4.5d, 5.2d, 5.1d, 5.0d, 5.0d, 4.6d, 4.7d);
}
