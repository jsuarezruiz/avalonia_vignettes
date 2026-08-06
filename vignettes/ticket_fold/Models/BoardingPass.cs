namespace TicketFold.Models;

/// <summary>
/// An airport, as printed on a boarding pass. Port of <c>_Airport</c>.
/// </summary>
/// <param name="Code">The three letter IATA code, such as <c>YEG</c>.</param>
/// <param name="City">The city the airport serves.</param>
public sealed record Airport(string Code, string City);

/// <summary>
/// How long a flight takes. Port of <c>_Duration</c>.
/// </summary>
/// <param name="Hours">Whole hours in the air.</param>
/// <param name="Minutes">Minutes on top of <paramref name="Hours"/>.</param>
public sealed record FlightDuration(int Hours, int Minutes)
{
    /// <summary>
    /// Returns the duration the way the ticket prints it. The leading tab is in the original, and
    /// it is what nudges the text clear of the plane's route above it.
    /// </summary>
    public override string ToString() => $"\t{Hours}H {Minutes}M";
}

/// <summary>
/// Everything one boarding pass shows. Port of <c>BoardingPassData</c>.
/// </summary>
public sealed record BoardingPass
{
    /// <summary>
    /// Gets the passenger's name.
    /// </summary>
    public required string PassengerName { get; init; }

    /// <summary>
    /// Gets the airport the flight leaves from.
    /// </summary>
    public required Airport Origin { get; init; }

    /// <summary>
    /// Gets the airport the flight arrives at.
    /// </summary>
    public required Airport Destination { get; init; }

    /// <summary>
    /// Gets how long the flight takes.
    /// </summary>
    public required FlightDuration Duration { get; init; }

    /// <summary>
    /// Gets when boarding begins.
    /// </summary>
    public required TimeOnly BoardingTime { get; init; }

    /// <summary>
    /// Gets when the flight departs.
    /// </summary>
    public required DateTime Departs { get; init; }

    /// <summary>
    /// Gets when the flight arrives.
    /// </summary>
    public required DateTime Arrives { get; init; }

    /// <summary>
    /// Gets the departure gate.
    /// </summary>
    public required string Gate { get; init; }

    /// <summary>
    /// Gets the boarding zone.
    /// </summary>
    public required int Zone { get; init; }

    /// <summary>
    /// Gets the seat.
    /// </summary>
    public required string Seat { get; init; }

    /// <summary>
    /// Gets the fare class.
    /// </summary>
    public required string FlightClass { get; init; }

    /// <summary>
    /// Gets the flight number.
    /// </summary>
    public required string FlightNumber { get; init; }
}
