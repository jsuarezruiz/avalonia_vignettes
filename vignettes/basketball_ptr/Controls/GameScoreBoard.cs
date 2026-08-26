using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Animation;
using BasketballPullToRefresh.Models;

namespace BasketballPullToRefresh.Controls;

/// <summary>
/// A game's score, which rolls over to the new numbers when a refresh brings them in. Port of
/// <c>game_score.dart</c>.
/// </summary>
/// <remarks>
/// One board rather than two counters, because a refresh can change who is winning: the outgoing
/// pair keeps the colours it was drawn with while the incoming pair arrives in its own, and only
/// the whole game says which those are.
/// </remarks>
[TemplatePart(PartHomeCurrent, typeof(TextBlock))]
[TemplatePart(PartHomeIncoming, typeof(TextBlock))]
[TemplatePart(PartAwayCurrent, typeof(TextBlock))]
[TemplatePart(PartAwayIncoming, typeof(TextBlock))]
public sealed class GameScoreBoard : TemplatedControl
{
    /// <summary>
    /// The height a number rolls through, which is the height of the board itself.
    /// </summary>
    public const double RollHeight = 36d;

    private const string PartHomeCurrent = "PART_HomeCurrent";
    private const string PartHomeIncoming = "PART_HomeIncoming";
    private const string PartAwayCurrent = "PART_AwayCurrent";
    private const string PartAwayIncoming = "PART_AwayIncoming";

    public static readonly StyledProperty<BasketballGameData?> GameProperty =
        AvaloniaProperty.Register<GameScoreBoard, BasketballGameData?>(nameof(Game));

    public static readonly StyledProperty<IBrush?> WinnerBrushProperty =
        AvaloniaProperty.Register<GameScoreBoard, IBrush?>(nameof(WinnerBrush));

    private static readonly TimeSpan RollDuration = TimeSpan.FromSeconds(1d);

    private readonly AnimationController _roll;

    private TextBlock? _homeCurrent;
    private TextBlock? _homeIncoming;
    private TextBlock? _awayCurrent;
    private TextBlock? _awayIncoming;

    private BasketballGameData? _current;
    private BasketballGameData? _incoming;

    public GameScoreBoard() =>
        _roll = new AnimationController(this, OnRollProgressChanged) { Duration = RollDuration };

    /// <summary>
    /// Gets or sets the game whose score is on the board.
    /// </summary>
    public BasketballGameData? Game
    {
        get => GetValue(GameProperty);
        set => SetValue(GameProperty, value);
    }

    /// <summary>
    /// Gets or sets the colour a finished game's winning score is printed in.
    /// </summary>
    public IBrush? WinnerBrush
    {
        get => GetValue(WinnerBrushProperty);
        set => SetValue(WinnerBrushProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _homeCurrent = e.NameScope.Find<TextBlock>(PartHomeCurrent);
        _homeIncoming = e.NameScope.Find<TextBlock>(PartHomeIncoming);
        _awayCurrent = e.NameScope.Find<TextBlock>(PartAwayCurrent);
        _awayIncoming = e.NameScope.Find<TextBlock>(PartAwayIncoming);

        Show(_current);
        OnRollProgressChanged(_roll.Value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != GameProperty)
        {
            return;
        }

        var game = Game;

        if (_current is null || game is null)
        {
            _current = game;
            _incoming = null;

            Show(game);
            _roll.SetValue(0d);

            return;
        }

        // A refresh landing mid roll takes over from where it is.
        _incoming = game;

        Show(_current);
        ShowIncoming(_incoming);

        _roll.SetValue(0d);
        _roll.Forward();
    }

    private static string Points(int score) => score.ToString(CultureInfo.InvariantCulture);

    private void Show(BasketballGameData? game)
    {
        if (_homeCurrent is null || _awayCurrent is null)
        {
            return;
        }

        _homeCurrent.Text = game is null ? string.Empty : Points(game.HomeScore);
        _awayCurrent.Text = game is null ? string.Empty : Points(game.AwayScore);

        _homeCurrent.Foreground = BrushFor(game?.HomeHasWon ?? false);
        _awayCurrent.Foreground = BrushFor(game?.AwayHasWon ?? false);
    }

    private void ShowIncoming(BasketballGameData game)
    {
        if (_homeIncoming is null || _awayIncoming is null)
        {
            return;
        }

        _homeIncoming.Text = Points(game.HomeScore);
        _awayIncoming.Text = Points(game.AwayScore);

        _homeIncoming.Foreground = BrushFor(game.HomeHasWon);
        _awayIncoming.Foreground = BrushFor(game.AwayHasWon);
    }

    private IBrush? BrushFor(bool hasWon) => hasWon ? WinnerBrush ?? Foreground : Foreground;

    private void OnRollProgressChanged(double progress)
    {
        // The outgoing pair leaves through the top, the incoming pair follows from below.
        var offset = -FlutterEasings.EaseInOut.Ease(progress) * RollHeight;

        SetRollOffset(_homeCurrent, offset);
        SetRollOffset(_awayCurrent, offset);
        SetRollOffset(_homeIncoming, offset + RollHeight);
        SetRollOffset(_awayIncoming, offset + RollHeight);

        if (progress < 1d || _incoming is null)
        {
            return;
        }

        // Landed: the number that arrived is now the one on the board.
        _current = _incoming;
        _incoming = null;

        Show(_current);
        _roll.SetValue(0d);
    }

    private void SetRollOffset(TextBlock? text, double offset)
    {
        if (text is not null)
        {
            text.RenderTransform = new TranslateTransform(0d, offset);
        }
    }
}
