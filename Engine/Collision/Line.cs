namespace Pure.Engine.Collision;

using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Numerics;

/// <summary>
/// Represents a line segment in 2D space defined by two points. Useful for
/// collision detection, debugging, raycasting and many other things.
/// </summary>
public struct Line
{
    /// <summary>
    /// Gets or sets the start point of the line.
    /// </summary>
    public (float x, float y) A { get; set; }
    /// <summary>
    /// Gets or sets the end point of the line.
    /// </summary>
    public (float x, float y) B { get; set; }
    /// <summary>
    /// Gets the length of the line.
    /// </summary>
    public float Length
    {
        get => Vector2.Distance(new(A.x, A.y), new(B.x, B.y));
    }
    /// <summary>
    /// Gets the angle of the line in degrees.
    /// </summary>
    public float Angle
    {
        get => ToAngle(Direction);
    }
    /// <summary>
    /// Gets the direction of the line as a normalized vector.
    /// </summary>
    public (float x, float y) Direction
    {
        get => Normalize((B.x - A.x, B.y - A.y));
    }
    /// <summary>
    /// Gets or sets the color of the line.
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// Initializes a new instance of the line with the specified start and end points.
    /// </summary>
    /// <param name="a">The start point of the line.</param>
    /// <param name="b">The end point of the line.</param>
    /// <param name="color">The color of the line.</param>
    public Line((float x, float y) a, (float x, float y) b, uint color = uint.MaxValue)
    {
        A = a;
        B = b;
        Color = color;
    }
    public Line(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        A = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
        B = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
        Color = BitConverter.ToUInt32(Get<uint>());

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public Line(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(A.x));
        result.AddRange(BitConverter.GetBytes(A.y));
        result.AddRange(BitConverter.GetBytes(B.x));
        result.AddRange(BitConverter.GetBytes(B.y));
        result.AddRange(BitConverter.GetBytes(Color));
        return Compress(result.ToArray());
    }
    /// <returns>
    ///     A bundle tuple containing the two points and the color of the line.
    /// </returns>
    public (float ax, float ay, float bx, float by, uint color) ToBundle()
    {
        return this;
    }
    /// <returns>
    ///     A string representation of this line in the format of its bundle tuple.".
    /// </returns>
    public override string ToString()
    {
        return $"A{A} B{B} Color{Color}";
    }

    /// <summary>
    /// Checks if this line is crossing any rectangles in the given map.
    /// </summary>
    /// <param name="solidMap">The map to check for crossing.</param>
    /// <returns>
    /// True if this line is crossing with the specified map, otherwise false.
    /// </returns>
    public bool IsCrossing(SolidMap solidMap)
    {
        return IsCrossing(solidMap.GetNeighborRects(this).ToArray());
    }
    /// <summary>
    ///     Checks if this line is crossing with any of the rectangles in the
    ///     specified hitbox.
    /// </summary>
    /// <param name="solidPack">The hitbox to check for crossing.</param>
    /// <returns>
    ///     True if this line is crossing with the specified hitbox,
    ///     otherwise false.
    /// </returns>
    public bool IsCrossing(SolidPack solidPack)
    {
        for (var i = 0; i < solidPack.Count; i++)
            if (IsCrossing(solidPack[i]))
                return true;

        return false;
    }
    /// <param name="solid">
    ///     The rectangle to check for crossing.
    /// </param>
    /// <returns>
    ///     True if this line is crossing with the specified
    ///     rectangle, otherwise false.
    /// </returns>
    public bool IsCrossing(Solid solid)
    {
        var (x, y) = solid.Position;
        var (w, h) = solid.Size;
        var t = new Line((x, y), (x + w, y));
        var r = new Line((x + w, y), (x + w, y + h));
        var b = new Line((x + w, y + h), (x, y + h));
        var l = new Line((x, y + h), (x, y));
        return IsCrossing(t) || IsCrossing(r) || IsCrossing(b) || IsCrossing(l);
    }
    /// <summary>
    /// Determines if this line is crossing another line.
    /// </summary>
    /// <param name="line">The other line to check for crossing.</param>
    /// <returns>True if the lines cross, false otherwise.</returns>
    public bool IsCrossing(Line line)
    {
        var (x, y, _) = CrossPoint(line);
        return float.IsNaN(x) == false && float.IsNaN(y) == false;
    }
    /// <param name="point">
    ///     The point to check for crossing.
    /// </param>
    /// <returns>
    ///     True if this line is crossing with the specified
    ///     rectangle, otherwise false.
    /// </returns>
    public bool IsCrossing((float x, float y) point)
    {
        var length = Length;
        var a = Vector2.Distance(new(A.x, A.y), new(point.x, point.y));
        var b = Vector2.Distance(new(B.x, B.y), new(point.x, point.y));
        return IsBetween(a + b, length - 0.01f, length + 0.01f);
    }
    /// <param name="point">
    ///     The point to check for crossing.
    /// </param>
    /// <returns>
    ///     True if this line is crossing with the specified
    ///     rectangle, otherwise false.
    /// </returns>
    public bool IsCrossing((float x, float y, uint color) point)
    {
        return IsCrossing((point.x, point.y));
    }

