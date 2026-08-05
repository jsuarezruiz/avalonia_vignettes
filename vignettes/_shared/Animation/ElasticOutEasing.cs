using Avalonia.Animation.Easings;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// Port of Flutter's <c>Curves.elasticOut</c> (<c>ElasticOutCurve</c>): an oscillating curve that
/// overshoots its target and settles. Avalonia's built-in <c>ElasticEaseOut</c> uses a different
/// period and does not land on exactly 1, so the vignettes use this instead.
/// </summary>
public sealed class ElasticOutEasing : Easing
{
    /// <summary>
    /// Gets or sets the duration of the oscillation. Flutter's <c>Curves.elasticOut</c> uses 0.4.
    /// </summary>
    public double Period { get; set; } = 0.4;

    /// <inheritdoc />
    public override double Ease(double progress)
    {
        if (progress <= 0d)
        {
            return 0d;
        }

        if (progress >= 1d)
        {
            return 1d;
        }

        var s = Period / 4d;
        return Math.Pow(2d, -10d * progress) * Math.Sin((progress - s) * (Math.PI * 2d) / Period) + 1d;
    }
}
