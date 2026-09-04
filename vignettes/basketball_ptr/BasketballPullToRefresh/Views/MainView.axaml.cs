using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
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
    public static readonly DirectProperty<MainView, PullMetrics> MetricsProperty =
        AvaloniaProperty.RegisterDirect<MainView, PullMetrics>(nameof(Metrics), o => o.Metrics);

    private const double FlutterControllerMilliseconds = 400d;
    private const double FlutterPullToController = 0.83d;
    private const double RelaxedControllerValue = 0.22d;
    private const double RelaxedPull = BasketballHoop.MaxPull * (1d - RelaxedControllerValue);

    private readonly IReadOnlyList<GameSlot> _games = DemoData.CreateInitialGames();
    private readonly AnimationController _pullAnimation;
    private readonly TranslateTransform _scoresTranslation = new();

    private PullMetrics _metrics = PullMetrics.ForScreen(0d);
    private RefreshVisualizerState _state = RefreshVisualizerState.Idle;
    private double _pullAnimationFrom;
    private double _pullAnimationTo;
    private bool _isRefreshing;

    public MainView()
    {
        InitializeComponent();

        DataContext = this;
        Games.ItemsSource = _games;
        ScoresScroller.RenderTransform = _scoresTranslation;
        ScoresScroller.RenderTransformOrigin = RelativePoint.TopLeft;

        _pullAnimation = new AnimationController(this, OnPullAnimationProgressChanged);

        Refresh.AddHandler(InputElement.PullGestureEvent, OnPullGesture);
        Refresh.RefreshRequested += OnRefreshRequested;
        GestureScroller.ScrollChanged += OnGestureScrollerScrollChanged;

        // The container's own threshold decides when letting go would refresh.
        Visualizer
            .GetObservable(RefreshVisualizer.RefreshVisualizerStateProperty)
            .Subscribe(new AnonymousObserver<RefreshVisualizerState>(OnVisualizerStateChanged));
    }

    /// <summary>
    /// Gets the sizes of the pull area, all derived from the height of the screen.
    /// </summary>
    public PullMetrics Metrics
    {
        get => _metrics;
        private set => SetAndRaise(MetricsProperty, ref _metrics, value);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Metrics = PullMetrics.ForScreen(finalSize.Height);
        UpdateScoresTranslation();

        return base.ArrangeOverride(finalSize);
    }

    private void OnPullGesture(object? sender, PullGestureEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        // A new pull takes over from a close still running.
        _pullAnimation.Stop();

        SetPull(Metrics.Extent > 0d
            ? Math.Clamp(e.Delta.Y / Metrics.Extent, 0d, BasketballHoop.MaxPull)
            : 0d);

        // The refresh recognizer has captured this pointer. Marking its routed event handled keeps
        // the nested ScrollViewer from also adjusting its offset during the same mouse movement.
        e.Handled = true;
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

        // Flutter does not leave the list at its 120% over-pull. Its 400 ms controller moves from
        // the release value to 0.22, which settles a full pull at 93.6% while the ball is in flight.
        RelaxForRefresh();

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
            deferral.Complete();
            _isRefreshing = false;
            Close();
        }
    }

    private void OnVisualizerStateChanged(RefreshVisualizerState state)
    {
        var previousState = _state;
        _state = state;

        Hoop.IsPending = state == RefreshVisualizerState.Pending;

        // Let RefreshContainer process release first. Closing directly from PullGestureEnded races
        // its Pending -> Refreshing transition and can cancel a valid mouse refresh.
        if (state == RefreshVisualizerState.Idle &&
            previousState is RefreshVisualizerState.Interacting or RefreshVisualizerState.Pending)
        {
            Close();
        }
    }

    private void Close()
    {
        if (Hoop.Pull <= 0d)
        {
            return;
        }

        // _reset() forwards Flutter's 400 ms controller from 1 - pull * .83, so the remaining
        // duration is proportional to the amount of pull still visible.
        AnimatePullTo(
            0d,
            TimeSpan.FromMilliseconds(FlutterControllerMilliseconds * Hoop.Pull * FlutterPullToController));
    }

    private void RelaxForRefresh()
    {
        var controllerValue = Math.Clamp(1d - (Hoop.Pull * FlutterPullToController), 0d, 1d);
        var duration = TimeSpan.FromMilliseconds(
            FlutterControllerMilliseconds * Math.Abs(RelaxedControllerValue - controllerValue));

        AnimatePullTo(RelaxedPull, duration);
    }

    private void AnimatePullTo(double target, TimeSpan duration)
    {
        _pullAnimationFrom = Hoop.Pull;
        _pullAnimationTo = target;
        _pullAnimation.Duration = duration;
        _pullAnimation.SetValue(0d);
        _pullAnimation.Forward();
    }

    private void OnPullAnimationProgressChanged(double progress) =>
        SetPull(_pullAnimationFrom + ((_pullAnimationTo - _pullAnimationFrom) * progress));

    private void SetPull(double pull)
    {
        Hoop.Pull = pull;
        UpdateScoresTranslation();
    }

    private void UpdateScoresTranslation()
    {
        _scoresTranslation.Y = Math.Min(Hoop.Pull * Metrics.Extent, Metrics.Height);
    }

    private void OnGestureScrollerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (Hoop.Pull <= 0d && !_isRefreshing && ScoresScroller.Offset != GestureScroller.Offset)
        {
            ScoresScroller.Offset = GestureScroller.Offset;
        }
    }

}
