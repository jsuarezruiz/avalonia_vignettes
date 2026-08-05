namespace AvaloniaVignettes.Shared.Animation;

/// <summary>
/// Describes a spring, mirroring Flutter's <c>SpringDescription</c>.
/// </summary>
/// <param name="Mass">The mass attached to the free end of the spring.</param>
/// <param name="Stiffness">The spring constant.</param>
/// <param name="Damping">The damping coefficient (not the damping ratio).</param>
public readonly record struct SpringDescription(double Mass, double Stiffness, double Damping)
{
    /// <summary>
    /// Creates a spring from a damping <paramref name="ratio"/>, where 1.0 is critically damped,
    /// less than 1.0 is under-damped (bouncy) and greater than 1.0 is over-damped.
    /// </summary>
    public static SpringDescription FromDampingRatio(double mass, double stiffness, double ratio = 1d) =>
        new(mass, stiffness, ratio * 2d * Math.Sqrt(mass * stiffness));

    /// <summary>
    /// The spring Flutter's <c>ScrollPhysics</c> uses for every ballistic scroll simulation.
    /// </summary>
    public static SpringDescription ScrollDefault { get; } = FromDampingRatio(mass: 0.5, stiffness: 100d, ratio: 1.1);
}

/// <summary>
/// Port of Flutter's <c>ScrollSpringSimulation</c>: a damped harmonic oscillator that carries a
/// value from <c>start</c> to <c>end</c> while preserving the release velocity. Positions are in
/// pixels and velocities in pixels per second.
/// </summary>
public sealed class SpringSimulation
{
    // Flutter's Tolerance.defaultTolerance, scaled for a device pixel ratio of 1.
    private const double DistanceTolerance = 0.01;
    private const double VelocityTolerance = 20d;

    private readonly double _endPosition;
    private readonly Func<double, double> _x;
    private readonly Func<double, double> _dx;

    /// <summary>
    /// Initializes a new simulation running from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    public SpringSimulation(SpringDescription spring, double start, double end, double velocity)
    {
        _endPosition = end;

        var distance = start - end;
        var cmk = spring.Damping * spring.Damping - 4d * spring.Mass * spring.Stiffness;

        if (cmk > 0d)
        {
            // Over-damped: two real roots, no oscillation.
            var sqrtCmk = Math.Sqrt(cmk);
            var r1 = (-spring.Damping - sqrtCmk) / (2d * spring.Mass);
            var r2 = (-spring.Damping + sqrtCmk) / (2d * spring.Mass);
            var c2 = (velocity - r1 * distance) / (r2 - r1);
            var c1 = distance - c2;

            _x = t => c1 * Math.Exp(r1 * t) + c2 * Math.Exp(r2 * t);
            _dx = t => c1 * r1 * Math.Exp(r1 * t) + c2 * r2 * Math.Exp(r2 * t);
        }
        else if (cmk < 0d)
        {
            // Under-damped: oscillates around the target.
            var w = Math.Sqrt(4d * spring.Mass * spring.Stiffness - spring.Damping * spring.Damping) /
                    (2d * spring.Mass);
            // Note: Flutter writes this as `-(damping / 2.0 * mass)`, which is a long-standing typo
            // in its _UnderdampedSolution. The correct decay rate is used here instead.
            var r = -spring.Damping / (2d * spring.Mass);
            var c1 = distance;
            var c2 = (velocity - r * distance) / w;

            _x = t => Math.Exp(r * t) * (c1 * Math.Cos(w * t) + c2 * Math.Sin(w * t));
            _dx = t => Math.Exp(r * t) *
                       ((-w * c1 + r * c2) * Math.Sin(w * t) + (c2 * w + c1 * r) * Math.Cos(w * t));
        }
        else
        {
            // Critically damped.
            var r = -spring.Damping / (2d * spring.Mass);
            var c1 = distance;
            var c2 = velocity - r * distance;

            _x = t => (c1 + c2 * t) * Math.Exp(r * t);
            _dx = t => ((c1 + c2 * t) * r + c2) * Math.Exp(r * t);
        }
    }

    /// <summary>
    /// Gets the position at <paramref name="time"/> seconds after the simulation started.
    /// </summary>
    public double PositionAt(double time)
    {
        var offset = _x(time);

        return IsSettled(offset, _dx(time)) ? _endPosition : _endPosition + offset;
    }

    /// <summary>
    /// Gets the velocity at <paramref name="time"/> seconds after the simulation started.
    /// </summary>
    public double VelocityAt(double time)
    {
        var velocity = _dx(time);

        return IsSettled(_x(time), velocity) ? 0d : velocity;
    }

    /// <summary>
    /// Gets a value indicating whether the simulation has settled at its end position.
    /// </summary>
    public bool IsDone(double time) => IsSettled(_x(time), _dx(time));

    // Both solutions are evaluated once and passed in: this is asked on every frame of a fling, and
    // going through IsDone first would work each of them out twice over.
    private static bool IsSettled(double offset, double velocity) =>
        Math.Abs(offset) < DistanceTolerance && Math.Abs(velocity) < VelocityTolerance;
}
