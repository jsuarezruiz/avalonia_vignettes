using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 600);
        var view = capture.View;
        var stack = FrameCapture.Find<FormCardStack>(window);
        var pages = stack.GetVisualChildren().OfType<FormPage>().ToList();

        capture.Write("1_summary");

        // Use the real submit button so the sample's routed NextRequested event is covered.
        var summary = pages.OfType<SummaryPage>().FirstOrDefault()
                      ?? throw new InvalidOperationException("No SummaryPage was found in the stack.");

        FrameCapture.Find<SubmitButton>(summary)
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        if (stack.Index != 1)
            throw new InvalidOperationException("The summary button must advance through NextRequested.");

        await capture.SampleAsync((int[])[80, 160, 400], time => $"2_push_{time:0000}");

        // Fill the information page in, which fills its button up.
        var information = pages.OfType<InformationPage>().FirstOrDefault()
                          ?? throw new InvalidOperationException("No InformationPage was found in the stack.");

        Fill(information, "email", "javier@example.com");
        await Task.Delay(100);
        capture.Write("3_email");

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
        capture.Write("4_information_filled");

        // An unfinished page fills its button only as far as it has got, and says so when pressed.
        var postal = information.GetVisualDescendants().OfType<FormField>().First(x => x.FieldKey == "postal");
        postal.Value = string.Empty;

        FrameCapture.Find<SubmitButton>(information)
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await Task.Delay(100);
        capture.Write("5_incomplete");

        postal.Value = "41001";
        await Task.Delay(100);

        // The country picker opens as a page of its own.
        var options = FrameCapture.Find<OptionsOverlay>(view);
        options.Show(FrameCapture.Find<DropDownField>(information));

        await Task.Delay(200);
        capture.Write("6_options");

        // Exercise the native ListBox's two-way binding and Done button, not the backing CLR
        // setter. A getter-only registered DirectProperty previously discarded the selection.
        var countries = FrameCapture.Find<ListBox>(options);
        countries.SelectedItem = "France";
        FrameCapture.Find<Button>(options).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var countryField = FrameCapture.Find<DropDownField>(information);
        if (countryField.Value != "France" || !information.GetVisualDescendants().OfType<FormField>().Any(x => x.FieldKey == "company"))
            throw new InvalidOperationException("Country selection must persist and rebuild its address fields.");
        options.Show(countryField);
        countries.SelectedItem = "United States";
        FrameCapture.Find<Button>(options).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        await Task.Delay(100);

        // Use the real continue button so completion validation and NextRequested are both covered.
        FrameCapture.Find<SubmitButton>(information)
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        if (stack.Index != 2)
            throw new InvalidOperationException("A complete information form must advance to payment.");

        // On to payment, and type a card number that names its network.
        await Task.Delay(500);

        var payment = pages.OfType<PaymentPage>().FirstOrDefault()
                      ?? throw new InvalidOperationException("No PaymentPage was found in the stack.");

        Fill(payment, "ccNumber", "4111222233334440");
        Fill(payment, "ccName", "Javier Suarez");
        Fill(payment, "ccExpDate", "1230");
        Fill(payment, "ccCode", "123");

        var paymentFields = payment.GetVisualDescendants()
            .OfType<FormField>()
            .ToDictionary(field => field.FieldKey);

        if (paymentFields["ccNumber"].Value != "4111 2222 3333 4440" ||
            paymentFields["ccExpDate"].Value != "12/30" ||
            paymentFields["ccCode"].Value != "123")
        {
            throw new InvalidOperationException("Credit-card fields must retain their Flutter-style masks.");
        }

        await Task.Delay(700);

        var purchase = FrameCapture.Find<SubmitButton>(payment);
        if (purchase.Completion < 0.999d)
            throw new InvalidOperationException("A valid payment form must completely fill its submit button.");

        var notifications = FrameCapture.Find<CheckBox>(payment);
        if (notifications.IsChecked != true)
            throw new InvalidOperationException("Shipping notifications must start selected.");

        notifications.IsChecked = false;
        if (notifications.IsChecked != false)
            throw new InvalidOperationException("Shipping notifications must be toggleable.");
        notifications.IsChecked = true;

        capture.Write("7_payment");

        ScrollToEnd(payment);
        await Task.Delay(200);
        capture.Write("7b_payment_end");

        // And back down the stack through the page's real back button/routed event.
        var back = payment.GetVisualDescendants()
            .OfType<Button>()
            .FirstOrDefault(button => button.Name == "PART_BackArea")
            ?? throw new InvalidOperationException("The payment page has no back button.");

        back.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (stack.Index != 1)
            throw new InvalidOperationException("The payment back button must pop the card stack.");

        await Task.Delay(150);
        capture.Write("8_pop");

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
