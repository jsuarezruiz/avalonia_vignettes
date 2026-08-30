using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ParticleSwipe.Effects;

/// <summary>
/// A strip of animation frames packed into one image, plus a tinted copy of that strip for every
/// colour asked for. Port of <c>sprite_sheet.dart</c> and of the colouring <c>drawAtlas</c> does.
/// </summary>
/// <remarks>
/// The original hands <c>drawAtlas</c> a colour per particle and blends it with the sprite, using
/// the frame purely as an alpha mask. Avalonia has no <c>drawAtlas</c>, so the same thing is done
/// once per colour instead of once per particle: the sheet is painted through as an opacity mask to
/// produce a tinted strip, which then draws as an ordinary image. A burst only ever uses two
/// colours, so this is two masked draws rather than several hundred.
/// </remarks>
public sealed class SpriteSheet
{
    private readonly Dictionary<uint, Bitmap> _tinted = [];
    private readonly Bitmap _source;

    /// <summary>
    /// Initializes a new sheet from <paramref name="uri"/>.
    /// </summary>
    /// <param name="uri">The sheet image.</param>
    /// <param name="length">How many frames it holds.</param>
    /// <param name="frameWidth">The width of a frame, in pixels.</param>
    /// <param name="frameHeight">The height of a frame; the image's height when zero.</param>
    public SpriteSheet(Uri uri, int length, int frameWidth, int frameHeight = 0)
    {
        _source = new Bitmap(AssetLoader.Open(uri));

        Length = length;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight == 0 ? (int)_source.PixelSize.Height : frameHeight;
    }

    /// <summary>
    /// Gets how many frames the sheet holds.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the width of one frame, in pixels.
    /// </summary>
    public int FrameWidth { get; }

    /// <summary>
    /// Gets the height of one frame, in pixels.
    /// </summary>
    public int FrameHeight { get; }

    /// <summary>
    /// Returns the region of the sheet holding frame <paramref name="index"/>.
    /// </summary>
    public Rect? GetFrame(int index)
    {
        if (index < 0 || index >= Length)
        {
            return null;
        }

        var columns = Math.Max(1, (int)_source.PixelSize.Width / FrameWidth);

        return new Rect(
            index % columns * FrameWidth,
            index / columns * FrameHeight,
            FrameWidth,
            FrameHeight);
    }

    /// <summary>
    /// Returns the sheet tinted to <paramref name="color"/>, including its alpha, building it the
    /// first time each colour is asked for.
    /// </summary>
    public Bitmap GetTinted(Color color)
    {
        if (_tinted.TryGetValue(color.ToUInt32(), out var cached))
        {
            return cached;
        }

        var size = _source.PixelSize;
        var bounds = new Rect(0d, 0d, size.Width, size.Height);
        var tinted = new RenderTargetBitmap(size);

        using (var context = tinted.CreateDrawingContext())
        using (context.PushOpacityMask(new ImageBrush(_source) { Stretch = Stretch.None }, bounds))
        {
            context.FillRectangle(new SolidColorBrush(color), bounds);
        }

        _tinted[color.ToUInt32()] = tinted;

        return tinted;
    }
}
