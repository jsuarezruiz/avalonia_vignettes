namespace BasketballPullToRefresh.Models;

/// <summary>
/// How far along a game is. Port of <c>BasketballGameQuarter</c>.
/// </summary>
public enum BasketballGameQuarter
{
    /// <summary>
    /// The first quarter.
    /// </summary>
    First,

    /// <summary>
    /// The second quarter.
    /// </summary>
    Second,

    /// <summary>
    /// The break between the halves.
    /// </summary>
    HalfTime,

    /// <summary>
    /// The third quarter.
    /// </summary>
    Third,

    /// <summary>
    /// The fourth quarter.
    /// </summary>
    Fourth,

    /// <summary>
    /// The game is over.
    /// </summary>
    Finished,
}
