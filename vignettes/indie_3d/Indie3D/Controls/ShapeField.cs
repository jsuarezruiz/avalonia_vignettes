using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Indie3D.Models;

namespace Indie3D.Controls;

/// <summary>
/// One layer of shapes, drawn through a blend mode. Port of <c>indie_3d_model.dart</c> and the
/// shared package's <c>MeshCustomPainter</c>, and of the <c>BlendMask</c> wrapped round them.
/// </summary>
/// <remarks>
/// The original projects every vertex, drops the triangles facing away, sorts what is left by depth
/// and fills it. Two of those steps earn nothing here: the original disables its own lighting, so a
/// layer is a single flat colour, which makes the depth order invisible and the inside of a shape
/// identical to its outline. So this walks the silhouette instead, the edges where a triangle facing
/// the camera meets one facing away, and fills the loops they make. Same picture, about fifty edges
/// instead of eight hundred triangles, and the frame cost falls with it.
/// <para>
/// Avalonia applies a blend mode to bitmaps and to nothing else, so the loops become a clip and the
/// colour arrives as a one pixel image stretched through it. That is also what the original does at
/// heart: <c>BlendMask</c> blends the whole layer once, not each triangle.
/// </para>
/// </remarks>
public sealed class ShapeField : Control
{
    public static readonly StyledProperty<ShapeScene?> SceneProperty =
        AvaloniaProperty.Register<ShapeField, ShapeScene?>(nameof(Scene));

    public static readonly StyledProperty<int> LayerProperty =
        AvaloniaProperty.Register<ShapeField, int>(nameof(Layer));

    public static readonly StyledProperty<BitmapBlendingMode> BlendModeProperty =
        AvaloniaProperty.Register<ShapeField, BitmapBlendingMode>(
            nameof(BlendMode),
            BitmapBlendingMode.SourceOver);

    public static readonly StyledProperty<double> LayerOpacityProperty =
        AvaloniaProperty.Register<ShapeField, double>(nameof(LayerOpacity), 1d);

    private Vector3[] _projected = [];
    private bool[] _facing = [];
    private bool[] _used = [];
    private int[] _firstEdgeAt = [];
    private int[] _nextEdgeFrom = [];

    static ShapeField() =>
        AffectsRender<ShapeField>(SceneProperty, LayerProperty, BlendModeProperty, LayerOpacityProperty);

