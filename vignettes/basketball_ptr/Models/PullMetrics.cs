using Avalonia;
using BasketballPullToRefresh.Controls;

namespace BasketballPullToRefresh.Models;

/// <summary>
/// The sizes of the pull area, all derived from the one number the original takes from the screen:
/// a third of its height, capped at 180.
/// </summary>
/// <param name="Extent">The height the pull is measured against.</param>
public sealed record PullMetrics(double Extent)
{
    /// <summary>The share of the screen the area is allowed to take.</summary>
    private const double ScreenShare = 0.325d;

    /// <summary>The tallest the area ever gets, however tall the screen is.</summary>
    private const double MaxExtent = 180d;

    /// <summary>Gets the area's height, which the pull can stretch to but not past.</summary>
    public double Height => Extent * BasketballHoop.MaxPull;

    /// <summary>Gets where the caption sits above the hoop.</summary>
    public Thickness CaptionMargin => new(0d, Extent * 0.08d, 0d, 0d);

    /// <summary>Works out the metrics for a screen of the given height.</summary>
    /// <param name="screenHeight">The height of the whole screen, app bar included.</param>
    public static PullMetrics ForScreen(double screenHeight) =>
        new(Math.Clamp(screenHeight * ScreenShare, 0d, MaxExtent));
}
