using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace BasketballPullToRefresh.Models;

/// <summary>
/// The league, the scores the app opens on and the scores a refresh brings back. Port of
/// <c>demo_data.dart</c>.
/// </summary>
public static class DemoData
{
    private const string ImageRoot = "avares://BasketballPullToRefresh/Assets/Images";

    /// <summary>
    /// How many games the list reports.
    /// </summary>
    public const int GameCount = 10;

    private static readonly Team Stars = new("Seattle", "Stars", Load("badge"));
    private static readonly Team Avalonias = new("Edmonton", "Avalonias", Load("avalonia"));
    private static readonly Team Birds = new("Birmingham", "Birds", Load("bird"));
    private static readonly Team Dribblers = new("LA", "Dribblers", Load("light"));
    private static readonly Team Cannons = new("Dallas", "Cannons", Load("maroon"));
    private static readonly Team Knights = new("New York", "Knights", Load("viking"));

    private static readonly Team[] League = [Stars, Avalonias, Birds, Dribblers, Cannons, Knights];

    /// <summary>
    /// Gets the sheet the spinning ball is cut from: 10 columns of 400x400 frames.
    /// </summary>
    public static Bitmap BallSpriteSheet { get; } = Load("basketball");

    /// <summary>
    /// Gets the backboard the hoop hangs from.
    /// </summary>
    public static Bitmap Backboard { get; } = Load("backboard");

    /// <summary>
    /// Gets the net, which the ball drops behind once it is through the hoop.
    /// </summary>
    public static Bitmap Net { get; } = Load("net");

    /// <summary>
    /// Gets the rim, drawn last so it sits in front of both the net and the ball.
    /// </summary>
    public static Bitmap Rim { get; } = Load("rim");

    /// <summary>
    /// Builds the scores the app opens on.
    /// </summary>
    public static IReadOnlyList<GameSlot> CreateInitialGames()
    {
        BasketballGameData[] games =
        [
            new(Knights, Avalonias, 63, 53, BasketballGameQuarter.HalfTime),
            new(Birds, Stars, 115, 105, BasketballGameQuarter.Fourth, new TimeSpan(0, 7, 6)),
            new(Dribblers, Cannons, 85, 88, BasketballGameQuarter.Fourth, new TimeSpan(0, 10, 28)),
            new(Knights, Birds, 97, 109, BasketballGameQuarter.Finished),
            new(Avalonias, Dribblers, 112, 102, BasketballGameQuarter.Finished),
            new(Knights, Avalonias, 63, 53, BasketballGameQuarter.HalfTime),
            new(Birds, Stars, 115, 105, BasketballGameQuarter.Fourth, new TimeSpan(0, 7, 6)),
            new(Dribblers, Cannons, 85, 88, BasketballGameQuarter.Fourth, new TimeSpan(0, 10, 28)),
            new(Knights, Birds, 97, 109, BasketballGameQuarter.Finished),
            new(Avalonias, Dribblers, 112, 102, BasketballGameQuarter.Finished),
        ];

        return [.. games.Select(game => new GameSlot(game))];
    }

    /// <summary>
    /// Makes up a fresh set of scores, as a refresh does. Port of <c>randomize</c>.
    /// </summary>
    public static IReadOnlyList<BasketballGameData> CreateRandomGames()
    {
        var quarters = Enum.GetValues<BasketballGameQuarter>();
        var games = new BasketballGameData[GameCount];

        for (var i = 0; i < games.Length; i++)
        {
            var home = Random.Shared.Next(League.Length);
            int away;

            do
            {
                away = Random.Shared.Next(League.Length);
            }
            while (away == home);

            games[i] = new BasketballGameData(
                League[home],
                League[away],
                Random.Shared.Next(160),
                Random.Shared.Next(160),
                quarters[Random.Shared.Next(quarters.Length)],
                new TimeSpan(0, Random.Shared.Next(30), Random.Shared.Next(60)));
        }

        return games;
    }

    private static Bitmap Load(string name) => new(AssetLoader.Open(new Uri($"{ImageRoot}/{name}.png")));
}
