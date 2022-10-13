using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Engine
{
	public static class Window
	{
		public static bool IsOpen => window != null && window.IsOpen;

		public static void Create(string title, int gridWidth, int gridHeight, string graphicsPath)
		{
			Window.gridWidth = gridWidth;
			Window.gridHeight = gridHeight;

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
			vertices = new Vertex[gridWidth * gridHeight * 4];
			cells = new Cell[gridWidth, gridHeight];
		}

		public static void Update()
		{
			window?.DispatchEvents();
			window?.Clear();

			UpdateGridVertices();
			window?.Draw(vertices, PrimitiveType.Quads, new(graphics));

			window?.Display();
		}

		public static void Fill(int cell, Color color)
		{
			if(cells == null)
				return;

			for(int y = 0; y < gridHeight; y++)
				for(int x = 0; x < gridWidth; x++)
					cells[x, y] = new() { ID = cell, Color = color };
		}
		public static void Set(int x, int y, int cell, Color color)
		{
			if(cells == null)
				return;

			x = x.Limit(0, gridWidth - 1);
			y = y.Limit(0, gridHeight - 1);

			cells[x, y] = new() { ID = cell, Color = color };
		}
		public static void Set(int cellIndex, int cell, Color color)
		{
			if(cells == null)
				return;

			var coords = cellIndex.ToCoords(gridWidth, gridHeight);
			cells[coords.X, coords.Y] = new() { ID = cell, Color = color };
		}
		public static void DisplayText(string text, int x, int y)
		{
			for(int i = 0; i < text.Length; i++)
			{
				var symbol = text[i];
			}


		}
		public static void SetSquare(int cell, int startX, int startY, int endX, int endY)
		{
			if(cells == null)
				return;

			startX = startX.Limit(0, gridWidth - 1);
			startY = startY.Limit(0, gridHeight - 1);
			endX = endX.Limit(0, gridWidth - 1);
			endY = endY.Limit(0, gridHeight - 1);

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

		private static Cell[,]? cells;
		private static Texture? graphics;
		private static int gridWidth, gridHeight;
		private static Vertex[]? vertices;
		private static RenderWindow? window;

		private static Vertex[] UpdateGridVertices()
		{
			if(cells == null || vertices == null || window == null)
				return Array.Empty<Vertex>();

			var cellWidth = (float)window.Size.X / gridWidth;
			var cellHeight = (float)window.Size.Y / gridHeight;

			for(int y = 0; y < gridHeight; y++)
				for(int x = 0; x < gridWidth; x++)
				{
					var cell = cells[x, y];
					var color = cell.Color.ToSFML();
					var texCoords = ((int)cell.ID).ToCoords(27, 27) * 4;
					var tx = new Vector2f(texCoords.X, texCoords.Y);
					var i = (y * gridWidth + x) * 4;

					vertices[i + 0] = new(new(x * cellWidth, y * cellHeight), color, tx);
					vertices[i + 1] = new(new((x + 1) * cellWidth, y * cellHeight), color, tx + new Vector2f(4, 0));
					vertices[i + 2] = new(new((x + 1) * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(4, 4));
					vertices[i + 3] = new(new(x * cellWidth, (y + 1) * cellHeight), color, tx + new Vector2f(0, 4));
				}
			return vertices;
		}
		#endregion
	}
}
