using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace TicketFold.Controls;

/// <summary>
/// The barcode face at the foot of an open ticket. Port of <c>flight_barcode.dart</c>.
/// </summary>
/// <remarks>
/// The barcode is a button in the original, and it takes the tap for itself rather than letting it
/// reach the ticket, so scanning your pass is not a way to fold it shut again.
/// </remarks>
public sealed class FlightBarcode : TemplatedControl
{
    public FlightBarcode() => Tapped += OnTapped;

    private static void OnTapped(object? sender, TappedEventArgs e)
    {
        // Swallow the tap so it never reaches the ticket and folds it shut.
        e.Handled = true;
    }
}
