using SFML.Graphics;

namespace Pure.Image
{
	public class Image
	{
		public (uint, uint) Size => (img.Size.X, img.Size.Y);

		public byte this[(int, int) pixel]
		{
			set
			{
				if(pixel.Item1 < 0 || pixel.Item1 >= img.Size.X ||
					pixel.Item2 < 0 || pixel.Item2 >= img.Size.Y)
					return;


				img.SetPixel((uint)pixel.Item1, (uint)pixel.Item2, ByteToColor(value));
			}
			get
			{
				if(pixel.Item1 < 0 || pixel.Item1 >= img.Size.X ||
					pixel.Item2 < 0 || pixel.Item2 >= img.Size.Y)
					return default;

				var color = img.GetPixel((uint)pixel.Item1, (uint)pixel.Item2);
				return ByteFromColor(color);
			}
		}

		public Image((uint, uint) size)
		{
			img = new SFML.Graphics.Image(size.Item1, size.Item2);
		}
		public Image(string path)
		{
			img = new SFML.Graphics.Image(path);

			for(uint y = 0; y < img.Size.Y; y++)
				for(uint x = 0; x < img.Size.X; x++)
				{
					var color = ByteFromColor(img.GetPixel(x, y));
					img.SetPixel(x, y, ByteToColor(color));
				}
		}

		public void Save(string path)
		{
			img.SaveToFile(path);
		}

		#region Backend
		private SFML.Graphics.Image img;

		private static Color ByteToColor(byte value)
		{
			var binary = Convert.ToString(value, 2).PadLeft(8, '0');
			var r = binary[0..3];
			var g = binary[3..6];
			var b = binary[6..8];
			var color = new Color
			{
				R = (byte)(Convert.ToByte(r, 2) * byte.MaxValue / 7),
				G = (byte)(Convert.ToByte(g, 2) * byte.MaxValue / 7),
				B = (byte)(Convert.ToByte(b, 2) * byte.MaxValue / 3)
			};
			return color;
		}
		private static byte ByteFromColor(Color color)
		{
			var r0 = MathF.Round(color.R / 36f);
			var g0 = MathF.Round(color.G / 36f);
			var b0 = MathF.Round(color.B / 85f);

			var r = Convert.ToString((int)r0, 2).PadLeft(3, '0');
			var g = Convert.ToString((int)g0, 2).PadLeft(3, '0');
			var b = Convert.ToString((int)b0, 2).PadLeft(2, '0');
			return Convert.ToByte($"{r}{g}{b}", 2);
		}
		#endregion
	}
}
