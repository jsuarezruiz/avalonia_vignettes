using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using DogSlider.Effects;

namespace DogSlider.Controls;

/// <summary>
/// A slider you set by dragging a ball along a line, with a dog that runs after it. Port of
/// <c>dog_slider.dart</c>.
/// </summary>
/// <remarks>
/// The dog is not tied to the handle: it chases it under its own physics, which is why it lags
/// behind a quick drag, drifts to a stop, and turns round when the ball goes the other way. Dragging
/// to zero sends the ball's target off the left edge, so the dog leaves the scene entirely.
/// <para>
/// Pressing the ball dips it into the line and pushes the line down with it; letting go springs both
/// back on an elastic curve.
/// </para>
/// </remarks>
public sealed class DogSlider : Slider
{
    public static readonly StyledProperty<double> HorizontalPaddingProperty =
        AvaloniaProperty.Register<DogSlider, double>(nameof(HorizontalPadding), 40d);

    public static readonly DirectProperty<DogSlider, Thickness> TrackMarginProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, Thickness>(nameof(TrackMargin), o => o.TrackMargin);

    public static readonly StyledProperty<double> ArcRadiusProperty =
        AvaloniaProperty.Register<DogSlider, double>(nameof(ArcRadius), 15d);

    public static readonly DirectProperty<DogSlider, double> BallOffsetProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, double>(nameof(BallOffset), o => o.BallOffset);

    public static readonly DirectProperty<DogSlider, double> ArcScaleYProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, double>(nameof(ArcScaleY), o => o.ArcScaleY);

    public static readonly DirectProperty<DogSlider, double> HandleXProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, double>(nameof(HandleX), o => o.HandleX);

    public static readonly DirectProperty<DogSlider, double> BallSizeProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, double>(nameof(BallSize), o => o.BallSize);

    public static readonly DirectProperty<DogSlider, double> BallLeftProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, double>(nameof(BallLeft), o => o.BallLeft);

    public static readonly DirectProperty<DogSlider, double> BallBottomProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, double>(nameof(BallBottom), o => o.BallBottom);

    public static readonly DirectProperty<DogSlider, bool> IsArrowVisibleProperty =
        AvaloniaProperty.RegisterDirect<DogSlider, bool>(nameof(IsArrowVisible), o => o.IsArrowVisible);

    private const double OffscreenX = -50d;

    private const double BottomPadding = 15d;

    private const double BallHop = 30d;

    private static readonly TimeSpan StartDelay = TimeSpan.FromMilliseconds(500);

    private static readonly TimeSpan PressDuration = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan ReleaseDuration = TimeSpan.FromMilliseconds(600);

    private static readonly Easing PressEasing = new BackEaseOut();
    private static readonly Easing ReleaseEasing = new ElasticOutEasing { Period = 0.3d };

    private readonly AnimationController _ball;
    private readonly CharacterPhysics _physics = new(OffscreenX);

    private FrameTicker? _ticker;
    private DogView? _dog;
    private double _ballProgress;
    private double _ballFrom;
    private double _ballTo;
    private double _ballOffset;
    private double _arcScaleY = 1d;
    private double _handleX;
    private bool _isArrowVisible = true;
    private bool _isInteracting;

    static DogSlider()
    {
        MinimumProperty.OverrideDefaultValue<DogSlider>(0d);
        MaximumProperty.OverrideDefaultValue<DogSlider>(1d);
        SmallChangeProperty.OverrideDefaultValue<DogSlider>(0.05d);
        LargeChangeProperty.OverrideDefaultValue<DogSlider>(0.1d);
        HorizontalPaddingProperty.Changed.AddClassHandler<DogSlider>((x, e) =>
        {
            x.RaisePropertyChanged(TrackMarginProperty, new Thickness(e.GetOldValue<double>(), 0d), x.TrackMargin);
            x.UpdateHandleFromValue(x.Bounds.Width);
        });
        ValueProperty.Changed.AddClassHandler<DogSlider>((x, _) => x.OnValueChanged());
        ArcRadiusProperty.Changed.AddClassHandler<DogSlider>((x, e) =>
            x.OnArcRadiusChanged(e.GetOldValue<double>()));
    }

