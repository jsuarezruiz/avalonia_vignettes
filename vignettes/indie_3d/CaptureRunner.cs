using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(900);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        var elapsed = 0;

        foreach (var time in (int[])[0, 600])
        {
            await Task.Delay(time - elapsed);
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, $"1_drift_{time:0000}");
        }

        // Each page brings its own model, colour and artist.
        var pages = FrameCapture.Find<AvaloniaVignettes.Shared.Controls.PageView>(view);

        foreach (var page in (int[])[1, 2])
        {
            pages.SelectedIndex = page;

            // Mid flight: the name of the page being left is wiping out, the next has yet to arrive.
            await Task.Delay(120);
            FrameCapture.Write(view, size, outputDirectory, $"2_page_{page}_wiping");

            await Task.Delay(800);
            FrameCapture.Write(view, size, outputDirectory, $"2_page_{page}");
        }

        // A tap shoves the shapes of the page away from it.
        view.Tap(new Point(size.Width / 2d, size.Height / 2d));

        await Task.Delay(500);
        FrameCapture.Write(view, size, outputDirectory, "3_tapped");

    }

}
