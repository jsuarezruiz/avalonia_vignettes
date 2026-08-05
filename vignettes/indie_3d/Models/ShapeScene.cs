using System.Numerics;
using Avalonia;

namespace Indie3D.Models;

/// <summary>
/// The shapes drifting behind and in front of the artists: where they are, how they turn, and what
/// a tap does to them. Port of <c>indie_3d_model_controller.dart</c>.
/// </summary>
/// <remarks>
/// Thirty six of them, a dozen of each model, dealt out six to a layer and two layers to a page, so
/// a page shows one model in one colour. They rise slowly and wrap round when they reach the top.
/// </remarks>
public sealed class ShapeScene
{
    /// <summary>
    /// How many shapes there are in all.
    /// </summary>
    public const int InstanceCount = 36;

    /// <summary>
    /// How many shapes a layer draws.
    /// </summary>
    public const int LayerSize = 6;

    /// <summary>
    /// How many layers the scene is dealt into.
    /// </summary>
    public const int LayerCount = InstanceCount / LayerSize;

    /// <summary>
    /// How far the shapes drift up each second.
    /// </summary>
    private const float RiseRate = 1f;

    /// <summary>
    /// Where a shape wraps round to the bottom.
    /// </summary>
    private const float WrapHeight = 16f;

    /// <summary>
    /// How quickly a shape's velocities bleed away.
    /// </summary>
    private const float Drag = 0.2f;

    /// <summary>
    /// How far back the camera sits.
    /// </summary>
    private const float CameraDistance = 5.2f;

    /// <summary>
    /// How quickly the camera catches up with the page being scrolled to.
    /// </summary>
    private const float CameraChase = 4f;

    private readonly Vector3[] _positions = new Vector3[InstanceCount];
    private readonly Quaternion[] _rotations = new Quaternion[InstanceCount];
    private readonly Vector3[] _linearVelocities = new Vector3[InstanceCount];
    private readonly Vector3[] _angularVelocities = new Vector3[InstanceCount];
    private readonly Vector3[] _spins = new Vector3[InstanceCount];
    private readonly Mesh[] _meshes;

    private Matrix4x4 _view = Matrix4x4.Identity;
    private Matrix4x4 _projection = Matrix4x4.Identity;
    private float _cameraOffset;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeScene"/> class.
    /// </summary>
    public ShapeScene()
    {
        _meshes = [ObjLoader.Load("torus"), ObjLoader.Load("star"), ObjLoader.Load("cube")];

        Scatter();
        SetCamera(0f);
    }

    /// <summary>
    /// Gets or sets where the camera is heading, which scrolling a page moves.
    /// </summary>
    public double TargetCameraOffset { get; set; }

    /// <summary>
    /// Gets the mesh a layer draws, which is the same for every shape in it.
    /// </summary>
    /// <param name="layer">The layer, from 0 to <see cref="LayerCount"/>.</param>
    public Mesh MeshFor(int layer) => _meshes[Math.Clamp(layer * LayerSize / 12, 0, _meshes.Length - 1)];

