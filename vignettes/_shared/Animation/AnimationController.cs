using Avalonia;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// Drives a value from 0 to 1 and back at a constant rate, the equivalent of Flutter's
/// <c>AnimationController</c>. Use it in place of a <see cref="Avalonia.Animation.Transition{T}"/>
/// when the animation has to be reversible from wherever it currently sits, or when a curve has to
/// be applied to the raw progress rather than to the interpolation.
/// </summary>
/// <remarks>
/// The value is deliberately left un-eased: <see cref="Duration"/> is the time for a full traverse,
/// so reversing half way through takes half the time, and applying a curve to
/// <see cref="Value"/> yourself reproduces Flutter's ordering. A transition cannot do either — it
/// always runs for its full duration and always eases the interpolation, which reverses the shape
/// of the curve instead of the direction of travel.
/// </remarks>
public sealed class AnimationController
{
    private readonly FrameTicker _ticker;
    private readonly Action<double> _onValueChanged;

    private TimeSpan _lastElapsed;
    private bool _hasLastElapsed;
    private int _direction;

    /// <summary>
    /// Initializes a new controller ticking off <paramref name="owner"/>'s clock and reporting
    /// every change to <paramref name="onValueChanged"/>.
    /// </summary>
    public AnimationController(Visual owner, Action<double> onValueChanged)
    {
        _ticker = new FrameTicker(owner, OnTick);
        _onValueChanged = onValueChanged;
    }

    /// <summary>
    /// Gets or sets the time a full 0 to 1 traverse takes.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Gets the current value, between 0 and 1.
    /// </summary>
    public double Value { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the controller is currently running.
    /// </summary>
    public bool IsAnimating => _direction != 0;

    /// <summary>
    /// Runs towards 1 from wherever the value currently sits.
    /// </summary>
    public void Forward() => Run(1);

    /// <summary>
    /// Runs towards 0 from wherever the value currently sits.
    /// </summary>
    public void Reverse() => Run(-1);

    /// <summary>
    /// Stops without changing the value.
    /// </summary>
    public void Stop()
    {
        _direction = 0;
        _ticker.Stop();
    }

    /// <summary>
    /// Jumps straight to <paramref name="value"/>, stopping any run in progress.
    /// </summary>
    public void SetValue(double value)
    {
        Stop();

        Value = Math.Clamp(value, 0d, 1d);
        _onValueChanged(Value);
    }

    private void Run(int direction)
    {
        var target = direction > 0 ? 1d : 0d;

        if (Value == target)
        {
            Stop();
            return;
        }

        _direction = direction;

        // The ticker measures from its own start, so the previous run's timestamp means nothing.
        _hasLastElapsed = false;
        _ticker.Start();
    }

    private void OnTick(TimeSpan elapsed)
    {
        // A restarted ticker reports elapsed from zero again, so the step is floored at zero rather
        // than allowed to run the animation backwards for one frame.
        var step = _hasLastElapsed ? Math.Max(0d, (elapsed - _lastElapsed).TotalSeconds) : 0d;

        _lastElapsed = elapsed;
        _hasLastElapsed = true;

        var rate = Duration > TimeSpan.Zero ? step / Duration.TotalSeconds : 1d;
        var target = _direction > 0 ? 1d : 0d;

        Value = Math.Clamp(Value + (_direction * rate), 0d, 1d);

        // Only the bound being travelled towards ends the run. Testing for either one would stop a
        // forward run on its first frame, which begins at zero and has not moved yet.
        if (Value == target)
        {
            Stop();
        }

        _onValueChanged(Value);
    }
}
