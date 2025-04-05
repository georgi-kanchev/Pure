global using SizeI = (int width, int height);
global using VecI = (int x, int y);
global using RangeF = (float a, float b);
global using RangeI = (int a, int b);
using BundleI = (int x, int y, int width, int height);
using BundleF = (float x, float y, float width, float height);

namespace Pure.Engine.Tiles;

public struct Area(int x, int y, int width, int height)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

    public VecI Position
    {
        get => (X, Y);
        set
        {
            X = value.x;
            Y = value.y;
        }
    }
    public SizeI Size
    {
        get => (Width, Height);
        set
        {
            Width = value.width;
            Height = value.height;
        }
    }

    public Area(VecI position, SizeI size) : this(position.x, position.y, size.width, size.height)
    {
    }

    public BundleI ToBundle()
    {
        return (X, Y, Width, Height);
    }
    public override string ToString()
    {
        return $"{nameof(Position)}{Position} {nameof(Size)}{Size}";
    }

    public static implicit operator Area(BundleF bundle)
    {
        return new((int)bundle.x, (int)bundle.y, (int)bundle.width, (int)bundle.height);
    }
    public static implicit operator BundleF(Area area)
    {
        return area.ToBundle();
    }
    public static implicit operator Area(BundleI bundle)
    {
        return new(bundle.x, bundle.y, bundle.width, bundle.height);
    }
    public static implicit operator BundleI(Area area)
    {
        return area.ToBundle();
    }
}