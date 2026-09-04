using System.Collections.Generic;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Controls;
using GooeyEdge.Effects;

namespace GooeyEdge.Controls;

/// <summary>
/// An Avalonia <see cref="Carousel"/> whose native page transition reveals the incoming page
/// through a liquid edge.
/// </summary>
/// <remarks>
/// <see cref="Carousel"/> owns selection, keyboard navigation, pointer capture and swipe
/// recognition. This specialization supplies only the visual transition and tracks the pointer's
/// vertical position so the liquid edge follows the gesture.
/// </remarks>
public sealed class GooeyCarousel : SwipeCarousel
{
    private static readonly StyledProperty<double> TransitionProgressProperty =
        AvaloniaProperty.Register<GooeyCarousel, double>("TransitionProgress");

    public static readonly DirectProperty<GooeyCarousel, int> DragIndexProperty =
        AvaloniaProperty.RegisterDirect<GooeyCarousel, int>(nameof(DragIndex), o => o.DragIndex);

    public static readonly DirectProperty<GooeyCarousel, bool> IsDragCompletedProperty =
        AvaloniaProperty.RegisterDirect<GooeyCarousel, bool>(nameof(IsDragCompleted), o => o.IsDragCompleted);

    internal static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(500);

    private const double CompletionRatio = 0.8d;
    private const double ClipMargin = 10d;
    private const int PointCount = 25;

    private readonly Effects.GooeyEdge _edge = new(count: PointCount);
    private readonly GooeyPageTransition _transition;
    private FrameTicker? _ticker;
    private Visual? _incomingPage;
    private Visual? _outgoingPage;
    private Point _pointerPosition;
    private bool _isForward;
    private int _dragIndex;
    private int _committedIndex;
    private bool _isDragCompleted;
    private double _progress;

    static GooeyCarousel()
    {
        IsSwipeEnabledProperty.OverrideDefaultValue<GooeyCarousel>(true);
        TransitionProgressProperty.Changed.AddClassHandler<GooeyCarousel>((x, e) =>
            x.ApplyTransition(e.GetNewValue<double>()));
    }

