using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using GooeyEdge.Controls;
using GooeyEdge.Views;

namespace GooeyEdge;

/// <summary>
/// Renders the vignette part-way through a page change so the edge can be checked against the
/// Flutter reference without a human driving the pointer.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. A page change is started and frames are grabbed
/// as the edge travels, each named for the milliseconds elapsed since it began.
/// </remarks>
internal static class CaptureRunner
{
    /// <summary>
    /// Milliseconds after the page change at which to grab a frame.
    /// </summary>
    private static readonly double[] SwipeFractions = [0d, 0.2d, 0.45d, 0.7d, 0.95d];


    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // Let the window settle so the bindings that depend on its bounds have run.
        await Task.Delay(700);

        var carousel = FrameCapture.Find<GooeyCarousel>(window);

        var view = FrameCapture.Find<MainView>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        foreach (var fraction in SwipeFractions)
        {
            await carousel.RunCaptureSwipeAsync(fraction);
            await Task.Delay(60);

            FrameCapture.Write(view, size, outputDirectory, fraction.ToString("0.00", CultureInfo.InvariantCulture));
        }

        FrameCapture.Shutdown();
    }

}
