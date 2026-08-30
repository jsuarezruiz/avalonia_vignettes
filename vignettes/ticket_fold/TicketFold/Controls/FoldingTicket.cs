using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using TicketFold.Effects;

namespace TicketFold.Controls;

/// <summary>
/// A stack of <see cref="FoldSegment"/> panels that concertinas open and shut. Port of
/// <c>folding_ticket.dart</c>.
/// </summary>
/// <remarks>
/// The panels unfold one after another rather than together: the whole animation is a single
/// progress value, and each panel takes an equal slice of it, so the second panel only starts to
/// swing down once the first has landed. Each panel hinges on its own top edge and carries the
/// panels below it, which is what makes the last one travel furthest, exactly as a folded paper
/// ticket does.
/// <para>
/// The control reports a height that grows with the fold and clips to it, so the panels are revealed
/// through a window that opens with them. That height is the only thing the surrounding list sees.
/// </para>
/// </remarks>
public sealed class FoldingTicket : Panel
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<FoldingTicket, bool>(nameof(IsOpen));

    public static readonly StyledProperty<TimeSpan?> FoldDurationProperty =
        AvaloniaProperty.Register<FoldingTicket, TimeSpan?>(nameof(FoldDuration));

    private const double MillisecondsPerSegment = 400d;

    private readonly AnimationController _controller;

    private double _foldRatio;

    static FoldingTicket() =>
        IsOpenProperty.Changed.AddClassHandler<FoldingTicket>((x, _) => x.OnIsOpenChanged());

    public FoldingTicket()
    {
        ClipToBounds = true;
        _controller = new AnimationController(this, OnFoldProgressChanged);

        Children.CollectionChanged += (_, _) => UpdateSegmentStates();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the ticket is unfolded.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets how long the whole fold takes. When null it is 400 ms for every panel that has
    /// to move, matching <c>Duration(milliseconds: 400 * (entries.length - 1))</c>.
    /// </summary>
    public TimeSpan? FoldDuration
    {
        get => GetValue(FoldDurationProperty);
        set => SetValue(FoldDurationProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var closedHeight = 0d;
        var openHeight = 0d;
        var widest = 0d;

        foreach (var child in Children)
        {
            if (child is not FoldSegment segment)
            {
                child.Measure(availableSize);
                continue;
            }

            var height = GetSegmentHeight(segment);

            segment.Measure(new Size(availableSize.Width, height));

            widest = Math.Max(widest, segment.DesiredSize.Width);
            openHeight += height;

            if (closedHeight == 0d)
            {
                closedHeight = height;
            }
        }

        var width = double.IsInfinity(availableSize.Width) ? widest : availableSize.Width;

        // Only the first panel is on show when shut, so the window opens from that panel's height to
        // the height of all of them.
        var reveal = FlutterEasings.EaseOut.Ease(_foldRatio);

        return new Size(width, closedHeight + ((openHeight - closedHeight) * reveal));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var movingSegments = GetSegmentCount() - 1;
        var hingeX = finalSize.Width / 2d;

        // Each panel is transformed by its own fold and then by every fold above it, so the matrices
        // accumulate down the stack the same way the widgets nest in the original.
        var accumulated = Matrix.Identity;
        var top = 0d;
        var index = 0;

        foreach (var child in Children)
        {
            if (child is not FoldSegment segment)
            {
                child.Arrange(new Rect(finalSize));
                continue;
            }

            var height = GetSegmentHeight(segment);
            var ratio = GetSegmentRatio(index, movingSegments);

            segment.Arrange(new Rect(0d, top, finalSize.Width, height));

            accumulated = FoldTransform.Create(ratio, new Point(hingeX, top)) * accumulated;

            // The accumulated matrix is expressed in this control's space, while a render transform
            // acts on the panel's own; shifting it by the panel's position bridges the two.
            var offset = new Vector(0d, top);

            SetSegmentTransform(
                segment,
                Matrix.CreateTranslation(offset) * accumulated * Matrix.CreateTranslation(-offset));

            top += height;
            index++;
        }

        return finalSize;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _controller.Stop();
    }

    private static void SetSegmentTransform(FoldSegment segment, Matrix matrix)
    {
        segment.RenderTransformOrigin = RelativePoint.TopLeft;

        if (segment.RenderTransform is MatrixTransform existing)
        {
            existing.Matrix = matrix;
            return;
        }

        segment.RenderTransform = new MatrixTransform(matrix);
    }

    /// <summary>
    /// Returns how far the panel at <paramref name="index"/> has unfolded. The progress is shared
    /// out so panel n runs over the nth slice of it, and panel 0 always reads as flat, which is what
    /// pins the top of the ticket in place.
    /// </summary>
    private double GetSegmentRatio(int index, int movingSegments) =>
        Math.Clamp((_foldRatio * movingSegments) + 1d - index, 0d, 1d);

    // Picks the face each panel shows, and drops the panels that are hidden behind one still folded
    // over. The original stops building its tree at that point, "don't build a stack if it isn't
    // needed", and without the same cut the panels underneath show through the top of the ticket.
    private void UpdateSegmentStates()
    {
        var movingSegments = GetSegmentCount() - 1;
        var isRendered = true;
        var index = 0;

        foreach (var child in Children)
        {
            if (child is not FoldSegment segment)
            {
                continue;
            }

            var ratio = GetSegmentRatio(index, movingSegments);

            segment.IsVisible = isRendered;

            if (isRendered)
            {
                segment.ApplyFoldRatio(ratio);
            }

            isRendered &= ratio > 0.5d;
            index++;
        }
    }

    private static double GetSegmentHeight(FoldSegment segment) =>
        double.IsNaN(segment.Height) ? segment.DesiredSize.Height : segment.Height;

    private int GetSegmentCount()
    {
        var count = 0;

        foreach (var child in Children)
        {
            if (child is FoldSegment)
            {
                count++;
            }
        }

        return count;
    }

    private void OnIsOpenChanged()
    {
        _controller.Duration = FoldDuration
            ?? TimeSpan.FromMilliseconds(MillisecondsPerSegment * (GetSegmentCount() - 1));

        if (IsOpen)
        {
            _controller.Forward();
        }
        else
        {
            _controller.Reverse();
        }
    }

    private void OnFoldProgressChanged(double progress)
    {
        // The curve is applied to the progress rather than to the interpolation, so closing retraces
        // the same shape backwards instead of mirroring it.
        _foldRatio = FlutterEasings.EaseInQuad.Ease(progress);

        UpdateSegmentStates();
        InvalidateMeasure();
    }
}
