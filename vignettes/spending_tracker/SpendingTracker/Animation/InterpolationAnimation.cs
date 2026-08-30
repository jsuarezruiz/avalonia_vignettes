using Avalonia;
using Avalonia.Animation.Easings;
using AvaloniaVignettes.Shared.Animation;

namespace SpendingTracker.Animation;

/// <summary>
/// Runs a number to a target the way Flutter's <c>AnimationController.animateTo</c> does.
/// </summary>
/// <remarks>
/// Two things set this apart from a transition, and the vignette leans on both.
/// <see cref="Duration"/> is the time for a full traverse of <see cref="Range"/>, so a short hop
/// takes proportionally less time, which is how a snap of a fifth of a month out of twenty one
/// lands in a couple of hundred milliseconds off a twelve second controller. And the curve shapes
/// the progress rather than the interpolation, so it reads the same whichever way the value is
/// travelling.
/// </remarks>
public sealed class InterpolationAnimation
{
    private readonly FrameTicker _ticker;
    private readonly Action<double> _onValueChanged;

    private double _from;
    private double _to;
    private TimeSpan _span;

    /// <summary>
    /// Initializes a new animation ticking off <paramref name="owner"/>'s clock and reporting every
    /// change to <paramref name="onValueChanged"/>.
    /// </summary>
    public InterpolationAnimation(Visual owner, Action<double> onValueChanged)
    {
        _ticker = new FrameTicker(owner, OnTick);
        _onValueChanged = onValueChanged;
    }

    /// <summary>
    /// Gets or sets the time a full traverse of <see cref="Range"/> takes.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Gets or sets the span the duration is measured against.
    /// </summary>
    public double Range { get; set; } = 1d;

    /// <summary>
    /// Gets or sets the curve the progress runs on.
    /// </summary>
    public Easing Easing { get; set; } = new LinearEasing();

    /// <summary>
    /// Gets the current value.
    /// </summary>
    public double Value { get; private set; }

    /// <summary>
    /// Jumps straight to <paramref name="value"/>, stopping any run in progress.
    /// </summary>
    public void SetValue(double value)
    {
        _ticker.Stop();

        Value = value;
        _onValueChanged(Value);
    }

    /// <summary>
    /// Runs to <paramref name="target"/> from wherever the value currently sits.
    /// </summary>
    public void AnimateTo(double target)
    {
        _from = Value;
        _to = target;
        _span = Range > 0d ? Duration * (Math.Abs(target - _from) / Range) : Duration;

        if (_span <= TimeSpan.Zero)
        {
            SetValue(target);
            return;
        }

        _ticker.Start();
    }

    /// <summary>
    /// Stops without changing the value.
    /// </summary>
    public void Stop() => _ticker.Stop();

    private void OnTick(TimeSpan elapsed)
    {
        var progress = Math.Clamp(elapsed / _span, 0d, 1d);

        Value = _from + ((_to - _from) * Easing.Ease(progress));

        if (progress >= 1d)
        {
            Value = _to;
            _ticker.Stop();
        }

        _onValueChanged(Value);
    }
}
