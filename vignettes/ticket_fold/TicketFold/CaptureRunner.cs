using System.Globalization;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using TicketFold.Controls;
using TicketFold.Views;

namespace TicketFold;

/// <summary>
/// Renders a complete open-and-close cycle so both directions of the fold can be checked against
/// the Flutter reference without a human tapping anything.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. The first ticket is folded open, held long enough to
/// read, and folded shut again. The two sets of sampled frames are named for the milliseconds
/// elapsed since each toggle.
/// </remarks>
internal static class CaptureRunner
{
    private static readonly int[] FrameTimes = [0, 100, 200, 300, 400, 500, 600, 700, 900, 1400];

    private static readonly TimeSpan OpenHold = TimeSpan.FromMilliseconds(1400);

    private static readonly TimeSpan FinalHold = TimeSpan.FromMilliseconds(210);

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // Let the window settle so the bindings that depend on its bounds have run.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);

        capture.Write("closed");

        var ticket = FrameCapture.Find<Ticket>(window);

        ticket.IsChecked = true;

        await capture.SampleAsync(
            FrameTimes,
            time => time.ToString("0000", CultureInfo.InvariantCulture));

        await Task.Delay(OpenHold);

        ticket.IsChecked = false;

        await capture.SampleAsync(
            FrameTimes,
            time => $"close_{time.ToString("0000", CultureInfo.InvariantCulture)}");

        // Give the loop a short, readable closed state before its first frame comes around again.
        await Task.Delay(FinalHold);
    }

}
