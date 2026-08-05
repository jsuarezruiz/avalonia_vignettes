using System.Numerics;
using Avalonia.Media;

namespace Indie3D.Models;

/// <summary>
/// A triangle mesh with one flat colour, as the vignette's models are. Port of the shared package's
/// <c>VertexMesh</c>.
/// </summary>
/// <remarks>
/// The mesh also works out which triangles share each edge when it is built, which is what lets a
/// frame draw an outline rather than several hundred triangles.
/// </remarks>
public sealed class Mesh
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh"/> class.
    /// </summary>
    /// <param name="vertices">The vertices, in model space.</param>
    /// <param name="indices">Three indices per triangle.</param>
    /// <param name="colour">The material's diffuse colour.</param>
    public Mesh(Vector3[] vertices, int[] indices, Color colour)
    {
        Vertices = vertices;
        Indices = indices;
        Colour = colour;
        Edges = BuildEdges(indices);
    }

    /// <summary>
    /// One edge of the mesh, and the triangles either side of it.
    /// </summary>
    /// <param name="From">The vertex the edge runs from, as its left triangle winds it.</param>
    /// <param name="To">The vertex the edge runs to.</param>
    /// <param name="Left">The triangle that winds the edge this way round.</param>
    /// <param name="Right">The triangle on the other side, or -1 at a hole in the mesh.</param>
    public readonly record struct Edge(int From, int To, int Left, int Right);

    /// <summary>
    /// Gets the vertices, in model space.
    /// </summary>
    public Vector3[] Vertices { get; }

    /// <summary>
    /// Gets three indices per triangle.
    /// </summary>
    public int[] Indices { get; }

    /// <summary>
    /// Gets the material's diffuse colour.
    /// </summary>
    public Color Colour { get; }

    /// <summary>
    /// Gets every edge of the mesh, with the triangles that share it.
    /// </summary>
    public Edge[] Edges { get; }

    /// <summary>
    /// Gets how many triangles the mesh has.
    /// </summary>
    public int TriangleCount => Indices.Length / 3;

    private static Edge[] BuildEdges(int[] indices)
    {
        var found = new Dictionary<(int, int), int>();
        var edges = new List<Edge>();

        for (var triangle = 0; triangle < indices.Length / 3; triangle++)
        {
            for (var corner = 0; corner < 3; corner++)
            {
                var from = indices[(triangle * 3) + corner];
                var to = indices[(triangle * 3) + ((corner + 1) % 3)];
                var key = from < to ? (from, to) : (to, from);

                if (found.TryGetValue(key, out var existing))
                {
                    edges[existing] = edges[existing] with { Right = triangle };
                }
                else
                {
                    found[key] = edges.Count;
                    edges.Add(new Edge(from, to, triangle, -1));
                }
            }
        }

        return [.. edges];
    }
}
