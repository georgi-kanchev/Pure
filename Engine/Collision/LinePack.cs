namespace Pure.Engine.Collision;

using System.Numerics;
using System.Runtime.InteropServices;

public class LinePack : Pack<Line>
{
    public float Angle { get; set; }

    public LinePack()
    {
    }
    public LinePack(params Line[] lines) : base(lines)
    {
    }
    public LinePack(params (float x, float y, uint color)[]? points)
    {
        if (points == null || points.Length < 2)
            return;

        var lines = new List<Line>();
        for (var i = 1; i < points.Length; i++)
        {
            var a = points[i - 1];
            var b = points[i];
            lines.Add(new((a.x, a.y), (b.x, b.y), a.color));
        }

        data.AddRange(lines);
    }
    public LinePack(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        var count = BitConverter.ToInt32(Get<int>());

        Position = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
        Scale = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));

        for (var i = 0; i < count; i++)
        {
            var ax = BitConverter.ToSingle(Get<float>());
            var ay = BitConverter.ToSingle(Get<float>());
            var bx = BitConverter.ToSingle(Get<float>());
            var by = BitConverter.ToSingle(Get<float>());
            var color = BitConverter.ToUInt32(Get<uint>());

            Add((ax, ay, bx, by, color));
        }

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public LinePack(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(data.Count));
        result.AddRange(BitConverter.GetBytes(Position.x));
        result.AddRange(BitConverter.GetBytes(Position.y));
        result.AddRange(BitConverter.GetBytes(Scale.width));
        result.AddRange(BitConverter.GetBytes(Scale.height));

        foreach (var r in data)
        {
            result.AddRange(BitConverter.GetBytes(r.A.x));
            result.AddRange(BitConverter.GetBytes(r.A.y));
            result.AddRange(BitConverter.GetBytes(r.B.x));
            result.AddRange(BitConverter.GetBytes(r.B.y));
            result.AddRange(BitConverter.GetBytes(r.Color));
        }

        return Compress(result.ToArray());
    }
    public (float ax, float ay, float bx, float by, uint color)[] ToBundle()
    {
        var result = new (float ax, float ay, float bx, float by, uint color)[data.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = this[i];
        return result;
    }
    public (float x, float y, uint color)[] ToBundlePoints()
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
        {
            var line = this[i];
            var (a, b) = (line.A, line.B);
            var pointA = (a.x, a.y, line.Color);
            var pointB = (b.x, b.y, line.Color);

            result.Add(pointA);
            result.Add(pointB);
        }

        return result.ToArray();
    }

    public bool IsOverlapping(LinePack linePack)
    {
        for (var j = 0; j < linePack.Count; j++)
            if (IsOverlapping(linePack[j]))
                return true;

        return false;
    }
    public bool IsOverlapping(SolidPack solidPack)
    {
        for (var i = 0; i < Count; i++)
            if (solidPack.IsOverlapping(this[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(Solid solid)
    {
        for (var i = 0; i < Count; i++)
            if (solid.IsOverlapping(this[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(Line line)
    {
        for (var i = 0; i < data.Count; i++)
            if (this[i].IsCrossing(line))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y) point)
    {
        for (var i = 0; i < Count; i++)
            if (this[i].IsCrossing(point))
                return true;

        return false;
    }

    public (float x, float y, uint color)[] CrossPoints(LinePack linePack)
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(linePack));

        return result.ToArray();
    }
    public (float x, float y, uint color)[] CrossPoints(SolidMap solidMap)
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(solidMap));

        return result.ToArray();
    }
    public (float x, float y, uint color)[] CrossPoints(SolidPack solidPack)
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(solidPack));

        return result.ToArray();
    }
    public (float x, float y, uint color)[] CrossPoints(Solid solid)
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(solid));

        return result.ToArray();
    }
    public (float x, float y, uint color)[] CrossPoints(Line line)
    {
        var result = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
            result.Add(data[i].CrossPoint(line));

        return result.ToArray();
    }

    public (float x, float y, uint color) ClosestPoint((float x, float y, uint color) point)
    {
        var result = ClosestPoint((point.x, point.y));
        result.color = point.color;
        return result;
    }
    public (float x, float y, uint color) ClosestPoint((float x, float y) point)
    {
        var closestPoints = new List<(float x, float y, uint color)>();
        for (var i = 0; i < data.Count; i++)
            closestPoints.Add(data[i].ClosestPoint(point));

        var result = (float.NaN, float.NaN, 0u);
        var closestDistance = float.MaxValue;
        foreach (var pt in closestPoints)
        {
            var distance = Vector2.Distance(new(), new());

            if (distance > closestDistance)
                continue;

            closestDistance = distance;
            result = pt;
        }

        return result;
    }

    public LinePack Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator LinePack(Line[] lines)
    {
        return new(lines);
    }
    public static implicit operator Line[](LinePack linePack)
    {
        return linePack.ToArray();
    }
    public static implicit operator (float x, float y, uint color)[](
        LinePack linePack)
    {
        return linePack.ToBundlePoints();
    }
    public static implicit operator (float ax, float ay, float bx, float by, uint color)[](
        LinePack linePack)
    {
        return linePack.ToBundle();
    }
    public static implicit operator LinePack(
        (float ax, float ay, float bx, float by, uint color)[] lines)
    {
        var result = new Line[lines.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = lines[i];

        return new(result);
    }
    public static implicit operator LinePack(
        (float x, float y, uint color)[] points)
    {
        return new(points);
    }
    public static implicit operator byte[](LinePack linePack)
    {
        return linePack.ToBytes();
    }
    public static implicit operator LinePack(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    protected override Line LocalToGlobal(Line local)
    {
        var (ax, ay) = local.A;
        var (bx, by) = local.B;
        var m = Matrix3x2.Identity;
        m *= Matrix3x2.CreateScale(Scale.width, Scale.height, new(Position.x, Position.y));
        m *= Matrix3x2.CreateRotation(MathF.PI / 180f * Angle);
        m *= Matrix3x2.CreateTranslation(new(Position.x, Position.y));
        var ra = Vector2.Transform(new(ax, ay), m);
        var rb = Vector2.Transform(new(bx, by), m);
        return (ra.X, ra.Y, rb.X, rb.Y, local.Color);
    }
#endregion
}