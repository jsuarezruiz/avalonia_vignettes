using Avalonia.Animation.Easings;

namespace BasketballPullToRefresh.Animation;

/// <summary>
/// A slice of a sine wave mapped into 0..1. Port of the vignette's <c>SineCurve</c>.
/// </summary>
/// <remarks>
/// Unlike an ordinary easing it need not be monotonic: a <see cref="Length"/> of two full turns
/// sends a value out and back twice, which is how the ball rattles around the rim.
/// </remarks>
public sealed class SineEasing : Easing
{
    /// <summary>Gets or sets the phase the wave starts at, in radians.</summary>
    public double Start { get; init; }

    /// <summary>Gets or sets how much of the wave is traversed, in radians.</summary>
    public double Length { get; init; } = Math.PI * 2d;

    /// <inheritdoc />
    public override double Ease(double progress) =>
        (Math.Sin(Start + (progress * Length)) * 0.5d) + 0.5d;
}