    public float IsLeftOf((float x, float y) point)
    {
        var (px, py) = point;
        return (B.x - A.x) * (py - A.y) - (B.y - A.y) * (px - A.x);
    }
    public float IsLeftOf((float x, float y, uint color) point)
    {
        return IsLeftOf((point.x, point.y));
    }

    /// <summary>
    /// Calculates all points of intersection between this line and the rectangles of the
    /// specified map.
    /// </summary>
    /// <param name="solidMap">The map to calculate the intersection points with.</param>
    /// <returns>
    /// An array of all points of intersection between this line and the specified map.
    /// </returns>
    public (float x, float y, uint color)[] CrossPoints(SolidMap solidMap)
    {
        var neighbours = solidMap.GetNeighborRects(this);
        var result = new List<(float, float, uint)>();

        foreach (var r in neighbours)
        {
            var crossPoints = CrossPoints(r);
            foreach (var p in crossPoints)
            {
                var (x, y, _) = p;
                if (float.IsNaN(x) == false &&
                    float.IsNaN(y) == false &&
                    result.Contains((x, y, uint.MaxValue)) == false)
                    result.Add((x, y, uint.MaxValue));
            }
        }

        return result.ToArray();
    }
    /// <summary>
    /// Calculates all points of intersection between this line and the rectangles of the
    /// specified hitbox.
    /// </summary>
    /// <param name="solidPack">The hitbox to calculate the intersection points with.</param>
    /// <returns> An array of all points of intersection between this line and the specified hitbox.
    /// </returns>
    public (float x, float y, uint color)[] CrossPoints(SolidPack solidPack)
    {
        var result = new List<(float, float, uint)>();
        for (var i = 0; i < solidPack.Count; i++)
            result.AddRange(CrossPoints(solidPack[i]));

        return result.ToArray();
    }
    /// <param name="solid">
    ///     The rectangle to calculate the intersection points with.
    /// </param>
    /// <returns>
    ///     An array of all points of intersection between this line and the specified
    ///     rectangle.
    /// </returns>
    public (float x, float y, uint color)[] CrossPoints(Solid solid)
    {
        var (x, y) = solid.Position;
        var (w, h) = solid.Size;
        var tl = (x, y);
        var tr = (x + w, y);
        var br = (x + w, y + h);
        var bl = (x, y + h);

        var up = new Line(tl, tr);
        var right = new Line(tr, br);
        var down = new Line(br, bl);
        var left = new Line(bl, tl);
        var result = new List<(float x, float y, uint)>();
        var points = new List<(float x, float y, uint)>
            { CrossPoint(up), CrossPoint(right), CrossPoint(down), CrossPoint(left) };

        for (var i = 0; i < points.Count; i++)
            if (float.IsNaN(points[i].x) == false && float.IsNaN(points[i].y) == false)
                result.Add(points[i]);

        return result.ToArray();
    }
    /// <param name="line">
    ///     The line to calculate the intersection with.
    /// </param>
    /// <returns>
    ///     The point of intersection between this line and the specified
    ///     line, or (<see cref="float.NaN" />, <see cref="float.NaN" />) if
    ///     the two lines do not intersect.
    /// </returns>
    public (float x, float y, uint color) CrossPoint(Line line)
    {
        var p = CrossPoint(A, B, line.A, line.B);
        return IsCrossing(p) && line.IsCrossing(p) ? p : (float.NaN, float.NaN, uint.MaxValue);
    }
    /// <param name="point">
    ///     The point to find the closest point on the line to.
    /// </param>
    /// <returns>
    ///     The point on the line that is closest to the given
    ///     line.
    /// </returns>
    public (float x, float y, uint color) ClosestPoint((float x, float y) point)
    {
        var ap = (point.x - A.x, point.y - A.y);
        var (abx, aby) = (B.x - A.x, B.y - A.y);

        var magnitude = LengthSquared((abx, aby));
        var product = Dot(ap, (abx, aby));
        var distance = product / magnitude;

        return distance < 0 ? (A.x, A.y, uint.MaxValue) :
            distance > 1 ? (B.x, A.y, uint.MaxValue) :
            (A.x + abx * distance, A.y + aby * distance, uint.MaxValue);
    }

