namespace Pure.Utilities;

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

	public byte R
	{
		get => r;
		set { r = value; UpdateValue(); }
	}
	public byte G
	{
		get => g;
		set { g = value; UpdateValue(); }
	}
	public byte B
	{
		get => b;
		set { b = value; UpdateValue(); }
	}
	public byte A
	{
		get => a;
		set { a = value; UpdateValue(); }
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

	public Color ToDark(float unit = 0.5f)
	{
		r = (byte)Map(unit, 0, 1, r, 0);
		g = (byte)Map(unit, 0, 1, g, 0);
		b = (byte)Map(unit, 0, 1, b, 0);
		UpdateValue();
		return this;
	}
	public Color ToBright(float unit = 0.5f)
	{
		r = (byte)Map(unit, 0, 1, r, 255);
		g = (byte)Map(unit, 0, 1, g, 255);
		b = (byte)Map(unit, 0, 1, b, 255);
		UpdateValue();
		return this;
	}

	public static implicit operator Color((byte, byte, byte) rgb)
	{
		return new Color(rgb.Item1, rgb.Item2, rgb.Item3);
	}
	public static implicit operator (byte, byte, byte)(Color color)
	{
		return (color.R, color.G, color.B);
	}
	public static implicit operator Color(uint value) => new(value);
	public static implicit operator uint(Color color) => color.v;

	public static bool operator ==(Color a, Color b) => a.v == b.v;
	public static bool operator !=(Color a, Color b) => a.v != b.v;

	public override int GetHashCode() => base.GetHashCode();
	public override bool Equals(object? obj) => base.Equals(obj);
	public override string ToString()
	{
		return $"{r} {g} {b}";
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
