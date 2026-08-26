using Avalonia;
using Avalonia.Controls;

namespace SpendingTracker;

/// <summary>
/// The scale everything is laid out against. Port of <c>main.dart</c>'s <c>appScale</c>.
/// </summary>
/// <remarks>
/// The original divides the screen height by 480 and multiplies its metrics by the result, so the
/// chart and the category rings grow with the phone. It reads the height off a <c>MediaQuery</c>,
/// which is a lookup through the widget tree, and the top level is the same lookup here. Reading it
/// during measure and render rather than caching it also means a resized window corrects itself.
/// </remarks>
internal static class AppScale
{
    private const double DesignHeight = 480d;

    /// <summary>
    /// Gets the scale <paramref name="visual"/> is being shown at.
    /// </summary>
    public static double Of(Visual visual) =>
        (TopLevel.GetTopLevel(visual)?.ClientSize.Height ?? DesignHeight) / DesignHeight;
}
