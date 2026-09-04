using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using TicketFold.Models;

namespace TicketFold.Controls;

/// <summary>
/// One boarding pass in the list: a summary that folds open into details and a barcode. Port of
/// <c>ticket.dart</c>.
/// </summary>
/// <remarks>
/// The light summary is printed on the back of the second panel rather than the front of the first,
/// which is why a shut ticket shows it: the panel is folded up over the top of the ticket, bringing
/// its reverse into view. Opening the ticket swings it down to reveal the details on its front, and
/// the dark summary that was underneath all along.
/// </remarks>
public sealed class Ticket : ToggleButton
{
    public static readonly StyledProperty<BoardingPass?> BoardingPassProperty =
        AvaloniaProperty.Register<Ticket, BoardingPass?>(nameof(BoardingPass));

    public static readonly DirectProperty<Ticket, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<Ticket, bool>(nameof(IsOpen), o => o.IsOpen);

    /// <summary>
    /// The height the list assumes an open ticket has. The real height is a little more; these are
    /// the round numbers <c>ticket.dart</c> scrolls by, kept as they are so the list settles in the
    /// same place the original does.
    /// </summary>
    public const double NominalOpenHeight = 400d;

    /// <summary>
    /// The height the list assumes a shut ticket has.
    /// </summary>
    public const double NominalClosedHeight = 160d;

    private bool _isOpen;

    static Ticket() =>
        IsCheckedProperty.Changed.AddClassHandler<Ticket>((x, e) =>
            x.IsOpen = e.GetNewValue<bool?>() == true);

    /// <summary>
    /// Gets or sets the boarding pass this ticket shows.
    /// </summary>
    public BoardingPass? BoardingPass
    {
        get => GetValue(BoardingPassProperty);
        set => SetValue(BoardingPassProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the ticket is folded open.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        private set => SetAndRaise(IsOpenProperty, ref _isOpen, value);
    }
}
