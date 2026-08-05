using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
    /// <summary>
    /// Milliseconds after a tab is picked at which to grab a frame.
    /// </summary>
    private static readonly int[] FrameTimes = [80, 200, 380, 700];


    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(700);

        var view = FrameCapture.Find<MainView>(window);
        var bar = FrameCapture.Find<NavBar>(window);
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "tab0_settled");

        for (var index = 1; index < bar.ItemCount; index++)
        {
            bar.SelectedIndex = index;

            var elapsed = 0;

            foreach (var time in FrameTimes)
            {
                await Task.Delay(Math.Max(0, time - elapsed));
                elapsed = time;

                FrameCapture.Write(view, size, outputDirectory, $"tab{index}_{time:0000}");
            }
        }

        FrameCapture.Shutdown();
    }

}
