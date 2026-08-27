using System.Globalization;
using Avalonia.Controls;
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


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // Let the window settle so the bindings that depend on its bounds have run.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 600);
        var cards = FrameCapture.Find<TravelCardList>(window);

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

            capture.Write(name);
        }

    }
}
