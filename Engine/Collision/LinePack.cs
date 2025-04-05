namespace Pure.Engine.Collision;

using System.Numerics;

public class LinePack : Pack<Line>
{
    public float Angle { get; set; }

    public LinePack()
    {
    }
    public LinePack(Line[]? lines) : base(lines)
    {
    }
    public LinePack(VecF[]? points)
    {
        if (points == null || points.Length < 2)
            return;

        var lines = new List<Line>();
        for (var i = 1; i < points.Length; i++)
        {
            var a = points[i - 1];
            var b = points[i];
            lines.Add(new((a.x, a.y), (b.x, b.y)));
        }

        data.AddRange(lines);
    }

    public LineBundle[] ToBundle()
    {
        var result = new LineBundle[data.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = this[i];
        return result;
    }
    public VecF[] ToBundlePoints()
    {
        var result = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
        {
            var line = this[i];
            var (a, b) = (line.A, line.B);
            var pointA = (a.x, a.y);
            var pointB = (b.x, b.y);

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
                    data[j] = new(ia, jb);
                else if (distB > MIN && distB < distance)
                    data[j] = new(ja, ib);
                else if (distC > MIN && distC < distance)
                    data[j] = new(ja, ia);
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
    public bool IsOverlapping(VecF point)
    {
        for (var i = 0; i < Count; i++)
            if (this[i].IsOverlapping(point))
                return true;

        return false;
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
        var (x, y, w, h) = solid.ToBundle();
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
    public bool IsContaining(VecF point)
    {
        if (data.Count < 3)
            return false;

        var line = new Line((point.x, point.y), (99999f, 99999f));
        return line.CrossPoints(this).Length % 2 == 1;
    }

    public VecF[] CrossPoints(LinePack linePack)
    {
        var result = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(linePack));

        return result.ToArray();
    }
    public VecF[] CrossPoints(SolidMap solidMap)
    {
        var result = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(solidMap));

        return result.ToArray();
    }
    public VecF[] CrossPoints(SolidPack solidPack)
    {
        var result = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(solidPack));

        return result.ToArray();
    }
    public VecF[] CrossPoints(Solid solid)
    {
        var result = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
            result.AddRange(data[i].CrossPoints(solid));

        return result.ToArray();
    }
    public VecF[] CrossPoints(Line line)
    {
        var result = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
        {
            var crossPoint = data[i].CrossPoint(line);
            result.Add(crossPoint);
        }

        return result.ToArray();
    }

    public VecF ClosestPoint(VecF point)
    {
        var closestPoints = new List<VecF>();
        for (var i = 0; i < data.Count; i++)
            closestPoints.Add(data[i].ClosestPoint(point));

        var result = (float.NaN, float.NaN);
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

    public void NormalizeToPoint(VecF point)
    {
        for (var i = 0; i < Count; i++)
        {
            var line = this[i];
            line.NormalizeToPoint(point);
            this[i] = line;
        }
    }

    public static implicit operator LinePack(Line[] lines)
    {
        return new(lines);
    }
    public static implicit operator Line[](LinePack linePack)
    {
        return linePack.ToArray();
    }
    public static implicit operator VecF[](LinePack linePack)
    {
        return linePack.ToBundlePoints();
    }
    public static implicit operator LineBundle[](LinePack linePack)
    {
        return linePack.ToBundle();
    }
    public static implicit operator LinePack(LineBundle[] lines)
    {
        var result = new Line[lines.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = lines[i];

        return new(result);
    }
    public static implicit operator LinePack(VecF[] points)
    {
        return new(points);
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
        return (ra.X, ra.Y, rb.X, rb.Y);
    }
#endregion
}