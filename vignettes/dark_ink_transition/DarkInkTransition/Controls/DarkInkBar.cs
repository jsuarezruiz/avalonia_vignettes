using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Animation;

namespace DarkInkTransition.Controls;

/// <summary>
/// The bar across the top, which changes colour and swaps its moon for a sun. Port of
/// <c>dark_ink_bar.dart</c>.
/// </summary>
/// <remarks>
/// Its three values run on one 500 ms clock but on different stretches of it, so the icon has faded
/// out before the bar recolours and the bar has recoloured before the icon comes back, the swap
/// happens in the gap where there is nothing to see.
/// <para>
/// The colours are interpolated in HSV, not RGB. Between this cyan and this indigo the two take
/// visibly different routes: RGB passes through a muddy grey, HSV swings round the wheel.
/// </para>
/// </remarks>
public sealed class DarkInkBar : TemplatedControl
{
    public static readonly StyledProperty<bool> IsDarkProperty =
        AvaloniaProperty.Register<DarkInkBar, bool>(nameof(IsDark));

    public static readonly DirectProperty<DarkInkBar, IBrush?> BarBackgroundProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, IBrush?>(nameof(BarBackground), o => o.BarBackground);

    public static readonly DirectProperty<DarkInkBar, IBrush?> BarForegroundProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, IBrush?>(nameof(BarForeground), o => o.BarForeground);

    public static readonly DirectProperty<DarkInkBar, double> ToggleOpacityProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, double>(nameof(ToggleOpacity), o => o.ToggleOpacity);

    public static readonly DirectProperty<DarkInkBar, Bitmap?> ToggleIconProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, Bitmap?>(nameof(ToggleIcon), o => o.ToggleIcon);

    public static readonly DirectProperty<DarkInkBar, Bitmap?> LogoIconProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, Bitmap?>(nameof(LogoIcon), o => o.LogoIcon);

    public static readonly DirectProperty<DarkInkBar, IBrush?> ToggleMaskProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, IBrush?>(nameof(ToggleMask), o => o.ToggleMask);

    public static readonly DirectProperty<DarkInkBar, IBrush?> RuleBrushProperty =
        AvaloniaProperty.RegisterDirect<DarkInkBar, IBrush?>(nameof(RuleBrush), o => o.RuleBrush);

    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(500);

    private static readonly Color LightColor = Color.FromRgb(0x67, 0xEC, 0xDC);
    private static readonly Color DarkColor = Color.FromRgb(0x17, 0x11, 0x37);
    private static readonly IBrush LightRule = new ImmutableSolidColorBrush(Color.FromRgb(0x2B, 0x77, 0x7E));
    private static readonly IBrush DarkRule = new ImmutableSolidColorBrush(Color.FromRgb(0x00, 0x98, 0xA3));

    private static readonly TweenSequence IconOpacity = new((1d, 0d, 20d), (0d, 0d, 20d), (0d, 1d, 20d));
    private static readonly TweenSequence BackgroundRun = new((0d, 0d, 20d), (0d, 1d, 10d), (1d, 1d, 20d));
    private static readonly TweenSequence ForegroundRun = new((1d, 1d, 35d), (1d, 0d, 10d), (0d, 0d, 55d));

    private static readonly Bitmap Moon = Load("icon-moon");
    private static readonly Bitmap Sun = Load("icon-sun");
    private static readonly Bitmap Logo = Load("icon-r");
    private static readonly IBrush MoonMask = new ImageBrush(Moon) { Stretch = Stretch.Uniform };
    private static readonly IBrush SunMask = new ImageBrush(Sun) { Stretch = Stretch.Uniform };

    private readonly Ramp _ramp = new();
    private readonly SolidColorBrush _backgroundInk = new();
    private readonly SolidColorBrush _foregroundInk = new();

    private FrameTicker? _ticker;
    private TimeSpan _lastTick;
    private IBrush? _barBackground;
    private IBrush? _barForeground;
    private double _toggleOpacity = 1d;
    private Bitmap? _toggleIcon = Moon;
    private IBrush? _toggleMask = MoonMask;
    private IBrush? _ruleBrush;

    static DarkInkBar() =>
        IsDarkProperty.Changed.AddClassHandler<DarkInkBar>((x, e) => x.OnDarkChanged(e.GetNewValue<bool>()));

    /// <summary>
    /// Raised when the toggle is pressed.
    /// </summary>
    public event EventHandler? ToggleRequested;

