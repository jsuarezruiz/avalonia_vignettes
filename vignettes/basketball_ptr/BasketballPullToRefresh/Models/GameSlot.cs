using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BasketballPullToRefresh.Models;

/// <summary>
/// One row of the scores list, holding whichever game is currently reported there.
/// </summary>
/// <remarks>
/// A refresh replaces the games, not the rows, so a score can roll from the old number to the new
/// one instead of simply appearing. The original's list has ten children built from the model, so a
/// rebuild hands each existing element new data.
/// </remarks>
public sealed class GameSlot(BasketballGameData game) : INotifyPropertyChanged
{
    private BasketballGameData _game = game;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the game reported in this row.
    /// </summary>
    public BasketballGameData Game
    {
        get => _game;
        set
        {
            if (Equals(_game, value))
            {
                return;
            }

            _game = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
