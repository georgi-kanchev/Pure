namespace Purity.Utilities
{
	public struct Color
	{
		public const byte Black = 0;
		public const byte Gray = 0b_010_010_01; // 127 127 127
		public const byte White = 255;

		public const byte Red = 0b_111_000_00;
		public const byte Green = 0b_000_111_00;
		public const byte Blue = 0b_000_000_11;

		public const byte Pink = 0b_111_011_10; // 255 105 180
		public const byte Magenta = 0b_111_000_11; // 255 0 255
		public const byte Violet = 0b_100_000_11; // 143 0 255;
		public const byte Purple = 0b_011_000_10; // 75 0 130;

		public const byte Yellow = 0b_111_111_00; // 255 255 0;
		public const byte Orange = 0b_111_010_00; // 255 165 0;
		public const byte Brown = 0b_010_001_00; // 150 105 25;

		public const byte Cyan = 0b_000_111_11; // 0 255 255;
		public const byte Azure = 0b_000_011_11; // 0 127 255;

		public byte R
		{
			get => red;
			set { red = value; UpdateValue(); }
		}
		public byte G
		{
			get => green;
			set { green = value; UpdateValue(); }
		}
		public byte B
		{
			get => blue;
			set { blue = value; UpdateValue(); }
		}

		public Color(byte value)
		{
			this.value = 0;
			red = 0;
			green = 0;
			blue = 0;

			Value = value;
		}
		public Color(byte red, byte green, byte blue)
		{
			value = 0;
			this.red = 0;
			this.green = 0;
			this.blue = 0;

			R = red;
			G = green;
			B = blue;
		}

		public static implicit operator Color((byte, byte, byte) rgb)
		{
			return new Color(rgb.Item1, rgb.Item2, rgb.Item3);
		}
		public static implicit operator (byte, byte, byte)(Color color)
		{
			return (color.R, color.G, color.B);
		}
		public static implicit operator Color(byte value)
		{
			return new Color(value);
		}
		public static implicit operator byte(Color color)
		{
			return color.value;
		}

		public static bool operator ==(Color a, Color b) => a.value == b.value;
		public static bool operator !=(Color a, Color b) => a.value != b.value;

		public override int GetHashCode() => base.GetHashCode();
		public override bool Equals(object? obj) => base.Equals(obj);
		public override string ToString()
		{
			return $"{value} | {red} {green} {blue}";
		}

		#region Backend
		private byte Value
		{
			get => value;
			set { this.value = value; UpdateRGB(); }
		}
		private byte value, red, green, blue;

		private void UpdateRGB()
		{
			var binary = Convert.ToString(Value, 2).PadLeft(8, '0');
			var r = binary[0..3];
			var g = binary[3..6];
			var b = binary[6..8];
			red = (byte)(Convert.ToByte(r, 2) * byte.MaxValue / 7);
			green = (byte)(Convert.ToByte(g, 2) * byte.MaxValue / 7);
			blue = (byte)(Convert.ToByte(b, 2) * byte.MaxValue / 3);
		}
		private void UpdateValue()
		{
			var r0 = MathF.Round(red / 36f);
			var g0 = MathF.Round(green / 36f);
			var b0 = MathF.Round(blue / 85f);

			var r = Convert.ToString((int)r0, 2).PadLeft(3, '0');
			var g = Convert.ToString((int)g0, 2).PadLeft(3, '0');
			var b = Convert.ToString((int)b0, 2).PadLeft(2, '0');
			Value = Convert.ToByte($"{r}{g}{b}", 2);
		}
		#endregion
	}
}
