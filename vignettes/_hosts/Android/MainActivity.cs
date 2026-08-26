using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace AvaloniaVignettes.Hosts.Android;

[Activity(
    Theme = "@style/VignetteTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity
{
}
