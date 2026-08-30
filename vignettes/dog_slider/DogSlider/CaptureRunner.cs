using System.Globalization;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using DogSlider.Controls;
using DogSlider.Views;

namespace DogSlider;

/// <summary>
/// Drives the slider across its range and grabs frames as the dog gives chase, so the walk and the
/// physics can be checked without a human dragging anything.
/// </summary>
/// <remarks>Enabled with <c>--capture &lt;directory&gt;</c>.</remarks>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // Long enough for the dog's start delay to pass.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 1200);
        var slider = FrameCapture.Find<Controls.DogSlider>(window);

        capture.Write("rest");

        // Send the ball down the line and watch the dog run after it. Half way keeps the dog
        // clear of the window edge so the settled pose can be seen whole.
        slider.Value = 0.55d;

        await capture.SampleAsync(
            (int[])[150, 400, 700, 1000, 1400, 2000],
            time => $"chase_{time.ToString("0000", CultureInfo.InvariantCulture)}");

        // Let the dog actually reach the ball and fold into a sit.
        await Task.Delay(2500);
        capture.Write("sitting");

        // Back to zero: the dog turns round and leaves.
        slider.Value = 0d;
        await capture.SampleAsync(
            (int[])[300, 700, 1100, 1600],
            time => $"leave_{time.ToString("0000", CultureInfo.InvariantCulture)}");

    }

}
