namespace ConstellationsList.Models;

/// <summary>
/// One constellation in the guide. Port of <c>ConstellationData</c>.
/// </summary>
/// <param name="Title">Its name.</param>
/// <param name="Subtitle">What it depicts.</param>
/// <param name="Image">The stem of its artwork's file names.</param>
public sealed record Constellation(string Title, string Subtitle, string Image)
{
    /// <summary>
    /// Gets the star chart's location.
    /// </summary>
    public Uri ChartUri => Asset("Constellation");

    /// <summary>
    /// Gets the lettering overlay's location.
    /// </summary>
    public Uri LabelUri => Asset("Text");

    private Uri Asset(string kind) =>
        new($"avares://ConstellationsList/Assets/Images/{Image}-{kind}@2x.png");
}
