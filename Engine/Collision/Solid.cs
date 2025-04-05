global using AreaF = (float x, float y, float width, float height);
global using AreaI = (int x, int y, int width, int height);

namespace Pure.Engine.Collision;

/// <summary>
/// Represents a solid in 2D space defined by its position and size.
/// </summary>
public struct Solid(float x, float y, float width, float height) : IEquatable<Solid>
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
    public float Width { get; set; } = width;
    public float Height { get; set; } = height;

    public VecF Position
    {
        get => (X, Y);
        set
        {
            X = value.x;
            Y = value.y;
        }
    }
    public SizeF Size
    {
        get => (Width, Height);
        set
        {
            Width = value.width;
            Height = value.height;
        }
    }

    public Solid(VecF position, SizeF size) : this(position.x, position.y, size.width, size.height)
    {
    }

    /// <returns>
    /// A bundle tuple containing the position, size and the color of the solid.</returns>
    public AreaF ToBundle()
    {
        return (X, Y, Width, Height);
    }
    /// <returns>
    /// A string that represents this solid. 
    /// The string has the format: "Position[x y] Size[width height]".</returns>
    public override string ToString()
    {
        return $"{nameof(Position)}{Position} {nameof(Size)}{Size}";
    }

    public bool IsOverlapping(LinePack linePack)
    {
        return linePack.IsOverlapping(this);
    }
    public bool IsOverlapping(SolidMap solidMap)
    {
        return solidMap.IsOverlapping(this);
    }
    /// <param name="solidPack">
    /// The hitbox to test for overlap with.</param>
    /// <returns>True if this solid overlaps with the specified 
    /// hitbox; otherwise, false.</returns>
    public bool IsOverlapping(SolidPack solidPack)
    {
        return solidPack.IsOverlapping(this);
    }
    /// <summary>
    /// Determines whether this solid is overlapping with the specified 
    /// solid.
    /// </summary>
    /// <param name="solid">The solid to test for overlap with.</param>
    /// <returns>True if this solids overlap; otherwise, false.</returns>
    public bool IsOverlapping(Solid solid)
    {
        var (x1, y1, w1, h1) = ToBundle();
        var (x2, y2, w2, h2) = solid.ToBundle();

        return x1 < x2 + w2 &&
               x1 + w1 > x2 &&
               y1 < y2 + h2 &&
               y1 + h1 > y2;
    }
    /// <param name="line"> 
    /// The line to test for overlap with.</param>
    /// <returns>True if this solid overlaps with the specified 
    /// line; otherwise, false.</returns>
    public bool IsOverlapping(Line line)
    {
        return IsOverlapping(line.A) || IsOverlapping(line.B) || line.IsOverlapping(this);
    }
    /// <param name="point">
    /// The line to test for overlap with.</param>
    /// <returns>True if this solid overlaps with the specified 
    /// point; otherwise, false.</returns>
    public bool IsOverlapping(VecF point)
    {
        var containsX = X < point.x && point.x < X + Width;
        var containsY = Y < point.y && point.y < Y + Height;
        return containsX && containsY;
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
        var (bx, by, w, h) = solid.ToBundle();
        return IsContaining((bx, by)) && IsContaining((bx + w, by + h));
    }
    public bool IsContaining(Line line)
    {
        return IsContaining(line.A) && IsContaining(line.B);
    }
    public bool IsContaining(VecF point)
    {
        return IsOverlapping(point);
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(Solid other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
    }
    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Solid other && Equals(other);
    }
    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(Solid left, Solid right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Solid left, Solid right)
    {
        return !(left == right);
    }

    public static implicit operator Solid(AreaI bundle)
    {
        return new(bundle.x, bundle.y, bundle.width, bundle.height);
    }
    public static implicit operator AreaI(Solid solid)
    {
        return ((int)solid.X, (int)solid.Y, (int)solid.Width, (int)solid.Height);
    }
    public static implicit operator Solid(AreaF bundle)
    {
        return new(bundle.x, bundle.y, bundle.width, bundle.height);
    }
    public static implicit operator AreaF(Solid solid)
    {
        return solid.ToBundle();
    }
}