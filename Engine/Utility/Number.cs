using System.Globalization;
using Range = (float a, float b);
using SizeI = (int width, int height);
using PointF = (float x, float y);
using static Pure.Engine.Utility.Noise;

namespace Pure.Engine.Utility;

/// <summary>
/// The type of number animations used by <see cref="Number.AnimateEase"/>.
/// Also known as 'easing functions'.
/// </summary>
public enum Ease
{
    /// <summary>
    /// Represents a linear/lerp animation, characterized by a constant rate of change.
    /// </summary>
    Line,
    /// <summary>
    /// Corresponds to a sine easing function, creating a gentle bending effect.
    /// </summary>
    Sine,
    /// <summary>
    /// Indicates a cubic easing function, resulting in a moderate bending motion.
    /// </summary>
    Cubic,
    /// <summary>
    /// Represents a quintic easing function, producing a strong bending effect.
    /// </summary>
    Quint,
    /// <summary>
    /// Refers to a circular easing function, often denoted as Circ, creating a circular motion.
    /// </summary>
    Circle,
    /// <summary>
    /// Describes an elastic easing function, simulating an elastic or rubber-band-like motion.
    /// </summary>
    Elastic,
    /// <summary>
    /// Represents a back easing function, generating a swinging or backward motion.
    /// </summary>
    Swing,
    /// <summary>
    /// Represents a bounce easing function, creating a bouncing effect.
    /// </summary>
    Bounce
}

/// <summary>
/// The type of number animation direction used by <see cref="Number.AnimateEase"/>.
/// </summary>
public enum Curve
{
    /// <summary>
    /// Eases in for a gradual start.
    /// </summary>
    In,
    /// <summary>
    /// Eases out for a gradual slowdown or stop.
    /// </summary>
    Out,
    /// <summary>
    /// Eases in first and then eases out, combining characteristics of both
    /// <see cref="In"/> and <see cref="Out"/>.
    /// </summary>
    InOut
}

public enum Noise { OpenSimplex2, OpenSimplex2S, Cellular, Perlin, ValueCubic, Value }

