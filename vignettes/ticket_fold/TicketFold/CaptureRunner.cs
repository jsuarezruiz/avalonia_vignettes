using System.Globalization;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using TicketFold.Controls;
using TicketFold.Views;

namespace TicketFold;

/// <summary>
/// Renders the vignette part-way through a fold so the panels can be checked against the Flutter
/// reference without a human tapping anything.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. The first ticket is folded open and frames are
/// grabbed as it goes, each named for the milliseconds elapsed since the tap.
/// </remarks>
internal static class CaptureRunner
{
    private static readonly int[] FrameTimes = [0, 100, 200, 300, 400, 500, 600, 700, 900, 1400];


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // Let the window settle so the bindings that depend on its bounds have run.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);

        capture.Write("closed");

        var ticket = FrameCapture.Find<Ticket>(window);

        ticket.IsOpen = true;

        await capture.SampleAsync(
            FrameTimes,
            time => time.ToString("0000", CultureInfo.InvariantCulture));

    }

}
