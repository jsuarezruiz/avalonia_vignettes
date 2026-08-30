using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SpendingTracker.Controls;

/// <summary>
/// The white halo behind a selected data point, as a bitmap so it can be added to what is under it.
/// </summary>
/// <remarks>
/// The original paints the halo with <c>BlendMode.plus</c>, and Avalonia's blend modes apply to
/// bitmaps and to nothing else, so the radial gradient is filled into one rather than drawn as a
/// shape. Its strength has to be baked in for the same reason: pushing an opacity round the draw
/// would put it behind a layer, and the layer would composite normally and lose the addition. The
/// ramp is written out by hand, which also pins the interpolation down exactly: white through to a
/// transparent black, unpremultiplied, the way Skia reads the original's two stops.
/// </remarks>
internal sealed class GlowSprite
{
    private const int Size = 64;

    private readonly WriteableBitmap _bitmap =
        new(new PixelSize(Size, Size), new Vector(96d, 96d), PixelFormat.Bgra8888, AlphaFormat.Premul);

    private readonly int[] _pixels = new int[Size * Size];

    private double _strength = double.NaN;

    /// <summary>
    /// Gets the halo at a given strength, redrawing it only when that changes.
    /// </summary>
    public Bitmap For(double strength)
    {
        if (strength != _strength)
        {
            _strength = strength;

            Fill(strength);
        }

        return _bitmap;
    }

    private void Fill(double strength)
    {
        const double radius = Size / 2d;

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var dx = x + 0.5d - radius;
                var dy = y + 0.5d - radius;
                var distance = Math.Min(1d, Math.Sqrt((dx * dx) + (dy * dy)) / radius);
                var alpha = strength * (1d - distance);

                // Premultiplied, and the colour fades to black as well as to nothing, so the
                // channels carry the falloff twice over.
                var channel = (int)Math.Round(alpha * (1d - distance) * 255d);

                _pixels[(y * Size) + x] =
                    ((int)Math.Round(alpha * 255d) << 24) | (channel << 16) | (channel << 8) | channel;
            }
        }

        using var buffer = _bitmap.Lock();

        for (var y = 0; y < Size; y++)
        {
            Marshal.Copy(_pixels, y * Size, buffer.Address + (y * buffer.RowBytes), Size);
        }
    }
}
