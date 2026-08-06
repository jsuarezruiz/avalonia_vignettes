using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using ParallaxTravelCardsList.Controls;
using ParallaxTravelCardsList.Views;

namespace ParallaxTravelCardsList;

/// <summary>
/// Renders the vignette to PNGs at a set of fixed drag positions so its layout can be checked
/// against the Flutter reference without a human driving the pointer.
/// </summary>
/// <remarks>
/// Enabled with <c>--capture &lt;directory&gt;</c>. Each frame names the page and normalised offset
/// it was rendered at, for example <c>page1.00_offset+0.50.png</c>.
/// </remarks>
internal static class CaptureRunner
{
    private static readonly (double Page, double Offset)[] Frames =
    [
        (1.00d, 0.00d),
        (1.00d, 0.25d),
        (1.00d, 0.50d),
        (1.00d, 1.00d),
        (1.00d, -0.50d),
        (1.50d, 0.60d),
        (2.00d, 0.00d),
        (3.00d, 0.00d),
    ];


    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        // Let the window settle so the bindings that depend on its bounds have run.
        await Task.Delay(600);

        var cards = FrameCapture.Find<TravelCardList>(window);

        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);
        var view = FrameCapture.Find<MainView>(window);

        foreach (var (page, offset) in Frames)
        {
            cards.SetCaptureState(page, offset);

            // Two layout passes: one for the new scroll offset, one for anything it invalidated.
            window.UpdateLayout();
            await Task.Delay(120);
            window.UpdateLayout();

            var name = string.Format(
                CultureInfo.InvariantCulture,
                "page{0:0.00}_offset{1:+0.00;-0.00;+0.00}",
                page,
                offset);

            FrameCapture.Write(view, size, outputDirectory, name);
        }

        FrameCapture.Shutdown();
    }
}
