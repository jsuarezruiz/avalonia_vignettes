using Avalonia.Media;
using AvaloniaVignettes.Shared.Models;

namespace ParallaxTravelCardsHero.Models;

/// <summary>
/// The single destination this vignette ships with. Port of <c>demo_data.dart</c>'s
/// <c>DemoData</c>.
/// </summary>
public static class DemoData
{
    /// <summary>
    /// Gets the destination the demo opens on.
    /// </summary>
    public static City City { get; } = new(
        name: "Paris",
        title: "Paris, France",
        description: "Get ready to explore the city of love filled with romantic scenery and experiences.",
        information: "Paris, located along the Seine River, in the north-central part of France. For centuries, "
                     + "Paris has been one of the world’s most important and attractive cities.",
        color: Color.FromRgb(0xFD, 0xEE, 0xD5),
        hotels:
        [
            new Hotel("Shangri-La Hotel Paris", Rating: 5, Reviews: 201, Price: 593),
            new Hotel("Hôtel Trinité Haussmann", Rating: 3, Reviews: 133, Price: 391),
            new Hotel("Maison Breguet", Rating: 4, Reviews: 128, Price: 399),
        ]);

    /// <summary>
    /// Gets the three experience categories shown on the details page.
    /// </summary>
    public static IReadOnlyList<Experience> Experiences { get; } =
    [
        new Experience("Arts"),
        new Experience("Food And Drink"),
        new Experience("Classes"),
    ];
}
