using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Input;
using ParticleSwipe.Models;

namespace ParticleSwipe.Controls;

/// <summary>
/// What a completed swipe asks for.
/// </summary>
public enum SwipeAction
{
    /// <summary>
    /// Delete the message, swiped from the right.
    /// </summary>
    Remove,

    /// <summary>
    /// Star the message, swiped from the left.
    /// </summary>
    Favorite,
}

/// <summary>
/// Carries the <see cref="SwipeAction"/> a row has asked for.
/// </summary>
public sealed class SwipeActionEventArgs : RoutedEventArgs
{
    public SwipeActionEventArgs(RoutedEvent routedEvent, object source, SwipeAction action)
        : base(routedEvent, source) => Action = action;

    /// <summary>
    /// Gets the action the swipe asked for.
    /// </summary>
    public SwipeAction Action { get; }
}

/// <summary>
/// One row of the inbox, draggable sideways to delete or to star. Port of <c>swipe_item.dart</c>.
/// </summary>
/// <remarks>
/// The original gets its feel from a horizontal scroll view whose content is a hair wider than the
/// viewport: every pixel of movement is therefore overscroll, and the custom physics decide both how
/// hard the row resists being pulled and how it springs back. There is no scroll view here, but the
/// two pieces that matter are ported directly: the resistance curve in
/// <see cref="ApplyPhysicsToUserOffset"/>, and the release spring.
/// <para>
/// Positive offsets mean the row has been pulled to the left, exposing the delete indicator on the
/// right, matching the sign of the scroll position the original reads.
/// </para>
/// </remarks>
public sealed class SwipeItem : TemplatedControl
{
    public static readonly StyledProperty<Email?> EmailProperty =
        AvaloniaProperty.Register<SwipeItem, Email?>(nameof(Email));

    public static readonly StyledProperty<bool> IsAlternateProperty =
        AvaloniaProperty.Register<SwipeItem, bool>(nameof(IsAlternate));

    public static readonly DirectProperty<SwipeItem, IBrush> IndicatorBrushProperty =
        AvaloniaProperty.RegisterDirect<SwipeItem, IBrush>(nameof(IndicatorBrush), o => o.IndicatorBrush);

    public static readonly DirectProperty<SwipeItem, ITransform> IndicatorTransformProperty =
        AvaloniaProperty.RegisterDirect<SwipeItem, ITransform>(nameof(IndicatorTransform), o => o.IndicatorTransform);

    public static readonly DirectProperty<SwipeItem, ITransform> ContentTransformProperty =
        AvaloniaProperty.RegisterDirect<SwipeItem, ITransform>(nameof(ContentTransform), o => o.ContentTransform);

    public static readonly DirectProperty<SwipeItem, double> ContentOpacityProperty =
        AvaloniaProperty.RegisterDirect<SwipeItem, double>(nameof(ContentOpacity), o => o.ContentOpacity);

    public static readonly DirectProperty<SwipeItem, HorizontalAlignment> IndicatorAlignmentProperty =
        AvaloniaProperty.RegisterDirect<SwipeItem, HorizontalAlignment>(
            nameof(IndicatorAlignment), o => o.IndicatorAlignment);

    public static readonly DirectProperty<SwipeItem, BoxShadows> IndicatorGlowProperty =
        AvaloniaProperty.RegisterDirect<SwipeItem, BoxShadows>(nameof(IndicatorGlow), o => o.IndicatorGlow);

