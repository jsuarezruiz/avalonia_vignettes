using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using SpendingTracker.Models;

namespace SpendingTracker.Views;

/// <summary>
/// The months the chart is showing, written out. Port of <c>spending_date_range.dart</c>.
/// </summary>
/// <remarks>
/// The start rounds up and the end stops a tenth of the window short, so a month only counts as on
/// show once most of it is.
/// </remarks>
public partial class DateRange : UserControl
{
    /// <summary>
    /// Defines the <see cref="Chart"/> property.
    /// </summary>
    public static readonly StyledProperty<Chart?> ChartProperty =
        AvaloniaProperty.Register<DateRange, Chart?>(nameof(Chart));

    /// <summary>
    /// Defines the <see cref="From"/> property.
    /// </summary>
    public static readonly DirectProperty<DateRange, string> FromProperty =
        AvaloniaProperty.RegisterDirect<DateRange, string>(nameof(From), o => o.From);

    /// <summary>
    /// Defines the <see cref="To"/> property.
    /// </summary>
    public static readonly DirectProperty<DateRange, string> ToProperty =
        AvaloniaProperty.RegisterDirect<DateRange, string>(nameof(To), o => o.To);

    /// <summary>
    /// The year the data starts in.
    /// </summary>
    private const int StartYear = 2018;

    private static readonly string[] MonthNames =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    ];

    private Chart? _subscribed;
    private string _from = string.Empty;
    private string _to = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateRange"/> class.
    /// </summary>
    public DateRange()
    {
        InitializeComponent();

        DataContext = this;
    }

    /// <summary>
    /// Gets or sets the window on to the data being described.
    /// </summary>
    public Chart? Chart
    {
        get => GetValue(ChartProperty);
        set => SetValue(ChartProperty, value);
    }

    /// <summary>
    /// Gets the first month on show.
    /// </summary>
    public string From
    {
        get => _from;
        private set => SetAndRaise(FromProperty, ref _from, value);
    }

    /// <summary>
    /// Gets the last month on show.
    /// </summary>
    public string To
    {
        get => _to;
        private set => SetAndRaise(ToProperty, ref _to, value);
    }

    /// <inheritdoc />
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

        Describe();
    }

    private static string Describe(int month) => $"{StartYear + (month / 12)} {MonthNames[month % 12]}";

    private void OnChartChanged(object? sender, PropertyChangedEventArgs e) => Describe();

    private void Describe()
    {
        if (Chart is not { } chart)
        {
            return;
        }

        var span = chart.DomainEnd - chart.DomainStart;

        From = Describe((int)Math.Ceiling(chart.DomainStart));
        To = Describe((int)Math.Floor(chart.DomainEnd - (0.1d * span)));
    }
}
