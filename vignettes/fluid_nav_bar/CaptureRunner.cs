using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using FluidNavBar.Controls;
using FluidNavBar.Views;

namespace FluidNavBar;

/// <summary>
/// Walks the three tabs and grabs frames while the dip is travelling.
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
        var bar = FrameCapture.Find<FluidNavBarView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "1_first");

        // Across to the last tab, caught while the pane is dipping and again once it has settled.
        bar.SelectedIndex = 2;

        await Task.Delay(220);
        FrameCapture.Write(view, size, outputDirectory, "2_dipping");

        await Task.Delay(1300);
        FrameCapture.Write(view, size, outputDirectory, "3_third");

        // And one step back, the same way.
        bar.SelectedIndex = 1;

        await Task.Delay(220);
        FrameCapture.Write(view, size, outputDirectory, "4_returning");

        await Task.Delay(1300);
        FrameCapture.Write(view, size, outputDirectory, "5_second");

        FrameCapture.Shutdown();
    }

}
