using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpendingTracker.Models;

/// <summary>
/// The window on to the data: which months are on screen, how tall the y axis runs and which point
/// is picked out. Port of <c>chart.dart</c>.
/// </summary>
/// <remarks>
/// The domain is measured in months and moves in fractions of one, so the chart slides smoothly
/// rather than a column at a time. Both ends clamp against each other, which is what stops a drag
/// from turning the window inside out and a zoom from shrinking it past a month and a fifth.
/// </remarks>
public sealed class Chart : INotifyPropertyChanged
{
    private const double MinimumDomain = 1.2d;

    private double _domainStart;
    private double _domainEnd;
    private double _rangeStart;
    private double _rangeEnd;
    private int _selectedDataPoint = -1;

    public Chart(params ChartDataSet[] dataSets)
    {
        DataSets = dataSets;
        MaxDomain = dataSets.Min(dataSet => (double)dataSet.Values.Count);

        _domainEnd = MaxDomain;
        _rangeStart = Min();
        _rangeEnd = Max();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the series being plotted.
    /// </summary>
    public IReadOnlyList<ChartDataSet> DataSets { get; }

    /// <summary>
    /// Gets how many months there are to look at.
    /// </summary>
    public double MaxDomain { get; }

    /// <summary>
    /// Gets or sets the first month on screen.
    /// </summary>
    public double DomainStart
    {
        get => _domainStart;
        set => Set(ref _domainStart, Math.Clamp(value, 0d, _domainEnd - MinimumDomain));
    }

    /// <summary>
    /// Gets or sets the last month on screen.
    /// </summary>
    public double DomainEnd
    {
        get => _domainEnd;
        set => Set(ref _domainEnd, Math.Clamp(value, _domainStart + MinimumDomain, MaxDomain));
    }

    /// <summary>
    /// Gets or sets the value the y axis starts at.
    /// </summary>
    public double RangeStart
    {
        get => _rangeStart;
        set
        {
            if (value < _rangeEnd)
            {
                Set(ref _rangeStart, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the value the y axis ends at.
    /// </summary>
    public double RangeEnd
    {
        get => _rangeEnd;
        set
        {
            if (value > _rangeStart)
            {
                Set(ref _rangeEnd, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the month picked out with a marker, or -1 for none.
    /// </summary>
    public int SelectedDataPoint
    {
        get => _selectedDataPoint;
        set
        {
            if (value >= 0 && value < MaxDomain)
            {
                Set(ref _selectedDataPoint, value);
            }
        }
    }

    /// <summary>
    /// Gets the first whole month on screen.
    /// </summary>
    public int RoundedDomainStart => Round(_domainStart);

    /// <summary>
    /// Gets the last whole month on screen.
    /// </summary>
    public int RoundedDomainEnd => Round(_domainEnd);

    /// <summary>
    /// Rounds the way Dart's <c>num.round</c> does, away from zero at a half. .NET rounds a half to
    /// the nearest even number instead, which would put the odd month on the wrong side.
    /// </summary>
    public static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Gets the smallest value on screen.
    /// </summary>
    public double Min()
    {
        var result = double.MaxValue;

        foreach (var dataSet in DataSets)
        {
            foreach (var value in dataSet.Slice(RoundedDomainStart, RoundedDomainEnd))
            {
                result = Math.Min(result, value);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the largest value on screen.
    /// </summary>
    public double Max()
    {
        var result = 0d;

        foreach (var dataSet in DataSets)
        {
            foreach (var value in dataSet.Slice(RoundedDomainStart, RoundedDomainEnd))
            {
                result = Math.Max(result, value);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets where the selected month sits across the domain, from 0 to 1.
    /// </summary>
    public double SelectedX() =>
        _selectedDataPoint == -1 ? 0d : (_selectedDataPoint - _domainStart) / (_domainEnd - _domainStart);

    /// <summary>
    /// Gets where a series' selected value sits up the range, from 0 to 1.
    /// </summary>
    public double SelectedY(int dataSetIndex) =>
        _selectedDataPoint == -1
            ? 0d
            : (DataSets[dataSetIndex].Values[_selectedDataPoint] - _rangeStart) / _rangeEnd;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
