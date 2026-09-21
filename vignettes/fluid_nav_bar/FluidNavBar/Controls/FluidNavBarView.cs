using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Animation;

namespace FluidNavBar.Controls;

/// <summary>
/// A tab control whose strip is the fluid bar. Port of <c>fluid_nav_bar.dart</c>, and of the
/// <c>Scaffold</c> that hosts it.
/// </summary>
/// <remarks>
/// The pages, the selection and the cross-fade between them are all a <see cref="TabControl"/>'s
/// job, so that is what this is; only the bar is written. The original's <c>extendBody: true</c>
/// puts the content behind the bar rather than above it, which is why the template stacks them
/// instead of docking.
/// <para>
/// Two values drive the bar. One runs the dip across to the pressed button over 620 ms. The other
/// is the edge's settledness, and it does not simply follow: it drops to nothing in 300 ms, waits,
/// then takes 1200 ms to come back, so the edge has flattened out before the dip arrives and
/// re-forms only once it is there.
/// </para>
/// </remarks>
public sealed class FluidNavBarView : TabControl
{
    public static readonly DirectProperty<FluidNavBarView, double> DipXProperty =
        AvaloniaProperty.RegisterDirect<FluidNavBarView, double>(nameof(DipX), o => o.DipX);

    public static readonly DirectProperty<FluidNavBarView, double> PaneDepthProperty =
        AvaloniaProperty.RegisterDirect<FluidNavBarView, double>(nameof(PaneDepth), o => o.PaneDepth);

    public static readonly DirectProperty<FluidNavBarView, bool> IsSelectionEnabledProperty =
        AvaloniaProperty.RegisterDirect<FluidNavBarView, bool>(nameof(IsSelectionEnabled), o => o.IsSelectionEnabled);

    private const double MaxButtonStripWidth = 400d;

    private static readonly TimeSpan TravelDuration = TimeSpan.FromMilliseconds(620);
    private static readonly TimeSpan FlattenDuration = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan SettleDuration = TimeSpan.FromMilliseconds(1200);

    private static readonly Easing SettlingEdge = new ElasticOutEasing { Period = 0.38d };

    private readonly Ramp _travel = new();
    private readonly Ramp _settle = new();

    private FrameTicker? _ticker;
    private TimeSpan _lastTick;
    private TimeSpan _settleCountdown = TimeSpan.MinValue;
    private double _dipX;
    private double _paneDepth = 1d;
    private bool _isSelectionEnabled = true;

    static FluidNavBarView() =>
        SelectedIndexProperty.Changed.AddClassHandler<FluidNavBarView>((x, e) => x.OnSelected(e.GetNewValue<int>()));

    /// <summary>
    /// Gets where along the bar the dip currently sits, in pixels.
    /// </summary>
    public double DipX
    {
        get => _dipX;
        private set => SetAndRaise(DipXProperty, ref _dipX, value);
    }

    /// <summary>
    /// Gets how settled the bar's edge is, from 0 mid-travel to 1 at rest.
    /// </summary>
    public double PaneDepth
    {
        get => _paneDepth;
        private set => SetAndRaise(PaneDepthProperty, ref _paneDepth, value);
    }

    /// <summary>
    /// Gets a value indicating whether a button may be pressed. The original refuses a press while
    /// the dip is travelling; here the buttons simply stop taking them, which comes to the same
    /// thing without having to undo a selection the framework has already made.
    /// </summary>
    public bool IsSelectionEnabled
    {
        get => _isSelectionEnabled;
        private set => SetAndRaise(IsSelectionEnabledProperty, ref _isSelectionEnabled, value);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);

        // The resting position is recomputed rather than animated to, so a resize does not send the
        // dip sliding across the bar.
        if (!_travel.IsRunning)
        {
            _travel.Set(PositionOf(SelectedIndex, finalSize.Width));
            DipX = _travel.Value;
        }

        return result;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, Advance);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private double PositionOf(int index, double width)
    {
        const double count = 3d;

        var strip = Math.Min(width, MaxButtonStripWidth);
        var start = (width - strip) / 2d;

        return start + (index * strip / count) + (strip / (count * 2d));
    }

    private void OnSelected(int index)
    {
        if (index < 0 || Bounds.Width <= 0d || _travel.IsRunning)
        {
            return;
        }

        _travel.AnimateTo(PositionOf(index, Bounds.Width), TravelDuration);

        // The edge flattens out ahead of the dip and only re-forms once it has arrived.
        _settle.Set(1d);
        _settle.AnimateTo(0d, FlattenDuration);
        _settleCountdown = SettleDelay;

        IsSelectionEnabled = false;

        _lastTick = TimeSpan.Zero;
        _ticker?.Start();
    }

    private void Advance(TimeSpan elapsed)
    {
        var step = elapsed - _lastTick;
        _lastTick = elapsed;

        if (step < TimeSpan.Zero)
        {
            step = TimeSpan.Zero;
        }

        if (_settleCountdown > TimeSpan.MinValue)
        {
            _settleCountdown -= step;

            if (_settleCountdown <= TimeSpan.Zero)
            {
                _settleCountdown = TimeSpan.MinValue;
                _settle.AnimateTo(1d, SettleDuration);
            }
        }

        _travel.Advance(step);
        _settle.Advance(step);

        DipX = _travel.Value;

        // Flattening and re-forming are shaped by different curves: one collapses away, the other
        // springs back, and which applies is decided by the direction of travel.
        PaneDepth = Lerp(
            FlutterEasings.EaseInExpo.Ease(_settle.Value),
            SettlingEdge.Ease(_settle.Value),
            (_settle.Sign * 0.5d) + 0.5d);

        IsSelectionEnabled = !_travel.IsRunning;

        if (!_travel.IsRunning && !_settle.IsRunning && _settleCountdown == TimeSpan.MinValue)
        {
            _ticker?.Stop();
        }
    }

    private static double Lerp(double from, double to, double progress) => from + ((to - from) * progress);
}
