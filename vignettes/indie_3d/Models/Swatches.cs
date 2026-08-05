using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Indie3D.Models;

/// <summary>
/// One pixel images of flat colours, which is how a fill reaches a blend mode: Avalonia blends
/// bitmaps and nothing else, so anything that has to blend is drawn as a colour through a clip.
/// </summary>
public static class Swatches
{
    private static readonly Dictionary<uint, Bitmap> Cache = [];

    /// <summary>
    /// Gets a one pixel white image.
    /// </summary>
    public static Bitmap White { get; } = Of(Colors.White);

    /// <summary>
    /// Gets a one pixel image of <paramref name="colour"/>.
    /// </summary>
    public static Bitmap Of(Color colour)
    {
        var key = colour.ToUInt32();

        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var bitmap = new WriteableBitmap(new PixelSize(1, 1), new Vector(96d, 96d), PixelFormat.Bgra8888);

        using (var buffer = bitmap.Lock())
        {
            // Color packs as ARGB, which little endian memory reads back as the BGRA the buffer wants.
            Marshal.WriteInt32(buffer.Address, unchecked((int)key));
        }

        Cache[key] = bitmap;

        return bitmap;
    }
}