    /// <summary>
    /// Raised when a swipe has travelled far enough to ask for an action.
    /// </summary>
    public static readonly RoutedEvent<SwipeActionEventArgs> SwipeActionEvent =
        RoutedEvent.Register<SwipeItem, SwipeActionEventArgs>(nameof(SwipeAction), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised once the row has finished collapsing and can be taken out of the list.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> RemovedEvent =
        RoutedEvent.Register<SwipeItem, RoutedEventArgs>(nameof(Removed), RoutingStrategies.Bubble);

    /// <summary>
    /// How far a row must travel for a swipe to count.
    /// </summary>
    public const double SwipeDistance = 96d;

    /// <summary>
    /// The height of a row before it starts collapsing.
    /// </summary>
    public const double NominalHeight = 110d;

    private static readonly Color DeleteColor = Color.FromRgb(0xCB, 0x4A, 0x65);

    private static readonly Color FavoriteColor = Color.FromRgb(0x4A, 0xC0, 0xCB);

    private const double MaxOverscroll = SwipeDistance * 1.2d;

    private const double DragSlop = 4d;

    private static readonly SpringDescription ReleaseSpring =
        SpringDescription.FromDampingRatio(mass: 0.15d, stiffness: 250d, ratio: 1.25d);

    private static readonly TimeSpan FavoriteReturnDuration = TimeSpan.FromMilliseconds(800);
    private static readonly TimeSpan CollapseDuration = TimeSpan.FromMilliseconds(200);

    private static readonly Easing FavoriteReturnEasing =
        new IntervalEasing(0.25d, 1d, FlutterEasings.EaseOutQuad);

    private readonly LinearGradientBrush _indicatorBrush;
    private readonly ScaleTransform _indicatorScale = new();
    private readonly TranslateTransform _indicatorTranslate = new();
    private readonly ScaleTransform _contentScale = new();
    private readonly TranslateTransform _contentTranslate = new();
    private readonly VelocityTracker _velocity = new();

    private readonly AnimationController _favoriteReturn;
    private readonly AnimationController _collapse;

    private FrameTicker? _springTicker;
    private SpringSimulation? _spring;

    private double _offset;
    private double _contentOpacity = 1d;
    private double _lastPointerX;
    private double _favoriteReturnFrom;
    private HorizontalAlignment _indicatorAlignment = HorizontalAlignment.Right;
    private BoxShadows _indicatorGlow;
    private Point _pressOrigin;
    private bool _isPressed;
    private bool _isDragging;
    private bool _isPerformingAction;
    private bool _isRemoving;

    static SwipeItem()
    {
        EmailProperty.Changed.AddClassHandler<SwipeItem>((x, e) => x.OnEmailChanged(e));
        IsAlternateProperty.Changed.AddClassHandler<SwipeItem>((x, e) =>
            x.PseudoClasses.Set(":alternate", e.GetNewValue<bool>()));
    }

    public SwipeItem()
    {
        Height = NominalHeight;

        _indicatorBrush = new LinearGradientBrush
        {
            GradientStops =
            {
                // Two stops share an offset, which makes a hard edge: a bright sliver right at the
                // swiped edge, and a soft wash spreading away from it.
                new GradientStop(Colors.Transparent, 0d),
                new GradientStop(Colors.Transparent, 0.012d),
                new GradientStop(Colors.Transparent, 0.012d),
                new GradientStop(Colors.Transparent, 1d),
            },
        };

        IndicatorTransform = new TransformGroup { Children = { _indicatorScale, _indicatorTranslate } };
        ContentTransform = new TransformGroup { Children = { _contentScale, _contentTranslate } };

        _favoriteReturn = new AnimationController(this, OnFavoriteReturnChanged) { Duration = FavoriteReturnDuration };
        _collapse = new AnimationController(this, OnCollapseChanged) { Duration = CollapseDuration };

        UpdateVisualState();
    }

    /// <summary>
    /// Occurs when a swipe has travelled far enough to ask for an action.
    /// </summary>
    public event EventHandler<SwipeActionEventArgs>? SwipeAction
    {
        add => AddHandler(SwipeActionEvent, value);
        remove => RemoveHandler(SwipeActionEvent, value);
    }

    /// <summary>
    /// Occurs once the row has finished collapsing.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Removed
    {
        add => AddHandler(RemovedEvent, value);
        remove => RemoveHandler(RemovedEvent, value);
    }

    /// <summary>
    /// Gets or sets the message this row shows.
    /// </summary>
    public Email? Email
    {
        get => GetValue(EmailProperty);
        set => SetValue(EmailProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this row takes the darker of the two fills.
    /// </summary>
    public bool IsAlternate
    {
        get => GetValue(IsAlternateProperty);
        set => SetValue(IsAlternateProperty, value);
    }

    /// <summary>
    /// Gets the wash drawn behind the row, which deepens as the swipe travels. The instance never
    /// changes; its stops and direction are what move, which is enough to keep the render in step.
    /// </summary>
    public IBrush IndicatorBrush => _indicatorBrush;

    /// <summary>
    /// Gets the transform that slides and grows the indicator as the row is pulled.
    /// </summary>
    public ITransform IndicatorTransform { get; }

    /// <summary>
    /// Gets the transform that carries and shrinks the message card.
    /// </summary>
    public ITransform ContentTransform { get; }

    /// <summary>
    /// Gets how visible the message card is; it fades almost out as the row is pulled.
    /// </summary>
    public double ContentOpacity
    {
        get => _contentOpacity;
        private set => SetAndRaise(ContentOpacityProperty, ref _contentOpacity, value);
    }

    /// <summary>
    /// Gets the edge the indicator is pinned to.
    /// </summary>
    public HorizontalAlignment IndicatorAlignment
    {
        get => _indicatorAlignment;
        private set => SetAndRaise(IndicatorAlignmentProperty, ref _indicatorAlignment, value);
    }

    /// <summary>
    /// Gets the halo behind the indicator. It is only ever lit while un-starring an already starred
    /// message, which is the one case where the indicator would otherwise look identical either way.
    /// </summary>
    public BoxShadows IndicatorGlow
    {
        get => _indicatorGlow;
        private set => SetAndRaise(IndicatorGlowProperty, ref _indicatorGlow, value);
    }

    /// <summary>
    /// Collapses the row to nothing, then raises <see cref="Removed"/>. This stands in for the
    /// original's <c>RemovedSwipeItem</c>, which the animated list swaps in for the deleted row.
    /// </summary>
    public void BeginRemove()
    {
        if (_isRemoving)
        {
            return;
        }

        _isRemoving = true;
        _isDragging = false;
        _isPressed = false;
        _spring = null;
        _springTicker?.Stop();

        PseudoClasses.Set(":removing", true);

        // The row is left showing nothing but a full-strength delete wash while it closes.
        SetOffset(0d);
        UpdateIndicatorBrush(DeleteColor, ratio: 1d, sign: 1);

        _collapse.SetValue(1d);
        _collapse.Reverse();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_isRemoving || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _isPressed = true;
        _pressOrigin = e.GetPosition(this);
        _lastPointerX = _pressOrigin.X;

        _spring = null;
        _springTicker?.Stop();

        _velocity.Clear();
        _velocity.Add(e, _pressOrigin.X);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isRemoving || (!_isPressed && !_isDragging))
        {
            return;
        }

        var position = e.GetPosition(this);

        if (!_isDragging)
        {
            var fromOriginX = position.X - _pressOrigin.X;
            var fromOriginY = position.Y - _pressOrigin.Y;

            // A drag that sets off vertically belongs to the list, not to the row.
            if (Math.Abs(fromOriginY) > DragSlop && Math.Abs(fromOriginY) > Math.Abs(fromOriginX))
            {
                _isPressed = false;
                return;
            }

            if (Math.Abs(fromOriginX) <= DragSlop)
            {
                return;
            }

            _isDragging = true;
            e.Pointer.Capture(this);
        }

        var delta = position.X - _lastPointerX;

        _lastPointerX = position.X;
        _velocity.Add(e, position.X);

        // The offset grows as the pointer travels left, so the applied delta is subtracted, exactly
        // as the original's scroll position does with `pixels -= applyPhysicsToUserOffset(delta)`.
        SetOffset(_offset - ApplyPhysicsToUserOffset(delta));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isDragging)
        {
            _velocity.Add(e, e.GetPosition(this).X);
            EndDrag();
        }

        _isPressed = false;
        e.Pointer.Capture(null);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        if (_isDragging)
        {
            EndDrag();
        }

        _isPressed = false;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _springTicker?.Stop();
        _favoriteReturn.Stop();
        _collapse.Stop();
    }

    /// <summary>
    /// Scales down a drag the further the row already is from rest, reaching a dead stop at
    /// <see cref="MaxOverscroll"/>. Port of <c>ConstrainedScrollPhysics.applyPhysicsToUserOffset</c>.
    /// </summary>
    /// <remarks>
    /// The formula measures resistance from where the row would end up rather than from where it is,
    /// so it is deliberately not symmetric: pulling further out is damped harder than easing back.
    /// </remarks>
    private double ApplyPhysicsToUserOffset(double offset)
    {
        // Both scroll bounds sit at zero, so how far the row is from rest is how far it is out of
        // range.
        var overscrollPast = Math.Abs(_offset);

        if (overscrollPast <= 0d)
        {
            return offset;
        }

        var ratio = Math.Max(0d, 1d - ((offset + overscrollPast) / MaxOverscroll));

        return offset * ratio;
    }

    private void EndDrag()
    {
        _isDragging = false;

        if (_isRemoving || _isPerformingAction)
        {
            return;
        }

        // The tracker measures the pointer, whose direction is the opposite of the offset's.
        StartSpring(-_velocity.Estimate());
    }

    private void StartSpring(double velocity)
    {
        _spring = new SpringSimulation(ReleaseSpring, _offset, 0d, velocity);
        _springTicker ??= new FrameTicker(this, OnSpringTick);
        _springTicker.Start();
    }

    private void OnSpringTick(TimeSpan elapsed)
    {
        if (_spring is not { } spring)
        {
            _springTicker?.Stop();
            return;
        }

        var seconds = Math.Max(0d, elapsed.TotalSeconds);

        SetOffset(spring.PositionAt(seconds));

        if (spring.IsDone(seconds))
        {
            _spring = null;
            _springTicker?.Stop();
            SetOffset(0d);
        }
    }

    // Moves the row and, as the original's scroll listener does, checks on every pixel whether the
    // swipe has gone far enough to act on.
    private void SetOffset(double offset)
    {
        _offset = offset;

        UpdateVisualState();
        HandleSwipe();
    }

    private void HandleSwipe()
    {
        if (_isRemoving || _isPerformingAction)
        {
            return;
        }

        if (_offset > SwipeDistance)
        {
            RaiseEvent(new SwipeActionEventArgs(SwipeActionEvent, this, Controls.SwipeAction.Remove));

            // Released straight away rather than sprung back: the row is on its way out anyway.
            _spring = null;
            _springTicker?.Stop();
            SetOffset(0d);
        }
        else if (_offset < -SwipeDistance)
        {
            _isPerformingAction = true;

            RaiseEvent(new SwipeActionEventArgs(SwipeActionEvent, this, Controls.SwipeAction.Favorite));

            _spring = null;
            _springTicker?.Stop();

            _favoriteReturnFrom = _offset;
            _favoriteReturn.SetValue(0d);
            _favoriteReturn.Forward();
        }
    }

    private void OnFavoriteReturnChanged(double progress)
    {
        var eased = FavoriteReturnEasing.Ease(progress);

        _offset = _favoriteReturnFrom * (1d - eased);
        UpdateVisualState();

        if (progress >= 1d)
        {
            _isPerformingAction = false;
        }
    }

    private void OnCollapseChanged(double progress)
    {
        SetCurrentValue(HeightProperty, NominalHeight * progress);

        if (progress <= 0d)
        {
            RaiseEvent(new RoutedEventArgs(RemovedEvent, this));
        }
    }

    private void OnEmailChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is Email old)
        {
            old.PropertyChanged -= OnEmailPropertyChanged;
        }