    /// <summary>
    /// Gets or sets the shapes to draw from.
    /// </summary>
    public ShapeScene? Scene
    {
        get => GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    /// <summary>
    /// Gets or sets which layer of the scene this draws.
    /// </summary>
    public int Layer
    {
        get => GetValue(LayerProperty);
        set => SetValue(LayerProperty, value);
    }

    /// <summary>
    /// Gets or sets how the layer is blended with what is behind it.
    /// </summary>
    public BitmapBlendingMode BlendMode
    {
        get => GetValue(BlendModeProperty);
        set => SetValue(BlendModeProperty, value);
    }

    /// <summary>
    /// Gets or sets how strongly the layer is laid over what is behind it.
    /// </summary>
    public double LayerOpacity
    {
        get => GetValue(LayerOpacityProperty);
        set => SetValue(LayerOpacityProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Scene is not { } scene || Bounds.Width <= 0d || Bounds.Height <= 0d)
        {
            return;
        }

        var mesh = scene.MeshFor(Layer);
        var geometry = new StreamGeometry();
        var drawn = false;

        using (var sink = geometry.Open())
        {
            // A layer is the union of its shapes, so a point inside two of them is still inside.
            // Even odd, the default, would cut a hole wherever two shapes crossed; non zero also
            // leaves a torus its own hole, since that loop only cancels its own outline.
            sink.SetFillRule(FillRule.NonZero);

            for (var slot = 0; slot < ShapeScene.LayerSize; slot++)
            {
                drawn |= Outline(sink, mesh, scene.TransformFor((Layer * ShapeScene.LayerSize) + slot));
            }
        }

        if (!drawn)
        {
            return;
        }

        // Only the shapes need blending, so the colour covers their bounds rather than the page.
        var covered = geometry.Bounds.Intersect(new Rect(Bounds.Size));

        if (covered.Width <= 0d || covered.Height <= 0d)
        {
            return;
        }

        using var clip = context.PushGeometryClip(geometry);
        using var blend = context.PushRenderOptions(new RenderOptions { BitmapBlendingMode = BlendMode });
        using var opacity = context.PushOpacity(LayerOpacity);

        context.DrawImage(Swatches.Of(mesh.Colour), covered);
    }

    private static Point ToScreen(Vector3 ndc, double width, double height) =>
        new((ndc.X + 1d) * width * 0.5d, (1d - ndc.Y) * height * 0.5d);

    /// <summary>
    /// Traces one shape's outline into <paramref name="sink"/>.
    /// </summary>
    private bool Outline(StreamGeometryContext sink, Mesh mesh, Matrix4x4 transform)
    {
        Project(mesh, transform);

        var edges = mesh.Edges;

        if (_used.Length < edges.Length)
        {
            _used = new bool[edges.Length];
        }

        if (_nextEdgeFrom.Length < edges.Length)
        {
            _nextEdgeFrom = new int[edges.Length];
        }

        Array.Clear(_used, 0, edges.Length);
        Array.Fill(_firstEdgeAt, -1);

        // An edge is on the outline when the triangles either side of it disagree about whether
        // they face the camera. Wind it the way the one that does faces.
        for (var i = 0; i < edges.Length; i++)
        {
            var left = _facing[edges[i].Left];
            var right = edges[i].Right >= 0 && _facing[edges[i].Right];

            if (left == right)
            {
                _used[i] = true;
                continue;
            }

            // Thread the edges leaving each vertex together, so following the outline needs no
            // lookup and no allocation.
            var from = left ? edges[i].From : edges[i].To;

            _nextEdgeFrom[i] = _firstEdgeAt[from];
            _firstEdgeAt[from] = i;
        }

        var drawn = false;

        for (var i = 0; i < edges.Length; i++)
        {
            if (!_used[i])
            {
                drawn |= Loop(sink, edges, i);
            }
        }

        return drawn;
    }

    private bool Loop(StreamGeometryContext sink, Mesh.Edge[] edges, int first)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        var edge = first;
        var start = Ends(edges[first]).From;
        var drawn = 0;

        while (edge >= 0 && !_used[edge])
        {
            var (from, to) = Ends(edges[edge]);

            _used[edge] = true;

            if (drawn == 0)
            {
                sink.BeginFigure(ToScreen(_projected[from], width, height), isFilled: true);
            }

            sink.LineTo(ToScreen(_projected[to], width, height));
            drawn++;

            edge = to == start ? -1 : NextFrom(to);
        }

        if (drawn > 0)
        {
            sink.EndFigure(isClosed: true);
        }

        return drawn > 0;

        (int From, int To) Ends(Mesh.Edge value) =>
            _facing[value.Left] ? (value.From, value.To) : (value.To, value.From);

        int NextFrom(int vertex)
        {
            for (var candidate = _firstEdgeAt[vertex]; candidate >= 0; candidate = _nextEdgeFrom[candidate])
            {
                if (!_used[candidate])
                {
                    return candidate;
                }
            }

            return -1;
        }
    }

    private void Project(Mesh mesh, Matrix4x4 transform)
    {
        if (_projected.Length != mesh.Vertices.Length)
        {
            _projected = new Vector3[mesh.Vertices.Length];
            _facing = new bool[mesh.TriangleCount];
            _firstEdgeAt = new int[mesh.Vertices.Length];
        }

        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            var position = Vector4.Transform(new Vector4(mesh.Vertices[i], 1f), transform);

            _projected[i] = position.W != 0f
                ? new Vector3(position.X / position.W, position.Y / position.W, position.Z / position.W)
                : new Vector3(position.X, position.Y, position.Z);
        }

        for (var triangle = 0; triangle < _facing.Length; triangle++)
        {
            var a = _projected[mesh.Indices[triangle * 3]];
            var b = _projected[mesh.Indices[(triangle * 3) + 1]];
            var c = _projected[mesh.Indices[(triangle * 3) + 2]];

            // Counter clockwise on screen is facing the camera, as the original has it.
            _facing[triangle] = Vector3.Cross(b - a, c - a).Z > 0f;
        }
    }
}
