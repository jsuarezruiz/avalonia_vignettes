using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;

namespace FluidNavBar.Controls;

/// <summary>
/// The bar itself: a filled pane whose top edge dips under the selected button and sloshes as that
/// dip travels. Port of <c>_BackgroundCurvePainter</c>.
/// </summary>
/// <remarks>
/// The edge is two cubic curves either side of a flat span. Every number that shapes them is
/// interpolated from <see cref="Depth"/>, each through a different remapping: the corner radius,
/// both control-point offsets, how deep the dip goes and how wide its flat bottom is. That spread
/// of timings, rather than any physics, is what makes the edge behave like a liquid.
/// </remarks>
public sealed class FluidPane : Control
{
    /// <summary>
    /// Defines the <see cref="DipX"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DipXProperty =
        AvaloniaProperty.Register<FluidPane, double>(nameof(DipX));

    /// <summary>
    /// Defines the <see cref="Depth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DepthProperty =
        AvaloniaProperty.Register<FluidPane, double>(nameof(Depth), 1d);

    /// <summary>
    /// Defines the <see cref="Fill"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<FluidPane, IBrush?>(nameof(Fill), Brushes.White);

    private const double RadiusTop = 54d;
    private const double RadiusBottom = 44d;
    private const double HorizontalControlTop = 0.6d;
    private const double HorizontalControlBottom = 0.5d;
    private const double PointControlTop = 0.35d;
    private const double PointControlBottom = 0.85d;
    private const double TopY = -10d;
    private const double BottomY = 54d;
    private const double TopDistance = 0d;
    private const double BottomDistance = 6d;

    private static readonly Easing Shape = new LinearPointEasing(0.5d, 2d);
    private static readonly Easing AnchorTiming = new LinearPointEasing(0.5d, 0.75d);
    private static readonly Easing DipTiming = new LinearPointEasing(0.5d, 0.8d);
    private static readonly Easing DepthTiming = new LinearPointEasing(0.2d, 0.7d);
    private static readonly Easing WidthTiming = new LinearPointEasing(0.5d, 0d);

    static FluidPane() => AffectsRender<FluidPane>(DipXProperty, DepthProperty, FillProperty);

    /// <summary>
    /// Gets or sets where along the pane the dip sits, in pixels.
    /// </summary>
    public double DipX
    {
        get => GetValue(DipXProperty);
        set => SetValue(DipXProperty, value);
    }

    /// <summary>
    /// Gets or sets how settled the edge is, from 0 while the dip is travelling to 1 at rest.
    /// </summary>
    public double Depth
    {
        get => GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush the pane is filled with.
    /// </summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var size = Bounds.Size;

        if (size.Width <= 0d || size.Height <= 0d || Fill is not { } fill)
        {
            return;
        }

        // Halved because the remapping runs up to two before coming back down, which is what gives
        // the edge its overshoot on the way through.
        var norm = Shape.Ease(Depth) / 2d;

        var radius = Lerp(RadiusTop, RadiusBottom, norm);

        // Where the curve leaves the pane's top edge.
        var anchorControl = Lerp(
            radius * HorizontalControlTop,
            radius * HorizontalControlBottom,
            AnchorTiming.Ease(norm));

        // How far the curve reaches back towards the dip, which is what sharpens or softens it.
        var dipControl = Lerp(radius * PointControlTop, radius * PointControlBottom, DipTiming.Ease(norm));

        var y = Lerp(TopY, BottomY, DepthTiming.Ease(norm));
        var width = Lerp(TopDistance, BottomDistance, WidthTiming.Ease(norm));

        var left = DipX - (width / 2d);
        var right = DipX + (width / 2d);

        var geometry = new StreamGeometry();

        using (var path = geometry.Open())
        {
            path.BeginFigure(new Point(0d, 0d), isFilled: true);
            path.LineTo(new Point(left - radius, 0d));
            path.CubicBezierTo(
                new Point(left - radius + anchorControl, 0d),
                new Point(left - dipControl, y),
                new Point(left, y));
            path.LineTo(new Point(right, y));
            path.CubicBezierTo(
                new Point(right + dipControl, y),
                new Point(right + radius - anchorControl, 0d),
                new Point(right + radius, 0d));
            path.LineTo(new Point(size.Width, 0d));
            path.LineTo(new Point(size.Width, size.Height));
            path.LineTo(new Point(0d, size.Height));
            path.EndFigure(isClosed: true);
        }

        context.DrawGeometry(fill, null, geometry);
    }

    private static double Lerp(double from, double to, double progress) => from + ((to - from) * progress);
}
