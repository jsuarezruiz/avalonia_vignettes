using Avalonia;
using Avalonia.Media;

namespace AvaloniaVignettes.Shared.Media;

/// <summary>
/// Builds the non-affine matrix behind the <c>Rotation3d</c> widget shared by the Flutter
/// vignettes:
/// <code>
/// Matrix4.identity()
///   ..setEntry(3, 2, 0.001)
///   ..rotateX(x)..rotateY(y)..rotateZ(z)
/// </code>
/// </summary>
/// <remarks>
/// The result rotates around the origin, so set <see cref="Visual.RenderTransformOrigin"/> to
/// <c>50%,50%</c> to match Flutter's <c>FractionalOffset.center</c> alignment.
/// <see cref="Avalonia.Media.Transform"/> cannot be derived from outside Avalonia, so rotations are
/// applied by feeding these matrices to a <see cref="MatrixTransform"/>.
/// </remarks>
public static class Rotation3D
{
    /// <summary>The perspective factor Flutter's vignettes use: <c>1 / 1000</c>.</summary>
    public const double DefaultPerspective = 0.001d;

    private const double DegreesToRadians = Math.PI / 180d;

    /// <summary>
    /// Creates the projected 2D matrix for a rotation around the Y axis, the only axis the travel
    /// cards use.
    /// </summary>
    /// <param name="degrees">The rotation around the Y axis, in degrees.</param>
    /// <param name="perspective">The perspective factor; see <see cref="DefaultPerspective"/>.</param>
    public static Matrix AroundY(double degrees, double perspective = DefaultPerspective)
    {
        var (sin, cos) = Math.SinCos(degrees * DegreesToRadians);

        // For an input point (x, y, 0): x' = cos * x, y' = y, w' = 1 - perspective * sin * x.
        return new Matrix(
            cos, 0d, -perspective * sin,
            0d, 1d, 0d,
            0d, 0d, 1d);
    }

    /// <summary>
    /// Creates the projected 2D matrix for a rotation applied in Flutter's order: X, then Y, then Z.
    /// </summary>
    /// <param name="rotationX">The rotation around the X axis, in degrees.</param>
    /// <param name="rotationY">The rotation around the Y axis, in degrees.</param>
    /// <param name="rotationZ">The rotation around the Z axis, in degrees.</param>
    /// <param name="perspective">The perspective factor; see <see cref="DefaultPerspective"/>.</param>
    public static Matrix Create(
        double rotationX,
        double rotationY,
        double rotationZ,
        double perspective = DefaultPerspective)
    {
        var (sx, cx) = Math.SinCos(rotationX * DegreesToRadians);
        var (sy, cy) = Math.SinCos(rotationY * DegreesToRadians);
        var (sz, cz) = Math.SinCos(rotationZ * DegreesToRadians);

        // R = Rx * Ry * Rz in Flutter's column-vector convention (p' = R * p). Only the entries
        // that survive the z = 0 projection matter.
        var r00 = cy * cz;
        var r01 = -cy * sz;
        var r10 = (cx * sz) + (sx * sy * cz);
        var r11 = (cx * cz) - (sx * sy * sz);
        var r20 = (sx * sz) - (cx * sy * cz);
        var r21 = (sx * cz) + (cx * sy * sz);

        // Row 3, column 2 of the 4x4 turns z into the homogeneous w: w' = 1 + perspective * z'.
        // Avalonia uses the row-vector convention ([x y 1] * M), so those columns become rows and
        // M13/M23 carry the perspective factors.
        return new Matrix(
            r00, r10, perspective * r20,
            r01, r11, perspective * r21,
            0d, 0d, 1d);
    }
}
