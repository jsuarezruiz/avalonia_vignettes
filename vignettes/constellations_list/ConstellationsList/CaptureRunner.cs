using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using ConstellationsList.Views;

namespace ConstellationsList;

/// <summary>
/// Records the same two journeys as the original preview: Aries flies from the list into its detail
/// page and back, the list is flung and settles, then Cetus flies into its detail page.
/// </summary>
/// <remarks>Enabled with <c>--capture &lt;directory&gt;</c>.</remarks>
internal static class CaptureRunner
{
    private static readonly TimeSpan FirstDetailHold = TimeSpan.FromMilliseconds(4700);

    private static readonly TimeSpan ReturnDuration = TimeSpan.FromMilliseconds(1000);

    private static readonly TimeSpan ScrollOutDuration = TimeSpan.FromMilliseconds(700);

    private static readonly TimeSpan ScrollBackDuration = TimeSpan.FromMilliseconds(1750);

    private static readonly TimeSpan BeforeSecondFlight = TimeSpan.FromMilliseconds(890);

    private static readonly TimeSpan SecondDetailHold = TimeSpan.FromMilliseconds(3735);

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 700);

        capture.Write("list");

        // Use the real row click path. It runs the shared-element card flight, page cross-fade,
        // star-flight sequence and delayed chart reveal together.
        Click(RowAt(window, 0));
        await Task.Delay(FirstDetailHold);
        capture.Write("aries_detail");

        Click(ReturnButton(window));
        await Task.Delay(ReturnDuration);
        capture.Write("returned");

        // Recreate the original preview's quick fling and rebound. Updating the native
        // ScrollViewer also exercises the normal ScrollChanged -> star-speed event path.
        var scroller = FrameCapture.Find<ScrollViewer>(window);
        await AnimateScrollAsync(scroller, 850d, ScrollOutDuration);
        await AnimateScrollAsync(scroller, 160d, ScrollBackDuration);
        capture.Write("scrolled");

        await Task.Delay(BeforeSecondFlight);

        Click(RowAt(window, 3));
        await Task.Delay(SecondDetailHold);
        capture.Write("cetus_detail");
    }

    private static Button RowAt(Visual root, int index) =>
        root.GetVisualDescendants()
            .OfType<Button>()
            .Where(button => button.Classes.Contains("constellationRow"))
            .ElementAt(index);

    private static Button ReturnButton(Visual root) =>
        root.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => !button.Classes.Contains("constellationRow"));

    private static void Click(Button button) =>
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    private static async Task AnimateScrollAsync(ScrollViewer scroller, double target, TimeSpan duration)
    {
        const int frameMilliseconds = 35;

        var start = scroller.Offset.Y;
        var frames = Math.Max(1, (int)Math.Ceiling(duration.TotalMilliseconds / frameMilliseconds));

        for (var frame = 1; frame <= frames; frame++)
        {
            var progress = frame / (double)frames;
            var eased = 1d - Math.Pow(1d - progress, 2d);

            scroller.Offset = new Vector(0d, start + ((target - start) * eased));
            await Task.Delay(frameMilliseconds);
        }
    }
}
