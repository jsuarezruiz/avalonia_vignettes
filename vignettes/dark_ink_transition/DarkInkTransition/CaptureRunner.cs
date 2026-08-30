using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using DarkInkTransition.Views;

namespace DarkInkTransition;

/// <summary>
/// Runs the ink both ways and grabs frames while it is spreading.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;

        capture.Write("1_light");

        // Into the dark: one frame early in the spread, one late, one settled. The ink runs 1500.
        view.Toggle();

        await Task.Delay(400);
        capture.Write("2_ink_spreading");

        await Task.Delay(600);
        capture.Write("3_ink_closing");

        await Task.Delay(900);
        capture.Write("4_dark");

        // And back again, caught once mid way.
        view.Toggle();

        await Task.Delay(700);
        capture.Write("5_returning");

        await Task.Delay(1200);
        capture.Write("6_light_again");

    }

}
