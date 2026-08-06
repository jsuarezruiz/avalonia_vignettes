using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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
public sealed class Ticket : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="BoardingPass"/> property.
    /// </summary>
    public static readonly StyledProperty<BoardingPass?> BoardingPassProperty =
        AvaloniaProperty.Register<Ticket, BoardingPass?>(nameof(BoardingPass));

    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Ticket, bool>(nameof(IsOpen));

    /// <summary>
    /// Raised after a tap has folded the ticket open or shut.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ToggledEvent =
        RoutedEvent.Register<Ticket, RoutedEventArgs>(nameof(Toggled), RoutingStrategies.Bubble);

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

    /// <summary>
    /// Initializes a new instance of the <see cref="Ticket"/> class.
    /// </summary>
    public Ticket() => Tapped += OnTapped;

    /// <summary>
    /// Occurs after a tap has folded the ticket open or shut.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Toggled
    {
        add => AddHandler(ToggledEvent, value);
        remove => RemoveHandler(ToggledEvent, value);
    }

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
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        SetCurrentValue(IsOpenProperty, !IsOpen);

        RaiseEvent(new RoutedEventArgs(ToggledEvent, this));
    }
}
