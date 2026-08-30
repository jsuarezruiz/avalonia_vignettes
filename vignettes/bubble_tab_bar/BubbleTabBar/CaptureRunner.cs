using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using BubbleTabBar.Controls;
using BubbleTabBar.Views;

namespace BubbleTabBar;

/// <summary>
/// Steps through the tabs and grabs frames as each pill grows, so the transition can be checked
/// against the Flutter reference without a human clicking anything.
/// </summary>
/// <remarks>Enabled with <c>--capture &lt;directory&gt;</c>.</remarks>
internal static class CaptureRunner
{
    private static readonly int[] FrameTimes = [80, 200, 380, 700];


    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);
        var bar = FrameCapture.Find<NavBar>(window);

        capture.Write("tab0_settled");

        for (var index = 1; index < bar.ItemCount; index++)
        {
            bar.SelectedIndex = index;

            await capture.SampleAsync(FrameTimes, time => $"tab{index}_{time:0000}");
        }

    }

}
