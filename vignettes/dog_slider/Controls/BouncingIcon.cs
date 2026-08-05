using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaVignettes.Shared.Animation;

namespace DogSlider.Controls;

/// <summary>
/// Slides its child back and forth to draw the eye, and fades in and out. Port of
/// <c>bouncing_icon.dart</c>.
/// </summary>
/// <remarks>
/// The nudge leans out quickly and eases back slowly: it uses a different curve in each direction,
/// which is what stops it reading as a plain oscillation.
/// </remarks>
public sealed class BouncingIcon : Decorator
{
    /// <summary>
    /// Defines the <see cref="IsShown"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShownProperty =
        AvaloniaProperty.Register<BouncingIcon, bool>(nameof(IsShown));

    /// <summary>
    /// Defines the <see cref="MaxBounce"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxBounceProperty =
        AvaloniaProperty.Register<BouncingIcon, double>(nameof(MaxBounce), 20d);

    private static readonly TimeSpan BounceDuration = TimeSpan.FromMilliseconds(700);
    private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(350);

    private readonly TranslateTransform _offset = new();
    private readonly AnimationController _bounce;
    private readonly DispatcherTimer _timer;

    private bool _isOut;

    static BouncingIcon() =>
        IsShownProperty.Changed.AddClassHandler<BouncingIcon>((x, e) =>
            x.SetCurrentValue(OpacityProperty, e.GetNewValue<bool>() ? 1d : 0d));

    /// <summary>
    /// Initializes a new instance of the <see cref="BouncingIcon"/> class.
    /// </summary>
    public BouncingIcon()
    {
        Opacity = 0d;
        RenderTransform = _offset;
        IsHitTestVisible = false;

        Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = FadeDuration }];

        _bounce = new AnimationController(this, OnBounceProgressChanged) { Duration = BounceDuration };
        _timer = new DispatcherTimer { Interval = BounceDuration };
        _timer.Tick += (_, _) => StartLeg();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the nudge is showing.
    /// </summary>
    public bool IsShown
    {
        get => GetValue(IsShownProperty);
        set => SetValue(IsShownProperty, value);
    }

    /// <summary>
    /// Gets or sets how far the nudge travels.
    /// </summary>
    public double MaxBounce
    {
        get => GetValue(MaxBounceProperty);
        set => SetValue(MaxBounceProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        StartLeg();
        _timer.Start();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _timer.Stop();
        _bounce.Stop();
    }

    private void StartLeg()
    {
        _isOut = !_isOut;

        _bounce.SetValue(0d);
        _bounce.Forward();
    }

    private void OnBounceProgressChanged(double progress)
    {
        // Out on an ease-in, back on an ease-out, exactly as the original swaps its curve.
        var easing = _isOut ? FlutterEasings.EaseIn : FlutterEasings.EaseOut;
        var eased = easing.Ease(progress);

        _offset.X = _isOut ? MaxBounce * eased : MaxBounce * (1d - eased);
    }
}