    /// <summary>
    /// Gets or sets a value indicating whether the dark scheme is showing.
    /// </summary>
    public bool IsDark
    {
        get => GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    /// <summary>
    /// Gets the bar's fill.
    /// </summary>
    public IBrush? BarBackground
    {
        get => _barBackground;
        private set => SetAndRaise(BarBackgroundProperty, ref _barBackground, value);
    }

    /// <summary>
    /// Gets the brush the bar's icons are tinted with.
    /// </summary>
    public IBrush? BarForeground
    {
        get => _barForeground;
        private set => SetAndRaise(BarForegroundProperty, ref _barForeground, value);
    }

    /// <summary>
    /// Gets the toggle icon's opacity, which dips to nothing while the icon is swapped.
    /// </summary>
    public double ToggleOpacity
    {
        get => _toggleOpacity;
        private set => SetAndRaise(ToggleOpacityProperty, ref _toggleOpacity, value);
    }

    /// <summary>
    /// Gets the toggle's current icon: a moon in the light scheme, a sun in the dark one.
    /// </summary>
    public Bitmap? ToggleIcon
    {
        get => _toggleIcon;
        private set => SetAndRaise(ToggleIconProperty, ref _toggleIcon, value);
    }

    /// <summary>
    /// Gets the logo in the middle of the bar.
    /// </summary>
    public Bitmap? LogoIcon
    {
        get => _logoIcon;
        private set => SetAndRaise(LogoIconProperty, ref _logoIcon, value);
    }

    private Bitmap? _logoIcon = Logo;

    /// <summary>Gets the image alpha mask used to tint the logo with the animated foreground.</summary>
    public IBrush LogoMask { get; } = new ImageBrush(Logo) { Stretch = Stretch.Uniform };

    /// <summary>Gets the moon or sun alpha mask. Brushes do not have a templated parent themselves.</summary>
    public IBrush? ToggleMask
    {
        get => _toggleMask;
        private set => SetAndRaise(ToggleMaskProperty, ref _toggleMask, value);
    }

    /// <summary>
    /// Gets the rule under the bar, which snaps rather than fading.
    /// </summary>
    public IBrush? RuleBrush
    {
        get => _ruleBrush;
        private set => SetAndRaise(RuleBrushProperty, ref _ruleBrush, value);
    }

    /// <summary>
    /// Raises <see cref="ToggleRequested"/>.
    /// </summary>
    public void RequestToggle() => ToggleRequested?.Invoke(this, EventArgs.Empty);

    protected override void OnApplyTemplate(Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (e.NameScope.Find("PART_Toggle") is ToggleButton toggle)
        {
            toggle.Click += (_, _) => RequestToggle();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, Advance);
        Apply();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private static Color LerpHsv(Color from, Color to, double progress)
    {
        var a = from.ToHsv();
        var b = to.ToHsv();

        return new HsvColor(
            a.A + ((b.A - a.A) * progress),
            a.H + ((b.H - a.H) * progress),
            a.S + ((b.S - a.S) * progress),
            a.V + ((b.V - a.V) * progress)).ToRgb();
    }

    private static Bitmap Load(string name) =>
        new(AssetLoader.Open(new Uri($"avares://DarkInkTransition/Assets/Images/{name}.png")));

    private void OnDarkChanged(bool isDark)
    {
        _ramp.AnimateTo(isDark ? 1d : 0d, Duration);

        _lastTick = TimeSpan.Zero;
        _ticker?.Start();

        Apply();
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
        var value = _ramp.Value;

        // The two brushes live for the bar's life; the run only moves their colour, which redraws
        // without a new brush and a new binding push per frame.
        _backgroundInk.Color = LerpHsv(LightColor, DarkColor, BackgroundRun.Evaluate(value));
        _foregroundInk.Color = LerpHsv(LightColor, DarkColor, ForegroundRun.Evaluate(value));
        BarBackground ??= _backgroundInk;
        BarForeground ??= _foregroundInk;
        ToggleOpacity = IconOpacity.Evaluate(value);

        // Swapped at the half way point, which is inside the window where it is invisible.
        ToggleIcon = value > 0.5d ? Sun : Moon;
        ToggleMask = value > 0.5d ? SunMask : MoonMask;

        // The rule snaps rather than fading, as the original's does.
        RuleBrush = IsDark ? DarkRule : LightRule;
    }
}
