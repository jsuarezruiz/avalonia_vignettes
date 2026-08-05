using Avalonia;

namespace DrinkRewardsList.Models;

/// <summary>
/// Every size in the rewards header, derived from the one number the original derives them from:
/// the header's own height.
/// </summary>
/// <param name="Height">The header's height, a fifth of the screen.</param>
public sealed record HeaderMetrics(double Height)
{
    /// <summary>
    /// Gets the inset around the header's contents.
    /// </summary>
    public Thickness Padding => new(Height * 0.08d);

    /// <summary>
    /// Gets the size of the "My Rewards" title.
    /// </summary>
    public double TitleFontSize => Height * 0.13d;

    /// <summary>
    /// Gets the size of the star beside the points total.
    /// </summary>
    public double StarSize => Height * 0.2d;

    /// <summary>
    /// Gets the size of the points total.
    /// </summary>
    public double PointsFontSize => Height * 0.3d;

    /// <summary>
    /// Gets the size of the caption under the points total.
    /// </summary>
    public double BalanceFontSize => Height * 0.1d;

    /// <summary>
    /// Gets the room the list leaves for the header above it.
    /// </summary>
    public Thickness ListPadding => new(0d, Height + 10d, 0d, 40d);
}