    public DogSlider()
    {
        _ball = new AnimationController(this, OnBallProgressChanged) { Duration = PressDuration };

        _physics.MoveStarted += (_, _) => SetDogWalking(true);
        _physics.DestinationReached += (_, _) => SetDogWalking(false);

        // Track buttons/Thumb handle the routed events. Observe the tunnel without handling or
        // capturing them so the artwork still animates when native Slider consumes the input.
        AddHandler(PointerPressedEvent, ObservePress, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, ObserveRelease, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>
    /// Gets or sets how far in from each edge the line's travel starts.
    /// </summary>
    public double HorizontalPadding
    {
        get => GetValue(HorizontalPaddingProperty);
        set => SetValue(HorizontalPaddingProperty, value);
    }

    /// <summary>
    /// Gets the native track's horizontal inset.
    /// </summary>
    public Thickness TrackMargin => new(HorizontalPadding, 0d);

    /// <summary>
    /// Gets or sets the width of the dip the ball rests in.
    /// </summary>
    public double ArcRadius
    {
        get => GetValue(ArcRadiusProperty);
        set => SetValue(ArcRadiusProperty, value);
    }

    /// <summary>
    /// Gets how far the ball has hopped out of its dip.
    /// </summary>
    public double BallOffset
    {
        get => _ballOffset;
        private set => SetAndRaise(BallOffsetProperty, ref _ballOffset, value);
    }

    /// <summary>
    /// Gets how deep the dip in the line is; it flattens as the ball lifts out.
    /// </summary>
    public double ArcScaleY
    {
        get => _arcScaleY;
        private set => SetAndRaise(ArcScaleYProperty, ref _arcScaleY, value);
    }

    /// <summary>
    /// Gets where along the line the ball sits.
    /// </summary>
    public double HandleX
    {
        get => _handleX;
        private set => SetAndRaise(HandleXProperty, ref _handleX, value);
    }

    /// <summary>
    /// Gets how big the ball is drawn; it is sized from the dip it sits in.
    /// </summary>
    public double BallSize => ArcRadius * 1.35d;

    /// <summary>
    /// Gets where the ball's left edge sits.
    /// </summary>
    public double BallLeft => HandleX - (BallSize / 2d);

    /// <summary>
    /// Gets how far the ball sits above the bottom of the slider.
    /// </summary>
    public double BallBottom => BottomPadding + 4d + BallOffset;

    /// <summary>
    /// Gets a value indicating whether the nudge arrow beside the ball is showing.
    /// </summary>
    public bool IsArrowVisible
    {
        get => _isArrowVisible;
        private set => SetAndRaise(IsArrowVisibleProperty, ref _isArrowVisible, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _dog = e.NameScope.Find<DogView>("PART_Dog");

        // Flutter creates the dog at OffscreenX before the delayed physics ticker starts. Position
        // the templated dog immediately as well; otherwise Canvas.Left defaults to zero and the
        // sitting pose covers the ball until the first physics update, making the first drag look
        // unlike every subsequent one.
        UpdateDog();
    }

    private void ObservePress(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        // Avalonia's Slider owns the value change, pointer capture and keyboard behavior. The
        // vignette only layers its ball-hop animation on top of that standard interaction.
        _isInteracting = true;
        StartBall(1d, PressDuration);
    }

    private void ObserveRelease(object? sender, PointerReleasedEventArgs e) => EndInteraction();

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndInteraction();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _ticker?.Stop();
        _ball.Stop();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);

        // Bounds is not updated until arrange has finished, so the size being arranged is the only
        // width available here. Reading Bounds instead leaves the ball parked at zero on first show.
        UpdateHandleFromValue(finalSize.Width);

        return arranged;
    }

    private void OnTick(TimeSpan elapsed)
    {
        // The dog ignores the slider for a beat on startup, so it walks in rather than being there.
        if (elapsed < StartDelay)
        {
            return;
        }

        _physics.Update(elapsed - StartDelay);
        UpdateDog();
    }

    private void EndInteraction()
    {
        if (!_isInteracting) return;
        _isInteracting = false;
        StartBall(0d, ReleaseDuration);
    }

    private void OnValueChanged()
    {
        UpdateHandleFromValue(Bounds.Width);

        // The nudge only belongs at zero, and the value can get there without the ball being touched.
        UpdateArrowVisibility();
    }

    private void UpdateHandleFromValue(double width)
    {
        if (width <= HorizontalPadding * 2d)
        {
            return;
        }

        // The original's start-up formula and its drag formula disagree; the drag one is used for
        // both here, so setting the value in code lands the ball exactly where dragging to it would.
        // They only agree at zero, which is where the original starts, so the difference never shows.
        var oldBallLeft = BallLeft;

        HandleX = HorizontalPadding + (Value * (width - (HorizontalPadding * 2d)));
        RaisePropertyChanged(BallLeftProperty, oldBallLeft, BallLeft);

        UpdateTarget();
    }

    private void UpdateTarget() => _physics.TargetX = Value == 0d ? OffscreenX : HandleX;

    private void StartBall(double target, TimeSpan duration)
    {
        _ballFrom = _ballProgress;
        _ballTo = target;

        _ball.Duration = duration;
        _ball.SetValue(0d);
        _ball.Forward();
    }

    private void OnBallProgressChanged(double progress)
    {
        var easing = _ballTo > _ballFrom ? PressEasing : ReleaseEasing;
        var oldBallBottom = BallBottom;

        _ballProgress = _ballFrom + ((_ballTo - _ballFrom) * easing.Ease(progress));

        BallOffset = _ballProgress * BallHop;
        RaisePropertyChanged(BallBottomProperty, oldBallBottom, BallBottom);

        // The dip flattens out in the first half of the hop and stays flat after that.
        ArcScaleY = Math.Max(0d, 1d - (_ballProgress * 2d));

        UpdateArrowVisibility();
    }

    private void UpdateArrowVisibility() => IsArrowVisible = Value == 0d && _ballProgress < 0.2d;

    private void OnArcRadiusChanged(double oldRadius)
    {
        var oldBallSize = oldRadius * 1.35d;
        var oldBallLeft = HandleX - (oldBallSize / 2d);

        RaisePropertyChanged(BallSizeProperty, oldBallSize, BallSize);
        RaisePropertyChanged(BallLeftProperty, oldBallLeft, BallLeft);
    }

    private void SetDogWalking(bool isWalking)
    {
        if (_dog is { } dog)
        {
            dog.IsWalking = isWalking;
        }
    }

    private void UpdateDog()
    {
        if (_dog is not { } dog)
        {
            return;
        }

        dog.IsFlipped = _physics.IsFlipped;

        // During OnApplyTemplate the child has not been arranged yet, but its explicit template
        // width is already available. Using that width keeps the initial pose fully off-screen.
        var width = dog.Bounds.Width > 0d ? dog.Bounds.Width : dog.Width;
        Canvas.SetLeft(dog, _physics.Position - (width / 2d));
    }
}
