using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaVignettes.Shared.Animation;
using AvaloniaVignettes.Shared.Media;
using ParallaxTravelCardsHero.Models;

namespace ParallaxTravelCardsHero.Controls;

/// <summary>
/// Everything in the scenery that is drawn rather than written: the road, the clouds, the skyline
/// and its trees, and the leaves blowing past. Port of the layers <c>city_scenery.dart</c> stacks
/// above its card copy.
/// </summary>
/// <remarks>
/// Every layer is placed from the <em>screen</em> size, not from this control's own, because that is
/// what the original reads out of <c>MediaQuery</c>. The control's bounds are the box the card
/// currently occupies, which changes throughout the flight while the artwork keeps its scale. That
/// difference is the whole reason the card appears to open onto a scene rather than merely grow.
/// <para>
/// The clouds and the leaves run on their own clocks, independent of
/// <see cref="AnimationValue"/>, and advance by a fixed amount per frame exactly as the original's
/// tickers do.
/// </para>
/// </remarks>
public sealed class SceneryLayers : Control
{
    public static readonly StyledProperty<double> AnimationValueProperty =
        AvaloniaProperty.Register<SceneryLayers, double>(nameof(AnimationValue));

    public static readonly StyledProperty<double> ScreenWidthProperty =
        AvaloniaProperty.Register<SceneryLayers, double>(nameof(ScreenWidth));

    public static readonly StyledProperty<double> ScreenHeightProperty =
        AvaloniaProperty.Register<SceneryLayers, double>(nameof(ScreenHeight));

    public static readonly StyledProperty<City?> CityProperty =
        AvaloniaProperty.Register<SceneryLayers, City?>(nameof(City));

    private const double RoadScale = 0.55d * 0.2d;

    private static readonly Leaf[] Leaves =
    [
        new(RotationScale: 1.5d, Travel: Drift(FlutterEasings.EaseInOutSine), Path: t => (Math.Sin(t) * 15d) + 200d),
        new(RotationScale: 1.7d, Travel: Drift(FlutterEasings.LinearToEaseOut), Path: t => (-Math.Cos(t) * 30d) + 130d),
        new(RotationScale: 1.2d, Travel: Drift(FlutterEasings.Ease), Path: t => (Math.Atan(t) * 10d) + 150d),
    ];

    private static readonly Easing RoadFade = new IntervalEasing(0.7d, 1d, FlutterEasings.EaseIn);
    private static readonly Easing CitySize = new IntervalEasing(0.25d, 1d, FlutterEasings.EaseIn);
    private static readonly Easing CityPosition = new IntervalEasing(0.5d, 1d, FlutterEasings.EaseIn);
    private static readonly Easing TreesFade = new IntervalEasing(0.75d, 1d, FlutterEasings.EaseIn);

    private FrameTicker? _ticker;

    static SceneryLayers() =>
        AffectsRender<SceneryLayers>(
            AnimationValueProperty,
            ScreenWidthProperty,
            ScreenHeightProperty,
            CityProperty);

