using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ParallaxTravelCardsHero.Controls;

/// <summary>
/// The artwork every copy of the scenery shares, loaded once. The flight puts two sceneries on
/// screen at the same time, so decoding these per instance would be wasted work.
/// </summary>
internal static class SceneryAssets
{
    private const string ImageRoot = "avares://ParallaxTravelCardsHero/Assets/Images";

    /// <summary>
    /// Gets the strip of ground the skyline stands on.
    /// </summary>
    public static Bitmap Ground { get; } = Load("Ground");

    /// <summary>
    /// Gets the road that unrolls from the horizon as the card opens.
    /// </summary>
    public static Bitmap Road { get; } = Load("Road");

    /// <summary>
    /// Gets the nearer, larger cloud.
    /// </summary>
    public static Bitmap CloudLarge { get; } = Load("CloudLarge");

    /// <summary>
    /// Gets the further, smaller cloud.
    /// </summary>
    public static Bitmap CloudSmall { get; } = Load("CloudSmall");

    /// <summary>
    /// Gets the tree, drawn four times at two sizes.
    /// </summary>
    public static Bitmap Tree { get; } = Load("Tree");

    /// <summary>
    /// Gets the leaf that tumbles across the scene.
    /// </summary>
    public static Bitmap Leaf { get; } = Load("BlowingLeaf");

    private static Bitmap Load(string name) => new(AssetLoader.Open(new Uri($"{ImageRoot}/{name}.png")));
}
