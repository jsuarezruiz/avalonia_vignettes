using System.Globalization;
using Avalonia.Controls;
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
    private static readonly int[] FrameTimes = [120, 300, 500, 800, 1200, 1800, 2600];


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);
        var cards = window.GetVisualDescendants().OfType<DrinkCard>().ToList();

        capture.Write("closed");

        // The first card is affordable and fills to the brim; the third only part way.
        foreach (var (index, name) in new[] { (0, "coffee"), (2, "latte") })
        {
            cards[index].IsChecked = true;

            await capture.SampleAsync(
                FrameTimes,
                time => $"{name}_{time.ToString("0000", CultureInfo.InvariantCulture)}");

            cards[index].IsChecked = false;
            await Task.Delay(1400);
        }

    }

}
