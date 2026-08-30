using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaVignettes.Shared.Models;

namespace ParallaxTravelCardsHero.Models;

/// <summary>
/// A travel destination, the three artwork layers that make up its skyline, and the copy shown for
/// it on both pages. Port of <c>demo_data.dart</c>'s <c>CityData</c>.
/// </summary>
public sealed class City
{
    private const string ImageRoot = "avares://ParallaxTravelCardsHero/Assets/Images";

    public City(
        string name,
        string title,
        string description,
        string information,
        Color color,
        IReadOnlyList<Hotel> hotels)
    {
        Name = name;
        Title = title;
        Description = description;
        Information = information;
        Color = color;
        Hotels = hotels;

        BackImage = LoadLayer("Back");
        MiddleImage = LoadLayer("Middle");
        FrontImage = LoadLayer("Front");
    }

    /// <summary>
    /// Gets the asset folder name, for example <c>Paris</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the headline shown on the card and at the top of the details.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the supporting copy shown on the card.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the longer copy shown on the details page.
    /// </summary>
    public string Information { get; }

    /// <summary>
    /// Gets the pastel colour the card starts out filled with.
    /// </summary>
    public Color Color { get; }

    /// <summary>
    /// Gets the hotel recommendations for this destination.
    /// </summary>
    public IReadOnlyList<Hotel> Hotels { get; }

    /// <summary>
    /// Gets the rearmost skyline layer.
    /// </summary>
    public Bitmap BackImage { get; }

    /// <summary>
    /// Gets the middle skyline layer.
    /// </summary>
    public Bitmap MiddleImage { get; }

    /// <summary>
    /// Gets the frontmost skyline layer, the one carrying the landmark.
    /// </summary>
    public Bitmap FrontImage { get; }

    private Bitmap LoadLayer(string layer) =>
        new(AssetLoader.Open(new Uri($"{ImageRoot}/{Name}/{Name}-{layer}.png")));
}
