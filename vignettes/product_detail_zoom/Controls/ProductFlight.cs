using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using AvaloniaVignettes.Shared.Animation;

namespace ProductDetailZoom.Controls;

/// <summary>
/// What the shared element actually shows while it crosses: the speaker spinning through its sheet,
/// with the callouts and headline arriving over it. Port of
/// <c>product_details_hero_flight.dart</c>.
/// </summary>
/// <remarks>
/// The spin is over by 80% of the flight, leaving the last fifth for the overlay to finish settling,
/// so the speaker stops turning before the writing has quite stopped moving.
/// </remarks>
public sealed class ProductFlight : TemplatedControl
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<ProductFlight, double>(nameof(Progress));

    public static readonly StyledProperty<Bitmap?> SpriteSheetProperty =
        AvaloniaProperty.Register<ProductFlight, Bitmap?>(nameof(SpriteSheet));

    public static readonly DirectProperty<ProductFlight, double> SpriteFrameProperty =
        AvaloniaProperty.RegisterDirect<ProductFlight, double>(nameof(SpriteFrame), o => o.SpriteFrame);

    private const double LastFrame = 59d;

    private static readonly Easing Spin = new IntervalEasing(0d, 0.8d);

    private double _spriteFrame;

    static ProductFlight() =>
        ProgressProperty.Changed.AddClassHandler<ProductFlight>((x, e) =>
            x.SpriteFrame = LastFrame * Spin.Ease(Math.Clamp(e.GetNewValue<double>(), 0d, 1d)));

    /// <summary>
    /// Gets or sets how far through the flight this is, from 0 to 1.
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// Gets or sets the sheet the speaker is drawn from.
    /// </summary>
    public Bitmap? SpriteSheet
    {
        get => GetValue(SpriteSheetProperty);
        set => SetValue(SpriteSheetProperty, value);
    }

    /// <summary>
    /// Gets which frame of the spin to show.
    /// </summary>
    public double SpriteFrame
    {
        get => _spriteFrame;
        private set => SetAndRaise(SpriteFrameProperty, ref _spriteFrame, value);
    }
}
