using Avalonia.Animation.Easings;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// One segment of a <see cref="TweenSequence"/>. Port of Flutter's <c>TweenSequenceItem</c>.
/// </summary>
/// <param name="From">The value the segment starts at.</param>
/// <param name="To">The value the segment ends at.</param>
/// <param name="Weight">The segment's share of the timeline, relative to the other segments.</param>
/// <param name="Easing">
/// The curve shaping the segment, standing in for Flutter's
/// <c>tween.chain(CurveTween(curve: ...))</c>. Left null, the segment is linear.
/// </param>
public readonly record struct TweenSegment(double From, double To, double Weight, Easing? Easing = null)
{
    /// <summary>Reads a <c>(from, to, weight)</c> tuple as a linear segment.</summary>
    /// <param name="segment">The segment's start, end and weight.</param>
    public static implicit operator TweenSegment((double From, double To, double Weight) segment) =>
        new(segment.From, segment.To, segment.Weight);
}

/// <summary>
/// A value built from a run of segments, each taking a share of the timeline proportional to its
/// weight. Port of Flutter's <c>TweenSequence</c>.
/// </summary>
/// <remarks>
/// It expresses a value that holds, moves, holds again and moves back without giving each phase a
/// clock of its own: one sequence rather than three animations to keep in step.
/// </remarks>
public sealed class TweenSequence
{
    private readonly (TweenSegment Segment, double Start, double End)[] _segments;

    /// <summary>
    /// Initializes a new sequence from segments of <c>(from, to, weight)</c>, or from
    /// <see cref="TweenSegment"/>s where a segment carries a curve of its own.
    /// </summary>
    public TweenSequence(params TweenSegment[] segments)
    {
        var total = segments.Sum(x => x.Weight);
        var built = new (TweenSegment, double, double)[segments.Length];
        var elapsed = 0d;

        for (var i = 0; i < segments.Length; i++)
        {
            var share = segments[i].Weight / total;

            built[i] = (segments[i], elapsed, elapsed + share);
            elapsed += share;
        }

        _segments = built;
    }

    /// <summary>Gets the value at <paramref name="progress"/> through the whole sequence.</summary>
    public double Evaluate(double progress)
    {
        var t = Math.Clamp(progress, 0d, 1d);

        foreach (var (segment, start, end) in _segments)
        {
            if (t > end)
            {
                continue;
            }

            var span = end - start;
            var within = span > 0d ? (t - start) / span : 1d;
            var eased = segment.Easing?.Ease(within) ?? within;

            return segment.From + ((segment.To - segment.From) * eased);
        }

        return _segments[^1].Segment.To;
    }
}
