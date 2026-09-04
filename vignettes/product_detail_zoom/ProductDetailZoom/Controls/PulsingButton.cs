using Avalonia;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Animation;

namespace ProductDetailZoom.Controls;

/// <summary>
/// A round button with a halo that swells out of it and fades, over and over. Port of
/// <c>pulsing_button.dart</c>.
/// </summary>
/// <remarks>
/// One 1200 ms loop drives both the halo and the button's own fill: the halo grows from half size to
/// full while fading from 70% to nothing, and the fill breathes between 70% and 90% opacity on the
/// same clock, so the button itself pulses very slightly in step with the ring leaving it.
/// </remarks>
public sealed class PulsingButton : Button
{
    public static readonly StyledProperty<Avalonia.Media.Geometry?> IconProperty =
        AvaloniaProperty.Register<PulsingButton, Avalonia.Media.Geometry?>(nameof(Icon));

    public static readonly DirectProperty<PulsingButton, double> HaloScaleProperty =
        AvaloniaProperty.RegisterDirect<PulsingButton, double>(nameof(HaloScale), o => o.HaloScale);

    public static readonly DirectProperty<PulsingButton, double> HaloOpacityProperty =
        AvaloniaProperty.RegisterDirect<PulsingButton, double>(nameof(HaloOpacity), o => o.HaloOpacity);

    public static readonly DirectProperty<PulsingButton, double> FillOpacityProperty =
        AvaloniaProperty.RegisterDirect<PulsingButton, double>(nameof(FillOpacity), o => o.FillOpacity);

    private static readonly TimeSpan PulseDuration = TimeSpan.FromMilliseconds(1200);

    private FrameTicker? _ticker;
    private double _haloScale = 0.5d;
    private double _haloOpacity = 0.7d;
    private double _fillOpacity = 0.7d;

    /// <summary>
    /// Gets or sets the glyph drawn in the middle, on the usual 24 unit grid.
    /// </summary>
    public Avalonia.Media.Geometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets how far the halo has swelled, from a half to full size.
    /// </summary>
    public double HaloScale
    {
        get => _haloScale;
        private set => SetAndRaise(HaloScaleProperty, ref _haloScale, value);
    }

    /// <summary>
    /// Gets how visible the halo is; it fades right out as it swells.
    /// </summary>
    public double HaloOpacity
    {
        get => _haloOpacity;
        private set => SetAndRaise(HaloOpacityProperty, ref _haloOpacity, value);
    }

    /// <summary>
    /// Gets the button's own fill opacity, which breathes with the halo.
    /// </summary>
    public double FillOpacity
    {
        get => _fillOpacity;
        private set => SetAndRaise(FillOpacityProperty, ref _fillOpacity, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, Advance);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private void Advance(TimeSpan elapsed)
    {
        // The loop restarts rather than reversing, so the halo always travels outwards.
        var progress = elapsed.TotalMilliseconds % PulseDuration.TotalMilliseconds / PulseDuration.TotalMilliseconds;

        HaloScale = 0.5d + (0.5d * progress);
        HaloOpacity = 0.7d * (1d - progress);
        FillOpacity = 0.7d + (0.2d * progress);
    }
}
