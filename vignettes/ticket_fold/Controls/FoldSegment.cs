using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TicketFold.Controls;

/// <summary>
/// One panel of a <see cref="FoldingTicket"/>, printed on both sides. Port of <c>FoldEntry</c>.
/// </summary>
/// <remarks>
/// Which face you see is decided by how far the panel has turned: past halfway it is the front,
/// before that the back. The back is mirrored vertically, because a panel folded over its own top
/// edge arrives upside down and the artwork has to be righted again, the same reason
/// <c>FoldEntry</c>'s constructor wraps its back in a half turn.
/// </remarks>
public sealed class FoldSegment : Control
{
    public static readonly StyledProperty<Control?> FrontProperty =
        AvaloniaProperty.Register<FoldSegment, Control?>(nameof(Front));

    public static readonly StyledProperty<Control?> BackProperty =
        AvaloniaProperty.Register<FoldSegment, Control?>(nameof(Back));

    private const double FaceFlipRatio = 0.5d;

    static FoldSegment()
    {
        FrontProperty.Changed.AddClassHandler<FoldSegment>((x, e) => x.OnFaceChanged(e, isBack: false));
        BackProperty.Changed.AddClassHandler<FoldSegment>((x, e) => x.OnFaceChanged(e, isBack: true));
    }

    /// <summary>
    /// Gets or sets the face shown once the panel has turned past halfway.
    /// </summary>
    public Control? Front
    {
        get => GetValue(FrontProperty);
        set => SetValue(FrontProperty, value);
    }

    /// <summary>
    /// Gets or sets the face shown while the panel is still folded over.
    /// </summary>
    public Control? Back
    {
        get => GetValue(BackProperty);
        set => SetValue(BackProperty, value);
    }

    /// <summary>
    /// Shows the face that belongs to <paramref name="ratio"/>.
    /// </summary>
    /// <param name="ratio">How far the panel has unfolded, from 0 folded to 1 flat.</param>
    internal void ApplyFoldRatio(double ratio)
    {
        var isFrontFacing = ratio >= FaceFlipRatio;

        if (Front is { } front)
        {
            front.IsVisible = isFrontFacing;
        }

        if (Back is { } back)
        {
            back.IsVisible = !isFrontFacing;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Front?.Measure(availableSize);
        Back?.Measure(availableSize);

        // The panel is sized by the ticket, which always hands down a fixed height; the faces are
        // only measured here so they have a desired size if it ever has to fall back to theirs.
        var width = double.IsInfinity(availableSize.Width)
            ? Math.Max(Front?.DesiredSize.Width ?? 0d, Back?.DesiredSize.Width ?? 0d)
            : availableSize.Width;

        var height = double.IsInfinity(availableSize.Height)
            ? Math.Max(Front?.DesiredSize.Height ?? 0d, Back?.DesiredSize.Height ?? 0d)
            : availableSize.Height;

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var bounds = new Rect(finalSize);

        Front?.Arrange(bounds);
        Back?.Arrange(bounds);

        return finalSize;
    }

    private void OnFaceChanged(AvaloniaPropertyChangedEventArgs e, bool isBack)
    {
        if (e.OldValue is Control old)
        {
            LogicalChildren.Remove(old);
            VisualChildren.Remove(old);
        }

        if (e.NewValue is not Control added)
        {
            return;
        }

        if (isBack)
        {
            added.RenderTransform = new ScaleTransform(1d, -1d);
            added.RenderTransformOrigin = RelativePoint.Center;
        }

        LogicalChildren.Add(added);
        VisualChildren.Add(added);
    }
}
