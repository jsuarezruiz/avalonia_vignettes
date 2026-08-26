using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using SpendingTracker.Views;

namespace SpendingTracker;

/// <summary>
/// Drags the chart about and grabs frames along the way.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(900);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "1_opening");

        view.Select(7);

        await Task.Delay(400);

        FrameCapture.Write(view, size, outputDirectory, "2_selected");

        // Part way through a month, which is what a drag leaves behind before the snap catches up.
        view.Slide(3.4d);

        await Task.Delay(120);

        FrameCapture.Write(view, size, outputDirectory, "3_dragged");

        view.Release();

        await Task.Delay(1200);

        FrameCapture.Write(view, size, outputDirectory, "4_settled");

    }

}
