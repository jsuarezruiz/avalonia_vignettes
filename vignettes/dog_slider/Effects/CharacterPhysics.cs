namespace DogSlider.Effects;

/// <summary>
/// Chases a target along one axis under acceleration and friction, and says which way it is facing.
/// Port of <c>moving_character_physics_2d.dart</c>.
/// </summary>
/// <remarks>
/// The character does not track the target exactly; it accelerates towards it, is slowed by
/// friction, and gives up once it is within <see cref="StopDistance"/>. That gap is what stops the
/// dog standing on the ball, and the wind-down is what makes it drift to a halt rather than stop
/// dead.
/// <para>
/// Arrival is detected by the velocity reaching zero rather than by the distance closing, so a
/// character still coasting counts as moving. That is what the walk and sit animations key off.
/// </para>
/// </remarks>
public sealed class CharacterPhysics
{
    private const double RestVelocity = 0.1d;

    private const double FramesPerSecond = 60d;

    private double _velocity;
    private bool _hasLastTick;
    private TimeSpan _lastTick;

    public CharacterPhysics(double startX = 0d)
    {
        Position = startX;
        TargetX = startX;
    }

    /// <summary>
    /// Raised the moment the character sets off.
    /// </summary>
    public event EventHandler? MoveStarted;

    /// <summary>
    /// Raised once the character has come to rest.
    /// </summary>
    public event EventHandler? DestinationReached;

    /// <summary>
    /// Gets where the character is.
    /// </summary>
    public double Position { get; private set; }

    /// <summary>
    /// Gets or sets where the character is heading.
    /// </summary>
    public double TargetX { get; set; }

    /// <summary>
    /// Gets a value indicating whether the character is facing left.
    /// </summary>
    public bool IsFlipped { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the character has stopped.
    /// </summary>
    public bool IsAtDestination { get; private set; } = true;

    /// <summary>
    /// Gets or sets how hard the character accelerates.
    /// </summary>
    public double Acceleration { get; set; } = 0.3d;

    /// <summary>
    /// Gets or sets the fastest it will travel.
    /// </summary>
    public double MaxSpeed { get; set; } = 3.5d;

    /// <summary>
    /// Gets or sets how much speed is shed each frame.
    /// </summary>
    public double Friction { get; set; } = 0.11d;

    /// <summary>
    /// Gets or sets how close counts as arrived.
    /// </summary>
    public double StopDistance { get; set; } = 30d;

    /// <summary>
    /// Advances the character. <paramref name="elapsed"/> is the ticker's running total, not a delta.
    /// </summary>
    public void Update(TimeSpan elapsed)
    {
        // Floored at zero so a restarted ticker cannot run the character backwards for a frame.
        var dt = _hasLastTick ? Math.Max(0d, (elapsed - _lastTick).TotalSeconds) : 0d;

        _lastTick = elapsed;
        _hasLastTick = true;

        Position += _velocity * dt * FramesPerSecond;
        _velocity *= 1d - Friction;

        if (Math.Abs(Position - TargetX) > StopDistance)
        {
            if (IsAtDestination)
            {
                MoveStarted?.Invoke(this, EventArgs.Empty);
            }

            IsAtDestination = false;
            _velocity += Acceleration * Math.Sign(TargetX - Position);

            if (Math.Abs(_velocity) > MaxSpeed)
            {
                _velocity = MaxSpeed * Math.Sign(_velocity);
            }

            IsFlipped = _velocity < 0d;
        }

        if (Math.Abs(_velocity) < RestVelocity)
        {
            _velocity = 0d;
        }

        if (!IsAtDestination && _velocity == 0d)
        {
            IsAtDestination = true;
            DestinationReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
