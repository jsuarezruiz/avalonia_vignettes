using Avalonia.Animation.Easings;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// Port of Flutter's <c>Interval</c> curve: holds at 0 until <see cref="Begin"/>, runs
/// <see cref="Curve"/> over the window, then holds at 1. It is how the vignettes delay part of an
/// animation without giving it a clock of its own.
/// </summary>
public sealed class IntervalEasing : Easing
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntervalEasing"/> class.
    /// </summary>
    /// <param name="begin">Where the inner curve starts, as a fraction of the whole animation.</param>
    /// <param name="end">Where the inner curve finishes.</param>
    /// <param name="curve">The curve to run over the window; linear when null.</param>
    public IntervalEasing(double begin, double end, Easing? curve = null)
    {
        Begin = begin;
        End = end;
        Curve = curve;
    }

    /// <summary>
    /// Gets where the inner curve starts, as a fraction of the whole animation.
    /// </summary>
    public double Begin { get; }

    /// <summary>
    /// Gets where the inner curve finishes, as a fraction of the whole animation.
    /// </summary>
    public double End { get; }

    /// <summary>
    /// Gets the curve run over the window, or null for linear.
    /// </summary>
    public Easing? Curve { get; }

    /// <inheritdoc />
    public override double Ease(double progress)
    {
        var t = Math.Clamp((progress - Begin) / (End - Begin), 0d, 1d);

        // The ends are returned untouched so a curve that does not pass exactly through 0 and 1
        // cannot make the animation jump at either edge.
        return t is 0d or 1d ? t : Curve?.Ease(t) ?? t;
    }
}
