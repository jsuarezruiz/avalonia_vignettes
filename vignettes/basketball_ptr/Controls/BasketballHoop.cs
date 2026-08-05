using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Controls;
using BasketballPullToRefresh.Animation;
using BasketballPullToRefresh.Models;

namespace BasketballPullToRefresh.Controls;

/// <summary>
/// The hoop uncovered by pulling the list down, and the ball thrown at it while the scores are
/// fetched. Port of <c>pull_to_refresh_container.dart</c> and <c>spinning_basketball.dart</c>.
/// </summary>
/// <remarks>
/// Every measurement is a fraction of <see cref="Extent"/>, and the artwork's aspect ratios turn
/// each width into a height. The pull shrinks the hoop; the throw springs it back.
/// </remarks>
public sealed class BasketballHoop : Control
{
    /// <summary>How far the list can be pulled, as a multiple of <see cref="Extent"/>.</summary>
    public const double MaxPull = 1.2d;

    /// <summary>Defines the <see cref="Pull"/> property.</summary>
    public static readonly StyledProperty<double> PullProperty =
        AvaloniaProperty.Register<BasketballHoop, double>(nameof(Pull));

    /// <summary>Defines the <see cref="Extent"/> property.</summary>
    public static readonly StyledProperty<double> ExtentProperty =
        AvaloniaProperty.Register<BasketballHoop, double>(nameof(Extent), 180d);

    /// <summary>Defines the <see cref="IsPending"/> property.</summary>
    public static readonly StyledProperty<bool> IsPendingProperty =
        AvaloniaProperty.Register<BasketballHoop, bool>(nameof(IsPending));

    /// <summary>Defines the <see cref="Caption"/> property.</summary>
    public static readonly DirectProperty<BasketballHoop, string> CaptionProperty =
        AvaloniaProperty.RegisterDirect<BasketballHoop, string>(nameof(Caption), o => o.Caption);

    /// <summary>The ball's flight, from off screen to dropping away.</summary>
    private static readonly TimeSpan ThrowDuration = TimeSpan.FromSeconds(2.5d);

    /// <summary>
    /// Where the scores count as arrived. The original dispatches its
    /// <c>DoneLoadingNotification</c> here, so the list closes while the ball is still dropping.
    /// </summary>
    private const double ArrivedAt = 0.9d;

    /// <summary>Where the caption says so.</summary>
    private const double UpdatedAt = 6d / 7d;

    /// <summary>Where the ball passes behind the rim and the net.</summary>
    private const double BehindHoopAt = 0.28d;

    // The artwork's proportions, which turn each width into a height.
    private const double BackboardRatio = 0.69375d;
    private const double NetRatio = 0.984375d;
    private const double RimRatio = 0.121739d;

    /// <summary>One frame of the ball sheet, which holds sixty in ten columns.</summary>
    private const int BallFrameSize = 400;

    /// <summary>The first half of Flutter's <c>ElasticOutCurve(0.65)</c>.</summary>
    private static readonly Easing HalfSpring = new HalfElasticOutEasing();

    /// <summary>The wave the ball rattles across the rim on, out and back twice.</summary>
    private static readonly Easing BallSwing = new SineEasing { Start = -Math.PI / 2d, Length = Math.PI * 4d };

    /// <summary>The wave the ball's size pulses on.</summary>
    private static readonly Easing BallPulse = new SineEasing { Length = Math.PI * 4d };

    /// <summary>The half wave that lands the ball on the rim.</summary>
    private static readonly Easing BallDrop = new SineEasing { Start = -Math.PI / 2d, Length = Math.PI };

    /// <summary>How much scale the pull takes off the hoop, and the spring back.</summary>
    private static readonly TweenSequence HoopScale = new(
        new TweenSegment(0.5d, 0d, 2d, HalfSpring),
        (0d, 0d, 5d));

    /// <summary>Which frame to draw, spinning through four passes.</summary>
    private static readonly TweenSequence BallFrame = new(
        (0d, 19d, 2d),
        (20d, 39d, 2d),
        (20d, 39d, 2d),
        (40d, 59d, 1d));

    /// <summary>The ball's place across the screen, as a share of 160 scaled points.</summary>
    private static readonly TweenSequence BallX = new(
        (0d, 0.08d, 1.2d),
        (0.08d, 0.12d, 0.6d),
        new TweenSegment(0.12d, -0.12d, 4.2d, BallSwing),
        new TweenSegment(0.12d, 0d, 1d, FlutterEasings.EaseInSine));

    /// <summary>The ball's height above the rim, as a share of half the extent.</summary>
    private static readonly TweenSequence BallY = new(
        new TweenSegment(1.7d, -0.72d, 1.3d, FlutterEasings.EaseOutSine),
        new TweenSegment(-0.72d, 0.02d, 0.7d, BallDrop),
        (0.02d, 0.02d, 4d),
        new TweenSegment(0.02d, 0.3d, 1d, FlutterEasings.EaseInCubic));

    /// <summary>The ball's size, relative to nominal.</summary>
    private static readonly TweenSequence BallSize = new(
        (1d, 1d, 2d),
        new TweenSegment(1.05d, 0.9d, 4d, BallPulse),
        (1d, 1d, 1d));

    private readonly AnimationController _throw;

    private TaskCompletionSource? _arrived;
    private string _caption = string.Empty;

    static BasketballHoop() => AffectsRender<BasketballHoop>(PullProperty, ExtentProperty);

    /// <summary>Initializes a new instance of the <see cref="BasketballHoop"/> class.</summary>
    public BasketballHoop()
    {
        _throw = new AnimationController(this, OnThrowProgressChanged) { Duration = ThrowDuration };

        UpdateCaption();
    }

