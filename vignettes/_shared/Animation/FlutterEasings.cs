using Avalonia.Animation.Easings;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// The curves Flutter's <c>Curves</c> class exposes under the familiar CSS names, as Avalonia
/// easings.
/// </summary>
/// <remarks>
/// Every one of these is a cubic bezier in Flutter, including the ones whose names suggest a
/// polynomial: <c>Curves.easeInQuad</c> is <c>Cubic(0.55, 0.085, 0.68, 0.53)</c>, not
/// <c>t * t</c>. Reaching for Avalonia's <c>QuadraticEaseIn</c> or <c>CubicEaseOut</c> would
/// therefore change the motion, so the vignettes name the bezier control points instead and let
/// <see cref="SplineEasing"/> solve them.
/// </remarks>
public static class FlutterEasings
{
    public static Easing Ease { get; } = new SplineEasing(0.25d, 0.1d, 0.25d, 1d);

    public static Easing EaseIn { get; } = new SplineEasing(0.42d, 0d, 1d, 1d);

    public static Easing EaseInOut { get; } = new SplineEasing(0.42d, 0d, 0.58d, 1d);

    public static Easing EaseInSine { get; } = new SplineEasing(0.47d, 0d, 0.745d, 0.715d);

    public static Easing EaseInCubic { get; } = new SplineEasing(0.55d, 0.055d, 0.675d, 0.19d);

    public static Easing EaseOut { get; } = new SplineEasing(0d, 0d, 0.58d, 1d);

    public static Easing EaseOutCubic { get; } = new SplineEasing(0.215d, 0.61d, 0.355d, 1d);

    public static Easing EaseInExpo { get; } = new SplineEasing(0.95d, 0.05d, 0.795d, 0.035d);

    public static Easing EaseInQuint { get; } = new SplineEasing(0.755d, 0.05d, 0.855d, 0.06d);

    public static Easing EaseInQuad { get; } = new SplineEasing(0.55d, 0.085d, 0.68d, 0.53d);

    public static Easing EaseInOutQuad { get; } = new SplineEasing(0.455d, 0.03d, 0.515d, 0.955d);

    public static Easing EaseOutQuart { get; } = new SplineEasing(0.165d, 0.84d, 0.44d, 1d);

    public static Easing EaseOutQuad { get; } = new SplineEasing(0.25d, 0.46d, 0.45d, 0.94d);

    public static Easing EaseInOutSine { get; } = new SplineEasing(0.445d, 0.05d, 0.55d, 0.95d);

    public static Easing EaseOutSine { get; } = new SplineEasing(0.39d, 0.575d, 0.565d, 1d);

    public static Easing LinearToEaseOut { get; } = new SplineEasing(0.35d, 0.91d, 0.33d, 0.97d);

    /// <summary>
    /// Flutter's <c>Curves.fastOutSlowIn</c>, the Material standard easing, and the curve every
    /// <c>Hero</c> flight runs on.
    /// </summary>
    public static Easing FastOutSlowIn { get; } = new SplineEasing(0.4d, 0d, 0.2d, 1d);
}
