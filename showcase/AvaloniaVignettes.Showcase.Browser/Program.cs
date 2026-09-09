using Avalonia;
using Avalonia.Browser;
using AvaloniaVignettes.Showcase;

namespace AvaloniaVignettes.Showcase.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args) =>
        BuildAvaloniaApp().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .WithInterFont();
}
