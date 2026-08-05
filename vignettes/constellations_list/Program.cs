using System.Diagnostics;
using Avalonia;
using Avalonia.Logging;

namespace ConstellationsList;

/// <summary>
/// Desktop bootstrapper.
/// </summary>
public static class Program
{
    /// <summary>
    /// The application entry point.
    /// </summary>
    /// <param name="args">
    /// Supports <c>--diagnostics</c>, which prints Avalonia's warnings (binding failures in
    /// particular) to the console, and <c>--capture &lt;directory&gt;</c>, which renders the
    /// reference frames described by <see cref="CaptureRunner"/> and exits.
    /// </param>
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--diagnostics"))
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Builds the Avalonia application, also used by the XAML previewer.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(LogEventLevel.Warning);
}
