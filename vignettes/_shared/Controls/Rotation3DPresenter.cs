using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaVignettes.Shared.Media;

namespace AvaloniaVignettes.Shared.Controls;

/// <summary>
/// Rotates its child in 3D around the child's centre. Port of the <c>Rotation3d</c> widget shared
/// by the Flutter vignettes.
/// </summary>
/// <remarks>
/// Angles are in degrees. A perspective divide is applied so the edge rotating away from the viewer
/// shrinks, which is what gives a rotated card its sense of depth rather than looking merely
/// squashed.
/// </remarks>
public sealed class Rotation3DPresenter : Decorator
{
    /// <summary>
    /// Defines the <see cref="RotationX"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RotationXProperty =
        AvaloniaProperty.Register<Rotation3DPresenter, double>(nameof(RotationX));

    /// <summary>
    /// Defines the <see cref="RotationY"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RotationYProperty =
        AvaloniaProperty.Register<Rotation3DPresenter, double>(nameof(RotationY));

    /// <summary>
    /// Defines the <see cref="RotationZ"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RotationZProperty =
        AvaloniaProperty.Register<Rotation3DPresenter, double>(nameof(RotationZ));

    /// <summary>
    /// Defines the <see cref="Perspective"/> property.
    /// </summary>
    public static readonly StyledProperty<double> PerspectiveProperty =
        AvaloniaProperty.Register<Rotation3DPresenter, double>(nameof(Perspective), Rotation3D.DefaultPerspective);

    private readonly MatrixTransform _transform = new(Matrix.Identity);

    static Rotation3DPresenter()
    {
        RotationXProperty.Changed.AddClassHandler<Rotation3DPresenter>((x, _) => x.UpdateTransform());
        RotationYProperty.Changed.AddClassHandler<Rotation3DPresenter>((x, _) => x.UpdateTransform());
        RotationZProperty.Changed.AddClassHandler<Rotation3DPresenter>((x, _) => x.UpdateTransform());
        PerspectiveProperty.Changed.AddClassHandler<Rotation3DPresenter>((x, _) => x.UpdateTransform());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rotation3DPresenter"/> class.
    /// </summary>
    public Rotation3DPresenter()
    {
        RenderTransform = _transform;
        RenderTransformOrigin = RelativePoint.Center;
    }

    /// <summary>
    /// Gets or sets the rotation around the X axis, in degrees.
    /// </summary>
    public double RotationX
    {
        get => GetValue(RotationXProperty);
        set => SetValue(RotationXProperty, value);
    }

    /// <summary>
    /// Gets or sets the rotation around the Y axis, in degrees.
    /// </summary>
    public double RotationY
    {
        get => GetValue(RotationYProperty);
        set => SetValue(RotationYProperty, value);
    }

    /// <summary>
    /// Gets or sets the rotation around the Z axis, in degrees.
    /// </summary>
    public double RotationZ
    {
        get => GetValue(RotationZProperty);
        set => SetValue(RotationZProperty, value);
    }

    /// <summary>
    /// Gets or sets the perspective factor; see <see cref="Rotation3D.DefaultPerspective"/>.
    /// </summary>
    public double Perspective
    {
        get => GetValue(PerspectiveProperty);
        set => SetValue(PerspectiveProperty, value);
    }

    private void UpdateTransform() =>
        _transform.Matrix = Rotation3D.Create(RotationX, RotationY, RotationZ, Perspective);
}