    /// <summary>
    /// Works out the projection for a viewport of the given size.
    /// </summary>
    public void SetViewport(Size size)
    {
        if (size.Width > 0d && size.Height > 0d)
        {
            _projection = Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 2f,
                (float)(size.Width / size.Height),
                0.01f,
                100f);
        }
    }

    /// <summary>
    /// Gets the transform that takes a shape from model space to the screen.
    /// </summary>
    /// <param name="instance">Which shape, from 0 to <see cref="InstanceCount"/>.</param>
    public Matrix4x4 TransformFor(int instance) =>
        Matrix4x4.CreateFromQuaternion(_rotations[instance]) *
        Matrix4x4.CreateTranslation(_positions[instance]) *
        _view *
        _projection;

    /// <summary>
    /// Moves everything on by <paramref name="seconds"/>.
    /// </summary>
    public void Advance(double seconds)
    {
        var dt = (float)seconds;

        for (var i = 0; i < InstanceCount; i++)
        {
            _linearVelocities[i] = Slowed(_linearVelocities[i]);
            _angularVelocities[i] = Slowed(_angularVelocities[i]);

            _positions[i] += new Vector3(0f, RiseRate * dt, 0f) + (_linearVelocities[i] * dt);

            // The exponential map of a pure quaternion is a turn of twice its length about itself.
            var turn = (_angularVelocities[i] + _spins[i]) * 0.5f * dt;

            if (turn.Length() > 0f)
            {
                _rotations[i] = Quaternion.CreateFromAxisAngle(Vector3.Normalize(turn), turn.Length() * 2f)
                    * _rotations[i];
            }

            if (_positions[i].Y >= WrapHeight)
            {
                _positions[i].Y = -WrapHeight;
            }
        }

        _cameraOffset += (float)(TargetCameraOffset - _cameraOffset) * CameraChase * dt;

        SetCamera(_cameraOffset);
    }

    /// <summary>
    /// Shoves the shapes of one page away from where it was tapped, and sets them spinning.
    /// </summary>
    /// <param name="position">Where the tap landed.</param>
    /// <param name="viewport">The size of the page that was tapped.</param>
    /// <param name="page">Which page was tapped.</param>
    public void Tap(Point position, Size viewport, int page)
    {
        if (!Matrix4x4.Invert(_view * _projection, out var inverse))
        {
            return;
        }

        // The tap is a point on the screen; the shapes are in the world. Send it back out through
        // the camera at the depth the shapes drift at.
        var cameraNdc = Divided(Vector4.Transform(new Vector4(0f, 0f, -2f, 1f), _view * _projection));
        var ndc = new Vector4(
            (float)((position.X / viewport.Width * 2d) - 1d),
            (float)(((position.Y / viewport.Height * 2d) - 1d) * -1d),
            cameraNdc.Z,
            1f);

        var world = Divided(Vector4.Transform(ndc, inverse));

        for (var i = page * 12; i < (page * 12) + 12 && i < InstanceCount; i++)
        {
            var force = _positions[i] - new Vector3(world.X, world.Y, _positions[i].Z);
            var tangent = Vector3.Cross(force, new Vector3(0f, 0f, -1f));
            var push = Math.Clamp(8f / force.Length(), 0f, 24f);

            _linearVelocities[i] += Vector3.Normalize(force) * push;
            _angularVelocities[i] += Vector3.Normalize(tangent) * 4f;
        }
    }

    /// <summary>
    /// Bleeds a velocity away. Per frame rather than per second, as the original has it.
    /// </summary>
    private static Vector3 Slowed(Vector3 velocity)
    {
        var length = velocity.Length();

        return length > 0f ? velocity - (Vector3.Normalize(velocity) * 0.5f * Drag * length) : velocity;
    }

    private static Vector4 Divided(Vector4 value) =>
        value.W != 0f ? new Vector4(value.X / value.W, value.Y / value.W, value.Z / value.W, 1f) : value;

    private static float Random(float from, float to) =>
        from + ((float)System.Random.Shared.NextDouble() * (to - from));

    private void SetCamera(float offset) => _view = Matrix4x4.CreateLookAt(
        new Vector3(-offset, 0f, CameraDistance),
        new Vector3(-offset, 0f, 0f),
        Vector3.UnitY);

    private void Scatter()
    {
        for (var i = 0; i < InstanceCount; i++)
        {
            _positions[i] = new Vector3(Random(-4f, 4f), Random(-19f, 9f), Random(-4f, 0f));
            _rotations[i] = Quaternion.CreateFromAxisAngle(
                Vector3.Normalize(new Vector3(Random(-1f, 1f), Random(-1f, 1f), Random(-1f, 1f))),
                Random(0f, MathF.PI * 2f));
            _spins[i] = new Vector3(Random(-0.5f, 0.5f), Random(-0.5f, 0.5f), Random(-0.5f, 0.5f));
        }

        // Push any two that landed on top of each other apart, over and over, until the field is
        // evenly spread. The original relaxes it two hundred times.
        for (var pass = 0; pass < 200; pass++)
        {
            for (var j = 0; j < InstanceCount; j++)
            {
                for (var k = j + 1; k < InstanceCount; k++)
                {
                    var apart = _positions[k] - _positions[j];

                    if (new Vector2(apart.X, apart.Y).Length() >= 5f)
                    {
                        continue;
                    }

                    var nudge = Vector3.Normalize(new Vector3(Random(-1f, 1f), Random(-1f, 1f), 0f)) * 0.2f;

                    _positions[j] -= nudge;
                    _positions[k] += nudge;

                    _positions[j].X = Math.Clamp(_positions[j].X, -5f, 5f);
                    _positions[k].X = Math.Clamp(_positions[k].X, -5f, 5f);
                }
            }
        }
    }
}
