namespace BasketballPullToRefresh.Models;

/// <summary>
/// A badge above the score: the quarter and the clock while a game is on, one caption once it is at
/// half time or over.
/// </summary>
/// <param name="Text">What the badge reads.</param>
/// <param name="IsLive">Whether it is live news, drawn white on orange rather than black on grey.</param>
public sealed record GameBadge(string Text, bool IsLive);
