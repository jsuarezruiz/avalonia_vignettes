using Avalonia;

namespace TicketFold.Effects;

/// <summary>
/// Builds the matrix that folds one panel of the ticket about its top edge. Port of the
/// <c>Matrix4</c> assembled in <c>folding_ticket.dart</c>:
/// <code>
/// Matrix4.identity()
///   ..setEntry(3, 2, 0.001)
///   ..setEntry(1, 2, 0.2)
///   ..rotateX(pi * (ratio - 1.0));
/// </code>
/// </summary>
/// <remarks>
/// Two entries are set before the rotation, and between them they are what make the fold read as
/// paper rather than as a flat scale. <c>(3, 2)</c> is the perspective term, so the far end of a
/// tilted panel is drawn smaller. <c>(1, 2)</c> leans depth into the vertical axis, which is what
/// tips the panel towards the viewer as it swings down instead of letting it rotate edge-on and
/// vanish.
/// </remarks>
public static class FoldTransform
{
    /// <summary>
    /// The perspective factor, <c>1 / 1000</c>.
    /// </summary>
    public const double Perspective = 0.001d;

    /// <summary>
    /// How much depth is leaned into the vertical axis.
    /// </summary>
    public const double DepthShear = 0.2d;

    /// <summary>
    /// Creates the fold matrix for a panel, in a space whose origin is the hinge the panel turns
    /// about.
    /// </summary>
    /// <param name="ratio">
    /// How far the panel has unfolded: 0 is folded right back over its hinge, 1 is flat.
    /// </param>
    public static Matrix Create(double ratio)
    {
        var (sin, cos) = Math.SinCos(Math.PI * (ratio - 1d));

        // Flutter multiplies its 4x4 onto a column vector and divides through by w at the end, so
        // for a point (x, y, 0) the three surviving terms are
        //   x' = x
        //   y' = y * (cos + shear * sin)
        //   w' = 1 + perspective * y * sin.
        // Avalonia's Matrix is the row-vector transpose of that ([x y 1] * M) with M13 and M23
        // carrying the perspective, which is what makes a non-affine transform expressible at all.
        return new Matrix(
            1d, 0d, 0d,
            0d, cos + (DepthShear * sin), Perspective * sin,
            0d, 0d, 1d);
    }

    /// <summary>
    /// Creates the fold matrix for a panel hinged at <paramref name="hinge"/>, expressed in the
    /// coordinate space that point belongs to.
    /// </summary>
    /// <param name="ratio">How far the panel has unfolded; see <see cref="Create(double)"/>.</param>
    /// <param name="hinge">The point the panel turns about, Flutter's <c>Alignment.topCenter</c>.</param>
    public static Matrix Create(double ratio, Point hinge) =>
        Matrix.CreateTranslation(-hinge.X, -hinge.Y)
        * Create(ratio)
        * Matrix.CreateTranslation(hinge.X, hinge.Y);
}
