using UIKit;

namespace AvaloniaVignettes.Hosts.iOS;

public static class Program
{
    public static void Main(string[] args) =>
        UIApplication.Main(args, null, typeof(VignetteApplication));
}
