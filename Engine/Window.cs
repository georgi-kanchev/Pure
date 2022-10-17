using SFML.Graphics; // textures and vertices etc.
using SFML.System; // vectors etc.
using SFML.Window; // window etc.

namespace Purity.Engine
{
	/// <summary>
	/// Uses <see cref="SFML"/> to create a window and display the provided cells on it.
	/// </summary>
	public class Window
	{
		public bool IsOpen => window != null && window.IsOpen;
		public string Title
		{
			get => title;
			set { title = value; window.SetTitle(title); }
		}
		public int Width
		{
			get => size.X;
			set => size.Y = Math.Max(value, 1);
		}
		public int Height
		{
			get => size.X;
			set => size.Y = Math.Max(value, 1);
		}

		public Window(string graphicsPath = "graphics.png", int scale = 40)
		{
			var desktopW = VideoMode.DesktopMode.Width;
			var desktopH = VideoMode.DesktopMode.Height;
			size.X = (int)desktopW / scale;
			size.Y = (int)desktopH / scale;
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
			tileSize = (int)graphics.Size.X / 26;
			vertices = new Vertex[size.X * size.Y * 4];
			cells = new Cell[size.X, size.Y];

			Fill(27, byte.MaxValue);
		}

		public void Update()
		{
			window?.DispatchEvents();
			window?.Clear();

			UpdateGridVertices();
			window?.Draw(vertices, PrimitiveType.Quads, new(graphics));

			window?.Display();
		}

		public void Fill(int cell, byte color)
		{
			if(cells == null)
				return;

			for(int y = 0; y < size.Y; y++)
				for(int x = 0; x < size.X; x++)
					cells[x, y] = new() { ID = cell, Color = color };
		}
		public void Set(int x, int y, int cell, byte color)
		{
			if(cells == null)
				return;

			x = Limit(x, 0, size.X - 1);
			y = Limit(y, 0, size.Y - 1);

			cells[x, y] = new() { ID = cell, Color = color };
		}
		public void SetAtIndex(int index, int cell, byte color)
		{
			if(cells == null)
				return;

			var coords = IndexToCoords(index, size.X, size.Y);
			cells[coords.Item1, coords.Item2] = new() { ID = cell, Color = color };
		}
		public void DisplayText(string text, int x, int y)
		{
			for(int i = 0; i < text.Length; i++)
			{
				var symbol = text[i];
			}


		}
		public void SetSquare(int cell, int startX, int startY, int endX, int endY)
		{
			if(cells == null)
				return;

			startX = Limit(startX, 0, size.X - 1);
			startY = Limit(startY, 0, size.Y - 1);
			endX = Limit(endX, 0, size.X - 1);
			endY = Limit(endY, 0, size.Y - 1);

			for(int y = startY; y < endY + 1; y++)
				for(int x = startX; x < endX + 1; x++)
					cells[x, y].ID = cell;
		}

		#region Backend
		private class Cell
		{
			public byte Color;
			public int ID;
		}

		private readonly Vertex[]? vertices;

		private const int DEFAULT_WINDOW_WIDTH = 1280, DEFAULT_WINDOW_HEIGHT = 720;
		private string title;
		private Vector2i size;
		private readonly int tileSize;

		private readonly Cell[,] cells;
		private readonly Texture graphics;
		private readonly RenderWindow window;

		private Vertex[] UpdateGridVertices()
		{
			if(cells == null || vertices == null || window == null)
				return Array.Empty<Vertex>();

			var cellWidth = (float)window.Size.X / size.X;
			var cellHeight = (float)window.Size.Y / size.Y;

			for(int y = 0; y < size.Y; y++)
				for(int x = 0; x < size.X; x++)
				{
					var cell = cells[x, y];
					var color = ByteToColor(cell.Color);
					var texCoords = IndexToCoords(cell.ID, 27, 27);
					var tx = new Vector2f(texCoords.Item1 * tileSize, texCoords.Item2 * tileSize);
					var i = (y * size.X + x) * 4;

					vertices[i + 0] = new(new(x * cellWidth, y * cellHeight), color, tx);
					vertices[i + 1] = new(new((x + 1) * cellWidth, y * cellHeight), color, tx + new Vector2f(tileSize, 0));
					vertices[i + 2] = new(new((x + 1) * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(tileSize, tileSize));
					vertices[i + 3] = new(new(x * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(0, tileSize));
				}
			return vertices;
		}

		private static int Limit(int number, int rangeA, int rangeB)
		{
			if(number < rangeA)
				return rangeA;
			else if(number > rangeB)
				return rangeB;
			return number;
		}
		private static (int, int) IndexToCoords(int index, int width, int height)
		{
			index = index < 0 ? 0 : index;
			index = index > width * height - 1 ? width * height - 1 : index;

			return (index % width, index / width);
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
