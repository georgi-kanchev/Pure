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
    ///     A string representation of this line in the format of its bundle tuple.
    /// </returns>
    public override string ToString()
    {
        return $"A{A} B{B} Angle({Angle}°)";
    }

    public bool IsOverlapping(LinePack linePack)
    {
        for (var i = 0; i < linePack.Count; i++)
            if (IsOverlapping(linePack[i]))
                return true;

        return false;
    }
    /// <summary>
    /// Checks if this line is crossing any rectangles in the given map.
    /// </summary>
    /// <param name="solidMap">The map to check for crossing.</param>
    /// <returns>
    /// True if this line is crossing with the specified map, otherwise false.
    /// </returns>
    public bool IsOverlapping(SolidMap solidMap)
    {
        return solidMap.IsOverlapping(this);
    }
    /// <summary>
    /// Checks if this line is crossing with any of the rectangles in the specified hitbox.
    /// </summary>
    /// <param name="solidPack">The hitbox to check for crossing.</param>
    /// <returns>
    /// True if this line is crossing with the specified hitbox, otherwise false.
    /// </returns>
    public bool IsOverlapping(SolidPack solidPack)
    {
        for (var i = 0; i < solidPack.Count; i++)
            if (IsOverlapping(solidPack[i]))
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
    public bool IsOverlapping(Solid solid)
    {
        var (x1, y1) = A;
        var (x2, y2) = B;
        var (x, y) = solid.Position;
        var (w, h) = solid.Size;
        var (tax, tay) = (x, y);
        var (tbx, tby) = (x + w, y);
        var (rax, ray) = (x + w, y);
        var (rbx, rby) = (x + w, y + h);
        var (bax, bay) = (x + w, y + h);
        var (bbx, bby) = (x, y + h);
        var (lax, lay) = (x, y + h);
        var (lbx, lby) = (x, y);
        return LinesAreCrossing(x1, y1, x2, y2, tax, tay, tbx, tby) ||
               LinesAreCrossing(x1, y1, x2, y2, rax, ray, rbx, rby) ||
               LinesAreCrossing(x1, y1, x2, y2, bax, bay, bbx, bby) ||
               LinesAreCrossing(x1, y1, x2, y2, lax, lay, lbx, lby);
    }
    /// <summary>
    /// Determines if this line is crossing another line.
    /// </summary>
    /// <param name="line">The other line to check for crossing.</param>
    /// <returns>True if the lines cross, false otherwise.</returns>
    public bool IsOverlapping(Line line)
    {
        var (ax1, ay1) = A;
        var (bx1, by1) = B;
        var (ax2, ay2) = line.A;
        var (bx2, by2) = line.B;

        if ((A == line.B && B == line.A) ||
            (A == line.A && B == line.B))
            return true;

        return LinesAreCrossing(ax1, ay1, bx1, by1, ax2, ay2, bx2, by2);
    }
    /// <param name="point">
    ///     The point to check for crossing.
    /// </param>
    /// <returns>
    ///     True if this line is crossing with the specified
    ///     rectangle, otherwise false.
    /// </returns>
    public bool IsOverlapping((float x, float y) point)
    {
        var (ax, ay) = A;
        var (bx, by) = B;
        var (apx, apy) = (point.x - ax, point.y - ay);
        var (abx, aby) = (bx - ax, by - ay);
        var dotProduct = apx * abx + apy * aby;
        var dx = bx - ax;
        var dy = by - ay;
        var lengthSquared = dx * dx + dy * dy;

        if (dotProduct < 0 || dotProduct > lengthSquared)
            return false;

        var distanceSquared = apx * apx + apy * apy - dotProduct * dotProduct / lengthSquared;
        return distanceSquared <= 0.01f * 0.01f;
    }
    /// <param name="point">
    ///     The point to check for crossing.
    /// </param>
    /// <returns>
    ///     True if this line is crossing with the specified
    ///     rectangle, otherwise false.
    /// </returns>
    public bool IsOverlapping((float x, float y, uint color) point)
    {
        return IsOverlapping((point.x, point.y));
    }

    public (float x, float y, uint color)[] CrossPoints(LinePack linePack)
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < linePack.Count; i++)
        {
            var crossPoint = CrossPoint(linePack[i]);
            if (float.IsNaN(crossPoint.x) == false && float.IsNaN(crossPoint.y) == false)
                result.Add(crossPoint);
        }

        return result.ToArray();
    }
    public (float x, float y, uint color)[] CrossPoints(SolidMap solidMap)
    {
        var neighbours = solidMap.GetNeighborRects(this);
        var result = new List<(float x, float y, uint color)>();

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
        var (ax1, ay1) = A;
        var (bx1, by1) = B;
        var (ax2, ay2) = line.A;
        var (bx2, by2) = line.B;
        return LinesCrossPoint(line.Color, ax1, ay1, bx1, by1, ax2, ay2, bx2, by2);
    }

    public (float x, float y, uint color) ClosestPoint((float x, float y, uint color) point)
    {
        var result = ClosestPoint((point.x, point.y));
        result.color = point.color;
        return result;
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
        var (ax, ay) = A;
        var (bx, by) = B;
        var (apx, apy) = (point.x - ax, point.y - ay);
        var (abx, aby) = (bx - ax, by - ay);
        var magnitude = abx * abx + aby * aby;
        var dot = apx * abx + apy * aby;
        var distance = dot / magnitude;

        return distance < 0 ? (ax, ay, uint.MaxValue) :
            distance > 1 ? (bx, ay, uint.MaxValue) :
            (ax + abx * distance, ay + aby * distance, uint.MaxValue);
    }

    public bool IsLeftOf((float x, float y) point)
    {
        var (px, py) = point;
        return (B.x - A.x) * (py - A.y) - (B.y - A.y) * (px - A.x) < 0;
    }
    public bool IsLeftOf((float x, float y, uint color) point)
    {
        return IsLeftOf((point.x, point.y));
    }

    public Line NormalizeToPoint((float x, float y, uint color) point)
    {
        return NormalizeToPoint((point.x, point.y));
    }
    public Line NormalizeToPoint((float x, float y) point)
    {
        return IsLeftOf(point) ? this : new(B, A, Color);
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
    private static float AngleWrap(float angle)
    {
        return (angle % 360 + 360) % 360;
    }

    // these two ugly methods are made with speed in mind, avoiding line constructors
    private static bool LinesAreCrossing(float ax1, float ay1, float bx1, float by1, float ax2, float ay2, float bx2, float by2)
    {
        var (x, y, _) = LinesCrossPoint(0, ax1, ay1, bx1, by1, ax2, ay2, bx2, by2);
        return float.IsNaN(x) == false && float.IsNaN(y) == false;
    }
    private static (float x, float y, uint color) LinesCrossPoint(uint color, float ax1, float ay1, float bx1, float by1, float ax2, float ay2, float bx2, float by2)
    {
        var dx1 = bx1 - ax1;
        var dy1 = by1 - ay1;
        var dx2 = bx2 - ax2;
        var dy2 = by2 - ay2;
        var det = dx1 * dy2 - dy1 * dx2;

        if (det == 0)
            return (float.NaN, float.NaN, uint.MaxValue);

        var s = ((ay1 - ay2) * dx2 - (ax1 - ax2) * dy2) / det;
        var t = ((ay1 - ay2) * dx1 - (ax1 - ax2) * dy1) / det;

        if (s is < 0 or > 1 || t is < 0 or > 1)
            return (float.NaN, float.NaN, uint.MaxValue);

        var intersectionX = ax1 + s * dx1;
        var intersectionY = ay1 + s * dy1;
        return (intersectionX, intersectionY, color);
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            stream.Write(data, 0, data.Length);
        }

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