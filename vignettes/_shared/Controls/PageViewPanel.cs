using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// The items panel used by <see cref="PageView"/>. Lays children out edge to edge in a horizontal
/// strip of fixed-width pages, offset by the owner's <see cref="PageView.ScrollPixels"/>.
/// </summary>
/// <remarks>
/// Matching Flutter's <c>PageView</c>, a page is <c>viewport * viewportFraction</c> wide and the
/// strip is padded by half the leftover viewport, so page <c>n</c> is centred in the viewport when
/// the scroll offset is <c>n * pageWidth</c>.
/// </remarks>
public sealed class PageViewPanel : Panel
{
    private PageView? _owner;

    private PageView? Owner => _owner ??= this.FindAncestorOfType<PageView>();

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var viewport = double.IsInfinity(availableSize.Width) ? 0d : availableSize.Width;
        var pageWidth = PageView.GetPageWidth(viewport, Owner?.ViewportFraction ?? 1d);
        var childConstraint = new Size(pageWidth, availableSize.Height);

        var height = 0d;

        foreach (var child in Children)
        {
            child.Measure(childConstraint);
            height = Math.Max(height, child.DesiredSize.Height);
        }

        return new Size(viewport, double.IsInfinity(availableSize.Height) ? height : availableSize.Height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var owner = Owner;
        var pageWidth = PageView.GetPageWidth(finalSize.Width, owner?.ViewportFraction ?? 1d);
        var leadingPadding = (finalSize.Width - pageWidth) / 2d;
        var scroll = owner?.ScrollPixels ?? 0d;

        for (var i = 0; i < Children.Count; i++)
        {
            var x = leadingPadding + (i * pageWidth) - scroll;
            Children[i].Arrange(new Rect(x, 0d, pageWidth, finalSize.Height));
        }

        return finalSize;
    }
}
