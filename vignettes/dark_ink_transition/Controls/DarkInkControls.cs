using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaVignettes.Shared.Animation;

namespace DarkInkTransition.Controls;

/// <summary>
/// The three buttons at the bottom, which drop out of sight and come back in the new colours. Port
/// of <c>dark_ink_controls.dart</c>.
/// </summary>
/// <remarks>
/// They leave and return on one 2000 ms clock, each started two hundredths later than the last, so
/// they go and come back as a ripple rather than together. The recolour happens at the half way
/// point, while all three are off the bottom of the screen and nobody can see them change.
/// </remarks>
public sealed class DarkInkControls : TemplatedControl
{
    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<DarkInkControls, bool>(nameof(IsDark));

    public static readonly DirectProperty<DarkInkControls, double> FirstOffsetProperty =
        AvaloniaProperty.RegisterDirect<DarkInkControls, double>(nameof(FirstOffset), o => o.FirstOffset);

    public static readonly DirectProperty<DarkInkControls, double> SecondOffsetProperty =
        AvaloniaProperty.RegisterDirect<DarkInkControls, double>(nameof(SecondOffset), o => o.SecondOffset);

    public static readonly DirectProperty<DarkInkControls, double> ThirdOffsetProperty =
        AvaloniaProperty.RegisterDirect<DarkInkControls, double>(nameof(ThirdOffset), o => o.ThirdOffset);

    public static readonly DirectProperty<DarkInkControls, IBrush?> ButtonBackgroundProperty =
        AvaloniaProperty.RegisterDirect<DarkInkControls, IBrush?>(nameof(ButtonBackground), o => o.ButtonBackground);

    public static readonly DirectProperty<DarkInkControls, IBrush?> ButtonForegroundProperty =
        AvaloniaProperty.RegisterDirect<DarkInkControls, IBrush?>(nameof(ButtonForeground), o => o.ButtonForeground);

    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(2000);

    private static readonly IBrush Light = new ImmutableSolidColorBrush(Color.FromRgb(0x67, 0xEC, 0xDC));
    private static readonly IBrush Dark = new ImmutableSolidColorBrush(Color.FromRgb(0x17, 0x11, 0x37));

    private static readonly TweenSequence First =
        new((0d, 100d, 10d), (100d, 100d, 76d), (100d, 0d, 10d), (0d, 0d, 4d));

    private static readonly TweenSequence Second =
        new((0d, 0d, 2d), (0d, 100d, 10d), (100d, 100d, 76d), (100d, 0d, 10d), (0d, 0d, 2d));

    private static readonly TweenSequence Third =
        new((0d, 0d, 4d), (0d, 100d, 10d), (100d, 100d, 76d), (100d, 0d, 10d));

    private readonly Ramp _ramp = new();

    private FrameTicker? _ticker;
    private TimeSpan _lastTick;
    private double _firstOffset;
    private double _secondOffset;
    private double _thirdOffset;
    private IBrush? _buttonBackground = Light;
    private IBrush? _buttonForeground = Dark;

    static DarkInkControls() =>
        IsDarkProperty.Changed.AddClassHandler<DarkInkControls>((x, _) => x.Restart());

    /// <summary>
    /// Gets or sets a value indicating whether the dark scheme is showing.
    /// </summary>
    public bool IsDark
    {
        get => GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    /// <summary>
    /// Gets how far the first button has dropped.
    /// </summary>
    public double FirstOffset
    {
        get => _firstOffset;
        private set => SetAndRaise(FirstOffsetProperty, ref _firstOffset, value);
    }

    /// <summary>
    /// Gets how far the second button has dropped.
    /// </summary>
    public double SecondOffset
    {
        get => _secondOffset;
        private set => SetAndRaise(SecondOffsetProperty, ref _secondOffset, value);
    }

    /// <summary>
    /// Gets how far the third button has dropped.
    /// </summary>
    public double ThirdOffset
    {
        get => _thirdOffset;
        private set => SetAndRaise(ThirdOffsetProperty, ref _thirdOffset, value);
    }

    /// <summary>
    /// Gets the buttons' fill.
    /// </summary>
    public IBrush? ButtonBackground
    {
        get => _buttonBackground;
        private set => SetAndRaise(ButtonBackgroundProperty, ref _buttonBackground, value);
    }

    /// <summary>
    /// Gets the buttons' icon colour.
    /// </summary>
    public IBrush? ButtonForeground
    {
        get => _buttonForeground;
        private set => SetAndRaise(ButtonForegroundProperty, ref _buttonForeground, value);
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

    private void Restart()
    {
        // The run always plays forwards, whichever way the scheme is going.
        _ramp.Set(0d);
        _ramp.AnimateTo(1d, Duration);

        _lastTick = TimeSpan.Zero;
        _ticker?.Start();
    }

    private void Advance(TimeSpan elapsed)
    {
        var step = elapsed - _lastTick;
        _lastTick = elapsed;

        _ramp.Advance(step > TimeSpan.Zero ? step : TimeSpan.Zero);

        var value = _ramp.Value;

        FirstOffset = First.Evaluate(value);
        SecondOffset = Second.Evaluate(value);
        ThirdOffset = Third.Evaluate(value);

        if (value > 0.5d)
        {
            ButtonBackground = IsDark ? Dark : Light;
            ButtonForeground = IsDark ? Light : Dark;
        }

        if (!_ramp.IsRunning)
        {
            _ticker?.Stop();
        }
    }
}
