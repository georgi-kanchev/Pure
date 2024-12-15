namespace Pure.Engine.Utility;

public struct Color
{
    public static Color Black
    {
        get => new(0, 0, 0);
    }
    public static Color Gray
    {
        get => new(127, 127, 127);
    }
    public static Color White
    {
        get => new(255, 255, 255);
    }

    public static Color Red
    {
        get => new(255, 0, 0);
    }
    public static Color Green
    {
        get => new(0, 255, 0);
    }
    public static Color Blue
    {
        get => new(0, 0, 255);
    }

    public static Color Pink
    {
        get => new(255, 105, 180);
    }
    public static Color Magenta
    {
        get => new(255, 0, 255);
    }
    public static Color Violet
    {
        get => new(143, 0, 255);
    }
    public static Color Purple
    {
        get => new(75, 0, 130);
    }

    public static Color Yellow
    {
        get => new(255, 255, 0);
    }
    public static Color Orange
    {
        get => new(255, 127, 80);
    }
    public static Color Brown
    {
        get => new(150, 105, 25);
    }

    public static Color Cyan
    {
        get => new(0, 255, 255);
    }
    public static Color Azure
    {
        get => new(0, 127, 255);
    }

    public static Color RandomBright
    {
        get
        {
            var r = (byte)(127, 255).Random();
            var g = (byte)(127, 255).Random();
            var b = (byte)(127, 255).Random();
            return new(r, g, b);
        }
    }
    public static Color RandomDark
    {
        get
        {
            var r = (byte)(0, 127).Random();
            var g = (byte)(0, 127).Random();
            var b = (byte)(0, 127).Random();
            return new(r, g, b);
        }
    }
    public static Color Random
    {
        get
        {
            var r = (byte)(0, 255).Random();
            var g = (byte)(0, 255).Random();
            var b = (byte)(0, 255).Random();
            return new(r, g, b);
        }
    }

    public byte R
    {
        get => r;
        set
        {
            r = value;
            UpdateValue();
        }
    }
    public byte G
    {
        get => g;
        set
        {
            g = value;
            UpdateValue();
        }
    }
    public byte B
    {
        get => b;
        set
        {
            b = value;
            UpdateValue();
        }
    }
    public byte A
    {
        get => a;
        set
        {
            a = value;
            UpdateValue();
        }
    }
    public uint Value
    {
        get => this;
        set
        {
            v = value;
            UpdateRGB();
        }
    }

    public Color(uint value)
    {
        v = value;
        r = 0;
        g = 0;
        b = 0;
        a = 0;

        UpdateRGB();
    }
    public Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        v = 0;
        r = red;
        g = green;
        b = blue;
        a = alpha;

        UpdateValue();
    }
    public Color((byte red, byte green, byte blue, byte alpha) bundle) : this(bundle.red, bundle.green, bundle.blue, bundle.alpha)
    {
    }
    public Color(byte rgb, byte alpha = 255) : this(rgb, rgb, rgb, alpha)
    {
    }

    public Color ToDark(float unit = 0.5f)
    {
        var red = (byte)Map(unit, 0, 1, R, 0);
        var green = (byte)Map(unit, 0, 1, G, 0);
        var blue = (byte)Map(unit, 0, 1, B, 0);
        return new(red, green, blue);
    }
    public Color ToColor(Color color, float unit = 0.5f)
    {
        var red = (byte)Map(unit, 0, 1, R, color.r);
        var green = (byte)Map(unit, 0, 1, G, color.g);
        var blue = (byte)Map(unit, 0, 1, B, color.b);
        var alpha = (byte)Map(unit, 0, 1, A, color.a);
        return new(red, green, blue, alpha);
    }
    public Color ToBright(float unit = 0.5f)
    {
        var red = (byte)Map(unit, 0, 1, R, 255);
        var green = (byte)Map(unit, 0, 1, G, 255);
        var blue = (byte)Map(unit, 0, 1, B, 255);
        return new(red, green, blue);
    }
    public Color ToTransparent(float unit = 0.5f)
    {
        return new(R, G, B, (byte)Map(unit, 0, 1, A, 0));
    }
    public Color ToOpaque(float unit = 0.5f)
    {
        return new(R, G, B, (byte)Map(unit, 0, 1, A, 255));
    }
    public Color ToOpposite()
    {
        return new((byte)(255 - R), (byte)(255 - G), (byte)(255 - B));
    }

    public (byte red, byte green, byte blue, byte alpha) ToBundle()
    {
        return this;
    }
    public override string ToString()
    {
        return $"R({r}) G({g}) B({b}) A({a})";
    }

    public string ToBrush(char brush = '#')
    {
        return $"{brush}{v:X}{brush}";
    }
    public static implicit operator Color((byte r, byte g, byte b, byte a) bundle)
    {
        return new(bundle.r, bundle.g, bundle.b, bundle.a);
    }
    public static implicit operator (byte r, byte g, byte b, byte a)(Color color)
    {
        return (color.R, color.G, color.B, color.A);
    }
    public static implicit operator Color(uint value)
    {
        return new(value);
    }
    public static implicit operator uint(Color color)
    {
        return color.v;
    }
    public static bool operator ==(Color a, Color b)
    {
        return a.v == b.v;
    }
    public static bool operator !=(Color a, Color b)
    {
        return a.v != b.v;
    }

    public bool Equals(Color other)
    {
        return r == other.r && g == other.g && b == other.b && a == other.a && v == other.v;
    }
    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(r, g, b, a, v);
    }

#region Backend
    private byte r, g, b, a;
    private uint v;

    private void UpdateRGB()
    {
        r = (byte)((v >> 24) & 255);
        g = (byte)((v >> 16) & 255);
        b = (byte)((v >> 8) & 255);
        a = (byte)((v >> 0) & 255);
    }
    private void UpdateValue()
    {
        v = (uint)((r << 24) + (g << 16) + (b << 8) + a);
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}