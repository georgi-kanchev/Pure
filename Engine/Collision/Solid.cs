namespace Pure.Engine.Collision;

/// <summary>
/// Represents a solid in 2D space defined by its position and size.
/// </summary>
public struct Solid : IEquatable<Solid>
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public (float x, float y) Position
    {
        get => (X, Y);
        set
        {
            X = value.x;
            Y = value.y;
        }
    }
    public (float width, float height) Size
    {
        get => (Width, Height);
        set
        {
            Width = value.width;
            Height = value.height;
        }
    }

    /// <summary>
    /// Gets or sets the color of the solid.
    /// </summary>
    public uint Color { get; set; }

    public Solid(float x, float y, float width, float height, uint color = uint.MaxValue)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Color = color;
    }
    public Solid((float x, float y) position, (float width, float height) size, uint color = uint.MaxValue) : this(position.x, position.y, size.width, size.height, color)
    {
    }
    public Solid(byte[] bytes)
    {
        var offset = 0;
        X = BitConverter.ToSingle(SolidMap.GetBytesFrom(bytes, 4, ref offset));
        Y = BitConverter.ToSingle(SolidMap.GetBytesFrom(bytes, 4, ref offset));
        Width = BitConverter.ToSingle(SolidMap.GetBytesFrom(bytes, 4, ref offset));
        Height = BitConverter.ToSingle(SolidMap.GetBytesFrom(bytes, 4, ref offset));
        Color = BitConverter.ToUInt32(SolidMap.GetBytesFrom(bytes, 4, ref offset));
    }
    public Solid(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(Position.x));
        result.AddRange(BitConverter.GetBytes(Position.y));
        result.AddRange(BitConverter.GetBytes(Size.width));
        result.AddRange(BitConverter.GetBytes(Size.height));
        result.AddRange(BitConverter.GetBytes(Color));
        return result.ToArray();
    }
    /// <returns>
    /// A bundle tuple containing the position, size and the color of the solid.</returns>
    public (float x, float y, float width, float height, uint color) ToBundle()
    {
        return (X, Y, Width, Height, Color);
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
        var (x1, y1, w1, h1, _) = ToBundle();
        var (x2, y2, w2, h2, _) = solid.ToBundle();

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
    public bool IsOverlapping((float x, float y) point)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (px, py) = point;

        var containsX = x < px && px < x + w;
        var containsY = y < py && py < y + h;
        return containsX && containsY;
    }
    public bool IsOverlapping((float x, float y, uint color) point)
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
    public bool IsContaining((float x, float y) point)
    {
        return IsOverlapping(point);
    }
    public bool IsContaining((float x, float y, uint color) point)
    {
        return IsOverlapping((point.x, point.y));
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(Solid other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height) && Color == other.Color;
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
        return HashCode.Combine(X, Y, Width, Height, Color);
    }

    public static bool operator ==(Solid left, Solid right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Solid left, Solid right)
    {
        return !(left == right);
    }

    public static implicit operator Solid((int x, int y, int width, int height) rectangle)
    {
        return new(rectangle.x, rectangle.y, rectangle.width, rectangle.height);
    }
    public static implicit operator Solid((int x, int y, int width, int height, uint color) bundle)
    {
        return new(bundle.x, bundle.y, bundle.width, bundle.height, bundle.color);
    }
    public static implicit operator (int x, int y, int width, int height, uint color)(Solid solid)
    {
        return ((int)solid.X, (int)solid.Y, (int)solid.Width, (int)solid.Height, solid.Color);
    }
    public static implicit operator (int x, int y, int width, int height)(Solid solid)
    {
        return ((int)solid.X, (int)solid.Y, (int)solid.Width, (int)solid.Height);
    }
    public static implicit operator Solid((float x, float y, float width, float height) rectangle)
    {
        return new(rectangle.x, rectangle.y, rectangle.width, rectangle.height);
    }
    public static implicit operator Solid((float x, float y, float width, float height, uint color) bundle)
    {
        return new(bundle.x, bundle.y, bundle.width, bundle.height, bundle.color);
    }
    public static implicit operator (float x, float y, float width, float height, uint color)(Solid solid)
    {
        return solid.ToBundle();
    }
    public static implicit operator (float x, float y, float width, float height)(Solid solid)
    {
        return (solid.Position.x, solid.Position.y, solid.Size.width, solid.Size.height);
    }
    public static implicit operator byte[](Solid solid)
    {
        return solid.ToBytes();
    }
    public static implicit operator Solid(byte[] bytes)
    {
        return new(bytes);
    }
}