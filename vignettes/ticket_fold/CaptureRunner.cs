using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
    /// <summary>
    /// Milliseconds after the fold begins at which to grab a frame.
    /// </summary>
    private static readonly int[] FrameTimes = [0, 100, 200, 300, 400, 500, 600, 700, 900, 1400];


    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // Let the window settle so the bindings that depend on its bounds have run.
        await Task.Delay(700);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "closed");

        var ticket = FrameCapture.Find<Ticket>(window);

        ticket.IsOpen = true;

        var elapsed = 0;

        foreach (var time in FrameTimes)
        {
            await Task.Delay(Math.Max(0, time - elapsed));
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, time.ToString("0000", CultureInfo.InvariantCulture));
        }

        FrameCapture.Shutdown();
    }

}
