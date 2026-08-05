using Avalonia;
using Avalonia.Controls;

namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// Drives a per-frame callback off the owning <see cref="TopLevel"/>'s compositor clock, reporting
/// the time elapsed since <see cref="Start"/>. This is the equivalent of Flutter's
/// <c>Ticker</c>/<c>AnimationController</c> pairing, and is what the vignettes use whenever an
/// animation is driven by physics rather than by a fixed-duration transition.
/// </summary>
public sealed class FrameTicker
{
    private readonly Visual _owner;
    private readonly Action<TimeSpan> _onTick;

    private TimeSpan _startedAt;
    private bool _hasStartTime;
    private bool _framePending;

    /// <summary>
    /// Initializes a new ticker for <paramref name="owner"/>, invoking <paramref name="onTick"/>
    /// once per rendered frame with the elapsed time since the ticker was started.
    /// </summary>
    public FrameTicker(Visual owner, Action<TimeSpan> onTick)
    {
        _owner = owner;
        _onTick = onTick;
    }

    /// <summary>Gets a value indicating whether the ticker is currently requesting frames.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Starts (or restarts) the ticker, resetting the elapsed time to zero.</summary>
    public void Start()
    {
        _hasStartTime = false;

        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        RequestFrame();
    }

    /// <summary>Stops the ticker. Safe to call when it is not running.</summary>
    public void Stop() => IsRunning = false;

    // Stopping and starting again from inside the tick would otherwise leave two frames asked for
    // — one from the restart, one from the tail below — and the callback would run twice a frame.
    private void RequestFrame()
    {
        if (_framePending)
        {
            return;
        }

        if (TopLevel.GetTopLevel(_owner) is { } topLevel)
        {
            _framePending = true;
            topLevel.RequestAnimationFrame(OnFrame);
        }
        else
        {
            IsRunning = false;
        }
    }

    private void OnFrame(TimeSpan now)
    {
        _framePending = false;

        if (!IsRunning)
        {
            return;
        }

        if (!_hasStartTime)
        {
            _startedAt = now;
            _hasStartTime = true;
        }

        _onTick(now - _startedAt);

        if (IsRunning)
        {
            RequestFrame();
        }
    }
}
