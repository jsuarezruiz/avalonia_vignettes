using Avalonia.Animation.Easings;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// Remaps time by moving one point along it and interpolating linearly either side. Port of
/// <c>LinearPointCurve</c>.
/// </summary>
/// <remarks>
/// Reading <c>LinearPointCurve(0.28, 0)</c> as "nothing happens until 28%, then run to the end" is
/// the quickest way to see what one of these is for; <c>LinearPointCurve(0.25, 1)</c> is the
/// opposite, finishing in the first quarter. Unlike a Flutter <c>Curve</c>, it overrides
/// <c>transform</c> rather than <c>transformInternal</c>, so it is not clamped at either end and can
/// legitimately return values outside 0 to 1.
/// </remarks>
public sealed class LinearPointEasing : Easing
{
    private readonly double _input;
    private readonly double _lowerScale;
    private readonly double _upperScale;
    private readonly double _upperOffset;

    /// <summary>Initializes a new instance of the <see cref="LinearPointEasing"/> class.</summary>
    /// <param name="input">Where along the input the moved point sits.</param>
    /// <param name="output">Where that point is moved to.</param>
    public LinearPointEasing(double input, double output)
    {
        _input = input;
        _lowerScale = output / input;
        _upperScale = (1d - output) / (1d - input);
        _upperOffset = 1d - _upperScale;
    }

    /// <inheritdoc />
    public override double Ease(double progress) =>
        progress < _input ? progress * _lowerScale : (progress * _upperScale) + _upperOffset;
}

/// <summary>
/// An elastic curve that oscillates about a half rather than settling on one. Port of
/// <c>CenteredElasticOutCurve</c>.
/// </summary>
/// <remarks>
/// It reads 0.5 at both ends and swings either side in between, which is what makes it a squash: fed
/// to a scale, the shape stretches and rebounds around its resting size rather than growing into it.
/// </remarks>
public sealed class CenteredElasticOutEasing : Easing
{
    /// <summary>Gets or sets the duration of the oscillation.</summary>
    public double Period { get; set; } = 0.4d;

    /// <inheritdoc />
    public override double Ease(double progress) =>
        (Math.Pow(2d, -10d * progress) * Math.Sin(progress * 2d * Math.PI / Period)) + 0.5d;
}

/// <summary>
/// The mirror of <see cref="CenteredElasticOutEasing"/>, oscillating into rather than out of its
/// resting value. Port of <c>CenteredElasticInCurve</c>.
/// </summary>
public sealed class CenteredElasticInEasing : Easing
{
    /// <summary>Gets or sets the duration of the oscillation.</summary>
    public double Period { get; set; } = 0.4d;

    /// <inheritdoc />
    public override double Ease(double progress) =>
        (-Math.Pow(2d, 10d * (progress - 1d)) * Math.Sin((progress - 1d) * 2d * Math.PI / Period)) + 0.5d;
}
