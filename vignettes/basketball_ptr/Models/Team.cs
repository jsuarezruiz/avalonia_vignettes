using Avalonia.Media.Imaging;

namespace BasketballPullToRefresh.Models;

/// <summary>
/// One of the teams in the league.
/// </summary>
/// <param name="City">The city the team plays for.</param>
/// <param name="Name">The team's name.</param>
/// <param name="Logo">The team's crest.</param>
/// <remarks>
/// The original keeps the cities, the names and the crests in three parallel lists indexed
/// together, which is to say it keeps teams.
/// </remarks>
public sealed record Team(string City, string Name, Bitmap Logo);
