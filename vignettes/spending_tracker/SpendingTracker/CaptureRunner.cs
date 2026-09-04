using Avalonia.Controls;
using Avalonia;
using AvaloniaVignettes.Shared.Capture;
using SpendingTracker.Views;

namespace SpendingTracker;

/// <summary>
/// Drags the chart about and grabs frames along the way.
/// </summary>
internal static class CaptureRunner
{

    public static async Task RunAsync(Window window, string outputDirectory)
    {
        var capture = await FrameCapture.StartAsync<MainView>(window, outputDirectory, 900);
        var view = capture.View;

        capture.Write("1_opening");

        var categories = FrameCapture.Find<CategoryList>(window);
        var categoryUpdates = 0;
        void ObserveCategoryUpdate(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == CategoryList.BillsProperty) categoryUpdates++;
        }
        categories.PropertyChanged += ObserveCategoryUpdate;
        view.Select(7);
        categories.PropertyChanged -= ObserveCategoryUpdate;
        if (categoryUpdates == 0 || Math.Abs(categories.Bills + categories.Personal + categories.Restaurants - 1d) > 1e-9)
        {
            throw new InvalidOperationException("Selecting a month must refresh the normalized category shares.");
        }

        await Task.Delay(400);

        capture.Write("2_selected");

        // Part way through a month, which is what a drag leaves behind before the snap catches up.
        view.Slide(3.4d);

        await Task.Delay(120);

        capture.Write("3_dragged");

        view.Release();

        await Task.Delay(1200);

        capture.Write("4_settled");

    }

}
