using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Input;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// Carries the scroll state of a <see cref="PageView"/> to its notification events, mirroring
/// Flutter's <c>ScrollNotification</c> metrics.
/// </summary>
public sealed class PageScrollEventArgs : RoutedEventArgs
{
    internal PageScrollEventArgs(RoutedEvent routedEvent, double pixels, double delta, double page)
        : base(routedEvent)
    {
        Pixels = pixels;
        Delta = delta;
        Page = page;
    }

    /// <summary>Gets the current scroll offset in pixels.</summary>
    public double Pixels { get; }

    /// <summary>Gets the change in <see cref="Pixels"/> since the previous notification.</summary>
    public double Delta { get; }

    /// <summary>Gets the fractional page the view is resting at.</summary>
    public double Page { get; }
}

/// <summary>
/// A horizontally paged items control, equivalent to Flutter's <c>PageView</c> with a
/// <c>viewportFraction</c> below 1: pages sit edge to edge, the current one is centred, and its
/// neighbours peek in from either side.
/// </summary>
/// <remarks>
/// Dragging moves the strip one-to-one with the pointer, resisting past the ends the way
/// <c>BouncingScrollPhysics</c> does, and releasing runs the same <c>ScrollSpringSimulation</c>
/// Flutter's <c>PageScrollPhysics</c> uses to snap to the nearest page.
/// </remarks>
public class PageView : ItemsControl
{
    /// <summary>Defines the <see cref="ViewportFraction"/> property.</summary>
    public static readonly StyledProperty<double> ViewportFractionProperty =
        AvaloniaProperty.Register<PageView, double>(nameof(ViewportFraction), 1d);

    /// <summary>Defines the <see cref="ScrollPixels"/> property.</summary>
    public static readonly StyledProperty<double> ScrollPixelsProperty =
        AvaloniaProperty.Register<PageView, double>(nameof(ScrollPixels));

