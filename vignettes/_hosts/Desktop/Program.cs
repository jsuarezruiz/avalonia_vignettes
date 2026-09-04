using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Logging;

namespace AvaloniaVignettes.Hosts.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--diagnostics"))
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<VignetteApp>()
            .UsePlatformDetect()
            .AfterSetup(_ => ApplyApplicationMetadata())
            .WithInterFont()
            .LogToTrace(LogEventLevel.Warning);

    private static void ApplyApplicationMetadata()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var applicationName = entryAssembly?
            .GetCustomAttribute<AssemblyTitleAttribute>()?
            .Title;

        if (string.IsNullOrWhiteSpace(applicationName))
        {
            applicationName = entryAssembly?.GetName().Name ?? "Avalonia Vignette";
        }

        if (Application.Current is { } application)
        {
            application.Name = applicationName;
        }

        var dockIconApplied = MacOSDockIcon.Apply(applicationName);
        Trace.WriteLine(
            $"Desktop metadata applied: name='{applicationName}', macOS Dock icon={dockIconApplied}.");
    }
}