    /// <summary>
    /// Implicitly converts a tuple of two points and a color into a line.
    /// </summary>
    /// <param name="bundle">The tuple to convert.</param>
    /// <returns>A new line instance.</returns>
    public static implicit operator Line((float ax, float ay, float bx, float by, uint color) bundle)
    {
        return new((bundle.ax, bundle.ay), (bundle.bx, bundle.by), bundle.color);
    }
    /// <summary>
    /// Implicitly converts a line into a tuple bundle of two points and a color.
    /// </summary>
    /// <param name="line">The line to convert.</param>
    /// <returns>A tuple bundle containing the two points and the color of the line.</returns>
    public static implicit operator (float ax, float ay, float bx, float by, uint color)(Line line)
    {
        return (line.A.x, line.A.y, line.B.x, line.B.y, line.Color);
    }
    public static implicit operator byte[](Line line)
    {
        return line.ToBytes();
    }
    public static implicit operator Line(byte[] bytes)
    {
        return new(bytes);
    }

    #region Backend
    private static (float, float, uint) CrossPoint(
        (float x, float y) a,
        (float x, float y) b,
        (float x, float y) c,
        (float x, float y) d)
    {
        var a1 = b.y - a.y;
        var b1 = a.x - b.x;
        var c1 = a1 * a.x + b1 * a.y;
        var a2 = d.y - c.y;
        var b2 = c.x - d.x;
        var c2 = a2 * c.x + b2 * c.y;
        var determinant = a1 * b2 - a2 * b1;

        if (determinant == 0)
            return (float.NaN, float.NaN, uint.MaxValue);

        var x = (b2 * c1 - b1 * c2) / determinant;
        var y = (a1 * c2 - a2 * c1) / determinant;
        return (x, y, uint.MaxValue);
    }
    private static float ToAngle((float x, float y) direction)
    {
        //Vector2 to Radians: atan2(Vector2.y, Vector2.x)
        //Radians to Angle: radians * (180 / Math.PI)

        var rad = MathF.Atan2(direction.y, direction.x);
        var result = rad * (180f / MathF.PI);
        return AngleWrap(result);
    }
    private static (float, float) Normalize((float, float) direction)
    {
        var (x, y) = direction;
        var distance = MathF.Sqrt(x * x + y * y);
        return (x / distance, y / distance);
    }
    private static bool IsBetween(
        float number,
        float rangeA,
        float rangeB,
        bool inclusiveA = false,
        bool inclusiveB = false)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        var l = inclusiveA ? rangeA <= number : rangeA < number;
        var u = inclusiveB ? rangeB >= number : rangeB > number;
        return l && u;
    }
    private static float LengthSquared((float, float) vector)
    {
        var (x, y) = vector;
        var sum = x * x + y * y;

        return MathF.Pow(MathF.Sqrt(sum), 2);
    }
    private static float Dot((float, float) a, (float, float) b)
    {
        var (ax, ay) = a;
        var (bx, by) = b;
        return ax * bx + ay * by;
    }
    private static float AngleWrap(float angle)
    {
        return (angle % 360 + 360) % 360;
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);
        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using var stream = new DeflateStream(input, CompressionMode.Decompress);
        stream.CopyTo(output);
        return output.ToArray();
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
    #endregion
}