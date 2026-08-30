using System.Globalization;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Platform;

namespace Indie3D.Models;

/// <summary>
/// Reads the Wavefront models the vignette ships. Port of the shared package's <c>OBJLoader</c>,
/// less the parts it never uses: these models carry no textures, and their faces are triangles.
/// </summary>
public static class ObjLoader
{
    private const string MeshRoot = "avares://Indie3D/Assets/Meshes";

    /// <summary>
    /// Loads the mesh of the given name, along with the colour its material names.
    /// </summary>
    /// <param name="name">The file name, without its extension.</param>
    public static Mesh Load(string name)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var material = string.Empty;

        foreach (var line in ReadLines($"{name}.obj"))
        {
            if (line.StartsWith("v ", StringComparison.Ordinal))
            {
                var parts = Split(line);

                vertices.Add(new Vector3(Number(parts[1]), Number(parts[2]), Number(parts[3])));
            }
            else if (line.StartsWith("f ", StringComparison.Ordinal))
            {
                var parts = Split(line);

                // Only the position index is used: these meshes are flat shaded and untextured.
                for (var corner = 1; corner <= 3; corner++)
                {
                    indices.Add(int.Parse(parts[corner].Split('/')[0], CultureInfo.InvariantCulture) - 1);
                }
            }
            else if (line.StartsWith("mtllib ", StringComparison.Ordinal))
            {
                material = Split(line)[1];
            }
        }

        return new Mesh([.. vertices], [.. indices], LoadColour(material));
    }

    private static Color LoadColour(string material)
    {
        foreach (var line in ReadLines(material))
        {
            if (!line.StartsWith("Kd ", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = Split(line);

            return Color.FromRgb(Channel(parts[1]), Channel(parts[2]), Channel(parts[3]));
        }

        return Colors.White;
    }

    private static IEnumerable<string> ReadLines(string file)
    {
        using var stream = AssetLoader.Open(new Uri($"{MeshRoot}/{file}"));
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            yield return line.Replace("\r", string.Empty);
        }
    }

    private static string[] Split(string line) =>
        line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static float Number(string value) => float.Parse(value, CultureInfo.InvariantCulture);

    private static byte Channel(string value) => (byte)Math.Round(Number(value) * 255f);
}
