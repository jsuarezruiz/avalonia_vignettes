namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// A value that runs to a target at a steady rate. The equivalent of driving a Flutter
/// <c>AnimationController</c> with <c>animateTo</c>, which interpolates linearly from wherever the
/// controller currently sits.
/// </summary>
/// <remarks>
/// <see cref="Sign"/> stands in for the controller's <c>velocity.sign</c>: which way it is
/// travelling, or zero when it is not. The pane reads it to decide which of two curves to shape its
/// edge with, so a dip that is deepening looks different from one that is filling back in.
/// </remarks>
public sealed class Ramp
{
    private double _from;
    private double _to;
    private TimeSpan _duration;
    private TimeSpan _elapsed;

    /// <summary>Gets the current value.</summary>
    public double Value { get; private set; }

    /// <summary>Gets a value indicating whether the value is still travelling.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Gets which way the value is travelling: -1, 0 or 1.</summary>
    public int Sign { get; private set; }

    /// <summary>Jumps to a value, abandoning any run in progress.</summary>
    public void Set(double value)
    {
        Value = value;
        IsRunning = false;
        Sign = 0;
    }

    /// <summary>Starts running to <paramref name="target"/> over <paramref name="duration"/>.</summary>
    public void AnimateTo(double target, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero || target == Value)
        {
            Set(target);
            return;
        }

        _from = Value;
        _to = target;
        _duration = duration;
        _elapsed = TimeSpan.Zero;

        IsRunning = true;
        Sign = Math.Sign(target - Value);
    }

    /// <summary>Advances the run by one frame.</summary>
    public void Advance(TimeSpan step)
    {
        if (!IsRunning)
        {
            return;
        }

        _elapsed += step;

        var progress = Math.Clamp(_elapsed / _duration, 0d, 1d);

        Value = _from + ((_to - _from) * progress);

        if (progress >= 1d)
        {
            IsRunning = false;
            Sign = 0;
        }
    }
}
