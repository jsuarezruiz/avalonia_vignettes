using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using TicketFold.Models;

namespace TicketFold.Controls;

/// <summary>
/// The details face of a boarding pass: gate, zone, seat and times. Port of
/// <c>flight_details.dart</c>.
/// </summary>
public sealed class FlightDetails : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="BoardingPass"/> property.
    /// </summary>
    public static readonly StyledProperty<BoardingPass?> BoardingPassProperty =
        AvaloniaProperty.Register<FlightDetails, BoardingPass?>(nameof(BoardingPass));

    /// <summary>
    /// Defines the <see cref="DepartsLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightDetails, string?> DepartsLabelProperty =
        AvaloniaProperty.RegisterDirect<FlightDetails, string?>(nameof(DepartsLabel), o => o.DepartsLabel);

    /// <summary>
    /// Defines the <see cref="ArrivesLabel"/> property.
    /// </summary>
    public static readonly DirectProperty<FlightDetails, string?> ArrivesLabelProperty =
        AvaloniaProperty.RegisterDirect<FlightDetails, string?>(nameof(ArrivesLabel), o => o.ArrivesLabel);

    /// <summary>
    /// The format behind <c>DateFormat('MMM d, H:mm')</c>.
    /// </summary>
    private const string TimestampFormat = "MMM d, H:mm";

    private string? _departsLabel;
    private string? _arrivesLabel;

    static FlightDetails() =>
        BoardingPassProperty.Changed.AddClassHandler<FlightDetails>((x, _) => x.UpdateLabels());

    /// <summary>
    /// Gets or sets the boarding pass being shown.
    /// </summary>
    public BoardingPass? BoardingPass
    {
        get => GetValue(BoardingPassProperty);
        set => SetValue(BoardingPassProperty, value);
    }

    /// <summary>
    /// Gets the departure timestamp, as printed.
    /// </summary>
    public string? DepartsLabel
    {
        get => _departsLabel;
        private set => SetAndRaise(DepartsLabelProperty, ref _departsLabel, value);
    }

    /// <summary>
    /// Gets the arrival timestamp, as printed.
    /// </summary>
    public string? ArrivesLabel
    {
        get => _arrivesLabel;
        private set => SetAndRaise(ArrivesLabelProperty, ref _arrivesLabel, value);
    }

    private void UpdateLabels()
    {
        if (BoardingPass is not { } pass)
        {
            DepartsLabel = ArrivesLabel = null;
            return;
        }

        DepartsLabel = Format(pass.Departs);
        ArrivesLabel = Format(pass.Arrives);

        static string Format(DateTime value) =>
            value.ToString(TimestampFormat, CultureInfo.InvariantCulture).ToUpperInvariant();
    }
}
