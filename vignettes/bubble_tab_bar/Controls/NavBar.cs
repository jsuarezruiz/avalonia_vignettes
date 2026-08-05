using Avalonia.Controls;

namespace BubbleTabBar.Controls;

/// <summary>
/// A bottom tab bar of pills, with the page for the picked tab above it. Port of
/// <c>navbar.dart</c> together with the demo shell that hosts it.
/// </summary>
/// <remarks>
/// This is a <see cref="TabControl"/> because that is exactly what the vignette is: a strip of tabs
/// that selects between pages. Deriving from it means selection, keyboard support and the content
/// swap all come for free, and what is left to write is only the part that makes the vignette its
/// own thing — the pills, and the cross-fade between pages.
/// </remarks>
public sealed class NavBar : TabControl
{
    /// <summary>
    /// Creates a pill for each tab, so tabs can be declared as plain content.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        new NavBarButton();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not NavBarButton;
    }
}
