namespace DrinkRewardsList.Models;

/// <summary>
/// The rewards the vignette ships with. Port of <c>DemoData</c>.
/// </summary>
public static class DemoData
{
    /// <summary>
    /// How many points this customer has earned. Enough for the first two drinks, which is what
    /// makes some cards fill to the brim and others only part way.
    /// </summary>
    public const int EarnedPoints = 150;

    /// <summary>
    /// Gets the drinks, cheapest first.
    /// </summary>
    public static IReadOnlyList<Drink> Drinks { get; } =
    [
        new Drink("Coffee", 100, "Coffee.png"),
        new Drink("Tea", 150, "Tea.png"),
        new Drink("Latte", 250, "Latte.png"),
        new Drink("Frappuccino", 350, "Frappuccino.png"),
        new Drink("Pressed Juice", 450, "Juice.png"),
    ];

    /// <summary>
    /// Returns where <paramref name="drink"/> sits in the list, or -1.
    /// </summary>
    public static int IndexOf(Drink drink)
    {
        for (var i = 0; i < Drinks.Count; i++)
        {
            if (Equals(Drinks[i], drink))
            {
                return i;
            }
        }

        return -1;
    }
}