    /// <summary>Defines the <see cref="Page"/> property.</summary>
    public static readonly DirectProperty<PageView, double> PageProperty =
        AvaloniaProperty.RegisterDirect<PageView, double>(nameof(Page), o => o.Page);

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<PageView, int>(nameof(SelectedIndex), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly DirectProperty<PageView, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<PageView, object?>(nameof(SelectedItem), o => o.SelectedItem);

    /// <summary>Defines the <see cref="IsDragging"/> property.</summary>
    public static readonly DirectProperty<PageView, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<PageView, bool>(nameof(IsDragging), o => o.IsDragging);

    /// <summary>Raised when a drag begins, equivalent to Flutter's <c>ScrollStartNotification</c>.</summary>
    public static readonly RoutedEvent<PageScrollEventArgs> ScrollStartedEvent =
        RoutedEvent.Register<PageView, PageScrollEventArgs>(nameof(ScrollStarted), RoutingStrategies.Bubble);

    /// <summary>Raised whenever the offset changes, equivalent to <c>ScrollUpdateNotification</c>.</summary>
    public static readonly RoutedEvent<PageScrollEventArgs> ScrollUpdatedEvent =
        RoutedEvent.Register<PageView, PageScrollEventArgs>(nameof(ScrollUpdated), RoutingStrategies.Bubble);

    /// <summary>Raised when the pointer is lifted, before the snap simulation starts.</summary>
    public static readonly RoutedEvent<PageScrollEventArgs> DragReleasedEvent =
        RoutedEvent.Register<PageView, PageScrollEventArgs>(nameof(DragReleased), RoutingStrategies.Bubble);

    // Flutter's Tolerance.velocity for a device pixel ratio of 1: below this a fling is treated as
    // a plain release and the view snaps to the nearest page rather than the next one.
    private const double FlingVelocityThreshold = 20d;

    private readonly VelocityTracker _velocity = new();

    private FrameTicker? _ticker;
    private SpringSimulation? _simulation;
    private double _lastPointerX;
    private double _pageWidth;
    private double _page;
    private object? _selectedItem;
    private bool _isDragging;
    private bool _updatingSelection;

    static PageView()
    {
        // The viewport clips, exactly as Flutter's Viewport does inside a PageView.
        ClipToBoundsProperty.OverrideDefaultValue<PageView>(true);
        ItemsPanelProperty.OverrideDefaultValue<PageView>(new FuncTemplate<Panel?>(() => new PageViewPanel()));
        ScrollPixelsProperty.Changed.AddClassHandler<PageView>((x, e) => x.OnScrollPixelsChanged(e));
        SelectedIndexProperty.Changed.AddClassHandler<PageView>((x, e) => x.OnSelectedIndexChanged(e));
        ViewportFractionProperty.Changed.AddClassHandler<PageView>((x, _) => x.RefreshPageWidth());
        BoundsProperty.Changed.AddClassHandler<PageView>((x, _) => x.RefreshPageWidth());
    }

    /// <summary>Occurs when a drag begins.</summary>
    public event EventHandler<PageScrollEventArgs>? ScrollStarted
    {
        add => AddHandler(ScrollStartedEvent, value);
        remove => RemoveHandler(ScrollStartedEvent, value);
    }

    /// <summary>Occurs whenever the scroll offset changes, whether by drag or by simulation.</summary>
    public event EventHandler<PageScrollEventArgs>? ScrollUpdated
    {
        add => AddHandler(ScrollUpdatedEvent, value);
        remove => RemoveHandler(ScrollUpdatedEvent, value);
    }

    /// <summary>Occurs when the pointer is released after a drag.</summary>
    public event EventHandler<PageScrollEventArgs>? DragReleased
    {
        add => AddHandler(DragReleasedEvent, value);
        remove => RemoveHandler(DragReleasedEvent, value);
    }

    /// <summary>
    /// Gets or sets the fraction of the viewport each page occupies. A value below 1 leaves the
    /// neighbouring pages partly visible on either side.
    /// </summary>
    public double ViewportFraction
    {
        get => GetValue(ViewportFractionProperty);
        set => SetValue(ViewportFractionProperty, value);
    }

    /// <summary>Gets or sets the scroll offset in pixels. Page <c>n</c> is centred at <c>n * pageWidth</c>.</summary>
    public double ScrollPixels
    {
        get => GetValue(ScrollPixelsProperty);
        set => SetValue(ScrollPixelsProperty, value);
    }

    /// <summary>Gets the fractional page currently on screen, the equivalent of <c>PageController.page</c>.</summary>
    public double Page
    {
        get => _page;
        private set => SetAndRaise(PageProperty, ref _page, value);
    }

    /// <summary>Gets or sets the index of the page nearest the centre of the viewport.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Gets the item at <see cref="SelectedIndex"/>.</summary>
    public object? SelectedItem
    {
        get => _selectedItem;
        private set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    /// <summary>Gets a value indicating whether the user is currently dragging the strip.</summary>
    public bool IsDragging
    {
        get => _isDragging;
        private set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    /// <summary>Gets the width of a single page.</summary>
    public double PageWidth => _pageWidth > 0d ? _pageWidth : Math.Max(1d, Bounds.Width * ViewportFraction);

    /// <summary>
    /// Control themes are resolved by exact type, and a paged strip needs nothing more than the
    /// presenter an <see cref="ItemsControl"/> already provides, so its theme is reused.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    internal static double GetPageWidth(double viewportWidth, double viewportFraction) =>
        Math.Max(1d, viewportWidth * viewportFraction);

    /// <summary>
    /// Jumps to <paramref name="page"/> without animating, the equivalent of
    /// <c>PageController.jumpToPage</c>.
    /// </summary>
    public void JumpToPage(int page)
    {
        StopSimulation();
        ScrollPixels = ClampPage(page) * PageWidth;
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        StopSimulation();

        _lastPointerX = e.GetPosition(this).X;
        _velocity.Clear();
        _velocity.Add(e, _lastPointerX);

        IsDragging = true;
        e.Pointer.Capture(this);

        RaiseScrollEvent(ScrollStartedEvent, 0d);
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!IsDragging)
        {
            return;
        }

        var x = e.GetPosition(this).X;

        // Dragging left moves the strip forward, so the scroll offset grows as the pointer's x shrinks.
        var requested = _lastPointerX - x;
        _lastPointerX = x;

        _velocity.Add(e, x);
        ScrollPixels += ApplyOverscrollFriction(requested);
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        EndDrag(VelocityTracker.TimestampOf(e), e.GetPosition(this).X);
        e.Pointer.Capture(null);
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        EndDrag(null, null);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopSimulation();
    }

    /// <summary>
    /// Recomputes the page width after a viewport or viewport-fraction change, keeping whichever
    /// page was on screen centred. Before the first layout there is no offset to preserve, so
    /// <see cref="SelectedIndex"/> decides where the strip starts — this is what honours an initial
    /// page set in markup.
    /// </summary>
    private void RefreshPageWidth()
    {
        var width = Bounds.Width * ViewportFraction;

        if (width <= 0d || Math.Abs(width - _pageWidth) < double.Epsilon)
        {
            return;
        }

        var page = _pageWidth > 0d ? ScrollPixels / _pageWidth : SelectedIndex;

        _pageWidth = width;
        ScrollPixels = page * width;

        InvalidateStrip();
    }

    private void OnScrollPixelsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var oldValue = e.GetOldValue<double>();
        var newValue = e.GetNewValue<double>();

        InvalidateStrip();

        Page = newValue / PageWidth;
        UpdateSelectionFromPage();

        RaiseScrollEvent(ScrollUpdatedEvent, newValue - oldValue);
    }

    private void OnSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        SelectedItem = ItemFromIndex(e.GetNewValue<int>());

        if (!_updatingSelection)
        {
            // The selection was set from the outside, so bring that page into view.
            JumpToPage(e.GetNewValue<int>());
        }
    }

