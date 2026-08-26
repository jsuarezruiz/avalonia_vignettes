using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaVignettes.Shared.Animation;
using GooeyEdge.Effects;

namespace GooeyEdge.Controls;

/// <summary>
/// Swipes between full-bleed pages, revealing the incoming one through a liquid edge that follows
/// the pointer. Port of <c>gooey_carousel.dart</c>.
/// </summary>
/// <remarks>
/// Two pages are on screen while a swipe is in progress: the current one underneath, and the
/// incoming one on top, clipped to the wobbling edge. Once the drag passes its threshold the edge
/// stops being pulled back and is drawn the rest of the way across on its own, so letting go
/// mid-flight still completes the transition.
/// <para>
/// The swap into the base page is deferred until the next gesture begins. That is what lets the
/// reveal outlive the page change and finish at its own pace, and it is the reason this is a panel
/// rather than a <see cref="Carousel"/>: a carousel commits first and then animates for a duration
/// it owns, tearing the transition down as soon as it reaches the end.
/// </para>
/// </remarks>
public sealed class GooeyCarousel : Panel
{
    public static readonly DirectProperty<GooeyCarousel, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<GooeyCarousel, int>(nameof(SelectedIndex), o => o.SelectedIndex);

    public static readonly DirectProperty<GooeyCarousel, int> DragIndexProperty =
        AvaloniaProperty.RegisterDirect<GooeyCarousel, int>(nameof(DragIndex), o => o.DragIndex);

    public static readonly DirectProperty<GooeyCarousel, bool> IsDragCompletedProperty =
        AvaloniaProperty.RegisterDirect<GooeyCarousel, bool>(nameof(IsDragCompleted), o => o.IsDragCompleted);

    private const double SwipeActivationDistance = 20d;

    private const double SwipeCompletionRatio = 0.8d;

    private const double MinimumAvailableWidthRatio = 0.5d;

    private const double ClipMargin = 10d;

    private const int PointCount = 25;

    private readonly Effects.GooeyEdge _edge = new(count: PointCount);

    private FrameTicker? _ticker;
    private Point _dragOrigin;
    private double _dragDirection;
    private int _selectedIndex;
    private int _dragIndex;
    private bool _hasDragIndex;
    private bool _isDragCompleted;

    public GooeyCarousel() => ClipToBounds = true;

