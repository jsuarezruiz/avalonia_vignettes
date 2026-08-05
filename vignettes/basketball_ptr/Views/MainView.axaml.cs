using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Reactive;
using AvaloniaVignettes.Shared.Animation;
using BasketballPullToRefresh.Controls;
using BasketballPullToRefresh.Models;

namespace BasketballPullToRefresh.Views;

/// <summary>
/// The scores screen: a list of games that fetches new ones when it is pulled down. Port of
/// <c>demo.dart</c>.
/// </summary>
/// <remarks>
/// <see cref="RefreshContainer"/> owns the gesture: it recognises the pull, pushes the list down,
/// sets the threshold, and holds the list open until a deferral is completed.
/// <para>
/// It cannot draw this indicator, though. <see cref="RefreshVisualizer"/> rotates and fades its own
/// content as the pull goes, so the visualizer here is left empty and the hoop is drawn behind the
/// list instead. That is where the original has it: its scene is pinned to the top of the screen and
/// uncovered downwards, where a visualizer would slide in from above.
/// </para>
/// <para>
/// The container reports its pull ratio to nobody, but the gesture does:
/// <see cref="InputElement.PullGestureEvent"/> carries the distance the ratio is measured from.
/// </para>
/// </remarks>
public partial class MainView : UserControl
{
    /// <summary>Defines the <see cref="Metrics"/> property.</summary>
    public static readonly DirectProperty<MainView, PullMetrics> MetricsProperty =
        AvaloniaProperty.RegisterDirect<MainView, PullMetrics>(nameof(Metrics), o => o.Metrics);

    /// <summary>How long the list takes to close: the container's implicit offset animation.</summary>
    private static readonly TimeSpan CloseDuration = TimeSpan.FromMilliseconds(150);

    private readonly IReadOnlyList<GameSlot> _games = DemoData.CreateInitialGames();
    private readonly AnimationController _close;

    private PullMetrics _metrics = PullMetrics.ForScreen(0d);
    private RefreshVisualizerState _state = RefreshVisualizerState.Idle;
    private double _pullOnRelease;
    private bool _isRefreshing;

    /// <summary>Initializes a new instance of the <see cref="MainView"/> class.</summary>
    public MainView()
    {
        InitializeComponent();

        DataContext = this;
        Games.ItemsSource = _games;

        _close = new AnimationController(this, OnCloseProgressChanged) { Duration = CloseDuration };

        Refresh.AddHandler(InputElement.PullGestureEvent, OnPullGesture);
        Refresh.AddHandler(InputElement.PullGestureEndedEvent, OnPullGestureEnded);
        Refresh.RefreshRequested += OnRefreshRequested;

        // The container's own threshold decides when letting go would refresh.
        Visualizer
            .GetObservable(RefreshVisualizer.RefreshVisualizerStateProperty)
            .Subscribe(new AnonymousObserver<RefreshVisualizerState>(OnVisualizerStateChanged));
    }

    /// <summary>Gets the sizes of the pull area, all derived from the height of the screen.</summary>
    public PullMetrics Metrics
    {
        get => _metrics;
        private set => SetAndRaise(MetricsProperty, ref _metrics, value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        Metrics = PullMetrics.ForScreen(finalSize.Height);

        return base.ArrangeOverride(finalSize);
    }

    private void OnPullGesture(object? sender, PullGestureEventArgs e)
    {
        // A new pull takes over from a close still running.
        _close.Stop();

        Hoop.Pull = Metrics.Extent > 0d
            ? Math.Clamp(e.Delta.Y / Metrics.Extent, 0d, BasketballHoop.MaxPull)
            : 0d;
    }

    private void OnPullGestureEnded(object? sender, PullGestureEndedEventArgs e)
    {
        // The container has already decided whether the pull counted: it handles the same event
        // further down, on the presenter the gesture came from.
        if (_state != RefreshVisualizerState.Refreshing)
        {
            Close();
        }
    }

    private async void OnRefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        // One pull arrives twice: a container given a visualizer subscribes to it twice, on the
        // property being set and again on its template applying (Avalonia 12.1). Answer once.
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;

        // The deferral holds the list open. The throw reports back before it ends, as the
        // original's DoneLoadingNotification does, so the list closes while the ball drops away.
        var deferral = e.GetDeferral();

        try
        {
            await Hoop.ThrowAsync();

            foreach (var (slot, game) in _games.Zip(DemoData.CreateRandomGames()))
            {
                slot.Game = game;
            }
        }
        finally
        {
            _isRefreshing = false;

            deferral.Complete();
            Close();
        }
    }

    private void OnVisualizerStateChanged(RefreshVisualizerState state)
    {
        _state = state;

        Hoop.IsPending = state == RefreshVisualizerState.Pending;
    }

    private void Close()
    {
        _pullOnRelease = Hoop.Pull;

        if (_pullOnRelease <= 0d)
        {
            return;
        }

        _close.SetValue(0d);
        _close.Forward();
    }

    private void OnCloseProgressChanged(double progress) => Hoop.Pull = _pullOnRelease * (1d - progress);
}
