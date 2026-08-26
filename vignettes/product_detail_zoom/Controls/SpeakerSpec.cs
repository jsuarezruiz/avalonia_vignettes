using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using AvaloniaVignettes.Shared.Animation;

namespace ProductDetailZoom.Controls;

/// <summary>
/// One callout on the zoomed speaker: a label, a line that draws itself downwards, and a dot at the
/// end of it. Port of <c>_SpeakerAttribute</c>, renamed because in C# the <c>Attribute</c> suffix
/// belongs to <see cref="System.Attribute"/>.
/// </summary>
/// <remarks>
/// The three parts run on their own slices of the same clock: the dot is in first, the label slides
/// down into place while the line is still growing, so a callout assembles rather than appearing.
/// </remarks>
public sealed class SpeakerSpec : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SpeakerSpec, string?>(nameof(Text));

    public static readonly StyledProperty<double> LineLengthProperty =
        AvaloniaProperty.Register<SpeakerSpec, double>(nameof(LineLength), 150d);

    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<SpeakerSpec, double>(nameof(Progress));

    public static readonly DirectProperty<SpeakerSpec, double> LineExtentProperty =
        AvaloniaProperty.RegisterDirect<SpeakerSpec, double>(nameof(LineExtent), o => o.LineExtent);

    public static readonly DirectProperty<SpeakerSpec, Thickness> DotOffsetProperty =
        AvaloniaProperty.RegisterDirect<SpeakerSpec, Thickness>(nameof(DotOffset), o => o.DotOffset);

    public static readonly DirectProperty<SpeakerSpec, double> LabelOffsetProperty =
        AvaloniaProperty.RegisterDirect<SpeakerSpec, double>(nameof(LabelOffset), o => o.LabelOffset);

    public static readonly DirectProperty<SpeakerSpec, double> LabelOpacityProperty =
        AvaloniaProperty.RegisterDirect<SpeakerSpec, double>(nameof(LabelOpacity), o => o.LabelOpacity);

    public static readonly DirectProperty<SpeakerSpec, double> DotOpacityProperty =
        AvaloniaProperty.RegisterDirect<SpeakerSpec, double>(nameof(DotOpacity), o => o.DotOpacity);

    private const double LineTop = 17d;

    private const double LineLeft = 5d;

    private const double LabelHeight = 18d;

    private static readonly Easing LineGrowth = FlutterEasings.EaseInOutQuad;
    private static readonly Easing LabelSlide = new IntervalEasing(0.2d, 1d);
    private static readonly Easing LabelFade = new IntervalEasing(0.15d, 0.95d);
    private static readonly Easing DotFade = new IntervalEasing(0d, 0.3d);

    private double _lineExtent;
    private Thickness _dotOffset = new(LineLeft, LineTop, 0d, 0d);
    private double _labelOffset = -LabelHeight / 2d;
    private double _labelOpacity;
    private double _dotOpacity;

    static SpeakerSpec()
    {
        ProgressProperty.Changed.AddClassHandler<SpeakerSpec>((x, _) => x.Refresh());
        LineLengthProperty.Changed.AddClassHandler<SpeakerSpec>((x, _) => x.Refresh());
    }

    /// <summary>
    /// Gets or sets the label.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets how long the line grows to.
    /// </summary>
    public double LineLength
    {
        get => GetValue(LineLengthProperty);
        set => SetValue(LineLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets how far through its arrival this callout is, from 0 to 1.
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// Gets how long the line currently is.
    /// </summary>
    public double LineExtent
    {
        get => _lineExtent;
        private set => SetAndRaise(LineExtentProperty, ref _lineExtent, value);
    }

    /// <summary>
    /// Gets where the dot sits, as a margin from the callout's top-left.
    /// </summary>
    public Thickness DotOffset
    {
        get => _dotOffset;
        private set => SetAndRaise(DotOffsetProperty, ref _dotOffset, value);
    }

    /// <summary>
    /// Gets how far above its resting place the label currently is.
    /// </summary>
    public double LabelOffset
    {
        get => _labelOffset;
        private set => SetAndRaise(LabelOffsetProperty, ref _labelOffset, value);
    }

    /// <summary>
    /// Gets the label's opacity.
    /// </summary>
    public double LabelOpacity
    {
        get => _labelOpacity;
        private set => SetAndRaise(LabelOpacityProperty, ref _labelOpacity, value);
    }

    /// <summary>
    /// Gets the dot's opacity.
    /// </summary>
    public double DotOpacity
    {
        get => _dotOpacity;
        private set => SetAndRaise(DotOpacityProperty, ref _dotOpacity, value);
    }

    private void Refresh()
    {
        var progress = Math.Clamp(Progress, 0d, 1d);
        var extent = LineLength * LineGrowth.Ease(progress);

        LineExtent = extent;
        DotOffset = new Thickness(LineLeft, LineTop + extent, 0d, 0d);
        LabelOffset = -0.5d * (1d - LabelSlide.Ease(progress)) * LabelHeight;
        LabelOpacity = LabelFade.Ease(progress);
        DotOpacity = DotFade.Ease(progress);
    }
}
