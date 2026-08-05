using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Indie3D.Controls;

/// <summary>
/// The grain laid over the whole page. Port of the noise image and the <c>BlendMask</c> round it.
/// </summary>
/// <remarks>
/// The original draws it at its own size rather than scaling it to the page, so the grain stays the
/// same size whatever the screen is. This is the one layer Avalonia can blend without help, since
/// it is a bitmap already.
/// </remarks>
public sealed class NoiseOverlay : Control
{
    /// <summary>
    /// How strongly the grain is laid over the page.
    /// </summary>
    private const double Strength = 0.24d;

    private static readonly Bitmap Noise =
        new(AssetLoader.Open(new Uri("avares://Indie3D/Assets/Images/noise.png")));

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        using var blend = context.PushRenderOptions(
            new RenderOptions { BitmapBlendingMode = BitmapBlendingMode.ColorDodge });
        using var opacity = context.PushOpacity(Strength);

        // BoxFit.none: the grain is drawn at its own size, centred, and clipped by the page.
        var origin = new Point(
            (Bounds.Width - Noise.Size.Width) / 2d,
            (Bounds.Height - Noise.Size.Height) / 2d);

        context.DrawImage(Noise, new Rect(origin, Noise.Size));
    }
}
