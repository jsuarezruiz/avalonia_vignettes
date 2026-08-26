using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ParallaxTravelCardsHero.Models;

/// <summary>
/// One of the experience categories on the details page. Port of the strings
/// <c>_ExperiencesSection</c> builds its cards from.
/// </summary>
public sealed class Experience
{
    private const string ImageRoot = "avares://ParallaxTravelCardsHero/Assets/Images/Experiences";

    public Experience(string title)
    {
        Title = title;
        Image = new Bitmap(AssetLoader.Open(new Uri($"{ImageRoot}/{title.Replace(" ", string.Empty)}.png")));
    }

    /// <summary>
    /// Gets the label shown under the picture.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the picture at the top of the card.
    /// </summary>
    public Bitmap Image { get; }
}
