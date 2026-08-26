using Avalonia;
using Avalonia.Media;

namespace FluidNavBar.Controls;

/// <summary>
/// One icon: a handful of contours that can be drawn whole, or up to a fraction of their combined
/// length. Port of <c>fluid_icon_data.dart</c>.
/// </summary>
/// <remarks>
/// The originals are <c>ui.Path</c>s measured with <c>computeMetrics</c> and cut with
/// <c>extractPath</c>. Avalonia exposes no path measurement at all, so each contour is held as the
/// polyline it flattens to instead, dense enough that the difference is far below a pixel, and
/// enough to make both the length and the cut trivial arithmetic. Round joins hide the facets, and
/// the original strokes these with round joins anyway.
/// </remarks>
public sealed class FluidIconData
{
    private readonly Contour[][] _paths;

    private FluidIconData(params Contour[][] paths) => _paths = paths;

    /// <summary>
    /// A house: a body and a roof.
    /// </summary>
    public static FluidIconData Home { get; } = new(
        [Contour.RoundedRectangle(-10d, -2d, 10d, 10d, 2d)],
        [Contour.Closed(new Point(-14d, -2d), new Point(14d, -2d), new Point(0d, -16d))]);

    /// <summary>
    /// A person: a head, and the shoulders below it.
    /// </summary>
    public static FluidIconData User { get; } = new(
        [Contour.Arc(new Rect(-5d, -16d, 10d, 10d), 0d, 1.9d * Math.PI)],
        [Contour.Arc(new Rect(-10d, 0d, 20d, 20d), 0d, -1d * Math.PI)]);

    /// <summary>
    /// Four panes in a square.
    /// </summary>
    public static FluidIconData Window { get; } = new(
        [Contour.RoundedRectangle(-12d, -12d, -2d, -2d, 2d)],
        [Contour.RoundedRectangle(2d, -12d, 12d, -2d, 2d)],
        [Contour.RoundedRectangle(-12d, 2d, -2d, 12d, 2d)],
        [Contour.RoundedRectangle(2d, 2d, 12d, 12d, 2d)]);

    /// <summary>
    /// Builds the geometry for this icon, drawn from the start of each of its paths up to
    /// <paramref name="fill"/> of that path's length.
    /// </summary>
    /// <remarks>
    /// Each path fills on its own clock rather than the icon filling as one, which is what the
    /// original does by cutting every path to the same fraction.
    /// </remarks>
    public Geometry Build(double fill)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            foreach (var path in _paths)
            {
                var length = path.Sum(x => x.Length);
                var cut = length * fill;

                foreach (var contour in path)
                {
                    if (cut <= 0d)
                    {
                        break;
                    }

                    contour.Write(context, Math.Min(cut, contour.Length));
                    cut -= contour.Length;
                }
            }
        }

        return geometry;
    }

    private sealed class Contour
    {
        private readonly Point[] _points;
        private readonly double[] _lengths;

        private Contour(Point[] points)
        {
            _points = points;
            _lengths = new double[points.Length];

            for (var i = 1; i < points.Length; i++)
            {
                var step = points[i] - points[i - 1];

                _lengths[i] = _lengths[i - 1] + Math.Sqrt((step.X * step.X) + (step.Y * step.Y));
            }
        }

        /// <summary>
        /// Gets the total length along this contour.
        /// </summary>
        public double Length => _lengths[^1];

        /// <summary>
        /// An open run through the given points.
        /// </summary>
        public static Contour Open(params Point[] points) => new(points);

        /// <summary>
        /// A run through the given points and back to the first.
        /// </summary>
        public static Contour Closed(params Point[] points) => new([.. points, points[0]]);

        /// <summary>
        /// A rounded rectangle, starting where the top edge leaves the upper-left corner and running
        /// clockwise, where Skia's <c>addRRect</c> starts, which is what decides where the fill
        /// begins.
        /// </summary>
        public static Contour RoundedRectangle(double left, double top, double right, double bottom, double radius)
        {
            List<Point> points = [new(left + radius, top)];

            Corner(right - radius, top + radius, -Math.PI / 2d);
            Corner(right - radius, bottom - radius, 0d);
            Corner(left + radius, bottom - radius, Math.PI / 2d);
            Corner(left + radius, top + radius, Math.PI);

            points.Add(points[0]);

            return new Contour([.. points]);

            void Corner(double centreX, double centreY, double from)
            {
                const int steps = 6;

                points.Add(new Point(centreX + (radius * Math.Cos(from)), centreY + (radius * Math.Sin(from))));

                for (var i = 1; i <= steps; i++)
                {
                    var angle = from + (Math.PI / 2d * i / steps);

                    points.Add(new Point(centreX + (radius * Math.Cos(angle)), centreY + (radius * Math.Sin(angle))));
                }
            }
        }

        /// <summary>
        /// An arc of an ellipse, swept from <paramref name="start"/> by <paramref name="sweep"/>
        /// radians. Angles run clockwise from the positive x axis, as Flutter's <c>arcTo</c> does.
        /// </summary>
        public static Contour Arc(Rect bounds, double start, double sweep)
        {
            var centre = bounds.Center;
            var radiusX = bounds.Width / 2d;
            var radiusY = bounds.Height / 2d;

            // One segment per two degrees of sweep, which holds these radii well inside a pixel.
            var steps = Math.Max(2, (int)Math.Ceiling(Math.Abs(sweep) / (Math.PI / 90d)));
            var points = new Point[steps + 1];

            for (var i = 0; i <= steps; i++)
            {
                var angle = start + (sweep * i / steps);

                points[i] = new Point(
                    centre.X + (radiusX * Math.Cos(angle)),
                    centre.Y + (radiusY * Math.Sin(angle)));
            }

            return new Contour(points);
        }

        /// <summary>
        /// Writes this contour into <paramref name="context"/>, cut at <paramref name="cut"/>.
        /// </summary>
        public void Write(StreamGeometryContext context, double cut)
        {
            context.BeginFigure(_points[0], isFilled: false);

            for (var i = 1; i < _points.Length; i++)
            {
                if (_lengths[i] <= cut)
                {
                    context.LineTo(_points[i]);
                    continue;
                }

                // The cut falls inside this segment, so it ends part of the way along it.
                var span = _lengths[i] - _lengths[i - 1];
                var along = span > 0d ? (cut - _lengths[i - 1]) / span : 0d;

                context.LineTo(_points[i - 1] + ((_points[i] - _points[i - 1]) * along));
                break;
            }

            context.EndFigure(isClosed: false);
        }
    }
}
