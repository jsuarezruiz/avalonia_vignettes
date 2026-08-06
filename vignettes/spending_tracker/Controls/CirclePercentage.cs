using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using SpendingTracker.Animation;

namespace SpendingTracker.Controls;

/// <summary>
/// One expense category: a name, a ring and the share it takes. Port of
/// <c>circle_percentage_widget.dart</c>.
/// </summary>
/// <remarks>
/// The ring counts up to its share rather than appearing at it, and the count is timed for a full
/// turn, so a small share fills quickly and a large one takes its time. A new share is picked up
/// from wherever the ring has got to.
/// </remarks>
public sealed class CirclePercentage : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<CirclePercentage, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Percent"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PercentProperty =
        AvaloniaProperty.Register<CirclePercentage, double>(nameof(Percent));

    /// <summary>
    /// Defines the <see cref="Color0"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> Color0Property =
        PercentageRing.Color0Property.AddOwner<CirclePercentage>();

    /// <summary>
    /// Defines the <see cref="Color1"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> Color1Property =
        PercentageRing.Color1Property.AddOwner<CirclePercentage>();

    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly DirectProperty<CirclePercentage, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<CirclePercentage, double>(nameof(Value), o => o.Value);

    /// <summary>
    /// Defines the <see cref="Label"/> property.
    /// </summary>
    public static readonly DirectProperty<CirclePercentage, string> LabelProperty =
        AvaloniaProperty.RegisterDirect<CirclePercentage, string>(nameof(Label), o => o.Label);

    private readonly InterpolationAnimation _count;

    private bool _attached;
    private double _value;
    private string _label = "0%";

    /// <summary>
    /// Initializes a new instance of the <see cref="CirclePercentage"/> class.
    /// </summary>
    public CirclePercentage() =>
        _count = new InterpolationAnimation(this, OnCounted)
        {
            Duration = TimeSpan.FromMilliseconds(2400),
        };

    /// <summary>
    /// Gets or sets the category's name.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the share the ring counts up to, from 0 to 1.
    /// </summary>
    public double Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour the ring starts at.
    /// </summary>
    public Color Color0
    {
        get => GetValue(Color0Property);
        set => SetValue(Color0Property, value);
    }

    /// <summary>
    /// Gets or sets the colour the ring ends at.
    /// </summary>
    public Color Color1
    {
        get => GetValue(Color1Property);
        set => SetValue(Color1Property, value);
    }

    /// <summary>
    /// Gets how far the ring has counted.
    /// </summary>
    public double Value
    {
        get => _value;
        private set => SetAndRaise(ValueProperty, ref _value, value);
    }

    /// <summary>
    /// Gets the share written out, as the ring shows it.
    /// </summary>
    public string Label
    {
        get => _label;
        private set => SetAndRaise(LabelProperty, ref _label, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // A count needs frames, so it can only start once there is a top level to ask for them.
        _attached = true;

        _count.AnimateTo(Percent);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _attached = false;

        _count.Stop();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PercentProperty && _attached)
        {
            _count.AnimateTo(Percent);
        }
    }

    private void OnCounted(double value)
    {
        Value = value;
        Label = $"{(int)(value * 100d)}%";
    }
}
