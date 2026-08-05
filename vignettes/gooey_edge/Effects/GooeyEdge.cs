using Avalonia;
using Avalonia.Media;

namespace GooeyEdge.Effects;

/// <summary>
/// A row of springy points that together form one wobbling, liquid-looking edge.
/// Port of <c>gooey_edge.dart</c>.
/// </summary>
/// <remarks>
/// Each point carries a horizontal velocity in a normalised space where x is how far the edge has
/// stretched (0 at rest, 1 fully across) and y is the position along the edge. Four forces act on
/// it every frame: a pull back to the near edge, a pull towards the far edge, a pull towards the
/// touch point that falls off with distance, and a pull towards each neighbour, the last of which
/// is what makes the line behave like a sheet rather than a set of independent springs.
/// </remarks>
public sealed class GooeyEdge
{
    private readonly GooeyPoint[] _points;

    private TimeSpan _lastTick;
    private bool _hasLastTick;

    /// <summary>
    /// Initializes a new edge made of <paramref name="count"/> points.
    /// </summary>
    public GooeyEdge(int count = 10, GooeyEdgeSide side = GooeyEdgeSide.Left)
    {
        Side = side;
        _points = new GooeyPoint[count];

        for (var i = 0; i < count; i++)
        {
            _points[i] = new GooeyPoint(0d, i / (double)(count - 1));
        }
    }

    /// <summary>
    /// Gets or sets which edge the line runs along.
    /// </summary>
    public GooeyEdgeSide Side { get; set; }

    /// <summary>
    /// Gets or sets how strongly points are pulled back to the near edge.
    /// </summary>
    public double EdgeTension { get; set; } = 0.01d;

    /// <summary>
    /// Gets or sets how strongly points are pulled on towards the far edge.
    /// </summary>
    public double FarEdgeTension { get; set; }

    /// <summary>
    /// Gets or sets how strongly points are pulled towards the touch point.
    /// </summary>
    public double TouchTension { get; set; } = 0.1d;

    /// <summary>
    /// Gets or sets how strongly each point is pulled towards its neighbours.
    /// </summary>
    public double PointTension { get; set; } = 0.25d;

    /// <summary>
    /// Gets or sets the per-frame velocity decay.
    /// </summary>
    public double Damping { get; set; } = 0.9d;

    /// <summary>
    /// Gets or sets how far along the edge the touch's influence reaches.
    /// </summary>
    public double MaxTouchDistance { get; set; } = 0.15d;

    /// <summary>
    /// Gets the touch position in the edge's own normalised space, or null when nothing is
    /// touching. Set through <see cref="ApplyTouchOffset(Point?, Size)"/>.
    /// </summary>
    public Point? TouchOffset { get; private set; }

    /// <summary>
    /// Returns every point to rest, clearing both position and velocity.
    /// </summary>
    public void Reset()
    {
        foreach (var point in _points)
        {
            point.X = 0d;
            point.VelocityX = 0d;
        }

        // The next tick starts a fresh clock, so it must not be measured against the old one.
        _hasLastTick = false;
    }

    /// <summary>
    /// Sets the touch position, rotating it into the edge's own space so the simulation only ever
    /// has to think in terms of a left-hand edge stretching rightwards.
    /// </summary>
    /// <param name="offset">The touch position in control coordinates, or null to release.</param>
    /// <param name="size">The size of the control the touch was measured against.</param>
    public void ApplyTouchOffset(Point? offset = null, Size size = default)
    {
        if (offset is not { } point || size.Width <= 0d || size.Height <= 0d)
        {
            TouchOffset = null;
            return;
        }

        var fraction = new Point(point.X / size.Width, point.Y / size.Height);

        TouchOffset = Side switch
        {
            GooeyEdgeSide.Left => fraction,
            GooeyEdgeSide.Right => new Point(1d - fraction.X, 1d - fraction.Y),
            GooeyEdgeSide.Top => new Point(fraction.Y, 1d - fraction.X),
            _ => new Point(1d - fraction.Y, fraction.X),
        };
    }

