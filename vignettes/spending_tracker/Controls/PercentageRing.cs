using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace SpendingTracker.Controls;

/// <summary>
/// A ring filled anticlockwise from the top. Port of <c>circle_percentage_painter.dart</c>.
/// </summary>
public sealed class PercentageRing : Control
{
    /// <summary>
    /// Defines the <see cref="Percent"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PercentProperty =
        AvaloniaProperty.Register<PercentageRing, double>(nameof(Percent));

    /// <summary>
    /// Defines the <see cref="Color0"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> Color0Property =
        AvaloniaProperty.Register<PercentageRing, Color>(nameof(Color0), Colors.White);

    /// <summary>
    /// Defines the <see cref="Color1"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> Color1Property =
        AvaloniaProperty.Register<PercentageRing, Color>(nameof(Color1), Colors.Transparent);

    /// <summary>
    /// How wide the ring is drawn, before the app scale.
    /// </summary>
    private const double DesignSize = 42d;

    /// <summary>
    /// How thick the ring is drawn, before the app scale.
    /// </summary>
    private const double DesignThickness = 5d;

    private static readonly IImmutableBrush TrackBrush = new ImmutableSolidColorBrush(Color.Parse("#FF5B668C"));

    static PercentageRing() =>
        AffectsRender<PercentageRing>(PercentProperty, Color0Property, Color1Property);

    // The ring redraws every frame while it fills, but only the arc changes: the pens and the
    // gradient depend on the size and the two colours, so they are kept until one of those moves.
    private ImmutablePen? _trackPen;
    private ImmutablePen? _fillPen;
    private (double Thickness, double Height, Color Color0, Color Color1) _pensFor;

    /// <summary>
    /// Gets or sets how much of the ring is filled, from 0 to 1.
    /// </summary>
    public double Percent
    {
        get => GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour the fill starts at, at the top of the ring.
    /// </summary>
    public Color Color0
    {
        get => GetValue(Color0Property);
        set => SetValue(Color0Property, value);
    }

    /// <summary>
    /// Gets or sets the colour the fill ends at, at the bottom of the ring.
    /// </summary>
    public Color Color1
    {
        get => GetValue(Color1Property);
        set => SetValue(Color1Property, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Bounds.Size;

        if (size.Width <= 0d || size.Height <= 0d)
        {
            return;
        }

        var radiusX = size.Width / 2d;
        var radiusY = size.Height / 2d;
        var centre = new Point(radiusX, radiusY);
        var thickness = DesignThickness * AppScale.Of(this);

        var wanted = (thickness, size.Height, Color0, Color1);

        if (_trackPen is null || _fillPen is null || wanted != _pensFor)
        {
            _pensFor = wanted;
            _trackPen = new ImmutablePen(TrackBrush, thickness, lineCap: PenLineCap.Square);

            var fill = new ImmutableLinearGradientBrush(
                [new ImmutableGradientStop(0d, Color0), new ImmutableGradientStop(1d, Color1)],
                startPoint: new RelativePoint(0d, 0d, RelativeUnit.Absolute),
                endPoint: new RelativePoint(0d, size.Height, RelativeUnit.Absolute));

            _fillPen = new ImmutablePen(fill, thickness, lineCap: PenLineCap.Square);
        }

        context.DrawEllipse(null, _trackPen, centre, radiusX, radiusY);

        if (Percent <= 0d)
        {
            return;
        }

        context.DrawGeometry(null, _fillPen, Arc(centre, radiusX, radiusY, Percent));
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = DesignSize * AppScale.Of(this);

        return new Size(size, size);
    }

    /// <summary>
    /// Traces the filled part of the ring, starting at the top and going anticlockwise.
    /// </summary>
    private static StreamGeometry Arc(Point centre, double radiusX, double radiusY, double percent)
    {
        const double start = -Math.PI / 2d;

        var sweep = -2d * Math.PI * percent;
        var geometry = new StreamGeometry();

        // A quarter turn at a time, so no segment is ever large enough for an arc's two solutions to
        // be ambiguous, and a full ring stays expressible.
        var steps = (int)Math.Ceiling(Math.Abs(sweep) / (Math.PI / 2d));
        var step = sweep / steps;
        var angle = start;

        using (var sink = geometry.Open())
        {
            sink.BeginFigure(At(angle), isFilled: false);

            for (var i = 0; i < steps; i++)
            {
                angle += step;

                sink.ArcTo(
                    At(angle),
                    new Size(radiusX, radiusY),
                    0d,
                    isLargeArc: false,
                    SweepDirection.CounterClockwise);
            }

            sink.EndFigure(isClosed: false);
        }

        return geometry;

        Point At(double value) =>
            new(centre.X + (radiusX * Math.Cos(value)), centre.Y + (radiusY * Math.Sin(value)));
    }
}
