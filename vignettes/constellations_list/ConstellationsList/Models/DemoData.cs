namespace ConstellationsList.Models;

/// <summary>
/// The constellations the vignette ships with. Port of <c>DemoData</c>.
/// </summary>
/// <remarks>Five of them, listed three times over, which is what gives the list its length.</remarks>
public static class DemoData
{
    private const int Repeats = 3;

    /// <summary>
    /// Gets the list, in order.
    /// </summary>
    public static IReadOnlyList<Constellation> Constellations { get; } = Build();

    private static Constellation[] Build()
    {
        Constellation[] set =
        [
            new("Aries", "The Ram", "Aries"),
            new("Cassiopeia", "The Queen", "Cassiopeia"),
            new("Camelopardalis", "The Giraffe", "Camelopardalis"),
            new("Cetus", "The Whale", "Cetus"),
            new("Pisces", "The Fishes", "Pisces"),
        ];

        return [.. Enumerable.Range(0, Repeats).SelectMany(_ => set)];
    }
}
