using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using SpendingTracker.Models;

namespace SpendingTracker.Views;

/// <summary>
/// Three made up categories the outgoings are split across. Port of
/// <c>spending_category_list.dart</c>.
/// </summary>
/// <remarks>
/// The shares are random and add up to one, and a fresh set is drawn whenever the window moves on to
/// a different month, which is what the rings count between. The original redraws them on every
/// notification the chart sends rather than only on the ones that change the month, because rebuilds
/// in Flutter reach the whole subtree; there is nothing to rebuild here, so this follows what the
/// month check was written to do.
/// </remarks>
public partial class CategoryList : UserControl
{
    public static readonly StyledProperty<Chart?> ChartProperty =
        AvaloniaProperty.Register<CategoryList, Chart?>(nameof(Chart));

    public static readonly DirectProperty<CategoryList, double> BillsProperty =
        AvaloniaProperty.RegisterDirect<CategoryList, double>(nameof(Bills), o => o.Bills);

    public static readonly DirectProperty<CategoryList, double> PersonalProperty =
        AvaloniaProperty.RegisterDirect<CategoryList, double>(nameof(Personal), o => o.Personal);

    public static readonly DirectProperty<CategoryList, double> RestaurantsProperty =
        AvaloniaProperty.RegisterDirect<CategoryList, double>(nameof(Restaurants), o => o.Restaurants);

    private const double DesignHeight = 120d;

    private Chart? _subscribed;
    private int _month;
    private double _bills;
    private double _personal;
    private double _restaurants;

    public CategoryList()
    {
        InitializeComponent();

        DataContext = this;

        Draw();
    }

    /// <summary>
    /// Gets or sets the window on to the data the categories follow.
    /// </summary>
    public Chart? Chart
    {
        get => GetValue(ChartProperty);
        set => SetValue(ChartProperty, value);
    }

    /// <summary>
    /// Gets the share going on bills.
    /// </summary>
    public double Bills
    {
        get => _bills;
        private set => SetAndRaise(BillsProperty, ref _bills, value);
    }

    /// <summary>
    /// Gets the share going on everything personal.
    /// </summary>
    public double Personal
    {
        get => _personal;
        private set => SetAndRaise(PersonalProperty, ref _personal, value);
    }

    /// <summary>
    /// Gets the share going on eating out.
    /// </summary>
    public double Restaurants
    {
        get => _restaurants;
        private set => SetAndRaise(RestaurantsProperty, ref _restaurants, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);

        return new Size(
            double.IsInfinity(availableSize.Width) ? 0d : availableSize.Width,
            DesignHeight * AppScale.Of(this));
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
            _month = chart.RoundedDomainStart;
            chart.PropertyChanged += OnChartChanged;
        }
    }

    private void OnChartChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Chart is not { } chart || chart.RoundedDomainStart == _month)
        {
            return;
        }

        _month = chart.RoundedDomainStart;

        Draw();
    }

    private void Draw()
    {
        var bills = Random.Shared.NextDouble();
        var personal = Random.Shared.NextDouble();
        var restaurants = Random.Shared.NextDouble();
        var total = bills + personal + restaurants;

        Bills = bills / total;
        Personal = personal / total;
        Restaurants = restaurants / total;
    }
}
