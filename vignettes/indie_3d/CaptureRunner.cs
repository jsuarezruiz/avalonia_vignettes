using Avalonia;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using Indie3D.Views;

namespace Indie3D;

/// <summary>
/// Lets the shapes drift and swipes through the artists, grabbing frames as it goes.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;

        await capture.SampleAsync((int[])[0, 600], time => $"1_drift_{time:0000}");

        // Each page brings its own model, colour and artist.
        var pages = FrameCapture.Find<AvaloniaVignettes.Shared.Controls.PageView>(view);

        foreach (var page in (int[])[1, 2])
        {
            pages.SelectedIndex = page;

            // Mid flight: the name of the page being left is wiping out, the next has yet to arrive.
            await Task.Delay(120);
            capture.Write($"2_page_{page}_wiping");

            await Task.Delay(800);
            capture.Write($"2_page_{page}");
        }

        // A tap shoves the shapes of the page away from it.
        view.Tap(new Point(capture.Size.Width / 2d, capture.Size.Height / 2d));

        await Task.Delay(500);
        capture.Write("3_tapped");

    }

}
