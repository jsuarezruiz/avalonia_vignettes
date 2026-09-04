using System.Collections.Generic;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// Carries the scroll state of a <see cref="ProgressCarousel"/> to its notification events.
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

    /// <summary>
    /// Gets the current scroll offset in pixels.
    /// </summary>
    public double Pixels { get; }

    /// <summary>
    /// Gets the change in <see cref="Pixels"/> since the previous notification.
    /// </summary>
    public double Delta { get; }

    /// <summary>
    /// Gets the fractional page currently in the viewport.
    /// </summary>
    public double Page { get; }
}

/// <summary>
/// Avalonia's <see cref="Carousel"/>, with the fractional page progress exposed for vignettes
/// whose artwork reacts continuously while a native carousel transition is moving.
/// </summary>
/// <remarks>
/// Selection, keyboard navigation, virtualization, swipe recognition, pointer capture and paging
/// are all owned by <see cref="Carousel"/>. This type only observes its progress transition and
/// translates it into the same continuous values the artwork needs.
/// </remarks>
public class ProgressCarousel : SwipeCarousel
{
    private static readonly StyledProperty<double> ProgrammaticProgressProperty =
        AvaloniaProperty.Register<ProgressCarousel, double>("ProgrammaticProgress");

    public static readonly DirectProperty<ProgressCarousel, double> PageProperty =
        AvaloniaProperty.RegisterDirect<ProgressCarousel, double>(nameof(Page), o => o.Page);

    public static readonly DirectProperty<ProgressCarousel, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<ProgressCarousel, bool>(nameof(IsDragging), o => o.IsDragging);

