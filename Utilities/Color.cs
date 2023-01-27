namespace Pure.Utilities
{
	public struct Color
	{
		public const byte Black = 0b_000_000_00;
		public const byte Gray = 0b_010_010_01; // 127 127 127
		public const byte White = 0b_111_111_11;

		public const byte Red = 0b_111_000_00;
		public const byte Green = 0b_000_111_00;
		public const byte Blue = 0b_000_000_11;

		public const byte Pink = 0b_111_011_10; // 255 105 180
		public const byte Magenta = 0b_111_000_11; // 255 0 255
		public const byte Violet = 0b_100_000_11; // 143 0 255;
		public const byte Purple = 0b_011_000_10; // 75 0 130;

		public const byte Yellow = 0b_111_111_00; // 255 255 0;
		public const byte Orange = 0b_111_011_00; // 255 165 0;
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
			red = (byte)((value >> 5) * 255 / 7);
			green = (byte)(((value >> 2) & 0x07) * 255 / 7);
			blue = (byte)((value & 0x03) * 255 / 3);
		}
		private void UpdateValue()
		{
			Value = (byte)((R * 7 / 255) << 5 + (G * 7 / 255) << 2 + (B * 3 / 255));
		}
		#endregion
	}
}
