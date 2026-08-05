using Avalonia.Media;

namespace Indie3D.Models;

/// <summary>
/// The two faces the vignette sets, for the drawing that does not go through a style.
/// </summary>
public static class Fonts
{
    /// <summary>
    /// Gets Staatliches, which the names and the chip are set in.
    /// </summary>
    public static FontFamily Display { get; } = new("avares://Indie3D/Assets/Fonts#Staatliches");

    /// <summary>
    /// Gets Roboto, which the small print is set in.
    /// </summary>
    public static FontFamily Body { get; } = new("avares://Indie3D/Assets/Fonts#Roboto");
}