    /// <summary>
    /// Gets the index of the page underneath.
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        private set => SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value);
    }

    /// <summary>
    /// Gets the index of the page being revealed, which keeps the last swipe's value until the next
    /// one starts. The Flutter original clears its own to null between gestures and passes
    /// <c>_dragIndex ?? 0</c> on, but the sun and moon overlay only reads the index when a swipe has
    /// completed, so it never sees the difference.
    /// </summary>
    public int DragIndex
    {
        get => _dragIndex;
        private set => SetAndRaise(DragIndexProperty, ref _dragIndex, value);
    }

    /// <summary>
    /// Gets a value indicating whether the current swipe has passed its threshold.
    /// </summary>
    public bool IsDragCompleted
    {
        get => _isDragCompleted;
        private set => SetAndRaise(IsDragCompletedProperty, ref _isDragCompleted, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        // The previous swipe is only folded into the base page now, which is what let its reveal
        // run to completion after the pointer was lifted.
        if (_hasDragIndex && IsDragCompleted)
        {
            SelectedIndex = DragIndex;
        }

        _hasDragIndex = false;
        IsDragCompleted = false;
        _dragDirection = 0d;
        _dragOrigin = e.GetPosition(this);

        _edge.FarEdgeTension = 0d;
        _edge.EdgeTension = 0.01d;
        _edge.Reset();
        _edge.ApplyTouchOffset();

        UpdatePageStates();
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!Equals(e.Pointer.Captured, this))
        {
            return;
        }

        var position = e.GetPosition(this);
        var dx = position.X - _dragOrigin.X;

        if (!IsSwipeActive(dx) || IsSwipeComplete(dx))
        {
            return;
        }

        // A right-hand edge measures its pull from the opposite side, so the simulation only ever
        // sees a line being drawn away from its own edge.
        if (_dragDirection == -1d)
        {
            dx = Bounds.Width + dx;
        }

        _edge.ApplyTouchOffset(new Point(dx, position.Y), Bounds.Size);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        _edge.ApplyTouchOffset();
        e.Pointer.Capture(null);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _edge.ApplyTouchOffset();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        UpdatePageStates();

        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
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
        var bounds = new Rect(finalSize);

        foreach (var child in Children)
        {
            child.Arrange(bounds);
        }

        return finalSize;
    }

    private void OnTick(TimeSpan elapsed)
    {
        _edge.Tick(elapsed);

        if (!_hasDragIndex || GetPage(DragIndex) is not { } page)
        {
            return;
        }

        var size = Bounds.Size;

        if (size.Width > 0d && size.Height > 0d)
        {
            page.Clip = _edge.BuildGeometry(size, ClipMargin);
        }
    }

    // Recognises the start of a swipe and picks the page being revealed: dragging right brings in
    // the previous page from the left edge, dragging left the next one from the right.
    private bool IsSwipeActive(double dx)
    {
        if (_dragDirection == 0d && Math.Abs(dx) > SwipeActivationDistance)
        {
            _dragDirection = Math.Sign(dx);
            _edge.Side = _dragDirection == 1d ? GooeyEdgeSide.Left : GooeyEdgeSide.Right;

            DragIndex = SelectedIndex - (int)_dragDirection;
            _hasDragIndex = true;

            UpdatePageStates();
        }

        return _dragDirection != 0d;
    }

    // Decides whether the swipe has gone far enough to commit, measured against the width still
    // ahead of where the page was first grabbed. Once it has, the tensions are flipped so the edge
    // is drawn towards the far side instead of springing back.
    private bool IsSwipeComplete(double dx)
    {
        if (_dragDirection == 0d)
        {
            return false;
        }

        if (IsDragCompleted)
        {
            return true;
        }

        var width = Bounds.Width;
        var available = _dragDirection == 1d ? width - _dragOrigin.X : _dragOrigin.X;
        var ratio = dx * _dragDirection / available;

        if (ratio > SwipeCompletionRatio && available / width > MinimumAvailableWidthRatio)
        {
            IsDragCompleted = true;
            _edge.FarEdgeTension = 0.01d;
            _edge.EdgeTension = 0d;
            _edge.ApplyTouchOffset();
        }

        return IsDragCompleted;
    }

    private void UpdatePageStates()
    {
        if (Children.Count == 0)
        {
            return;
        }

        var basePage = GetPage(SelectedIndex);
        var dragPage = _hasDragIndex ? GetPage(DragIndex) : null;

        foreach (var child in Children)
        {
            var isDragPage = ReferenceEquals(child, dragPage);

            child.IsVisible = isDragPage || ReferenceEquals(child, basePage);
            child.ZIndex = isDragPage ? 1 : 0;

            if (!isDragPage)
            {
                child.Clip = null;
            }
        }
    }

    /// <summary>
    /// Drives a swipe across the control without a pointer, stepping through the same handlers a
    /// drag would. Used only by the screenshot harness.
    /// </summary>
    internal async Task RunCaptureSwipeAsync(double fraction, int steps = 30)
    {
        var width = Bounds.Width;
        var y = Bounds.Height / 2d;

        _dragOrigin = new Point(width * 0.92d, y);
        _dragDirection = 0d;
        _hasDragIndex = false;
        IsDragCompleted = false;

        _edge.FarEdgeTension = 0d;
        _edge.EdgeTension = 0.01d;
        _edge.Reset();
        _edge.ApplyTouchOffset();

        UpdatePageStates();

        for (var i = 1; i <= steps; i++)
        {
            var dx = -width * fraction * i / steps;

            if (IsSwipeActive(dx) && !IsSwipeComplete(dx))
            {
                _edge.ApplyTouchOffset(new Point(width + dx, y), Bounds.Size);
            }

            await Task.Delay(16);
        }

        _edge.ApplyTouchOffset();
    }

    /// <summary>
    /// Gets the page at <paramref name="index"/>, wrapping the way Flutter's modulo does.
    /// </summary>
    private Control? GetPage(int index)
    {
        if (Children.Count == 0)
        {
            return null;
        }

        var count = Children.Count;
        return Children[((index % count) + count) % count];
    }
}