public static class Number
{
    public static float AnimateEase(this float unit, Ease ease, Curve curve)
    {
        var t = unit;
        switch (ease)
        {
            case Ease.Line:
            {
                return curve == Curve.In ? 1f - unit :
                    curve == Curve.Out ? unit :
                    unit < 0.5f ? unit.Map((0, 0.5f), (1f, 0)) : unit.Map((0.5f, 1f), (0, 1f));
            }
            case Ease.Sine:
            {
                return curve == Curve.In ? 1 - MathF.Cos(t * MathF.PI / 2) :
                    curve == Curve.Out ? 1 - MathF.Sin(t * MathF.PI / 2) :
                    -(MathF.Cos(MathF.PI * t) - 1) / 2;
            }
            case Ease.Cubic:
            {
                return curve == Curve.In ? t * t * t :
                    curve == Curve.Out ? 1 - MathF.Pow(1 - t, 3) :
                    t < 0.5 ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;
            }
            case Ease.Quint:
            {
                return curve == Curve.In ? t * t * t * t :
                    curve == Curve.Out ? 1 - MathF.Pow(1 - t, 5) :
                    t < 0.5 ? 16 * t * t * t * t * t : 1 - MathF.Pow(-2 * t + 2, 5) / 2;
            }
            case Ease.Circle:
            {
                return curve == Curve.In ? 1 - MathF.Sqrt(1 - MathF.Pow(t, 2)) :
                    curve == Curve.Out ? MathF.Sqrt(1 - MathF.Pow(t - 1, 2)) :
                    t < 0.5 ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * t, 2))) / 2 :
                    (MathF.Sqrt(1 - MathF.Pow(-2 * t + 2, 2)) + 1) / 2;
            }
            case Ease.Elastic:
            {
                return curve == Curve.In ? t == 0 ? 0 :
                    Math.Abs((int)(t - 1)) < 0.001f ? 1 :
                    -MathF.Pow(2, 10 * t - 10) *
                    MathF.Sin((t * 10 - 10.75f) *
                              (2 * MathF.PI / 3))
                    : curve == Curve.Out ? t == 0 ? 0 :
                    Math.Abs((int)(t - 1)) < 0.001f ? 1 :
                    MathF.Pow(2, -10 * t) *
                    MathF.Sin((t * 10 - 0.75f) * (2 * MathF.PI) / 3) +
                    1
                    : t == 0 ? 0
                    : Math.Abs((int)(t - 1)) < 0.001f ? 1
                    : t < 0.5f ? -(MathF.Pow(2, 20 * t - 10) *
                                   MathF.Sin((20f * t - 11.125f) *
                                             (2 * MathF.PI) /
                                             4.5f)) /
                                 2
                    : MathF.Pow(2, -20 * t + 10) *
                      MathF.Sin((20 * t - 11.125f) * (2 * MathF.PI) / 4.5f) /
                      2 +
                      1;
            }
            case Ease.Swing:
            {
                return curve == Curve.In ? 2.70158f * t * t * t - 1.70158f * t * t :
                    curve == Curve.Out ? 1 +
                                         2.70158f * MathF.Pow(t - 1, 3) +
                                         1.70158f * MathF.Pow(t - 1, 2) :
                    t < 0.5 ? MathF.Pow(2 * t, 2) * ((2.59491f + 1) * 2 * t - 2.59491f) / 2 :
                    (MathF.Pow(2 * t - 2, 2) * ((2.59491f + 1) * (t * 2 - 2) + 2.59491f) + 2) / 2;
            }
            case Ease.Bounce:
            {
                return curve == Curve.In ? 1 - EaseOutBounce(1 - t) :
                    curve == Curve.Out ? EaseOutBounce(t) :
                    t < 0.5f ? (1 - EaseOutBounce(1 - 2 * t)) / 2 : (1 + EaseOutBounce(2 * t - 1)) / 2;

                static float EaseOutBounce(float x)
                {
                    return x < 1 / 2.75f ? 7.5625f * x * x :
                        x < 2 / 2.75f ? 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f :
                        x < 2.5f / 2.75f ? 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f :
                        7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
                }
            }
            default: return default;
        }
    }
    public static PointF AnimateBezier(this float unit, params PointF[] curvePoints)
    {
        if (curvePoints.Length == 0)
            return (float.NaN, float.NaN);
        if (curvePoints.Length == 1)
            return curvePoints[0];

        var numPoints = curvePoints.Length;
        var xPoints = new float[numPoints];
        var yPoints = new float[numPoints];
        for (var i = 0; i < numPoints; i++)
        {
            xPoints[i] = curvePoints[i].x;
            yPoints[i] = curvePoints[i].y;
        }

        for (var k = 1; k < numPoints; k++)
            for (var i = 0; i < numPoints - k; i++)
            {
                xPoints[i] = (1 - unit) * xPoints[i] + unit * xPoints[i + 1];
                yPoints[i] = (1 - unit) * yPoints[i] + unit * yPoints[i + 1];
            }

        return (xPoints[0], yPoints[0]);
    }
    public static PointF AnimateSpline(this float unit, params PointF[] curvePoints)
    {
        if (curvePoints.Length < 4)
            return (float.NaN, float.NaN);

        var numSegments = curvePoints.Length - 3;
        var segmentFraction = 1f / numSegments;
        var segmentIndex = Math.Min((int)(unit / segmentFraction), numSegments - 1);
        var p0 = curvePoints[segmentIndex];
        var p1 = curvePoints[segmentIndex + 1];
        var p2 = curvePoints[segmentIndex + 2];
        var p3 = curvePoints[segmentIndex + 3];
        var u = (unit - segmentIndex * segmentFraction) / segmentFraction;
        var u2 = u * u;
        var u3 = u2 * u;
        var c0 = -0.5f * u3 + u2 - 0.5f * u;
        var c1 = 1.5f * u3 - 2.5f * u2 + 1f;
        var c2 = -1.5f * u3 + 2f * u2 + 0.5f * u;
        var c3 = 0.5f * u3 - 0.5f * u2;
        var t0 = c0 * p0.x + c1 * p1.x + c2 * p2.x + c3 * p3.x;
        var t1 = c0 * p0.y + c1 * p1.y + c2 * p2.y + c3 * p3.y;

        return (t0, t1);
    }

    /// <summary>
    /// Limits a float number to a specified range.
    /// </summary>
    /// <param name="number">The number to limit.</param>
    /// <param name="range">The range value.</param>
    /// <returns>The limited float number.</returns>
    public static float Limit(this float number, Range range)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        return Math.Clamp(number, range.a, range.b);
    }
    /// <summary>
    /// Limits an int number to a specified range.
    /// </summary>
    /// <param name="number">The number to limit.</param>
    /// <param name="range">The range value.</param>
    /// <returns>The limited int number.</returns>
    public static int Limit(this int number, (int a, int b) range)
    {
        return (int)Limit((float)number, range);
    }
    public static float Wrap(this float number, Range range)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        var d = range.b - range.a;
        return d < 0.001f ? range.a : ((number - range.a) % d + d) % d + range.a;
    }
    public static int Wrap(this int number, (int a, int b) range)
    {
        return (int)Wrap((float)number, range);
    }
    /// <summary>
    /// Wraps a value within a range of 0 to target number inclusive. Useful for
    /// wrapping an angle in the range [0-359].
    /// </summary>
    /// <param name="number">The value to wrap.</param>
    /// <param name="targetNumber">The upper range of the wrap.</param>
    /// <returns>The wrapped value within the specified range.</returns>
    public static float Wrap(this float number, float targetNumber)
    {
        return (number % targetNumber + targetNumber) % targetNumber;
    }
    /// <summary>
    /// Wraps a value within a range of 0 to target number inclusive. Useful for
    /// wrapping an angle in the range [0-359].
    /// </summary>
    /// <param name="number">The value to wrap.</param>
    /// <param name="targetNumber">The upper range of the wrap.</param>
    /// <returns>The wrapped value within the specified range.</returns>
    public static int Wrap(this int number, int targetNumber)
    {
        return (number % targetNumber + targetNumber) % targetNumber;
    }
    /// <summary>
    /// Rounds the given number to the nearest multiple of the specified interval.
    /// </summary>
    /// <param name="number">The number to be rounded.</param>
    /// <param name="interval">The interval to which the number should be rounded.</param>
    /// <returns>The nearest multiple of the specified interval.</returns>
    public static float Snap(this float number, float interval)
    {
        if (float.IsNaN(interval) || float.IsInfinity(number) || Math.Abs(interval) < 0.001f)
            return number;
        var remainder = number % interval;
        var halfway = interval / 2f;
        return remainder < halfway ?
            number - remainder :
            number + (interval - remainder);
    }
    /// <summary>
    /// Maps a float number from one range of values to another range of values.
    /// </summary>
    /// <param name="number">The number to map.</param>
    /// <param name="rangeIn">The first input range.</param>
    /// <param name="rangeOut">The second input range.</param>
    /// <returns>The mapped number.</returns>
    public static float Map(this float number, Range rangeIn, Range rangeOut)
    {
        if (Math.Abs(rangeIn.a - rangeIn.b) < 0.001f)
            return (rangeOut.a + rangeOut.b) / 2f;
        var target = rangeOut;
        var value = (number - rangeIn.a) / (rangeIn.b - rangeIn.a) * (target.b - target.a) + target.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? rangeOut.a : value;
    }
    /// <summary>
    /// Maps an int number from one range of values to another range of values.
    /// </summary>
    /// <param name="number">The number to map.</param>
    /// <param name="range">The first input range.</param>
    /// <param name="targetRange">The second input range.</param>
    /// <returns>The mapped number.</returns>
    public static int Map(this int number, (int a, int b) range, (int a, int b) targetRange)
    {
        return (int)Map((float)number, range, targetRange);
    }
    /// <summary>
    /// Moves the given float number by a certain speed 
    /// over a certain time.
    /// </summary>
    /// <param name="number">The float number to move.</param>
    /// <param name="speed">The speed at which to move the float number.</param>
    /// <param name="deltaTime">The time elapsed since the last move operation.</param>
    /// <returns>The new value of the moved float number.</returns>
    public static float Move(this float number, float speed, float deltaTime = 1)
    {
        return number + speed * deltaTime;
    }
    /// <summary>
    /// Moves a float number towards a targetNumber 
    /// by a given speed.
    /// </summary>
    /// <param name="number">The current number.</param>
    /// <param name="targetNumber">The target number to move towards.</param>
    /// <param name="speed">The speed of movement.</param>
    /// <param name="deltaTime">The time step for the movement.</param>
    /// <returns>The new number after the movement.</returns>
    public static float MoveTo(this float number, float targetNumber, float speed, float deltaTime = 1)
    {
        var goingPos = number < targetNumber;
        var result = Move(number, goingPos ? Sign(speed, false) : Sign(speed, true), deltaTime);
        if (goingPos && result > targetNumber)
            return targetNumber;
        if (goingPos == false && result < targetNumber)
            return targetNumber;
        return result;
    }

    /// <param name="amount">
    /// The number of values to distribute.</param>
    /// <param name="range">The range of values (inclusive). The order is maintained.</param>
    /// <returns>An array of evenly distributed numbers across the specified range.</returns>
    public static float[] Distribute(this int amount, Range range)
    {
        if (amount <= 0)
            return [];
        var result = new float[amount];
        var size = range.b - range.a;
        var spacing = size / (amount + 1);
        for (var i = 1; i <= amount; i++)
            result[i - 1] = range.a + i * spacing;
        return result;
    }
    /// <param name="number">
    /// The number whose sign to adjust.</param>
    /// <param name="sign">Indicates whether the sign of the number 
    /// should be negative.</param>
    /// <returns>The absolute value of the float number 
    /// with the specified sign.</returns>
    public static float Sign(this float number, bool sign)
    {
        return sign ? -MathF.Abs(number) : MathF.Abs(number);
    }
    /// <param name="number">
    /// The number whose sign to adjust.</param>
    /// <param name="sign">Indicates whether the sign of the number 
    /// should be negative.</param>
    /// <returns>The absolute value of the int number 
    /// with the specified sign.</returns>
    public static int Sign(this int number, bool sign)
    {
        return (int)Sign((float)number, sign);
    }
    /// <param name="number">
    /// The float number to check.</param>
    /// <returns>The number of decimal places of the given float number.</returns>
    public static int Precision(this float number)
    {
        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var split = number.ToString(CultureInfo.CurrentCulture).Split(cultDecPoint);
        return split.Length > 1 ? split[1].Length : 0;
    }
    public static string PadZeros(this float number, int amountOfZeros)
    {
        if (amountOfZeros == 0)
            return $"{number}";
        var format = amountOfZeros < 0 ? new('0', Math.Abs(amountOfZeros)) : "F" + amountOfZeros;
        var finalFormat = "{0:" + format + "}";
        return string.Format(finalFormat, number);
    }
    public static string PadZeros(this int number, int amountOfZeros)
    {
        var format = "{0:D" + Math.Abs(amountOfZeros) + "}";
        return string.Format(format, number);
    }

    public static bool IsBetween(this float number, Range range, (bool a, bool b) inclusive = default)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        var l = inclusive.a ? range.a <= number : range.a < number;
        var u = inclusive.b ? range.b >= number : range.b > number;
        return l && u;
    }
    public static bool IsBetween(this int number, (int a, int b) range, (bool a, bool b) inclusive = default)
    {
        return IsBetween((float)number, range, inclusive);
    }
    /// <summary>
    /// Checks whether the given float number is within the range defined by a 
    /// targetNumber and a range value.
    /// </summary>
    /// <param name="number">The float number to check.</param>
    /// <param name="targetNumber">The target number defining the center of the range.</param>
    /// <param name="range">The range value defining the size of the range.</param>
    /// <returns>True if the given float number is within the range defined by the target number and 
    /// the range value, false otherwise.</returns>
    public static bool IsWithin(this float number, float targetNumber, float range)
    {
        return IsBetween(number, (targetNumber - range, targetNumber + range), (true, true));
    }
    /// <summary>
    /// Checks whether the given int number is within the range defined by a 
    /// targetNumber and a range value.
    /// </summary>
    /// <param name="number">The int number to check.</param>
    /// <param name="targetNumber">The target number defining the center of the range.</param>
    /// <param name="range">The range value defining the size of the range.</param>
    /// <returns>True if the given int number is within the range defined by the target number and 
    /// the range value, false otherwise.</returns>
    public static bool IsWithin(this int number, int targetNumber, int range)
    {
        return IsBetween(number, (targetNumber - range, targetNumber + range), (true, true));
    }
    /// <summary>
    /// Determines if a random float value between 1 and 100 is less than or equal to the 
    /// given percentage value.
    /// </summary>
    /// <param name="percent">The percentage value to check (between 0 and 100).</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>True if the random float value is less than or equal to the percentage value, 
    /// false otherwise.</returns>
    public static bool HasChance(this float percent, float seed = float.NaN)
    {
        if (percent <= 0.0001f)
            return false;

        percent = percent.Limit((0, 100));
        // should not roll 0 so it doesn't return true with 0% (outside of roll)
        var n = Random((0f, 100f), seed);
        return n <= percent;
    }
    /// <summary>
    /// Determines if a random int value between 1 and 100 is less than or equal to the 
    /// given percentage value.
    /// </summary>
    /// <param name="percent">The percentage value to check (between 0 and 100).</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>True if the random int value is less than or equal to the percentage value, 
    /// false otherwise.</returns>
    public static bool HasChance(this int percent, float seed = float.NaN)
    {
        return HasChance((float)percent, seed);
    }

    public static float Random(this Range range, float seed = float.NaN)
    {
        var (a, b) = range;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (a == b)
            return a;

        if (a > b)
            (a, b) = (b, a);

        var r = b - a;

        long intSeed = float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : BitConverter.SingleToInt32Bits(seed);
        intSeed = (1103515245 * intSeed + 12345) % 2147483648;
        var normalized = (intSeed & 0x7FFFFFFF) / (float)2147483648;
        return a + normalized * r;
    }
    /// <summary>
    /// Returns a random int value between the given inclusive range of values.
    /// </summary>
    /// <param name="range">The two range values.</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>A random int value between the specified range of values.</returns>
    public static int Random(this (int a, int b) range, float seed = float.NaN)
    {
        return (int)Math.Round(Random(((float)range.a, range.b), seed));
    }

    public static int ToSeed(this int number, params int[] parameters)
    {
        var seed = 2654435769L;
        Seed(number);
        foreach (var p in parameters)
            seed = Seed(p);
        return (int)seed;

        long Seed(int a)
        {
            seed ^= a;
            seed = (seed ^ (seed >> 16)) * 2246822519L;
            seed = (seed ^ (seed >> 13)) * 3266489917L;
            seed ^= seed >> 16;
            return (int)seed;
        }
    }
    public static int ToIndex(this (int x, int y) indexes, SizeI size)
    {
        var result = indexes.x * size.width + indexes.y;
        return Math.Clamp(result, 0, size.width * size.height);
    }
    public static (int x, int y) ToIndexes(this int index, SizeI size)
    {
        index = Math.Clamp(index, 0, size.width * size.height);
        return (index % size.width, index / size.width);
    }
    public static float ToNoise(this PointF point, Noise noise = ValueCubic, float scale = 10f, int seed = 0)
    {
        var noiseValue = new FastNoiseLite(seed);
        noiseValue.SetNoiseType((FastNoiseLite.NoiseType)noise);
        noiseValue.SetFrequency(1f / scale);

        return noiseValue.GetNoise(point.x, point.y).Map((-1, 1), (0, 1));
    }
}