using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using SpendingTracker.Models;

namespace SpendingTracker.Views;

/// <summary>
/// What the months on screen add up to, in and out. Port of
/// <c>spending_income_expenses_header.dart</c>.
/// </summary>
/// <remarks>
/// The figures are in thousands in the data and in whole pounds here, and both totals roll over
/// rather than snapping, so dragging the chart counts them up and down as months come and go.
/// </remarks>
public partial class IncomeExpenseHeader : UserControl
{
    public static readonly StyledProperty<Chart?> ChartProperty =
        AvaloniaProperty.Register<IncomeExpenseHeader, Chart?>(nameof(Chart));

    public static readonly DirectProperty<IncomeExpenseHeader, string> IncomeTotalProperty =
        AvaloniaProperty.RegisterDirect<IncomeExpenseHeader, string>(nameof(IncomeTotal), o => o.IncomeTotal);

    public static readonly DirectProperty<IncomeExpenseHeader, string> IncomeAverageProperty =
        AvaloniaProperty.RegisterDirect<IncomeExpenseHeader, string>(nameof(IncomeAverage), o => o.IncomeAverage);

    public static readonly DirectProperty<IncomeExpenseHeader, string> ExpenseTotalProperty =
        AvaloniaProperty.RegisterDirect<IncomeExpenseHeader, string>(nameof(ExpenseTotal), o => o.ExpenseTotal);

    public static readonly DirectProperty<IncomeExpenseHeader, string> ExpenseAverageProperty =
        AvaloniaProperty.RegisterDirect<IncomeExpenseHeader, string>(nameof(ExpenseAverage), o => o.ExpenseAverage);

    private Chart? _subscribed;
    private string _incomeTotal = string.Empty;
    private string _incomeAverage = string.Empty;
    private string _expenseTotal = string.Empty;
    private string _expenseAverage = string.Empty;

    public IncomeExpenseHeader()
    {
        InitializeComponent();

        DataContext = this;
    }

    /// <summary>
    /// Gets or sets the window on to the data being summed.
    /// </summary>
    public Chart? Chart
    {
        get => GetValue(ChartProperty);
        set => SetValue(ChartProperty, value);
    }

    /// <summary>
    /// Gets what came in over the months on screen.
    /// </summary>
    public string IncomeTotal
    {
        get => _incomeTotal;
        private set => SetAndRaise(IncomeTotalProperty, ref _incomeTotal, value);
    }

    /// <summary>
    /// Gets what came in a month, on average.
    /// </summary>
    public string IncomeAverage
    {
        get => _incomeAverage;
        private set => SetAndRaise(IncomeAverageProperty, ref _incomeAverage, value);
    }

    /// <summary>
    /// Gets what went out over the months on screen.
    /// </summary>
    public string ExpenseTotal
    {
        get => _expenseTotal;
        private set => SetAndRaise(ExpenseTotalProperty, ref _expenseTotal, value);
    }

    /// <summary>
    /// Gets what went out a month, on average.
    /// </summary>
    public string ExpenseAverage
    {
        get => _expenseAverage;
        private set => SetAndRaise(ExpenseAverageProperty, ref _expenseAverage, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != ChartProperty)
        {
            return;
        }

        if (_subscribed is { } previous)
        {
            previous.PropertyChanged -= OnChartChanged;
        }

        _subscribed = change.GetNewValue<Chart?>();

        if (_subscribed is { } chart)
        {
            chart.PropertyChanged += OnChartChanged;
        }

        Summarise();
    }

    private void OnChartChanged(object? sender, PropertyChangedEventArgs e) => Summarise();

    private void Summarise()
    {
        if (Chart is not { } chart)
        {
            return;
        }

        var start = chart.RoundedDomainStart;
        var end = chart.RoundedDomainEnd;

        (IncomeTotal, IncomeAverage) = Summarise(chart, 0, start, end);
        (ExpenseTotal, ExpenseAverage) = Summarise(chart, 1, start, end);
    }

    private static (string Total, string Average) Summarise(Chart chart, int index, int start, int end)
    {
        var values = chart.DataSets[index].Slice(start, end);
        var sum = 0d;

        foreach (var value in values)
        {
            sum += value;
        }

        sum *= 1000d;

        return ($"${Money.Format(Round(sum))}", $"Monthly Average: ${Money.Format(Round(sum / values.Length))}");
    }

    private static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