        if (e.NewValue is Email added)
        {
            added.PropertyChanged += OnEmailPropertyChanged;
        }

        UpdateFavoriteState();
    }

    private void OnEmailPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
        UpdateFavoriteState();

    private void UpdateFavoriteState()
    {
        PseudoClasses.Set(":starred", Email?.IsFavorite == true);
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (_isRemoving)
        {
            return;
        }

        var ratio = Math.Min(1d, Math.Abs(_offset) / SwipeDistance);
        var sign = Math.Sign(_offset);
        var isLeftToRight = _offset < 0d;
        var distance = Math.Abs(_offset);

        PseudoClasses.Set(":favorite-swipe", isLeftToRight);

        UpdateIndicatorBrush(isLeftToRight ? FavoriteColor : DeleteColor, ratio, sign);

        // The indicator trails the row at half its speed and grows into place.
        _indicatorTranslate.X = distance * sign * -0.5d;
        _indicatorScale.ScaleX = _indicatorScale.ScaleY = 0.5d + (0.5d * ratio);

        IndicatorAlignment = sign < 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right;

        IndicatorGlow = Email?.IsFavorite == true && isLeftToRight
            ? new BoxShadows(new BoxShadow
            {
                Blur = 18d,
                Color = Color.FromArgb((byte)Math.Round(ratio * 255d), 0xFF, 0xFF, 0xFF),
            })
            : default;

        _contentTranslate.X = -_offset;
        _contentScale.ScaleX = _contentScale.ScaleY = 1d - (ratio * 0.1d);

        ContentOpacity = 1d - (ratio * 0.9d);
    }

    private void UpdateIndicatorBrush(Color color, double ratio, int sign)
    {
        var stops = _indicatorBrush.GradientStops;

        stops[0].Color = stops[1].Color = WithOpacity(color, ratio);
        stops[2].Color = WithOpacity(color, ratio * 0.30d);
        stops[3].Color = WithOpacity(color, ratio * 0.10d);

        // Flutter's Alignment runs from -1 on the left to 1 on the right; Avalonia's relative points
        // run from 0 to 1.
        _indicatorBrush.StartPoint = new RelativePoint((sign + 1d) / 2d, 0.5d, RelativeUnit.Relative);
        _indicatorBrush.EndPoint = new RelativePoint(((-sign * ratio) + 1d) / 2d, 0.5d, RelativeUnit.Relative);
    }

    private static Color WithOpacity(Color color, double opacity) =>
        new((byte)Math.Round(Math.Clamp(opacity, 0d, 1d) * 255d), color.R, color.G, color.B);
}
