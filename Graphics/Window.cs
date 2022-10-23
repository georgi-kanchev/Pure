using SFML.Graphics; // textures and vertices etc.
using SFML.System; // vectors etc.
using SFML.Window; // window etc.

namespace Purity.Graphics
{
	/// <summary>
	/// Uses <see cref="SFML"/> to create a window and display the provided cells on it.
	/// </summary>
	public class Window
	{
		public const int GRAPHICS_CELL_SIZE = 26;

		public bool IsOpen => window != null && window.IsOpen;
		public string Title
		{
			get => title;
			set { title = value; window.SetTitle(title); }
		}

		public (uint, uint) CellCount { get; }
		public uint TotalCellCount => CellCount.Item1 * CellCount.Item2;

		public Window(string graphicsPath = "graphics.png", uint scale = 40)
		{
			var desktopW = VideoMode.DesktopMode.Width;
			var desktopH = VideoMode.DesktopMode.Height;
			CellCount = (desktopW / scale, desktopH / scale);
			title = "Purity";

			window = new(new VideoMode(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT), title);
			window.Closed += (s, e) => window.Close();
			window.Resized += (s, e) =>
			{
				//var ratio = (float)desktopW / desktopH;
				//if(prevWindowSz.Y != e.Height)
				//	window.Size = new((uint)(e.Height * ratio), e.Height);
				//else if(prevWindowSz.X != e.Width)
				//	window.Size = new(e.Width, (uint)(e.Width / ratio));

				var view = window.GetView();
				view.Size = new(e.Width, e.Height);
				view.Center = new(e.Width / 2f, e.Height / 2f);
				window.SetView(view);
			};

			graphics = new(graphicsPath);
			tileSize = (int)graphics.Size.X / GRAPHICS_CELL_SIZE;
		}

		public void DrawBegin()
		{
			window?.DispatchEvents();
			window?.Clear();
		}
		public void Draw(uint[] cells, byte[] colors)
		{
			if(window == null || cells == null || colors == null || cells.Length != TotalCellCount || colors.Length != TotalCellCount)
				return;

			var verts = GetGridVertices(cells, colors);
			window?.Draw(verts, PrimitiveType.Quads, new(graphics));
		}
		public void DrawEnd()
		{
			window?.Display();
		}

		#region Backend
		private const int DEFAULT_WINDOW_WIDTH = 1280, DEFAULT_WINDOW_HEIGHT = 720;
		private string title;
		private readonly int tileSize;

		private readonly Texture graphics;
		private readonly RenderWindow window;

		private Vertex[] GetGridVertices(uint[] cells, byte[] colors)
		{
			if(cells == null || window == null)
				return Array.Empty<Vertex>();

			var cellWidth = (float)window.Size.X / CellCount.Item1;
			var cellHeight = (float)window.Size.Y / CellCount.Item2;
			var verts = new Vertex[cells.Length * 4];

			for(uint y = 0; y < CellCount.Item2; y++)
				for(uint x = 0; x < CellCount.Item1; x++)
				{
					var index = GetIndex(x, y);
					var cell = cells[index];
					var color = ByteToColor(colors[index]);
					var texCoords = IndexToCoords(cell, 27, 27);
					var tx = new Vector2f(texCoords.Item1 * tileSize, texCoords.Item2 * tileSize);
					var i = (y * CellCount.Item1 + x) * 4;

					verts[i + 0] = new(new(x * cellWidth, y * cellHeight), color, tx);
					verts[i + 1] = new(new((x + 1) * cellWidth, y * cellHeight), color, tx + new Vector2f(tileSize, 0));
					verts[i + 2] = new(new((x + 1) * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(tileSize, tileSize));
					verts[i + 3] = new(new(x * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(0, tileSize));
				}
			return verts;
		}
		private static (uint, uint) IndexToCoords(uint index, uint width, uint height)
		{
			index = index < 0 ? 0 : index;
			index = index > width * height - 1 ? width * height - 1 : index;

			return (index % width, index / width);
		}
		private uint GetIndex(uint x, uint y)
		{
			return y * CellCount.Item1 + x;
		}
		private static Color ByteToColor(byte color)
		{
			var binary = Convert.ToString(color, 2).PadLeft(8, '0');
			var r = binary[0..3];
			var g = binary[3..6];
			var b = binary[6..8];
			var red = (byte)(Convert.ToByte(r, 2) * byte.MaxValue / 7);
			var green = (byte)(Convert.ToByte(g, 2) * byte.MaxValue / 7);
			var blue = (byte)(Convert.ToByte(b, 2) * byte.MaxValue / 3);
			return new(red, green, blue);
		}
		#endregion
	}
}
