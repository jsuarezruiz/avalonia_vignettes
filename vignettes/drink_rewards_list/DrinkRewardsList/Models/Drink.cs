namespace DrinkRewardsList.Models;

/// <summary>
/// One drink on the rewards list. Port of <c>DrinkData</c>.
/// </summary>
/// <param name="Title">The drink's name.</param>
/// <param name="RequiredPoints">How many points it costs.</param>
/// <param name="Image">The artwork's file name.</param>
public sealed record Drink(string Title, int RequiredPoints, string Image)
{
    /// <summary>
    /// Gets the drink's name as the card prints it.
    /// </summary>
    public string Label => Title.ToUpperInvariant();

}
