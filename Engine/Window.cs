using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Engine
{
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
			get => width;
			set => width = Math.Max(value, 1);
		}
		public int Height
		{
			get => height;
			set => height = Math.Max(value, 1);
		}

		public Window(string graphicsPath)
		{
			width = 80;
			height = 45;
			title = "Purity";

			window = new(new VideoMode(1280, 720), title);
			window.Closed += (s, e) => window.Close();
			window.Resized += (s, e) =>
			{
				var view = window.GetView();
				view.Size = new(e.Width, e.Height);
				view.Center = new(e.Width / 2f, e.Height / 2f);
				window.SetView(view);
			};

			graphics = new(graphicsPath);
			tileSize = (int)graphics.Size.X / 26;
			vertices = new Vertex[width * height * 4];
			cells = new Cell[width, height];

			Fill(27, Color.White);
		}

		public void Update()
		{
			window?.DispatchEvents();
			window?.Clear();

			UpdateGridVertices();
			window?.Draw(vertices, PrimitiveType.Quads, new(graphics));

			window?.Display();
		}

		public void Fill(int cell, Color color)
		{
			if(cells == null)
				return;

			for(int y = 0; y < height; y++)
				for(int x = 0; x < width; x++)
					cells[x, y] = new() { ID = cell, Color = color };
		}
		public void Set(int x, int y, int cell, Color color)
		{
			if(cells == null)
				return;

			x = x.Limit(0, width - 1);
			y = y.Limit(0, height - 1);

			cells[x, y] = new() { ID = cell, Color = color };
		}
		public void Set(int cellIndex, int cell, Color color)
		{
			if(cells == null)
				return;

			var coords = cellIndex.ToCoords(width, height);
			cells[coords.X, coords.Y] = new() { ID = cell, Color = color };
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

			startX = startX.Limit(0, width - 1);
			startY = startY.Limit(0, height - 1);
			endX = endX.Limit(0, width - 1);
			endY = endY.Limit(0, height - 1);

			for(int y = startY; y < endY + 1; y++)
				for(int x = startX; x < endX + 1; x++)
					cells[x, y].ID = cell;
		}

		#region Backend
		private class Cell
		{
			public Color Color { get; set; }
			public int ID { get; set; }
		}

		private readonly Vertex[]? vertices;

		private string title;
		private int width, height, tileSize;

		private readonly Cell[,] cells;
		private readonly Texture graphics;
		private readonly RenderWindow window;

		private Vertex[] UpdateGridVertices()
		{
			if(cells == null || vertices == null || window == null)
				return Array.Empty<Vertex>();

			var cellWidth = (float)window.Size.X / width;
			var cellHeight = (float)window.Size.Y / height;

			for(int y = 0; y < height; y++)
				for(int x = 0; x < width; x++)
				{
					var cell = cells[x, y];
					var color = cell.Color.ToSFML();
					var texCoords = cell.ID.ToCoords(27, 27) * tileSize;
					var tx = new Vector2f(texCoords.X, texCoords.Y);
					var i = (y * width + x) * 4;

					vertices[i + 0] = new(new(x * cellWidth, y * cellHeight), color, tx);
					vertices[i + 1] = new(new((x + 1) * cellWidth, y * cellHeight), color, tx + new Vector2f(tileSize, 0));
					vertices[i + 2] = new(new((x + 1) * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(tileSize, tileSize));
					vertices[i + 3] = new(new(x * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(0, tileSize));
				}
			return vertices;
		}
		#endregion
	}
}
