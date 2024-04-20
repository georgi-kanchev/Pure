namespace Pure.Engine.Utilities;

/// <summary>
/// Represents a color in RGBA format and performs color operations.
/// </summary>
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

    /// <summary>
    /// Gets or sets the red component of the color.
    /// </summary>
    public byte R
    {
        get => r;
        set
        {
            r = value;
            UpdateValue();
        }
    }
    /// <summary>
    /// Gets or sets the green component of the color.
    /// </summary>
    public byte G
    {
        get => g;
        set
        {
            g = value;
            UpdateValue();
        }
    }
    /// <summary>
    /// Gets or sets the blue component of the color.
    /// </summary>
    public byte B
    {
        get => b;
        set
        {
            b = value;
            UpdateValue();
        }
    }
    /// <summary>
    /// Gets or sets the alpha component of the color (opacity).
    /// </summary>
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

    /// <summary>
    /// Initializes a new color instance with the given uint value.
    /// </summary>
    /// <param name="value">The uint value representing the color in RGBA format.</param>
    public Color(uint value)
    {
        v = value;
        r = 0;
        g = 0;
        b = 0;
        a = 0;

        UpdateRGB();
    }
    /// <summary>
    /// Initializes a new color instance with the given RGBA components.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color (default: 255).</param>
    public Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        v = 0;
        r = red;
        g = green;
        b = blue;
        a = alpha;

        UpdateValue();
    }
    /// <summary>
    /// Initializes a new color instance with the given RGBA components as a bundle tuple.
    /// </summary>
    /// <param name="bundle">A bundle tuple containing the red, green, blue, and alpha
    /// components of the color.</param>
    public Color((byte red, byte green, byte blue, byte alpha) bundle)
        : this(bundle.red, bundle.green, bundle.blue, bundle.alpha)
    {
    }
    public Color(byte rgb, byte alpha = 255) : this(rgb, rgb, rgb, alpha)
    {
    }

    /// <summary>
    /// Converts the color to a darker shade.
    /// </summary>
    /// <param name="unit">The darkness level, expressed as a float value between 
    /// 0 and 1.</param>
    /// <returns>The new darkened color.</returns>
    public Color ToDark(float unit = 0.5f)
    {
        var red = (byte)Map(unit, 0, 1, R, 0);
        var green = (byte)Map(unit, 0, 1, G, 0);
        var blue = (byte)Map(unit, 0, 1, B, 0);
        return new(red, green, blue);
    }
    /// <summary>
    /// Converts the color to a brighter shade.
    /// </summary>
    /// <param name="unit">The brightness level, expressed as a float 
    /// value between 0 and 1.</param>
    /// <returns>The new brightened color.</returns>
    public Color ToBright(float unit = 0.5f)
    {
        var r = (byte)Map(unit, 0, 1, R, 255);
        var g = (byte)Map(unit, 0, 1, G, 255);
        var b = (byte)Map(unit, 0, 1, B, 255);
        return new(r, g, b);
    }

    /// <returns>
    /// A bundle tuple containing the red, green, blue, and alpha components of the color.</returns>
    public (byte red, byte green, byte blue, byte alpha) ToBundle()
    {
        return this;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }
    /// <returns>
    /// A string that represents this color.</returns>
    public override string ToString()
    {
        return $"R({r}) G({g}) B({b}) A({a})";
    }

    public string ToBrush(char brush = '#')
    {
        return $"{brush}{v}{brush}";
    }

    /// <param name="bundle">
    /// The bundle tuple of RGBA values to convert.</param>
    /// <returns>A new color instance initialized with the specified RGBA values.</returns>
    public static implicit operator Color((byte r, byte g, byte b, byte a) bundle)
    {
        return new Color(bundle.r, bundle.g, bundle.b, bundle.a);
    }
    /// <param name="color">
    /// The color instance to convert.</param>
    /// <returns>A bundle tuple of RGBA values initialized with the values of this color instance.</returns>
    public static implicit operator (byte r, byte g, byte b, byte a)(Color color)
    {
        return (color.R, color.G, color.B, color.A);
    }
    /// <param name="value">
    /// The uint value to convert.</param>
    /// <returns>A new color instance initialized with the specified uint value.</returns>
    public static implicit operator Color(uint value)
    {
        return new(value);
    }
    /// <param name="color">
    /// The color instance to convert.</param>
    /// <returns>A uint value initialized with the value of this color instance.</returns>
    public static implicit operator uint(Color color)
    {
        return color.v;
    }

    /// <summary>
    /// Determines whether two color instances are equal.
    /// </summary>
    /// <param name="a">The first color instance to compare.</param>
    /// <param name="b">The second color instance to compare.</param>
    public static bool operator ==(Color a, Color b)
    {
        return a.v == b.v;
    }
    /// <summary>
    /// Determines whether two color instances are different.
    /// </summary>
    /// <param name="a">The first color instance to compare.</param>
    /// <param name="b">The second color instance to compare.</param>
    public static bool operator !=(Color a, Color b)
    {
        return a.v != b.v;
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