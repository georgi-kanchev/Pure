namespace Pure.Engine.Tilemap;

public struct Area(int x, int y, int width, int height, uint color = uint.MaxValue)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

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
    public uint Color { get; set; } = color;

    public Area((int x, int y) position, (int width, int height) size, uint color = uint.MaxValue) : this(position.x, position.y, size.width, size.height, color)
    {
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
}