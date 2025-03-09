namespace Pure.Engine.Collision;

public class SolidPack : Pack<Solid>
{
    public SolidPack()
    {
    }
    public SolidPack(params Solid[] solids) : base(solids)
    {
    }

    public AreaF[] ToBundle()
    {
        var result = new AreaF[data.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = this[i];
        return result;
    }

    public void Merge()
    {
        var hasChanges = true;

        while (hasChanges)
        {
            hasChanges = false;

            for (var i = 0; i < data.Count; i++)
            {
                for (var j = i + 1; j < data.Count; j++)
                {
                    var merged = MergeSolids(data[i], data[j]);
                    if (merged == null)
                        continue;

                    data[i] = merged ?? default;
                    data.RemoveAt(j);
                    hasChanges = true;
                    break;
                }

                if (hasChanges)
                    break;
            }
        }

        Solid? MergeSolids(Solid a, Solid b)
        {
            if (Is(a.Y, b.Y) &&
                Is(a.Height, b.Height) &&
                (Is(a.X + a.Width, b.X) || Is(b.X + b.Width, a.X)))
                return (Math.Min(a.X, b.X), a.Y, a.Width + b.Width, a.Height, a.Color);
            if (Is(a.X, b.X) &&
                Is(a.Width, b.Width) &&
                (Is(a.Y + a.Height, b.Y) || Is(b.Y + b.Height, a.Y)))
                return (a.X, Math.Min(a.Y, b.Y), a.Width, a.Height + b.Height, a.Color);

            return null;
        }
        bool Is(float a, float b)
        {
            return Math.Abs(a - b) < 0.001f;
        }
    }

    public bool IsOverlapping(LinePack linePack)
    {
        return linePack.IsOverlapping(this);
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
            if (this[i].IsOverlapping(solid))
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
    public bool IsOverlapping(Point point)
    {
        return IsOverlapping((point.x, point.y));
    }

    public bool IsContaining(LinePack linePack)
    {
        for (var i = 0; i < linePack.Count; i++)
            if (IsContaining(linePack[i]) == false)
                return false;

        return true;
    }
    public bool IsContaining(SolidMap solidMap)
    {
        return IsContaining(solidMap.ToArray());
    }
    public bool IsContaining(SolidPack solidPack)
    {
        for (var i = 0; i < solidPack.Count; i++)
            if (IsContaining(solidPack[i]) == false)
                return false;

        return true;
    }
    public bool IsContaining(Solid solid)
    {
        var (x, y, w, h, _) = solid.ToBundle();
        return IsContaining((x, y)) && IsContaining((x + w, y + h));
    }
    public bool IsContaining(Line line)
    {
        return IsContaining(line.A) && IsContaining(line.B);
    }
    public bool IsContaining(VecF point)
    {
        return IsOverlapping(point);
    }
    public bool IsContaining(Point point)
    {
        return IsOverlapping((point.x, point.y));
    }

    public static implicit operator SolidPack(Solid[] solids)
    {
        return new(solids);
    }
    public static implicit operator Solid[](SolidPack solidPack)
    {
        return solidPack.ToArray();
    }
    public static implicit operator AreaF[](SolidPack solidPack)
    {
        return solidPack.ToBundle();
    }
    public static implicit operator SolidPack(AreaF[] solids)
    {
        var result = new Solid[solids.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = solids[i];
        return result;
    }

#region Backend
    protected override Solid LocalToGlobal(Solid local)
    {
        var (x, y) = local.Position;
        var (w, h) = local.Size;
        local.Position = (Position.x + x * Scale.width, Position.y + y * Scale.height);
        local.Size = (w * Scale.width, h * Scale.height);
        return local;
    }
#endregion
}