    private void UpdateSelectionFromPage()
    {
        if (ItemCount == 0)
        {
            return;
        }

        var index = ClampPage((int)Math.Round(Page, MidpointRounding.AwayFromZero));

        if (index == SelectedIndex)
        {
            // The item may only now have been materialised, so keep SelectedItem in step.
            SelectedItem = ItemFromIndex(index);
            return;
        }

        _updatingSelection = true;
        try
        {
            SelectedIndex = index;
        }
        finally
        {
            _updatingSelection = false;
        }
    }

    private object? ItemFromIndex(int index)
    {
        if (Items is not { Count: > 0 } items || index < 0 || index >= items.Count)
        {
            return null;
        }

        return items[index];
    }

    private int ClampPage(int page) => Math.Clamp(page, 0, Math.Max(0, ItemCount - 1));

    private double MaxScrollPixels => Math.Max(0d, (ItemCount - 1) * PageWidth);

    /// <summary>
    /// Damps a drag delta that pushes the strip past its ends, the way Flutter's
    /// <c>BouncingScrollPhysics.applyPhysicsToUserOffset</c> does.
    /// </summary>
    private double ApplyOverscrollFriction(double delta)
    {
        var pixels = ScrollPixels;
        var overscrollPastStart = Math.Max(-pixels, 0d);
        var overscrollPastEnd = Math.Max(pixels - MaxScrollPixels, 0d);
        var overscrollPast = Math.Max(overscrollPastStart, overscrollPastEnd);

        if (overscrollPast <= 0d)
        {
            return delta;
        }

        // Moving back towards the valid range is never damped.
        var movingBackIntoRange = (overscrollPastStart > 0d && delta > 0d) || (overscrollPastEnd > 0d && delta < 0d);

        if (movingBackIntoRange)
        {
            return delta;
        }

        var viewport = Math.Max(1d, Bounds.Width);
        var friction = 0.52d * Math.Pow(Math.Max(0d, 1d - (overscrollPast / viewport)), 2d);

        return delta * friction;
    }

    private void EndDrag(TimeSpan? time, double? x)
    {
        if (!IsDragging)
        {
            return;
        }

        if (time is { } t && x is { } px)
        {
            _velocity.Add(t, px);
        }

        IsDragging = false;

        var velocity = EstimateScrollVelocity();
        RaiseScrollEvent(DragReleasedEvent, 0d);
        StartSnapSimulation(velocity);
    }

    /// <summary>Estimates the release velocity in scroll pixels per second.</summary>
    /// <remarks>Negated because the scroll offset grows as the pointer travels left.</remarks>
    private double EstimateScrollVelocity() => -_velocity.Estimate();

    /// <summary>
    /// Starts the spring that carries the strip to a whole page, matching
    /// <c>PageScrollPhysics.createBallisticSimulation</c>.
    /// </summary>
    private void StartSnapSimulation(double velocity)
    {
        var pageWidth = PageWidth;
        var page = ScrollPixels / pageWidth;

        // A fling commits to the next page, anything slower snaps to whichever page is closest.
        if (velocity < -FlingVelocityThreshold)
        {
            page -= 0.5d;
        }
        else if (velocity > FlingVelocityThreshold)
        {
            page += 0.5d;
        }

        var target = ClampPage((int)Math.Round(page, MidpointRounding.AwayFromZero)) * pageWidth;

        if (Math.Abs(target - ScrollPixels) < 0.01d && Math.Abs(velocity) < FlingVelocityThreshold)
        {
            ScrollPixels = target;
            return;
        }

        _simulation = new SpringSimulation(SpringDescription.ScrollDefault, ScrollPixels, target, velocity);
        _ticker ??= new FrameTicker(this, OnSimulationTick);
        _ticker.Start();
    }

    private void OnSimulationTick(TimeSpan elapsed)
    {
        if (_simulation is not { } simulation)
        {
            StopSimulation();
            return;
        }

        var seconds = elapsed.TotalSeconds;
        ScrollPixels = simulation.PositionAt(seconds);

        if (simulation.IsDone(seconds))
        {
            StopSimulation();
        }
    }

    private void StopSimulation()
    {
        _simulation = null;
        _ticker?.Stop();
    }

    private void InvalidateStrip() => ItemsPanelRoot?.InvalidateArrange();

    private void RaiseScrollEvent(RoutedEvent<PageScrollEventArgs> routedEvent, double delta) =>
        RaiseEvent(new PageScrollEventArgs(routedEvent, ScrollPixels, delta, Page));
}
