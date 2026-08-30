using Avalonia.Platform;
using SkiaSharp;

namespace SparkleParty.Effects;

/// <summary>
/// The sheet the sparkles are cut from, and where each frame sits in it. Port of
/// <c>sprite_sheet.dart</c>.
/// </summary>
/// <remarks>
/// The frames are white shapes with an alpha channel. They arrive as the paint's shader and the
/// particles' own colours are blended into them, so the sheet supplies the shape and the effect
/// supplies the hue.
/// </remarks>
public sealed class SpriteSheet
{
    private readonly SKRect[] _frames;

    public SpriteSheet(string asset, int length, int frameWidth, int frameHeight)
    {
        using var stream = AssetLoader.Open(new Uri($"avares://SparkleParty/Assets/Images/{asset}.png"));

        Image = SKImage.FromEncodedData(SKData.Create(stream));
        Length = length;

        var columns = Math.Max(1, Image.Width / frameWidth);

        _frames = new SKRect[length];

        for (var i = 0; i < length; i++)
        {
            var x = i % columns * frameWidth;
            var y = i / columns * frameHeight;

            _frames[i] = new SKRect(x, y, x + frameWidth, y + frameHeight);
        }
    }

    /// <summary>
    /// Gets the sheet itself.
    /// </summary>
    public SKImage Image { get; }

    /// <summary>
    /// Gets how many frames the sheet holds.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets where a frame sits in the sheet.
    /// </summary>
    public SKRect Frame(int index) => _frames[Math.Clamp(index, 0, Length - 1)];
}