    public static readonly RoutedEvent<PageScrollEventArgs> ScrollStartedEvent =
        RoutedEvent.Register<ProgressCarousel, PageScrollEventArgs>(nameof(ScrollStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PageScrollEventArgs> ScrollUpdatedEvent =
        RoutedEvent.Register<ProgressCarousel, PageScrollEventArgs>(nameof(ScrollUpdated), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<PageScrollEventArgs> DragReleasedEvent =
        RoutedEvent.Register<ProgressCarousel, PageScrollEventArgs>(nameof(DragReleased), RoutingStrategies.Bubble);

    private double _page;
    private bool _isDragging;
    private int _swipeId = -1;
    private double _programmaticFrom;
    private double _programmaticTo;

    static ProgressCarousel()
    {
        IsSwipeEnabledProperty.OverrideDefaultValue<ProgressCarousel>(true);
        ProgrammaticProgressProperty.Changed.AddClassHandler<ProgressCarousel>((x, e) =>
            x.ReportPage(x._programmaticFrom
                + ((x._programmaticTo - x._programmaticFrom) * e.GetNewValue<double>())));
        SelectedIndexProperty.Changed.AddClassHandler<ProgressCarousel>((x, e) =>
        {
            if (!x.IsSwiping)
            {
                x._programmaticFrom = x.Page;
                x._programmaticTo = e.GetNewValue<int>();

                if (!x.IsLoaded || x.PageTransition is null)
                {
                    x.ReportPage(e.GetNewValue<int>());
                }
            }
        });
    }

    public ProgressCarousel()
    {
        PageTransition = new TrackingPageSlide(this)
        {
            Duration = TimeSpan.FromMilliseconds(350),
            Orientation = PageSlide.SlideAxis.Horizontal,
            SlideInEasing = new QuadraticEaseOut(),
            SlideOutEasing = new QuadraticEaseOut(),
        };

        AddHandler(InputElement.SwipeGestureEvent, OnSwipeGesture, RoutingStrategies.Bubble, true);
        AddHandler(InputElement.SwipeGestureEndedEvent, OnSwipeGestureEnded, RoutingStrategies.Bubble, true);
    }

    /// <summary>
    /// Occurs when a native carousel swipe begins.
    /// </summary>
    public event EventHandler<PageScrollEventArgs>? ScrollStarted
    {
        add => AddHandler(ScrollStartedEvent, value);
        remove => RemoveHandler(ScrollStartedEvent, value);
    }

    /// <summary>
    /// Occurs whenever the native carousel's fractional page changes.
    /// </summary>
    public event EventHandler<PageScrollEventArgs>? ScrollUpdated
    {
        add => AddHandler(ScrollUpdatedEvent, value);
        remove => RemoveHandler(ScrollUpdatedEvent, value);
    }

    /// <summary>
    /// Occurs when the pointer is released after a native carousel swipe.
    /// </summary>
    public event EventHandler<PageScrollEventArgs>? DragReleased
    {
        add => AddHandler(DragReleasedEvent, value);
        remove => RemoveHandler(DragReleasedEvent, value);
    }

    /// <summary>
    /// Gets the fractional page currently in the viewport.
    /// </summary>
    public double Page
    {
        get => _page;
        private set => SetAndRaise(PageProperty, ref _page, value);
    }

    /// <summary>
    /// Gets whether the pointer is still down in a native carousel swipe. This intentionally turns
    /// false before the carousel's completion animation finishes.
    /// </summary>
    public bool IsDragging
    {
        get => _isDragging;
        private set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    /// <summary>
    /// Gets the width of one carousel page.
    /// </summary>
    public double PageWidth => Math.Max(1d, Bounds.Width * ViewportFraction);

    /// <summary>
    /// Uses the native <see cref="Carousel"/> theme for this progress-reporting specialization.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Carousel);

    /// <summary>
    /// Selects a page using the carousel's normal programmatic transition.
    /// </summary>
    public void MoveToPage(int page) => SelectedIndex = Math.Clamp(page, 0, Math.Max(0, ItemCount - 1));

    private void OnSwipeGesture(object? sender, SwipeGestureEventArgs e)
    {
        if (!IsSwiping || _swipeId == e.Id)
        {
            return;
        }

        _swipeId = e.Id;
        IsDragging = true;
        RaiseScrollEvent(ScrollStartedEvent, 0d);
    }

    private void OnSwipeGestureEnded(object? sender, SwipeGestureEndedEventArgs e)
    {
        if (_swipeId != e.Id)
        {
            return;
        }

        _swipeId = -1;
        IsDragging = false;
        RaiseScrollEvent(DragReleasedEvent, 0d);
    }

    private void ReportSwipeProgress(double progress, bool forward, IReadOnlyList<PageTransitionItem> visibleItems)
    {
        // Fractional-viewport carousels report the real layout offset. In particular, programmatic
        // selection has already changed SelectedIndex before its offset animation starts.
        if (visibleItems.Count > 0)
        {
            var anchor = visibleItems[0];
            ReportPage(anchor.Index - anchor.ViewportCenterOffset);
            return;
        }

        var origin = Math.Clamp(SelectedIndex, 0, Math.Max(0, ItemCount - 1));
        ReportPage(origin + (forward ? progress : -progress));
    }

    private void PrepareProgrammaticProgress()
    {
        _programmaticTo = SelectedIndex;
        SetCurrentValue(ProgrammaticProgressProperty, 0d);
        ReportPage(_programmaticFrom);
    }

    /// <summary>
    /// Reports a fractional page supplied by a specialized carousel implementation.
    /// </summary>
    protected void ReportPage(double page)
    {
        var oldPixels = Page * PageWidth;

        Page = Math.Clamp(page, 0d, Math.Max(0d, ItemCount - 1d));

        var pixels = Page * PageWidth;
        RaiseScrollEvent(ScrollUpdatedEvent, pixels - oldPixels);
    }

    private void RaiseScrollEvent(RoutedEvent<PageScrollEventArgs> routedEvent, double delta) =>
        RaiseEvent(new PageScrollEventArgs(routedEvent, Page * PageWidth, delta, Page));

    private sealed class TrackingPageSlide(ProgressCarousel owner) : PageSlide
    {
        public override async Task Start(
            Visual? from,
            Visual? to,
            bool forward,
            CancellationToken cancellationToken)
        {
            owner.PrepareProgrammaticProgress();

            var progress = new Avalonia.Animation.Animation
            {
                Duration = Duration,
                Easing = SlideInEasing,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(ProgrammaticProgressProperty, 0d) },
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(ProgrammaticProgressProperty, 1d) },
                    },
                },
            };

            await Task.WhenAll(
                base.Start(from, to, forward, cancellationToken),
                progress.RunAsync(owner, cancellationToken));
        }

        public override void Update(
            double progress,
            Visual? from,
            Visual? to,
            bool forward,
            double pageLength,
            IReadOnlyList<PageTransitionItem> visibleItems)
        {
            base.Update(progress, from, to, forward, pageLength, visibleItems);
            owner.ReportSwipeProgress(progress, forward, visibleItems);
        }
    }
}
