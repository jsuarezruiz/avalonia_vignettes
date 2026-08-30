using Avalonia;
using Avalonia.Controls.Primitives;
using AvaloniaVignettes.Shared.Animation;

namespace SpendingTracker.Controls;

/// <summary>
/// A figure that rolls over when it changes, the old one leaving upwards as the new one arrives from
/// below. Port of <c>text_transition.dart</c>.
/// </summary>
/// <remarks>
/// The box is a line and a fifth tall and only ever holds one line, so the two never both settle:
/// the moment the new figure lands the old one is dropped and the roll resets. Its width is fixed,
/// either given or guessed from the outgoing figure's length, which is what keeps the summaries
/// either side of the divider from shuffling as the numbers count up.
/// </remarks>
public sealed class TextTransition : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TextTransition, string?>(nameof(Text));

    public static readonly StyledProperty<TimeSpan> TransitionDurationProperty =
        AvaloniaProperty.Register<TextTransition, TimeSpan>(
            nameof(TransitionDuration),
            TimeSpan.FromMilliseconds(400));

    public static readonly StyledProperty<double> FixedWidthProperty =
        AvaloniaProperty.Register<TextTransition, double>(nameof(FixedWidth), double.NaN);

    public static readonly DirectProperty<TextTransition, string?> CurrentProperty =
        AvaloniaProperty.RegisterDirect<TextTransition, string?>(nameof(Current), o => o.Current);

    public static readonly DirectProperty<TextTransition, string?> IncomingProperty =
        AvaloniaProperty.RegisterDirect<TextTransition, string?>(nameof(Incoming), o => o.Incoming);

    public static readonly DirectProperty<TextTransition, double> CurrentOffsetProperty =
        AvaloniaProperty.RegisterDirect<TextTransition, double>(nameof(CurrentOffset), o => o.CurrentOffset);

    public static readonly DirectProperty<TextTransition, double> IncomingOffsetProperty =
        AvaloniaProperty.RegisterDirect<TextTransition, double>(nameof(IncomingOffset), o => o.IncomingOffset);

    private const double LineHeight = 1.2d;

    private readonly AnimationController _roll;

    private bool _attached;
    private string? _current;
    private string? _incoming;
    private double _currentOffset;
    private double _incomingOffset;

    static TextTransition() =>
        AffectsMeasure<TextTransition>(CurrentProperty, FixedWidthProperty, FontSizeProperty);

    public TextTransition() => _roll = new AnimationController(this, OnRolled);

    /// <summary>
    /// Gets or sets the figure being shown.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets how long a roll takes.
    /// </summary>
    public TimeSpan TransitionDuration
    {
        get => GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the width to hold, or NaN to take it from the figure's length.
    /// </summary>
    public double FixedWidth
    {
        get => GetValue(FixedWidthProperty);
        set => SetValue(FixedWidthProperty, value);
    }

    /// <summary>
    /// Gets the figure on its way out, which is the one on show while nothing is rolling.
    /// </summary>
    public string? Current
    {
        get => _current;
        private set => SetAndRaise(CurrentProperty, ref _current, value);
    }

    /// <summary>
    /// Gets the figure on its way in, or null while nothing is rolling.
    /// </summary>
    public string? Incoming
    {
        get => _incoming;
        private set => SetAndRaise(IncomingProperty, ref _incoming, value);
    }

    /// <summary>
    /// Gets how far the outgoing figure has risen.
    /// </summary>
    public double CurrentOffset
    {
        get => _currentOffset;
        private set => SetAndRaise(CurrentOffsetProperty, ref _currentOffset, value);
    }

    /// <summary>
    /// Gets how far the incoming figure still has to rise.
    /// </summary>
    public double IncomingOffset
    {
        get => _incomingOffset;
        private set => SetAndRaise(IncomingOffsetProperty, ref _incomingOffset, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _attached = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _attached = false;

        _roll.Stop();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);

        // The width follows the outgoing figure, so a roll does not resize the box under it.
        var width = double.IsNaN(FixedWidth) ? (Current?.Length ?? 0) * FontSize / 1.4d : FixedWidth;

        return new Size(width, FontSize * LineHeight);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != TextProperty)
        {
            return;
        }

        var text = change.GetNewValue<string?>();

        // A roll needs frames, and there are none before there is a top level to ask, so a figure
        // arriving that early simply becomes the one on show. The original is in the same position:
        // its first figure comes from the constructor and only later ones roll.
        if (!_attached || Current is null)
        {
            Current = text;
            Incoming = null;

            return;
        }

        if (text == Current)
        {
            return;
        }

        Incoming = text;

        _roll.Duration = TransitionDuration;
        _roll.SetValue(0d);
        _roll.Forward();
    }

    private void OnRolled(double progress)
    {
        CurrentOffset = -progress * FontSize;
        IncomingOffset = FontSize - (progress * FontSize);

        if (progress < 1d)
        {
            return;
        }

        Current = Incoming;
        Incoming = null;

        CurrentOffset = 0d;

        _roll.SetValue(0d);
    }
}
