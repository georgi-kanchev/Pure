namespace Pure.Engine.Tilemap;

public struct Area
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public (int x, int y) Position
    {
        get => (X, Y);
        set
        {
            X = value.x;
            Y = value.y;
        }
    }
    public (int width, int height) Size
    {
        get => (Width, Height);
        set
        {
            Width = value.width;
            Height = value.height;
        }
    }
    public uint Color { get; set; }

    public Area(int x, int y, int width, int height, uint color = uint.MaxValue)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Color = color;
    }
    public Area((int x, int y) position, (int width, int height) size, uint color = uint.MaxValue) : this(position.x, position.y, size.width, size.height, color)
    {
    }
    public Area(byte[] bytes)
    {
        var offset = 0;
        X = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        Y = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        Width = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        Height = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        Color = BitConverter.ToUInt32(GetBytesFrom(bytes, 4, ref offset));
    }
    public Area(string base64) : this(Convert.FromBase64String(base64))
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
    public (int x, int y, int width, int height, uint color) ToBundle()
    {
        return (X, Y, Width, Height, Color);
    }
    public override string ToString()
    {
        return $"{nameof(Position)}{Position} {nameof(Size)}{Size}";
    }

    public static implicit operator Area((float x, float y, float width, float height) rectangle)
    {
        return new((int)rectangle.x, (int)rectangle.y, (int)rectangle.width, (int)rectangle.height);
    }
    public static implicit operator Area((float x, float y, float width, float height, uint color) bundle)
    {
        return new((int)bundle.x, (int)bundle.y, (int)bundle.width, (int)bundle.height, bundle.color);
    }
    public static implicit operator (float x, float y, float width, float height, uint color)(Area area)
    {
        return area.ToBundle();
    }
    public static implicit operator (float x, float y, float width, float height)(Area area)
    {
        return (area.Position.x, area.Position.y, area.Size.width, area.Size.height);
    }
    public static implicit operator Area((int x, int y, int width, int height) rectangle)
    {
        return new(rectangle.x, rectangle.y, rectangle.width, rectangle.height);
    }
    public static implicit operator Area((int x, int y, int width, int height, uint color) bundle)
    {
        return new(bundle.x, bundle.y, bundle.width, bundle.height, bundle.color);
    }
    public static implicit operator (int x, int y, int width, int height, uint color)(Area area)
    {
        return area.ToBundle();
    }
    public static implicit operator (int x, int y, int width, int height)(Area area)
    {
        return (area.Position.x, area.Position.y, area.Size.width, area.Size.height);
    }
    public static implicit operator byte[](Area area)
    {
        return area.ToBytes();
    }
    public static implicit operator Area(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}