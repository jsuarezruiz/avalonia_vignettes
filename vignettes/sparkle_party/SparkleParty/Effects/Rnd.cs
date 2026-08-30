namespace SparkleParty.Effects;

/// <summary>
/// The random helpers the effects are written in terms of. Port of <c>rnd.dart</c>.
/// </summary>
public static class Rnd
{
    private static readonly Random Source = Random.Shared;

    /// <summary>
    /// Gets a number from 0 to 1.
    /// </summary>
    public static double Ratio => Source.NextDouble();

    /// <summary>
    /// Gets a whole number from <paramref name="min"/> up to <paramref name="max"/>.
    /// </summary>
    public static int Int(int min, int max) => min + Source.Next(max - min);

    /// <summary>
    /// Gets a number between two bounds.
    /// </summary>
    public static double Double(double min, double max) => min + (Source.NextDouble() * (max - min));

    /// <summary>
    /// Gets true with the given likelihood.
    /// </summary>
    public static bool Bool(double chance = 0.5d) => Source.NextDouble() < chance;

    /// <summary>
    /// Gets 1 with the given likelihood, otherwise 0.
    /// </summary>
    public static int Bit(double chance = 0.5d) => Source.NextDouble() < chance ? 1 : 0;

    /// <summary>
    /// Gets an angle in degrees.
    /// </summary>
    public static double Degrees() => Double(0d, 360d);

    /// <summary>
    /// Gets an angle in radians.
    /// </summary>
    public static double Radians() => Double(0d, Math.PI * 2d);
}
