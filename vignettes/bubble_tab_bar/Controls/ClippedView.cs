using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace BubbleTabBar.Controls;

/// <summary>
/// Gives its child all the room it asks for on one axis and then shows only as much of it as fits.
/// Port of <c>clipped_view.dart</c>.
/// </summary>
/// <remarks>
/// The original reaches for a scroll view with scrolling switched off, purely to get that
/// measure-loose-then-clip behaviour. Plain clipping is not the same thing: a child measured against
/// the narrow box would wrap or compress instead of overflowing, and the collapsed tab would show a
/// squeezed label rather than no label at all.
/// </remarks>
public sealed class ClippedView : Decorator
{
    public static readonly StyledProperty<Orientation> ClipDirectionProperty =
        AvaloniaProperty.Register<ClippedView, Orientation>(nameof(ClipDirection), Orientation.Horizontal);

    public ClippedView() => ClipToBounds = true;

    /// <summary>
    /// Gets or sets the axis the child is allowed to overflow along.
    /// </summary>
    public Orientation ClipDirection
    {
        get => GetValue(ClipDirectionProperty);
        set => SetValue(ClipDirectionProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is not { } child)
        {
            return default;
        }

        var horizontal = ClipDirection == Orientation.Horizontal;

        child.Measure(horizontal
            ? new Size(double.PositiveInfinity, availableSize.Height)
            : new Size(availableSize.Width, double.PositiveInfinity));

        // Like a viewport: take what is offered along the clipped axis, hug the child across it.
        var width = horizontal && !double.IsInfinity(availableSize.Width)
            ? availableSize.Width
            : child.DesiredSize.Width;

        var height = !horizontal && !double.IsInfinity(availableSize.Height)
            ? availableSize.Height
            : child.DesiredSize.Height;

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is not { } child)
        {
            return finalSize;
        }

        var horizontal = ClipDirection == Orientation.Horizontal;

        child.Arrange(new Rect(
            0d,
            0d,
            horizontal ? child.DesiredSize.Width : finalSize.Width,
            horizontal ? finalSize.Height : child.DesiredSize.Height));

        return finalSize;
    }
}
