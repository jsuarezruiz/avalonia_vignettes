using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Models;

namespace ParallaxTravelCardsList.Models;

/// <summary>
/// A travel destination and the three artwork layers that make up its parallax scene.
/// Port of <c>demo_data.dart</c>'s <c>City</c>.
/// </summary>
public sealed class City
{
    private const string ImageRoot = "avares://ParallaxTravelCardsList/Assets/Images";

    public City(string name, string title, string description, Color color, IReadOnlyList<Hotel> hotels)
    {
        Name = name;
        Title = title;
        Description = description;
        Background = new ImmutableSolidColorBrush(color);
        Hotels = hotels;

        BackImage = LoadLayer("Back");
        MiddleImage = LoadLayer("Middle");
        FrontImage = LoadLayer("Front");
    }

    /// <summary>
    /// Gets the asset folder name, for example <c>Budapest</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the headline shown on the card.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the supporting copy shown on the card.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the pastel brush filling the card behind the artwork.
    /// </summary>
    public IBrush Background { get; }

    /// <summary>
    /// Gets the hotel recommendations for this destination.
    /// </summary>
    public IReadOnlyList<Hotel> Hotels { get; }

    /// <summary>
    /// Gets the rearmost parallax layer, which travels furthest as the card is dragged.
    /// </summary>
    public Bitmap BackImage { get; }

    /// <summary>
    /// Gets the middle parallax layer.
    /// </summary>
    public Bitmap MiddleImage { get; }

    /// <summary>
    /// Gets the frontmost parallax layer, which travels least as the card is dragged.
    /// </summary>
    public Bitmap FrontImage { get; }

    private Bitmap LoadLayer(string layer) =>
        new(AssetLoader.Open(new Uri($"{ImageRoot}/{Name}/{Name}-{layer}.png")));
}
