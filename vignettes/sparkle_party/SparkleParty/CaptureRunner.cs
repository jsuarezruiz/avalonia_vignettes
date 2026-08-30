using Avalonia;
using Avalonia.Controls;
using AvaloniaVignettes.Shared.Capture;
using SparkleParty.Views;

namespace SparkleParty;

/// <summary>
/// Plays each effect in turn and grabs frames, with and without a touch point.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;
        var names = (string[])["waterfall", "fireworks", "comet", "pinwheel"];

        for (var effect = 0; effect < names.Length; effect++)
        {
            view.Show(effect);

            await Task.Delay(1200);

            capture.Write($"{effect + 1}_{names[effect]}");

            // And again with the screen being touched, which is what each effect is played with.
            view.Touch(new Point(capture.Size.Width / 2d, capture.Size.Height * 0.45d));

            await Task.Delay(900);

            capture.Write($"{effect + 1}_{names[effect]}_touched");

            view.Touch(null);
        }

    }

}
