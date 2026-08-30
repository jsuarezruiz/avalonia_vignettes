using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;

namespace PlantForms.Controls;

/// <summary>
/// The form's pages, each sliding up over the last and nudging it further up the screen. Port of
/// <c>stack_pages_route.dart</c>.
/// </summary>
/// <remarks>
/// Every page's offset is a function of one number, <see cref="Position"/>, so a push and a pop are
/// the same animation run in opposite directions. A page still to come sits one screen below;
/// pages already passed stack up at a twentieth of the screen each, which is what leaves their
/// titles showing above the card in front.
/// </remarks>
public sealed class FormCardStack : Panel
{
    private const double StackStep = 0.05d;

    private static readonly TimeSpan SlideDuration = TimeSpan.FromMilliseconds(300);

    private readonly AnimationController _slide;

    private double _from;
    private double _to;
    private double _position;

    public FormCardStack()
    {
        _slide = new AnimationController(this, OnSlideProgressChanged) { Duration = SlideDuration };

        ClipToBounds = true;
    }

    /// <summary>
    /// Gets the page currently on top.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Gets whether a page can still be popped.
    /// </summary>
    public bool CanPop => Index > 0;

    // Gets how far through the stack the pages have travelled. Whole numbers are settled states,
    // anything between is mid slide.
    private double Position
    {
        get => _position;
        set
        {
            _position = value;

            UpdateOffsets(Bounds.Height);
        }
    }

    /// <summary>
    /// Slides the next page up over the current one.
    /// </summary>
    public void Push()
    {
        if (Index + 1 < Children.Count)
        {
            SlideTo(Index + 1);
        }
    }

    /// <summary>
    /// Slides the top page back down.
    /// </summary>
    public void Pop()
    {
        if (CanPop)
        {
            SlideTo(Index - 1);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            child.Arrange(new Rect(finalSize));
        }

        // Bounds is only set once arranging is over, so the height comes from the pass itself.
        UpdateOffsets(finalSize.Height);

        return finalSize;
    }

    private void SlideTo(int index)
    {
        Index = index;

        _from = Position;
        _to = index;

        _slide.SetValue(0d);
        _slide.Forward();
    }

    private void OnSlideProgressChanged(double progress) =>
        Position = _from + ((_to - _from) * progress);

    private void UpdateOffsets(double height)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var depth = Position - i;

            // Still to come: one screen below, closing linearly. Already arrived: stacked up, and
            // eased so a page settles into the pile rather than sliding evenly into it.
            var offset = depth < 0d
                ? -depth * height
                : -(Math.Floor(depth) + FlutterEasings.EaseInCubic.Ease(depth - Math.Floor(depth)))
                    * StackStep * height;

            Children[i].RenderTransform = new TranslateTransform(0d, offset);
            Children[i].IsHitTestVisible = i == Index;
        }
    }
}
