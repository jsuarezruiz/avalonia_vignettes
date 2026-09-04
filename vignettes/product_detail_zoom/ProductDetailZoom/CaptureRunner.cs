using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaVignettes.Shared.Capture;
using ProductDetailZoom.Views;

namespace ProductDetailZoom;

/// <summary>
/// Presses the zoom and grabs frames along the three second flight.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        // The zoom button holds itself back for a second before fading in.
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 1800);
        var product = FrameCapture.Find<ProductPage>(window);

        capture.Write("1_product");

        // The whole real path: the press, its 300 of fade, then the three second route with the
        // speaker spinning through the black middle of it. Exercise the native Button.Click route.
        product.ZoomButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        await Task.Delay(1000);
        capture.Write("2_leaving");

        await Task.Delay(800);
        capture.Write("3_spinning");

        await Task.Delay(1000);
        capture.Write("4_arriving");

        await Task.Delay(1200);
        capture.Write("5_detail");

    }

}
