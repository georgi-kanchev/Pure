namespace Pure.Engine.Collision;

using System.Numerics;

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
        var b = SolidMap.Decompress(bytes);
        var offset = 0;

        var count = BitConverter.ToInt32(Get());

        Position = (BitConverter.ToSingle(Get()), BitConverter.ToSingle(Get()));
        Scale = (BitConverter.ToSingle(Get()), BitConverter.ToSingle(Get()));

        for (var i = 0; i < count; i++)
        {
            var ax = BitConverter.ToSingle(Get());
            var ay = BitConverter.ToSingle(Get());
            var bx = BitConverter.ToSingle(Get());
            var by = BitConverter.ToSingle(Get());
            var color = BitConverter.ToUInt32(Get());

            Add((ax, ay, bx, by, color));
        }

        byte[] Get()
        {
            return SolidMap.GetBytesFrom(b, 4, ref offset);
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

        return SolidMap.Compress(result.ToArray());
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

    public void MergeClosestPoints(float distance = 0.3f)
    {
        for (var i = 0; i < data.Count; i++)
            for (var j = 0; j < data.Count; j++)
            {
                if (data[i].A == data[j].A && data[i].B == data[j].B)
                    continue;

                const float MIN = 0.001f;
                var (ia, ib) = (data[i].A, data[i].B);
                var (ja, jb) = (data[j].A, data[j].B);

                var distA = Vector2.Distance(new(ia.x, ia.y), new(ja.x, ja.y));
                var distB = Vector2.Distance(new(ib.x, ib.y), new(jb.x, jb.y));
                var distC = Vector2.Distance(new(ia.x, ia.y), new(jb.x, jb.y));

                if (distA > MIN && distA < distance)
                    data[j] = new(ia, jb, data[j].Color);
                else if (distB > MIN && distB < distance)
                    data[j] = new(ja, ib, data[j].Color);
                else if (distC > MIN && distC < distance)
                    data[j] = new(ja, ia, data[j].Color);
            }
    }

    public bool IsOverlapping(LinePack linePack)
    {
        for (var j = 0; j < linePack.Count; j++)
            if (IsOverlapping(linePack[j]))
                return true;

        return false;
    }
    public bool IsOverlapping(SolidMap solidMap)
    {
        return solidMap.IsOverlapping(this);
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
            if (this[i].IsOverlapping(line))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y) point)
    {
        for (var i = 0; i < Count; i++)
            if (this[i].IsOverlapping(point))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y, uint color) point)
    {
        return IsOverlapping((point.x, point.y));
    }

    public bool IsContaining(LinePack linePack)
    {
        for (var i = 0; i < linePack.Count; i++)
            if (IsContaining(linePack[i]) == false || IsOverlapping(linePack[i]))
                return false;

        return true;
    }
    public bool IsContaining(SolidMap solidMap)
    {
        var pack = solidMap.ToArray();
        for (var i = 0; i < solidMap.Count; i++)
            if (IsContaining(pack) == false || solidMap.IsOverlapping(this))
                return false;

        return true;
    }
    public bool IsContaining(SolidPack solidPack)
    {
        for (var i = 0; i < solidPack.Count; i++)
            if (IsContaining(solidPack[i]) == false || IsOverlapping(solidPack[i]))
                return false;

        return true;
    }
    public bool IsContaining(Solid solid)
    {
        var (x, y, w, h, _) = solid.ToBundle();
        return IsContaining((x, y)) &&
               IsContaining((x + w, y)) &&
               IsContaining((x + w, y + h)) &&
               IsContaining((x, y + h)) &&
               IsOverlapping(solid) == false;
    }
    public bool IsContaining(Line line)
    {
        return IsContaining(line.A) && IsContaining(line.B) && IsOverlapping(line) == false;
    }
    public bool IsContaining((float x, float y) point)
    {
        if (data.Count < 3)
            return false;

        var line = new Line((point.x, point.y), (99999f, 99999f));
        return line.CrossPoints(this).Length % 2 == 1;
    }
    public bool IsContaining((float x, float y, uint color) point)
    {
        return IsContaining((point.x, point.y));
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
        {
            var crossPoint = data[i].CrossPoint(line);
            result.Add(crossPoint);
        }

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

    public void NormalizeToPoint((float x, float y, uint color) point)
    {
        NormalizeToPoint((point.x, point.y));
    }
    public void NormalizeToPoint((float x, float y) point)
    {
        for (var i = 0; i < Count; i++)
        {
            var line = this[i];
            line.NormalizeToPoint(point);
            this[i] = line;
        }
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
    public static implicit operator (float x, float y, uint color)[](LinePack linePack)
    {
        return linePack.ToBundlePoints();
    }
    public static implicit operator (float ax, float ay, float bx, float by, uint color)[](LinePack linePack)
    {
        return linePack.ToBundle();
    }
    public static implicit operator LinePack((float ax, float ay, float bx, float by, uint color)[] lines)
    {
        var result = new Line[lines.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = lines[i];

        return new(result);
    }
    public static implicit operator LinePack((float x, float y, uint color)[] points)
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
        m *= Matrix3x2.CreateScale(Scale.width, Scale.height);
        m *= Matrix3x2.CreateRotation(MathF.PI / 180f * Angle);
        m *= Matrix3x2.CreateTranslation(new(Position.x, Position.y));
        var ra = Vector2.Transform(new(ax, ay), m);
        var rb = Vector2.Transform(new(bx, by), m);
        return (ra.X, ra.Y, rb.X, rb.Y, local.Color);
    }
#endregion
}