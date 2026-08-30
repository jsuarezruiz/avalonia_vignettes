using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ProductDetailZoom.Views;

/// <summary>
/// The artwork both pages share, loaded once.
/// </summary>
internal static class ProductAssets
{
    private const string ImageRoot = "avares://ProductDetailZoom/Assets/Images";

    /// <summary>
    /// Gets the sheet the speaker's spin is cut from: 10 columns of 360x500 frames.
    /// </summary>
    public static Bitmap SpriteSheet { get; } = Load("speaker_sprite");

    /// <summary>
    /// Gets the bag in the app bar.
    /// </summary>
    public static Bitmap Bag { get; } = Load("shopping_bag");

    /// <summary>
    /// Works out the size the speaker is drawn at, from <c>_initFrameValues</c>. A tall screen gives
    /// it almost the full width; a squarer one gets a proportionally smaller frame.
    /// </summary>
    public static (double Width, double Height) FrameFor(double screenWidth, double screenHeight)
    {
        if (screenWidth <= 0d)
        {
            return (0d, 0d);
        }

        var ratio = screenHeight / screenWidth;
        var frameRatio = ratio < 2d ? ratio / 2d : 0.95d;
        var width = screenWidth * frameRatio;

        return (width, 500d * width / 360d);
    }

    private static Bitmap Load(string name) => new(AssetLoader.Open(new Uri($"{ImageRoot}/{name}.png")));
}