    public GooeyCarousel()
    {
        ClipToBounds = true;
        _transition = new GooeyPageTransition(this);
        PageTransition = _transition;
        AddHandler(PointerPressedEvent, ObservePointer, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerMovedEvent, ObservePointer, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>
    /// Gets the index of the page being revealed.
    /// </summary>
    public int DragIndex
    {
        get => _dragIndex;
        private set => SetAndRaise(DragIndexProperty, ref _dragIndex, value);
    }

    /// <summary>
    /// Gets a value indicating whether the liquid reveal has crossed its completion threshold.
    /// </summary>
    public bool IsDragCompleted
    {
        get => _isDragCompleted;
        private set => SetAndRaise(IsDragCompletedProperty, ref _isDragCompleted, value);
    }

    /// <summary>
    /// Uses the native <see cref="Carousel"/> theme for this transition specialization.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Carousel);

    private void ObservePointer(object? sender, PointerEventArgs e) =>
        _pointerPosition = e.GetPosition(this);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _pointerPosition = new Point(Bounds.Width / 2d, Bounds.Height / 2d);
        _ticker ??= new FrameTicker(this, OnTick);
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _ticker?.Stop();
    }

    private void BeginTransition(Visual? from, Visual? to, bool forward, bool programmatic)
    {
        if (!ReferenceEquals(_incomingPage, to) || !ReferenceEquals(_outgoingPage, from) || _isForward != forward)
        {
            if (_incomingPage is not null)
            {
                _incomingPage.Clip = null;
                _incomingPage.ZIndex = 0;
            }

            _incomingPage = to;
            _outgoingPage = from;
            _isForward = forward;
            IsDragCompleted = false;
            _progress = 0d;

            if (programmatic)
            {
                _pointerPosition = new Point(Bounds.Width / 2d, Bounds.Height / 2d);
            }

            // Selection wraps, but the sky's rotation must keep turning in the same direction.
            // The Flutter source also keeps an unbounded logical index for this reason.
            var step = forward ? 1 : -1;
            var target = _committedIndex + step;
            if (programmatic && NormalizeIndex(target) != SelectedIndex)
            {
                var distance = SelectedIndex - NormalizeIndex(_committedIndex);
                if (WrapSelection && forward && distance < 0) distance += ItemCount;
                if (WrapSelection && !forward && distance > 0) distance -= ItemCount;
                target = _committedIndex + distance;
            }
            DragIndex = target;

            _edge.Side = forward ? GooeyEdgeSide.Right : GooeyEdgeSide.Left;
            _edge.FarEdgeTension = 0d;
            _edge.EdgeTension = 0.01d;
            _edge.Reset();
            _edge.ApplyTouchOffset();

            if (_incomingPage is not null)
            {
                _incomingPage.ZIndex = 1;
            }

            if (_outgoingPage is not null)
            {
                _outgoingPage.ZIndex = 0;
                _outgoingPage.Clip = null;
            }
        }
    }

    private void ApplyTransition(double progress)
    {
        progress = Math.Clamp(progress, 0d, 1d);
        _progress = progress;

        if (_incomingPage is null)
        {
            return;
        }

        if (progress <= 0d)
        {
            IsDragCompleted = false;
            _edge.Reset();
            _incomingPage.Clip = _edge.BuildGeometry(Bounds.Size, ClipMargin);
            _edge.ApplyTouchOffset();
            return;
        }

        if (progress >= 1d)
        {
            _committedIndex = DragIndex;
            IsDragCompleted = true;
            _incomingPage.Clip = null;
            return;
        }

        if (!IsDragCompleted && progress >= CompletionRatio)
        {
            IsDragCompleted = !IsSwiping;
            _edge.FarEdgeTension = 0.01d;
            _edge.EdgeTension = 0d;
            _edge.ApplyTouchOffset();
        }

        if (!IsDragCompleted)
        {
            var width = Math.Max(1d, Bounds.Width);
            var x = _isForward ? width * (1d - progress) : width * progress;
            var y = Math.Clamp(_pointerPosition.Y, 0d, Math.Max(0d, Bounds.Height));
            _edge.ApplyTouchOffset(new Point(x, y), Bounds.Size);
        }
    }

    private void OnTick(TimeSpan elapsed)
    {
        _edge.Tick(elapsed);

        if (_incomingPage is not null && _progress > 0d && _progress < 1d && Bounds.Width > 0d && Bounds.Height > 0d)
        {
            var completion = Math.Clamp((_progress - 0.6d) / 0.4d, 0d, 1d);
            _incomingPage.Clip = _edge.BuildGeometry(Bounds.Size, ClipMargin, completion);
        }
    }

    private int NormalizeIndex(int index)
    {
        if (ItemCount == 0)
        {
            return 0;
        }

        if (WrapSelection)
        {
            return (index % ItemCount + ItemCount) % ItemCount;
        }

        return Math.Clamp(index, 0, ItemCount - 1);
    }

    private void ResetVisual(Visual visual)
    {
        visual.Clip = null;
        visual.ZIndex = 0;

        if (ReferenceEquals(visual, _incomingPage))
        {
            _incomingPage = null;
        }

        if (ReferenceEquals(visual, _outgoingPage))
        {
            _outgoingPage = null;
        }
    }

    private sealed class GooeyPageTransition(GooeyCarousel owner) : IProgressPageTransition
    {
        public async Task Start(
            Visual? from,
            Visual? to,
            bool forward,
            CancellationToken cancellationToken)
        {
            // Carousel compares numeric indices for programmatic navigation. Across a wrapped
            // boundary that comparison points the wrong way for the continuous sky rotation.
            if (owner.WrapSelection && owner.ItemCount > 2)
            {
                var origin = owner.NormalizeIndex(owner._committedIndex);
                if (origin == owner.ItemCount - 1 && owner.SelectedIndex == 0) forward = true;
                else if (origin == 0 && owner.SelectedIndex == owner.ItemCount - 1) forward = false;
            }
            owner.BeginTransition(from, to, forward, programmatic: true);
            owner.SetCurrentValue(TransitionProgressProperty, 0d);
            owner.ApplyTransition(0d);

            var animation = new Avalonia.Animation.Animation
            {
                Duration = TransitionDuration,
                Easing = new QuadraticEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(TransitionProgressProperty, 0d) },
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(TransitionProgressProperty, 1d) },
                    },
                },
            };

            await animation.RunAsync(owner, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                owner.ApplyTransition(1d);
            }
        }

        public void Update(
            double progress,
            Visual? from,
            Visual? to,
            bool forward,
            double pageLength,
            IReadOnlyList<PageTransitionItem> visibleItems)
        {
            owner.BeginTransition(from, to, forward, programmatic: false);
            owner.ApplyTransition(progress);
        }

        public void Reset(Visual visual) => owner.ResetVisual(visual);
    }
}
