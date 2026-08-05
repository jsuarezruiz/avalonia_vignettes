using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Animation;

namespace FluidNavBar.Controls;

/// <summary>
/// One button on the bar: a white disc that rises out of the pane when picked, carrying an icon
/// that inks itself in. Port of <c>fluid_button.dart</c>.
/// </summary>
/// <remarks>
/// One clock drives all three of the disc's rise, its squash and the icon's fill, but each reads it
/// through a different remapping: the fill is over within the first quarter, the rise does not
/// begin until 28%, so they overlap rather than move together. Picking the button and dropping it
/// are not mirror images either: it takes 1666 ms to rise on an elastic curve and 833 ms to fall on
/// a steep one.
/// </remarks>
public sealed class FluidNavBarButton : TabItem
{
    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static new readonly StyledProperty<FluidIconData?> IconProperty =
        AvaloniaProperty.Register<FluidNavBarButton, FluidIconData?>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="RiseOffset"/> property.
    /// </summary>
    public static readonly DirectProperty<FluidNavBarButton, double> RiseOffsetProperty =
        AvaloniaProperty.RegisterDirect<FluidNavBarButton, double>(nameof(RiseOffset), o => o.RiseOffset);

    /// <summary>
    /// Defines the <see cref="IconScaleY"/> property.
    /// </summary>
    public static readonly DirectProperty<FluidNavBarButton, double> IconScaleYProperty =
        AvaloniaProperty.RegisterDirect<FluidNavBarButton, double>(nameof(IconScaleY), o => o.IconScaleY);

    /// <summary>
    /// Defines the <see cref="IconFill"/> property.
    /// </summary>
    public static readonly DirectProperty<FluidNavBarButton, double> IconFillProperty =
        AvaloniaProperty.RegisterDirect<FluidNavBarButton, double>(nameof(IconFill), o => o.IconFill);

    /// <summary>
    /// How far the disc rises when picked.
    /// </summary>
    private const double ActiveRise = 16d;

    /// <summary>
    /// How much of the squash comes from the elastic curve rather than sitting still.
    /// </summary>
    private const double SquashShare = 0.5d;

    private static readonly TimeSpan RiseDuration = TimeSpan.FromMilliseconds(1666);
    private static readonly TimeSpan FallDuration = TimeSpan.FromMilliseconds(833);

    private static readonly Easing RiseCurve = new ElasticOutEasing { Period = 0.38d };
    private static readonly Easing FallCurve = FlutterEasings.EaseInQuint;
    private static readonly Easing SquashOut = new CenteredElasticOutEasing { Period = 0.6d };
    private static readonly Easing SquashIn = new CenteredElasticInEasing { Period = 0.6d };

    /// <summary>
    /// The rise and the squash sit out the first 28% of the clock.
    /// </summary>
    private static readonly Easing MotionTiming = new LinearPointEasing(0.28d, 0d);

    /// <summary>
    /// The ink is in before the disc has begun to move.
    /// </summary>
    private static readonly Easing FillTiming = new LinearPointEasing(0.25d, 1d);

    private readonly Ramp _ramp = new();

    private FrameTicker? _ticker;
    private TimeSpan _lastTick;
    private double _rise;
    private double _iconScaleY = 1d;
    private double _iconFill;

    static FluidNavBarButton()
    {
        IsSelectedProperty.Changed.AddClassHandler<FluidNavBarButton>((x, _) => x.Restart());
    }

    /// <summary>
    /// Gets or sets the icon this button carries.
    /// </summary>
    public new FluidIconData? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets the disc's vertical offset, in pixels. Negative, because the disc rises out of the bar.
    /// </summary>
    public double RiseOffset
    {
        get => _rise;
        private set => SetAndRaise(RiseOffsetProperty, ref _rise, value);
    }

    /// <summary>
    /// Gets the vertical squash applied to the icon.
    /// </summary>
    public double IconScaleY
    {
        get => _iconScaleY;
        private set => SetAndRaise(IconScaleYProperty, ref _iconScaleY, value);
    }

    /// <summary>
    /// Gets how much of the icon's outline is inked in.
    /// </summary>
    public double IconFill
    {
        get => _iconFill;
        private set => SetAndRaise(IconFillProperty, ref _iconFill, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, Advance);
        Restart();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private void Restart()
    {
        if (_ticker is null)
        {
            return;
        }

        // Whichever way it is going, the run is timed from where the button currently sits, so
        // pressing another button part way through a rise does not make this one jump.
        var target = IsSelected ? 1d : 0d;
        var full = IsSelected ? RiseDuration : FallDuration;

        _ramp.AnimateTo(target, full * Math.Abs(target - _ramp.Value));

        Apply();

        _lastTick = TimeSpan.Zero;
        _ticker.Start();
    }

    private void Advance(TimeSpan elapsed)
    {
        var step = elapsed - _lastTick;
        _lastTick = elapsed;

        _ramp.Advance(step > TimeSpan.Zero ? step : TimeSpan.Zero);

        Apply();

        if (!_ramp.IsRunning)
        {
            _ticker?.Stop();
        }
    }

    private void Apply()
    {
        var motion = MotionTiming.Ease(_ramp.Value);

        RiseOffset = -ActiveRise * (IsSelected ? RiseCurve.Ease(motion) : FallCurve.Ease(motion));

        // The squash curves oscillate about a half rather than running 0 to 1, so the resting size
        // is the middle of their swing rather than either end of it.
        var squash = IsSelected ? SquashOut.Ease(motion) : SquashIn.Ease(motion);

        IconScaleY = 0.5d + (squash * SquashShare) + (0.5d - (SquashShare / 2d));
        IconFill = FillTiming.Ease(_ramp.Value);
    }
}
