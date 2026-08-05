using Avalonia;
using AvaloniaVignettes.Shared.Animation;

namespace DrinkRewardsList.Effects;

/// <summary>
/// One sloshing surface: a row of control points that swell away from the level and then spring
/// back. Port of <c>LiquidSimulation</c> in <c>liquid_painter.dart</c>.
/// </summary>
/// <remarks>
/// The surface is a chain of quadratic curves whose end points stay pinned on the level while the
/// control points between them move. Only the control points animate, which is what makes the
/// surface bulge and settle rather than slide.
/// <para>
/// Each control point runs the same three part sequence off the card's single clock: it waits,
/// swells straight to its full height, then unwinds on a long elastic curve. Heights alternate sign
/// down the row and are randomised per card, so no two fills look alike.
/// </para>
/// </remarks>
public sealed class LiquidSimulation
{
    /// <summary>
    /// How many curves make up the surface.
    /// </summary>
    public const int CurveCount = 4;

    /// <summary>
    /// The share of the clock the surface spends still, before anything moves.
    /// </summary>
    private const double WaitWeight = 10d;

    /// <summary>
    /// The share spent swelling to full height.
    /// </summary>
    private const double RiseWeight = 10d;

    /// <summary>
    /// The share spent springing back to the level.
    /// </summary>
    private const double SettleWeight = 60d;

    private static readonly ElasticOutEasing SettleEasing = new() { Period = 0.3d };

    private readonly double[] _heights = new double[CurveCount + 1];
    private readonly Point[] _controlPoints = new Point[CurveCount + 1];
    private readonly Point[] _endPoints = new Point[CurveCount + 1];

    /// <summary>
    /// Gets the control points, in a space where x runs 0 to 1 and y is in wave heights.
    /// </summary>
    public IReadOnlyList<Point> ControlPoints => _controlPoints;

    /// <summary>
    /// Gets the end points the curves pass through, all sitting on the level.
    /// </summary>
    public IReadOnlyList<Point> EndPoints => _endPoints;

    /// <summary>
    /// Gets how much wider than the card the surface is drawn.
    /// </summary>
    public double HorizontalScale { get; private set; } = 1d;

    /// <summary>
    /// Gets how far along the card the surface is shifted.
    /// </summary>
    public double HorizontalOffset { get; private set; }

    /// <summary>
    /// Rolls a fresh set of heights and offsets. Called every time a card is opened, so the same
    /// card sloshes differently each time.
    /// </summary>
    /// <param name="flipY">Whether the heights start below the level rather than above it.</param>
    public void Start(bool flipY)
    {
        var random = Random.Shared;
        const double Gap = 1d / (CurveCount * 2d);

        HorizontalScale = 1.25d + (random.NextDouble() * 0.5d);
        HorizontalOffset = -0.2d + (random.NextDouble() * 0.4d);

        // The end points are fixed on the level: the first at the left edge, the last at the right.
        _endPoints[0] = new Point(0d, 0d);

        for (var i = 1; i < CurveCount; i++)
        {
            _endPoints[i] = new Point(Gap * i * 2d, 0d);
        }

        _endPoints[CurveCount] = new Point(1d, 0d);

        for (var i = 0; i < _controlPoints.Length; i++)
        {
            _heights[i] = (0.5d + (random.NextDouble() * 0.5d))
                * (i % 2 == 0 ? 1d : -1d)
                * (flipY ? -1d : 1d);

            _controlPoints[i] = new Point(Gap + (Gap * i * 2d), _heights[i]);
        }
    }

    /// <summary>
    /// Moves every control point to where <paramref name="progress"/> puts it.
    /// </summary>
    public void Update(double progress)
    {
        const double Total = WaitWeight + RiseWeight + SettleWeight;
        const double RiseStart = WaitWeight / Total;
        const double SettleStart = (WaitWeight + RiseWeight) / Total;

        for (var i = 0; i < _controlPoints.Length; i++)
        {
            var height = _heights[i];

            var y = progress switch
            {
                < RiseStart => 0d,
                < SettleStart => height * ((progress - RiseStart) / (SettleStart - RiseStart)),
                _ => height * (1d - SettleEasing.Ease((progress - SettleStart) / (1d - SettleStart))),
            };

            _controlPoints[i] = new Point(_controlPoints[i].X, y);
        }
    }
}
