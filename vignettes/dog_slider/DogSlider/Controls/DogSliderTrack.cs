using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DogSlider.Controls;

/// <summary>
/// The line the dog walks along, which dips into an arc under the handle. Port of
/// <c>dog_slider_bg_painter.dart</c>.
/// </summary>
/// <remarks>
/// The line is drawn in two halves that meet at the bottom of the dip: the travelled part in the
/// accent colour, the rest in grey. The dip is an elliptical arc whose height is driven by the ball,
/// so pressing the ball down pushes the line down with it and letting go springs both back.
/// </remarks>
public sealed class DogSliderTrack : Control
{
    public static readonly StyledProperty<double> HandleXProperty =
        AvaloniaProperty.Register<DogSliderTrack, double>(nameof(HandleX), 230d);

    public static readonly StyledProperty<double> ArcScaleYProperty =
        AvaloniaProperty.Register<DogSliderTrack, double>(nameof(ArcScaleY), 1d);

    public static readonly StyledProperty<double> ArcRadiusProperty =
        AvaloniaProperty.Register<DogSliderTrack, double>(nameof(ArcRadius), 20d);

    public static readonly StyledProperty<double> BottomPaddingProperty =
        AvaloniaProperty.Register<DogSliderTrack, double>(nameof(BottomPadding), 10d);

    private const double StrokeWidth = 4d;

    private static readonly IPen AccentPen =
        new Pen(new SolidColorBrush(Color.FromRgb(0x35, 0x71, 0x71)), StrokeWidth);

    private static readonly IPen TrackPen =
        new Pen(new SolidColorBrush(Color.FromRgb(0xD1, 0xD0, 0xDA)), StrokeWidth);

    static DogSliderTrack() =>
        AffectsRender<DogSliderTrack>(
            HandleXProperty, ArcScaleYProperty, ArcRadiusProperty, BottomPaddingProperty);

    /// <summary>
    /// Gets or sets where along the line the handle sits.
    /// </summary>
    public double HandleX
    {
        get => GetValue(HandleXProperty);
        set => SetValue(HandleXProperty, value);
    }

    /// <summary>
    /// Gets or sets how deep the dip is, as a fraction of <see cref="ArcRadius"/>.
    /// </summary>
    public double ArcScaleY
    {
        get => GetValue(ArcScaleYProperty);
        set => SetValue(ArcScaleYProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the dip.
    /// </summary>
    public double ArcRadius
    {
        get => GetValue(ArcRadiusProperty);
        set => SetValue(ArcRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets how far the line sits above the bottom of the control.
    /// </summary>
    public double BottomPadding
    {
        get => GetValue(BottomPaddingProperty);
        set => SetValue(BottomPaddingProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Bounds.Size;

        if (size.Width <= 0d)
        {
            return;
        }

        var arcHeight = ArcScaleY * ArcRadius;
        var lineY = size.Height - ArcRadius - BottomPadding;
        var radius = new Size(ArcRadius, Math.Abs(arcHeight));

        // A dip that has been pushed the other way sweeps the opposite way round.
        var sweep = arcHeight < 0d ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;

        context.DrawGeometry(null, AccentPen, BuildHalf(
            new Point(0d, lineY),
            new Point(HandleX - ArcRadius, lineY),
            new Point(HandleX, lineY + arcHeight),
            radius,
            sweep));

        context.DrawGeometry(null, TrackPen, BuildHalf(
            new Point(HandleX, lineY + arcHeight),
            null,
            new Point(HandleX + ArcRadius, lineY),
            radius,
            sweep,
            new Point(size.Width, lineY)));
    }

    private static StreamGeometry BuildHalf(
        Point start,
        Point? lineTo,
        Point arcTo,
        Size radius,
        SweepDirection sweep,
        Point? tail = null)
    {
        var geometry = new StreamGeometry();

        using var figure = geometry.Open();

        figure.BeginFigure(start, isFilled: false);

        if (lineTo is { } corner)
        {
            figure.LineTo(corner);
        }

        figure.ArcTo(arcTo, radius, 0d, isLargeArc: false, sweep);

        if (tail is { } end)
        {
            figure.LineTo(end);
        }

        figure.EndFigure(isClosed: false);

        return geometry;
    }
}