    /// <summary>
    /// Advances the simulation. <paramref name="elapsed"/> is the ticker's running total, not a
    /// delta, matching the <c>Duration</c> Flutter's <c>Ticker</c> hands out.
    /// </summary>
    public void Tick(TimeSpan elapsed)
    {
        if (_points.Length == 0)
        {
            return;
        }

        // Time is measured in 60ths of a second so the tensions read as per-frame values, and is
        // capped at 1.5 frames so a stalled frame cannot fling the points across the screen. The
        // lower bound matters too: a caller that restarts its clock would otherwise hand back a
        // negative step, which drives the springs the wrong way and tears the edge apart.
        var t = _hasLastTick
            ? Math.Clamp((elapsed - _lastTick).TotalSeconds * 60d, 0d, 1.5d)
            : 0d;

        _lastTick = elapsed;
        _hasLastTick = true;

        var dampingT = Math.Pow(Damping, t);
        var touch = TouchOffset;

        // Velocities are accumulated first and positions moved afterwards, so every point sees the
        // same set of neighbour positions no matter which end the loop starts from.
        for (var i = 0; i < _points.Length; i++)
        {
            var point = _points[i];

            point.VelocityX -= point.X * EdgeTension * t;
            point.VelocityX += (1d - point.X) * FarEdgeTension * t;

            if (touch is { } touchPoint)
            {
                var ratio = Math.Max(0d, 1d - (Math.Abs(point.Y - touchPoint.Y) / MaxTouchDistance));
                point.VelocityX += (touchPoint.X - point.X) * TouchTension * ratio * t;
            }

            if (i > 0)
            {
                AddPointTension(point, _points[i - 1].X, t);
            }

            if (i < _points.Length - 1)
            {
                AddPointTension(point, _points[i + 1].X, t);
            }

            point.VelocityX *= dampingT;
        }

        foreach (var point in _points)
        {
            point.X += point.VelocityX * t;
        }
    }

    /// <summary>
    /// Builds the region on the near side of the edge, ready to be used as a clip.
    /// </summary>
    /// <param name="size">The size of the control being clipped.</param>
    /// <param name="margin">
    /// How far past the control the region extends, so the stroke of the edge is never cut off.
    /// </param>
    public Geometry BuildGeometry(Size size, double margin = 0d)
    {
        var geometry = new StreamGeometry();

        if (_points.Length < 2)
        {
            return geometry;
        }

        using var context = geometry.Open();

        var transform = new EdgeTransform(this, size, margin);

        // The far corners sit a whole margin past the edge, so the region always runs clear of the
        // control it is closing off.
        var outside = -margin;

        context.BeginFigure(transform.Apply(outside, 1d), isFilled: true);
        context.LineTo(transform.Apply(outside, 0d));
        context.LineTo(transform.Apply(_points[0]));

        var pt = transform.Apply(_points[0]);
        var pt1 = transform.Apply(_points[1]);

        context.LineTo(new Point(pt.X + ((pt1.X - pt.X) / 2d), pt.Y + ((pt1.Y - pt.Y) / 2d)));

        // Every interior point becomes a bezier control point, with the curve running between the
        // midpoints of consecutive segments. That is what keeps the line smooth rather than faceted.
        for (var i = 2; i < _points.Length; i++)
        {
            pt = pt1;
            pt1 = transform.Apply(_points[i]);

            var mid = new Point(pt.X + ((pt1.X - pt.X) / 2d), pt.Y + ((pt1.Y - pt.Y) / 2d));
            context.QuadraticBezierTo(pt, mid);
        }

        context.LineTo(pt1);
        context.EndFigure(isClosed: true);

        return geometry;
    }

    private void AddPointTension(GooeyPoint point, double x, double t) =>
        point.VelocityX += (x - point.X) * PointTension * t;

    /// <summary>
    /// A single point of the edge, in the normalised space described on the class.
    /// </summary>
    private sealed class GooeyPoint(double x, double y)
    {
        public double X { get; set; } = x;

        public double Y { get; } = y;

        public double VelocityX { get; set; }
    }

    /// <summary>
    /// Maps the edge's normalised space onto the control, rotating it a quarter turn at a time so
    /// the same simulation serves all four sides. Equivalent to <c>_getTransform</c>.
    /// </summary>
    private readonly struct EdgeTransform
    {
        private readonly GooeyEdgeSide _side;
        private readonly double _width;
        private readonly double _height;
        private readonly double _margin;

        public EdgeTransform(GooeyEdge edge, Size size, double margin)
        {
            _side = edge.Side;
            _margin = margin;

            var vertical = _side is GooeyEdgeSide.Top or GooeyEdgeSide.Bottom;

            _width = (vertical ? size.Height : size.Width) + (margin * 2d);
            _height = (vertical ? size.Width : size.Height) + (margin * 2d);
        }

        public Point Apply(double x, double y) => _side switch
        {
            GooeyEdgeSide.Left => new Point((_width * x) - _margin, _height * y),
            GooeyEdgeSide.Right => new Point((_width * (1d - x)) - _margin, _height * (1d - y)),
            GooeyEdgeSide.Top => new Point((_width * (1d - y)) - _margin, _height * x),
            _ => new Point((_width * y) - _margin, _height * (1d - x)),
        };

        public Point Apply(GooeyPoint point) => Apply(point.X, point.Y);
    }
}
