namespace Pure.Utilities;

/// <summary>
/// Represents a color in RGBA format and performs color operations.
/// </summary>
public struct Color
{
	public const uint Black = 255; // 0, 0, 0
	public const uint Gray = 2139062271; // 127, 127, 127
	public const uint White = uint.MaxValue; // 255, 255, 255

	public const uint Red = 4278190335; // 255, 0, 0
	public const uint Green = 16711935; // 0, 255, 0
	public const uint Blue = 65535; // 0, 0, 255

	public const uint Pink = 4285117695; // 255, 105, 180
	public const uint Magenta = 4278255615; // 255, 0, 255
	public const uint Violet = 2399207423; // 143, 0, 255
	public const uint Purple = 1258324735; // 75, 0, 130

	public const uint Yellow = 4294902015; // 255, 255, 0
	public const uint Orange = 4286533887; // 255, 127, 80
	public const uint Brown = 2523470335; // 150, 105, 25

	public const uint Cyan = 16777215; // 0, 255, 255
	public const uint Azure = 8388607; // 0, 127, 255

	/// <summary>
	/// Gets or sets the red component of the color.
	/// </summary>
	public byte R
	{
		get => r;
		set { r = value; UpdateValue(); }
	}
	/// <summary>
	/// Gets or sets the green component of the color.
	/// </summary>
	public byte G
	{
		get => g;
		set { g = value; UpdateValue(); }
	}
	/// <summary>
	/// Gets or sets the blue component of the color.
	/// </summary>
	public byte B
	{
		get => b;
		set { b = value; UpdateValue(); }
	}
	/// <summary>
	/// Gets or sets the alpha component of the color (opacity).
	/// </summary>
	public byte A
	{
		get => a;
		set { a = value; UpdateValue(); }
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
	public Color((byte red, byte green, byte blue, byte alpha) bundle) :
		this(bundle.red, bundle.green, bundle.blue, bundle.alpha)
	{ }

	/// <summary>
	/// Converts the color to a darker shade.
	/// </summary>
	/// <param name="unit">The darkness level, expressed as a float value between 
	/// 0 and 1 (default: 0.5).</param>
	/// <returns>The color itself, after being darkened.</returns>
	public Color ToDark(float unit = 0.5f)
	{
		r = (byte)Map(unit, 0, 1, r, 0);
		g = (byte)Map(unit, 0, 1, g, 0);
		b = (byte)Map(unit, 0, 1, b, 0);
		UpdateValue();
		return this;
	}
	/// <summary>
	/// Converts the color to a brighter shade.
	/// </summary>
	/// <param name="unit">The brightness level, expressed as a float 
	/// value between 0 and 1 (default: 0.5).</param>
	/// <returns>The color itself, after being brightened.</returns>
	public Color ToBright(float unit = 0.5f)
	{
		r = (byte)Map(unit, 0, 1, r, 255);
		g = (byte)Map(unit, 0, 1, g, 255);
		b = (byte)Map(unit, 0, 1, b, 255);
		UpdateValue();
		return this;
	}

	/// <returns>
	/// A bundle tuple containing the red, green, blue, and alpha components of the color.</returns>
	public (byte red, byte green, byte blue, byte alpha) ToBundle() => this;

	public override int GetHashCode() => base.GetHashCode();
	public override bool Equals(object? obj) => base.Equals(obj);
	/// <returns>
	/// A string that represents this color.</returns>
	public override string ToString()
	{
		return $"R({r}) G({g}) B({b}) A({a})";
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
	public static implicit operator Color(uint value) => new(value);
	/// <param name="color">
	/// The color instance to convert.</param>
	/// <returns>A uint value initialized with the value of this color instance.</returns>
	public static implicit operator uint(Color color) => color.v;

	/// <summary>
	/// Determines whether two color instances are equal.
	/// </summary>
	/// <param name="a">The first color instance to compare.</param>
	/// <param name="b">The second color instance to compare.</param>
	public static bool operator ==(Color a, Color b) => a.v == b.v;
	/// <summary>
	/// Determines whether two color instances are different.
	/// </summary>
	/// <param name="a">The first color instance to compare.</param>
	/// <param name="b">The second color instance to compare.</param>
	public static bool operator !=(Color a, Color b) => a.v != b.v;

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
