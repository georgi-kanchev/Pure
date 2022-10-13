using System.Diagnostics.CodeAnalysis;

namespace Engine
{
	public struct Color
	{
		public static Color Black => new(0);
		public static Color White => new(255);

		public static Color Red => new(255, 0, 0);
		public static Color Green => new(0, 255, 0);
		public static Color Blue => new(0, 0, 255);

		public static Color Yellow => new(255, 255, 0);
		public static Color Magenta => new(255, 0, 255);
		public static Color Cyan => new(0, 255, 255);

		public static Color Purple => new(75, 0, 130);
		public static Color Violet => new(143, 0, 255);
		public static Color Pink => new(255, 105, 180);

		public static Color Brown => new(150, 105, 25);

		public byte Value
		{
			get => value;
			set
			{
				this.value = value;

				var binary = Convert.ToString(Value, 2).PadLeft(8, '0');
				var r = binary[0..3];
				var g = binary[3..6];
				var b = binary[6..8];
				red = Convert.ToByte(r, 2);
				green = Convert.ToByte(g, 2);
				blue = Convert.ToByte(b, 2);
			}
		}
		public byte R => red;
		public byte G => green;
		public byte B => blue;

		public Color(byte value)
		{
			red = 0;
			green = 0;
			blue = 0;
			this.value = 0;

			Value = value;
		}
		public Color(byte red, byte green, byte blue)
		{
			value = 0;
			this.red = 0;
			this.green = 0;
			this.blue = 0;

			var r = Convert.ToString(red / 36, 2).PadLeft(3, '0');
			var g = Convert.ToString(green / 36, 2).PadLeft(3, '0');
			var b = Convert.ToString(blue / 85, 2).PadLeft(2, '0');
			Value = Convert.ToByte($"{r}{g}{b}", 2);
		}

		public static bool operator ==(Color a, Color b) => a.value == b.value;
		public static bool operator !=(Color a, Color b) => a.value != b.value;

		public override int GetHashCode() => base.GetHashCode();
		public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);

		#region Backend
		private byte value, red, green, blue;

		internal SFML.Graphics.Color ToSFML()
		{
			return new SFML.Graphics.Color((byte)(red * 36), (byte)(green * 36), (byte)(blue * 85));
		}
		#endregion
	}
}
