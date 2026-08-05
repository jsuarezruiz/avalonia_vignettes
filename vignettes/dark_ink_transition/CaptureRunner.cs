using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using DarkInkTransition.Views;

namespace DarkInkTransition;

/// <summary>
/// Runs the ink both ways and grabs frames while it is spreading.
/// </summary>
internal static class CaptureRunner
{

    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(900);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "1_light");

        // Into the dark: one frame early in the spread, one late, one settled. The ink runs 1500.
        view.Toggle();

        await Task.Delay(400);
        FrameCapture.Write(view, size, outputDirectory, "2_ink_spreading");

        await Task.Delay(600);
        FrameCapture.Write(view, size, outputDirectory, "3_ink_closing");

        await Task.Delay(900);
        FrameCapture.Write(view, size, outputDirectory, "4_dark");

        // And back again, caught once mid way.
        view.Toggle();

        await Task.Delay(700);
        FrameCapture.Write(view, size, outputDirectory, "5_returning");

        await Task.Delay(1200);
        FrameCapture.Write(view, size, outputDirectory, "6_light_again");

        FrameCapture.Shutdown();
    }

}
