using Avalonia.Input;

namespace AvaloniaVignettes.Shared.Input;

/// <summary>
/// Estimates how fast a pointer was travelling when it was let go, which is what a released drag
/// needs to hand on to a spring or a fling.
/// </summary>
/// <remarks>
/// Avalonia reports pointer positions but not velocities, so the last few samples are kept and the
/// slope taken across the window. Using the window rather than the final pair is deliberate: the
/// last move before a release is often a stray pixel over a very short interval, which on its own
/// reads as an enormous velocity.
/// </remarks>
public sealed class VelocityTracker
{
    /// <summary>
    /// How far back a sample still counts, matching the horizon Flutter's own tracker keeps.
    /// </summary>
    /// <remarks>
    /// Samples only arrive while the pointer is moving, so without this a drag that pauses and is
    /// then released estimates off the movement from before the pause and flings a control the
    /// finger had already brought to rest.
    /// </remarks>
    private static readonly TimeSpan Horizon = TimeSpan.FromMilliseconds(100d);

    private readonly List<(TimeSpan Time, double Position)> _samples;
    private readonly int _capacity;

    /// <summary>
    /// Initializes a new tracker keeping <paramref name="capacity"/> samples.
    /// </summary>
    public VelocityTracker(int capacity = 5)
    {
        _capacity = Math.Max(2, capacity);
        _samples = new List<(TimeSpan, double)>(_capacity);
    }

    /// <summary>
    /// Reads the timestamp off a pointer event as a <see cref="TimeSpan"/>.
    /// </summary>
    public static TimeSpan TimestampOf(PointerEventArgs e) => TimeSpan.FromMilliseconds(e.Timestamp);

    /// <summary>
    /// Drops every sample, ready for a new drag.
    /// </summary>
    public void Clear() => _samples.Clear();

    /// <summary>
    /// Records where the pointer was at <paramref name="time"/>.
    /// </summary>
    public void Add(TimeSpan time, double position)
    {
        if (_samples.Count == _capacity)
        {
            _samples.RemoveAt(0);
        }

        _samples.Add((time, position));
    }

    /// <summary>
    /// Records the position carried by <paramref name="e"/>.
    /// </summary>
    public void Add(PointerEventArgs e, double position) => Add(TimestampOf(e), position);

    /// <summary>
    /// Estimates the velocity in units per second, positive in the direction the position grows.
    /// Returns zero until there are two samples inside <see cref="Horizon"/> far enough apart to
    /// measure, which is what makes a release after a pause come out at rest.
    /// </summary>
    public double Estimate()
    {
        if (_samples.Count < 2)
        {
            return 0d;
        }

        var last = _samples[^1];

        // The oldest sample still within the horizon of the last one. Walking back from the end
        // rather than filtering keeps the run contiguous.
        var firstIndex = _samples.Count - 1;

        while (firstIndex > 0 && last.Time - _samples[firstIndex - 1].Time <= Horizon)
        {
            firstIndex--;
        }

        if (firstIndex == _samples.Count - 1)
        {
            return 0d;
        }

        var first = _samples[firstIndex];
        var seconds = (last.Time - first.Time).TotalSeconds;

        return seconds > 0d ? (last.Position - first.Position) / seconds : 0d;
    }
}
