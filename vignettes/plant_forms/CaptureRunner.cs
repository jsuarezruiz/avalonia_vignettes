using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaVignettes.Shared.Capture;
using PlantForms.Controls;
using PlantForms.Views;

namespace PlantForms;

/// <summary>
/// Fills the form in and walks its pages, grabbing frames as it goes, so the cards, the fields and
/// the stacking can be checked without a human typing anything.
/// </summary>
internal static class CaptureRunner
{

    /// <summary>
    /// Renders every frame to <paramref name="outputDirectory"/> and shuts the app down.
    /// </summary>
    public static async Task RunAsync(Window window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await Task.Delay(600);

        var view = FrameCapture.Find<MainView>(window);
        var stack = FrameCapture.Find<FormCardStack>(window);
        var pages = stack.GetVisualChildren().OfType<FormPage>().ToList();
        var size = new PixelSize((int)window.ClientSize.Width, (int)window.ClientSize.Height);

        FrameCapture.Write(view, size, outputDirectory, "1_summary");

        // Push the second page and watch it slide up over the first.
        stack.Push();

        var elapsed = 0;

        foreach (var time in (int[])[80, 160, 400])
        {
            await Task.Delay(time - elapsed);
            elapsed = time;

            FrameCapture.Write(view, size, outputDirectory, $"2_push_{time:0000}");
        }

        // Fill the information page in, which fills its button up.
        var information = pages.OfType<InformationPage>().FirstOrDefault()
                          ?? throw new InvalidOperationException("No InformationPage was found in the stack.");

        Fill(information, "email", "javier@example.com");
        await Task.Delay(100);
        FrameCapture.Write(view, size, outputDirectory, "3_email");

        foreach (var (key, value) in new[]
        {
            ("last_name", "Suarez"),
            ("address", "1 Alameda"),
            ("city", "Seville"),
            ("postal", "41001"),
            ("phone", "555 555 5555"),
        })
        {
            Fill(information, key, value);
        }

        await Task.Delay(200);
        ScrollToEnd(information);
        await Task.Delay(100);
        FrameCapture.Write(view, size, outputDirectory, "4_information_filled");

        // An unfinished page fills its button only as far as it has got, and says so when pressed.
        var postal = information.GetVisualDescendants().OfType<FormField>().First(x => x.FieldKey == "postal");
        postal.Value = string.Empty;

        FrameCapture.Find<SubmitButton>(information)
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await Task.Delay(100);
        FrameCapture.Write(view, size, outputDirectory, "5_incomplete");

        postal.Value = "41001";
        await Task.Delay(100);

        // The country picker opens as a page of its own.
        var options = FrameCapture.Find<OptionsOverlay>(view);
        options.Show(FrameCapture.Find<DropDownField>(information));

        await Task.Delay(200);
        FrameCapture.Write(view, size, outputDirectory, "6_options");

        options.IsVisible = false;
        await Task.Delay(100);

        // On to payment, and type a card number that names its network.
        stack.Push();
        await Task.Delay(500);

        var payment = pages.OfType<PaymentPage>().FirstOrDefault()
                      ?? throw new InvalidOperationException("No PaymentPage was found in the stack.");

        Fill(payment, "ccNumber", "4111222233334440");
        Fill(payment, "ccName", "Javier Suarez");
        Fill(payment, "ccExpDate", "1230");
        Fill(payment, "ccCode", "123");

        await Task.Delay(700);
        FrameCapture.Write(view, size, outputDirectory, "7_payment");

        ScrollToEnd(payment);
        await Task.Delay(200);
        FrameCapture.Write(view, size, outputDirectory, "7b_payment_end");

        // And back down the stack.
        stack.Pop();
        await Task.Delay(150);
        FrameCapture.Write(view, size, outputDirectory, "8_pop");

        FrameCapture.Shutdown();
    }

    private static void ScrollToEnd(FormPage page)
    {
        if (page.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault() is { } scroller)
        {
            scroller.Offset = new Vector(0d, scroller.Extent.Height);
        }
    }

    private static void Fill(FormPage page, string key, string value)
    {
        var field = page.GetVisualDescendants().OfType<FormField>().FirstOrDefault(x => x.FieldKey == key);

        if (field is not null)
        {
            field.Value = value;
        }
    }

}