    /// <summary>
    /// Gets or sets how far the list is pulled down, as a multiple of <see cref="Extent"/> up to
    /// <see cref="MaxPull"/>. Port of the demo's <c>_percentage</c>.
    /// </summary>
    public double Pull
    {
        get => GetValue(PullProperty);
        set => SetValue(PullProperty, value);
    }

    /// <summary>Gets or sets the height every measurement in the scene is a fraction of.</summary>
    public double Extent
    {
        get => GetValue(ExtentProperty);
        set => SetValue(ExtentProperty, value);
    }

    /// <summary>Gets or sets whether letting go now would refresh.</summary>
    public bool IsPending
    {
        get => GetValue(IsPendingProperty);
        set => SetValue(IsPendingProperty, value);
    }

    /// <summary>Gets the line above the hoop, which says what the pull will do.</summary>
    public string Caption
    {
        get => _caption;
        private set => SetAndRaise(CaptionProperty, ref _caption, value);
    }

    /// <summary>
    /// Throws the ball. Completes at <see cref="ArrivedAt"/>, before the throw itself ends.
    /// </summary>
    public Task ThrowAsync()
    {
        _arrived?.TrySetResult();
        _arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _throw.SetValue(0d);
        _throw.Forward();

        return _arrived.Task;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var extent = Extent;

        if (width <= 0d || extent <= 0d)
        {
            return;
        }

        var progress = _throw.Value;
        var centerX = width / 2d;

        // The pull takes size off the hoop; the throw springs it back.
        var scale = 0.8d - (FlutterEasings.EaseIn.Ease(Pull / 2d) * HoopScale.Evaluate(progress));

        var backboardWidth = 0.8d * extent * scale;
        var netWidth = 0.35d * extent * scale;
        var rimWidth = 0.4d * extent * scale;

        var backboardHeight = backboardWidth * BackboardRatio;
        var netHeight = netWidth * NetRatio;
        var rimHeight = rimWidth * RimRatio;

        var startY = extent * 0.08d * 3d;
        var backboardY = startY + (backboardHeight / 2d);
        var rimY = backboardY + (backboardHeight * 0.33d);
        var netY = rimY + (netHeight * 0.5d);

        context.DrawImage(
            DemoData.Backboard,
            Centered(centerX, backboardY, backboardWidth, backboardHeight));

        // Through the hoop, the ball passes behind the rim and the net.
        var isBehindHoop = progress > BehindHoopAt;

        if (isBehindHoop)
        {
            DrawBall(context, width, extent, progress);
        }

        context.DrawImage(DemoData.Net, Centered(centerX, netY, netWidth, netHeight));
        context.DrawImage(DemoData.Rim, Centered(centerX, rimY, rimWidth, rimHeight));

        if (!isBehindHoop)
        {
            DrawBall(context, width, extent, progress);
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(0d, Extent * MaxPull);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ExtentProperty)
        {
            InvalidateMeasure();
        }

        if (change.Property != PullProperty && change.Property != IsPendingProperty)
        {
            return;
        }

        Rewind();
        UpdateCaption();
    }

    private static Rect Centered(double centerX, double centerY, double width, double height) =>
        new(centerX - (width / 2d), centerY - (height / 2d), width, height);

    private void DrawBall(DrawingContext context, double width, double extent, double progress)
    {
        var size = 0.3d * extent * BallSize.Evaluate(progress) * 0.8d;

        // Aimed at the hoop's resting size, so a hoop still springing back cannot drag the flight
        // path with it.
        var backboardHeight = 0.8d * extent * 0.8d * BackboardRatio;
        var rimY = (extent * 0.08d * 3d) + (backboardHeight / 2d) + (backboardHeight * 0.33d);

        // Wider screens throw wider, up to half as wide again.
        var scaleX = Math.Clamp(width / 320d, 1d, 1.5d);

        var left = (BallX.Evaluate(progress) * 160d * scaleX) + (width / 2d) - (size / 2d);
        var top = (BallY.Evaluate(progress) * extent / 2d) + rimY - size;

        Sprite.DrawFrame(
            context,
            DemoData.BallSpriteSheet,
            BallFrameSize,
            BallFrameSize,
            BallFrame.Evaluate(progress),
            new Rect(left, top, size, size));
    }

    /// <summary>
    /// Puts the scene back to rest once both the list has closed and the throw has ended, so the
    /// next pull starts from a still hoop.
    /// </summary>
    private void Rewind()
    {
        if (Pull <= 0d && !_throw.IsAnimating)
        {
            _throw.SetValue(0d);
        }
    }

    private void OnThrowProgressChanged(double progress)
    {
        if (progress > ArrivedAt)
        {
            _arrived?.TrySetResult();
        }

        // The list closes first, so the throw is usually the last of the two to end.
        if (progress >= 1d)
        {
            Rewind();
        }

        UpdateCaption();
        InvalidateVisual();
    }

    private void UpdateCaption() => Caption = _throw.Value switch
    {
        > UpdatedAt => "Updated!",
        _ when _throw.IsAnimating => "Checking for latest scores",
        _ when IsPending => "Release to refresh",
        _ => "Pull down to refresh",
    };

    /// <summary>Flutter's <c>ElasticOutCurve(0.65)</c>, run only to its half way point.</summary>
    private sealed class HalfElasticOutEasing : Easing
    {
        private static readonly ElasticOutEasing Spring = new() { Period = 0.65d };

        /// <inheritdoc />
        public override double Ease(double progress) => Spring.Ease(progress * 0.5d);
    }
}
