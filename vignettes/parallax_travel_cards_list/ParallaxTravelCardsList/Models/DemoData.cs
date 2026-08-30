using Avalonia.Media;
using AvaloniaVignettes.Shared.Models;

namespace ParallaxTravelCardsList.Models;

/// <summary>
/// The destinations and hotels driving the vignette. Port of <c>demo_data.dart</c>'s <c>DemoData</c>.
/// </summary>
public static class DemoData
{
    /// <summary>
    /// The number of pages the list holds. Flutter's <c>PageView.builder</c> is given
    /// <c>itemCount: 8</c> and wraps the three cities with a modulo.
    /// </summary>
    public const int PageCount = 8;

    /// <summary>
    /// The page shown when the vignette opens, matching <c>initialPage: 1</c>.
    /// </summary>
    public const int InitialPage = 1;

    private static readonly Lazy<IReadOnlyList<City>> LazyCities = new(CreateCities);

    /// <summary>
    /// Gets the three destinations.
    /// </summary>
    public static IReadOnlyList<City> Cities => LazyCities.Value;

    /// <summary>
    /// Gets the eight pages of the list, cycling through <see cref="Cities"/> the way
    /// <c>cities[itemIndex % cities.length]</c> does in the Flutter original.
    /// </summary>
    public static IReadOnlyList<City> Pages { get; } =
        Enumerable.Range(0, PageCount).Select(i => Cities[i % Cities.Count]).ToArray();

    private static IReadOnlyList<City> CreateCities() =>
    [
        new City(
            name: "Pisa",
            title: "Pisa, Italy",
            description: "Discover a beautiful city where ancient and modern meet",
            color: Color.Parse("#ffdee5cf"),
            hotels:
            [
                new Hotel("Hotel Bologna", Rating: 4, Reviews: 201, Price: 120),
                new Hotel("Tree House", Rating: 5, Reviews: 85, Price: 98),
                new Hotel("Allegroitalia Pisa Tower Plaza", Rating: 4, Reviews: 128, Price: 119),
            ]),
        new City(
            name: "Budapest",
            title: "Budapest, Hungary",
            description: "Meet the city with rich history and indescribable culture",
            color: Color.Parse("#ffdaf3f7"),
            hotels:
            [
                new Hotel("Hotel Estilo Budapest", Rating: 5, Reviews: 762, Price: 87),
                new Hotel("Danubius Hotel", Rating: 3, Reviews: 3122, Price: 196),
                new Hotel("Golden Budapest Condominium", Rating: 5, Reviews: 213, Price: 217),
            ]),
        new City(
            name: "London",
            title: "London, England",
            description: "A diverse and exciting city with the world’s best sights and attractions!",
            color: Color.Parse("#fff9d9e2"),
            hotels:
            [
                new Hotel("InterContinental London Hotel", Rating: 3, Reviews: 1624, Price: 418),
                new Hotel("Brick Lane Hotel", Rating: 4, Reviews: 101, Price: 101),
                new Hotel("Park Villa Boutique House", Rating: 5, Reviews: 161, Price: 128),
            ]),
    ];
}
