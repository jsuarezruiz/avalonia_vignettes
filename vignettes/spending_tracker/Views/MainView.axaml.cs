using Avalonia.Controls;
using AvaloniaVignettes.Shared.Animation;
using SpendingTracker.Animation;
using SpendingTracker.Controls;
using SpendingTracker.Models;

namespace SpendingTracker.Views;

/// <summary>
/// The whole tracker: a summary, a chart to drag about, the months it covers and where the money
/// goes. Port of <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// Letting go of the chart snaps it to a whole month. The snap is timed for a full run down all
/// twenty one months, so the fraction actually left to travel lands it in a couple of hundred
/// milliseconds, and it eases in so the chart leaves the finger's speed behind gently.
/// </remarks>
public partial class MainView : UserControl
{
    private readonly InterpolationAnimation _snap;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        Chart = new Chart(DemoData.Income, DemoData.Expense)
        {
            DomainStart = 0d,
            DomainEnd = 13d,
            RangeStart = 0d,
            RangeEnd = 8d,
            SelectedDataPoint = 4,
        };

        _snap = new InterpolationAnimation(this, OnSnapped)
        {
            Duration = TimeSpan.FromMilliseconds(12000),
            Range = Chart.MaxDomain,
            Easing = FlutterEasings.EaseIn,
        };

        InitializeComponent();
    }

    /// <summary>
    /// Gets the window on to the data every part of the view shares.
    /// </summary>
    public Chart Chart { get; }

    /// <summary>
    /// Puts the marker on a month, as tapping the chart does.
    /// </summary>
    public void Select(int month) => Chart.SelectedDataPoint = month;

    /// <summary>
    /// Slides the chart along by a number of months, as dragging it does.
    /// </summary>
    public void Slide(double months)
    {
        if (months < 0d)
        {
            Chart.DomainStart += months;
            Chart.DomainEnd += months;
        }
        else
        {
            Chart.DomainEnd += months;
            Chart.DomainStart += months;
        }
    }

    /// <summary>
    /// Settles the chart on a whole month, as letting go of it does.
    /// </summary>
    public void Release()
    {
        _snap.SetValue(Chart.DomainStart);
        _snap.AnimateTo(Models.Chart.Round(Chart.DomainStart));
    }

    private void OnInteracted(object? sender, InteractEventArgs e)
    {
        if (e.Ended)
        {
            Release();
        }
    }

    private void OnSnapped(double value)
    {
        // Whichever end is moving into new ground goes first, so it is never clamped by the other.
        Slide(value - Chart.DomainStart);
    }
}