    /// <summary>
    /// Gets or sets how far the card has opened, from 0 for closed to 1 for the full scene.
    /// </summary>
    public double AnimationValue
    {
        get => GetValue(AnimationValueProperty);
        set => SetValue(AnimationValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the screen width, the equivalent of <c>MediaQuery.size.width</c>.
    /// </summary>
    public double ScreenWidth
    {
        get => GetValue(ScreenWidthProperty);
        set => SetValue(ScreenWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the screen height, the equivalent of <c>MediaQuery.size.height</c>.
    /// </summary>
    public double ScreenHeight
    {
        get => GetValue(ScreenHeightProperty);
        set => SetValue(ScreenHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the destination whose skyline is drawn.
    /// </summary>
    public City? City
    {
        get => GetValue(CityProperty);
        set => SetValue(CityProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds.Size;

        if (bounds.Width <= 0d || bounds.Height <= 0d || City is not { } city)
        {
            return;
        }

        var value = AnimationValue;

        // The original stacks these in this order, and the order is load-bearing: the trees stand in
        // front of the skyline, and the leaves blow in front of everything.
        DrawRoad(context, bounds, value);
        DrawClouds(context, bounds, value);
        DrawCityAndTrees(context, bounds, value, city);
        DrawLeaves(context, bounds, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _ticker ??= new FrameTicker(this, _ => Advance());
        _ticker.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _ticker?.Stop();
        SceneryClock.Release(this);
    }

    /// <summary>
    /// Scales a bitmap to fit inside <paramref name="available"/> without distorting it, which is
    /// what an unsized Flutter <c>Image</c> does with the constraints it is handed.
    /// </summary>
    private static Size Contain(Bitmap bitmap, Size available)
    {
        var scale = Math.Min(available.Width / bitmap.Size.Width, available.Height / bitmap.Size.Height);

        return scale >= 1d ? bitmap.Size : bitmap.Size * scale;
    }

    private static Size ForWidth(Bitmap bitmap, double width) =>
        new(width, width * bitmap.Size.Height / bitmap.Size.Width);

    private static double Lerp(double from, double to, double progress) => from + ((to - from) * progress);

    private void DrawRoad(DrawingContext context, Size bounds, double value)
    {
        var opacity = RoadFade.Ease(value);

        if (opacity <= 0d)
        {
            return;
        }

        // SizeTransition reveals a window of the child's height; the child itself is a Center, so it
        // is as tall as the card.
        var windowHeight = bounds.Height * value;

        // The window is centred in the card, then slid by a fraction of its own height.
        var windowTop = ((bounds.Height - windowHeight) / 2d) - (1.4d * (1d - value) * windowHeight);

        // The road sits at the bottom of the child, scaled to a height and centred horizontally.
        var height = (ScreenHeight * RoadScale) - 5d;
        var size = new Size(height * SceneryAssets.Road.Size.Width / SceneryAssets.Road.Size.Height, height);
        var destination = new Rect(
            (bounds.Width - size.Width) / 2d,
            windowTop + bounds.Height - height,
            size.Width,
            size.Height);

        using var _ = context.PushOpacity(opacity);
        using var __ = context.PushClip(new Rect(0d, windowTop, bounds.Width, windowHeight));

        context.DrawImage(SceneryAssets.Road, destination);
    }

    private void DrawClouds(DrawingContext context, Size bounds, double value)
    {
        if (value <= 0d)
        {
            return;
        }

        var drift = Lerp(-ScreenWidth * 0.1d, ScreenWidth * 1.8d, SceneryClock.CloudProgress);

        var large = ForWidth(SceneryAssets.CloudLarge, ScreenWidth * 0.2d);
        var small = ForWidth(SceneryAssets.CloudSmall, ScreenWidth * 0.15d);

        using var _ = context.PushOpacity(value);
        using var __ = context.PushClip(new Rect(bounds));

        context.DrawImage(
            SceneryAssets.CloudLarge,
            new Rect(new Point(drift - (ScreenWidth * 0.65d), ScreenHeight * 0.065d), large));

        context.DrawImage(
            SceneryAssets.CloudSmall,
            new Rect(new Point(drift * 0.5d, ScreenHeight * 0.12d), small));
    }

    private void DrawCityAndTrees(DrawingContext context, Size bounds, double value, City city)
    {
        var sizeProgress = CitySize.Ease(value);
        var size = new Size(
            Lerp(ScreenWidth * 0.55d, ScreenWidth, sizeProgress),
            Lerp(ScreenHeight * 0.24d, ScreenHeight * 0.35d, sizeProgress));

        // The skyline starts raised and drops to its resting place over the second half.
        var offsetY = Lerp(-ScreenHeight * 0.112d, 0d, CityPosition.Ease(value));

        using var _ = context.PushClip(new Rect(bounds));
        using var __ = context.PushTransform(Matrix.CreateTranslation(0d, offsetY));

        DrawCityImage(context, bounds, size, city);

        var treesOpacity = TreesFade.Ease(value);

        if (treesOpacity > 0d)
        {
            using var ___ = context.PushOpacity(treesOpacity);

            DrawTrees(context, bounds);
        }
    }

    private static void DrawCityImage(DrawingContext context, Size bounds, Size size, City city)
    {
        var ground = ForWidth(SceneryAssets.Ground, size.Width);

        // The skyline and its ground are a column, centred in the card as a unit.
        var left = (bounds.Width - size.Width) / 2d;
        var top = (bounds.Height - (size.Height + ground.Height)) / 2d;

        foreach (var layer in (Bitmap[])[city.BackImage, city.MiddleImage, city.FrontImage])
        {
            var fitted = Contain(layer, size);

            context.DrawImage(
                layer,
                new Rect(
                    left + ((size.Width - fitted.Width) / 2d),
                    top + size.Height - fitted.Height,
                    fitted.Width,
                    fitted.Height));
        }

        context.DrawImage(SceneryAssets.Ground, new Rect(new Point(left, top + size.Height), ground));
    }

    private void DrawTrees(DrawingContext context, Size bounds)
    {
        var back = ForWidth(SceneryAssets.Tree, ScreenWidth * 0.05d);
        var front = ForWidth(SceneryAssets.Tree, ScreenWidth * 0.08d);

        var backBottom = ScreenHeight * 0.07d;
        var frontBottom = ScreenHeight * 0.01d;

        Draw(bounds.Width - (ScreenWidth * 0.2d) - back.Width, backBottom, back);
        Draw(ScreenWidth * 0.2d, backBottom, back);
        Draw(bounds.Width - (ScreenWidth * 0.1d) - front.Width, frontBottom, front);
        Draw(ScreenWidth * 0.1d, frontBottom, front);

        void Draw(double left, double bottom, Size size) =>
            context.DrawImage(
                SceneryAssets.Tree,
                new Rect(new Point(left, bounds.Height - bottom - size.Height), size));
    }

    private void DrawLeaves(DrawingContext context, Size bounds, double value)
    {
        if (value <= 0d)
        {
            return;
        }

        // The width is jittered by up to a hundredth of a pixel per frame in the original, which is
        // below anything that can be drawn, so the leaves are simply sized from the screen here.
        var size = ForWidth(SceneryAssets.Leaf, ScreenWidth * 0.015d);
        var spin = FlutterEasings.EaseOutSine.Ease(SceneryClock.LeafProgress) * 360d;

        var pathInput = SceneryClock.LeafProgress * Math.PI * 2d;

        using var _ = context.PushOpacity(value);
        using var __ = context.PushClip(new Rect(bounds));

        foreach (var leaf in Leaves)
        {
            var left = Lerp(-10d, ScreenWidth + 10d, leaf.Travel.Ease(SceneryClock.LeafProgress));
            var top = bounds.Height - leaf.Path(pathInput) - size.Height;
            var origin = new Point(left + (size.Width / 2d), top + (size.Height / 2d));

            // Rotation3d turns about the child's centre, so the matrix is conjugated by that point.
            var matrix = Matrix.CreateTranslation(-origin.X, -origin.Y)
                         * Rotation3D.AroundY(spin * leaf.RotationScale)
                         * Matrix.CreateTranslation(origin.X, origin.Y);

            using var ___ = context.PushTransform(matrix);

            context.DrawImage(SceneryAssets.Leaf, new Rect(new Point(left, top), size));
        }
    }

    private void Advance()
    {
        SceneryClock.Advance(this);

        // Nothing on this clock is visible until the card starts opening, so a closed card costs
        // one tick a frame and no repaint.
        if (AnimationValue > 0d)
        {
            InvalidateVisual();
        }
    }

    private static Easing Drift(Easing curve) => new IntervalEasing(0d, 0.9d, curve);

    /// <summary>
    /// One leaf's drift, wavering path and spin rate.
    /// </summary>
    /// <param name="RotationScale">How much faster than the base rate this leaf spins.</param>
    /// <param name="Travel">How far across the card it has drifted, given the clock.</param>
    /// <param name="Path">Its height above the card's bottom edge, given the clock in radians.</param>
    private sealed record Leaf(double RotationScale, Easing Travel, Func<double, double> Path);
}
