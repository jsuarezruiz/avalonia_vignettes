using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using FluidNavBar.Controls;
using FluidNavBar.Views;

namespace FluidNavBar;

/// <summary>
/// Walks the three tabs and grabs frames while the dip is travelling.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var bar = FrameCapture.Find<FluidNavBarView>(window);

        capture.Write("1_first");

        // Across to the last tab, caught while the pane is dipping and again once it has settled.
        bar.SelectedIndex = 2;

        await Task.Delay(220);
        capture.Write("2_dipping");

        await Task.Delay(1300);
        capture.Write("3_third");

        // And one step back, the same way.
        bar.SelectedIndex = 1;

        await Task.Delay(220);
        capture.Write("4_returning");

        await Task.Delay(1300);
        capture.Write("5_second");

    }

}
