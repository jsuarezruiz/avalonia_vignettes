using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace AvaloniaVignettes.Showcase.Models;

public sealed record VignetteSample(
    string Id,
    string Number,
    string Title,
    string Category,
    string Description,
    string Instruction,
    Lazy<Bitmap> Preview,
    Uri CodeUri,
    ThemeVariant Theme,
    Func<Control> CreateView,
    IReadOnlyList<Uri> ResourceUris,
    IReadOnlyList<Uri> StyleUris);
