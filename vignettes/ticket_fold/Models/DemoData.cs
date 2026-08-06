namespace TicketFold.Models;

/// <summary>
/// The four boarding passes the vignette ships with. Port of <c>demo_data.dart</c>.
/// </summary>
public static class DemoData
{
    /// <summary>
    /// Gets the boarding passes, in the order they are listed.
    /// </summary>
    public static IReadOnlyList<BoardingPass> BoardingPasses { get; } =
    [
        new BoardingPass
        {
            PassengerName = "Ms. Jane Doe",
            Origin = new Airport("YEG", "Edmonton"),
            Destination = new Airport("LAX", "Los Angeles"),
            Duration = new FlightDuration(3, 30),
            BoardingTime = new TimeOnly(7, 10),
            Departs = new DateTime(2019, 10, 17, 23, 45, 0),
            Arrives = new DateTime(2019, 10, 18, 2, 15, 0),
            Gate = "50",
            Zone = 3,
            Seat = "12A",
            FlightClass = "Economy",
            FlightNumber = "AC237",
        },
        new BoardingPass
        {
            PassengerName = "Ms. Jane Doe",
            Origin = new Airport("YYC", "Calgary"),
            Destination = new Airport("YOW", "Ottawa"),
            Duration = new FlightDuration(3, 50),
            BoardingTime = new TimeOnly(12, 15),
            Departs = new DateTime(2019, 10, 17, 23, 45, 0),
            Arrives = new DateTime(2019, 10, 18, 2, 15, 0),
            Gate = "22",
            Zone = 1,
            Seat = "17C",
            FlightClass = "Economy",
            FlightNumber = "AC237",
        },
        new BoardingPass
        {
            PassengerName = "Ms. Jane Doe",
            Origin = new Airport("YEG", "Edmonton"),
            Destination = new Airport("MEX", "Mexico"),
            Duration = new FlightDuration(4, 15),
            BoardingTime = new TimeOnly(16, 45),
            Departs = new DateTime(2019, 10, 17, 23, 45, 0),
            Arrives = new DateTime(2019, 10, 18, 2, 15, 0),
            Gate = "30",
            Zone = 2,
            Seat = "22B",
            FlightClass = "Economy",
            FlightNumber = "AC237",
        },
        new BoardingPass
        {
            PassengerName = "Ms. Jane Doe",
            Origin = new Airport("YYC", "Calgary"),
            Destination = new Airport("YOW", "Ottawa"),
            Duration = new FlightDuration(3, 50),
            BoardingTime = new TimeOnly(12, 15),
            Departs = new DateTime(2019, 10, 17, 23, 45, 0),
            Arrives = new DateTime(2019, 10, 18, 2, 15, 0),
            Gate = "22",
            Zone = 1,
            Seat = "17C",
            FlightClass = "Economy",
            FlightNumber = "AC237",
        },
    ];
}
