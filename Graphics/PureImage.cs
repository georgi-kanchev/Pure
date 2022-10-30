using SFML.Graphics;

namespace Graphics
{
	public class PureImage
	{
		public PureImage(string imageFile)
		{
			img = new Image(imageFile);
		}

		public byte[] Purify()
		{
			var list = new List<byte>();
			for(uint y = 0; y < img.Size.Y; y++)
				for(uint x = 0; x < img.Size.X; x++)
				{
					var value = img.GetPixel(x, y).A < byte.MaxValue / 2 ? "1" : "0";
				}

			return list.ToArray();
		}

		#region Backend
		private readonly Image img;
		#endregion
	}
}
