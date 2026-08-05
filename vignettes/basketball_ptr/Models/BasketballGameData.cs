using System.Globalization;

namespace BasketballPullToRefresh.Models;

/// <summary>
/// One game on the scores list. Port of <c>BasketballGameData</c>.
/// </summary>
/// <param name="Home">The home team.</param>
/// <param name="Away">The away team.</param>
/// <param name="HomeScore">The home team's points.</param>
/// <param name="AwayScore">The away team's points.</param>
/// <param name="Quarter">How far along the game is.</param>
/// <param name="Time">How long the current quarter has left, if the game is being played.</param>
public sealed record BasketballGameData(
    Team Home,
    Team Away,
    int HomeScore,
    int AwayScore,
    BasketballGameQuarter Quarter,
    TimeSpan? Time = null)
{
    /// <summary>
    /// Gets a value indicating whether the game is over.
    /// </summary>
    public bool IsFinished => Quarter == BasketballGameQuarter.Finished;

    /// <summary>
    /// Gets a value indicating whether the home team has won.
    /// </summary>
    public bool HomeHasWon => IsFinished && HomeScore > AwayScore;

    /// <summary>
    /// Gets a value indicating whether the away team has won.
    /// </summary>
    public bool AwayHasWon => IsFinished && AwayScore > HomeScore;

    /// <summary>
    /// Gets the badges printed above the score. Port of <c>GameTime</c>.
    /// </summary>
    public IReadOnlyList<GameBadge> Badges => Quarter switch
    {
        BasketballGameQuarter.HalfTime => [new GameBadge("Half Time", IsLive: true)],
        BasketballGameQuarter.Finished => [new GameBadge("Final Score", IsLive: false)],
        _ => [new GameBadge(QuarterText, IsLive: true), new GameBadge(TimeText, IsLive: false)],
    };

    private string QuarterText => Quarter switch
    {
        BasketballGameQuarter.First => "Q1",
        BasketballGameQuarter.Second => "Q2",
        BasketballGameQuarter.Third => "Q3",
        BasketballGameQuarter.Fourth => "Q4",
        _ => string.Empty,
    };

    /// <summary>
    /// Gets the clock, in whole minutes rather than wrapped at an hour.
    /// </summary>
    private string TimeText => Time is not { } time
        ? "00:00"
        : string.Create(
            CultureInfo.InvariantCulture,
            $"{(int)time.TotalMinutes}:{time.Seconds:00}");
}
