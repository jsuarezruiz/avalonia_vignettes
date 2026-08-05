using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using DrinkRewardsList.Controls;
using DrinkRewardsList.Views;

namespace DrinkRewardsList;

/// <summary>
/// Opens a card and grabs frames as it springs open and fills, so the effect can be checked against
/// the Flutter reference without a human tapping anything.
/// </summary>
/// <remarks>Enabled with <c>--capture &lt;directory&gt;</c>.</remarks>
internal static class CaptureRunner
{
    /// <summary>
    /// Milliseconds after a card is opened at which to grab a frame.
    /// </summary>
    private static readonly int[] FrameTimes = [120, 300, 500, 800, 1200, 1800, 2600];


    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(700);

        var view = FrameCapture.Find<MainView>(window);
        var cards = window.GetVisualDescendants().OfType<DrinkCard>().ToList();
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "closed");

        // The first card is affordable and fills to the brim; the third only part way.
        foreach (var (index, name) in new[] { (0, "coffee"), (2, "latte") })
        {
            cards[index].IsOpen = true;

            var elapsed = 0;

            foreach (var time in FrameTimes)
            {
                await Task.Delay(Math.Max(0, time - elapsed));
                elapsed = time;

                FrameCapture.Write(view, size, outputDirectory, $"{name}_{time.ToString("0000", CultureInfo.InvariantCulture)}");
            }

            cards[index].IsOpen = false;
            await Task.Delay(1400);
        }

        FrameCapture.Shutdown();
    }

}
