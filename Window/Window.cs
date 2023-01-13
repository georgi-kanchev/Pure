using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Pure.Window
{
	/// <summary>
	/// Provides a simple way to create and interact with an OS window.
	/// </summary>
	public static class Window
	{
		/// <summary>
		/// Whether the OS window exists. This is <see langword="true"/> even when it
		/// is minimized or <see cref="IsHidden"/>.
		/// </summary>
		public static bool IsExisting
		{
			get => window != null && window.IsOpen;
			set
			{
				if(value == false)
					window.Close();
			}
		}
		/// <summary>
		/// The title on the title bar of the OS window.
		/// </summary>
		public static string Title
		{
			get => title;
			set { title = value; window.SetTitle(title); }
		}
		/// <summary>
		/// The size of the OS window.
		/// </summary>
		public static (uint, uint) Size
		{
			get => (window.Size.X, window.Size.Y);
			set => window.Size = new(value.Item1, value.Item2);
		}
		/// <summary>
		/// Whether the OS window acts as a background process.
		/// </summary>
		public static bool IsHidden
		{
			get => isHidden;
			set { isHidden = value; window.SetVisible(value == false); }
		}
		/// <summary>
		/// Returns whether the OS window is currently focused.
		/// </summary>
		public static bool IsFocused => window.HasFocus();

		/// <summary>
		/// Determines whether the <see cref="Window"/> <paramref name="isActive"/>.
		/// An application loop should ideally activate it at the very start and
		/// deactivate it at the very end.
		/// </summary>
		public static void Activate(bool isActive)
		{
			if(isActive)
			{
				window.DispatchEvents();
				window.Clear();
				window.SetActive();
				return;
			}

			MouseButton.Update();
			MouseCursor.TryDrawCursor();
			window.Display();
		}

		/// <summary>
		/// Draws a tilemap onto the OS window. Its graphics image is loaded from a
		/// <paramref name="path"/> (default graphics if <see langword="null"/>) using a
		/// <paramref name="tileSize"/> and a <paramref name="tileMargin"/>, then it is cached
		/// for future draws. The tilemap's contents are decided by <paramref name="tiles"/>
		/// and <paramref name="colors"/>.
		/// </summary>
		public static void DrawTilemap(int[,] tiles, byte[,] colors, (uint, uint) tileSize,
			(uint, uint) tileMargin = default, string? path = default)
		{
			if(tiles == null || colors == null || tiles.Length != colors.Length)
				return;

			path ??= "default";

			TryLoadGraphics(path);
			var verts = GetTilemapVertices(tiles, colors, tileSize, tileMargin, path);
			window.Draw(verts, PrimitiveType.Quads, new(graphics[path]));
		}
		/// <summary>
		/// Draws a sprite onto the OS window. Its graphics are decided by a <paramref name="tile"/>
		/// from the last <see cref="DrawTilemap"/> call and a <paramref name="color"/>. The sprite's
		/// <paramref name="position"/> is also relative to the previously drawn tilemap.
		/// </summary>
		public static void DrawSprite((float, float) position, int tile, byte color)
		{
			if(prevDrawTilemapGfxPath == null)
				return;

			var verts = GetSpriteVertices(position, tile, color);
			window.Draw(verts, PrimitiveType.Quads, new(graphics[prevDrawTilemapGfxPath]));
		}
		/// <summary>
		/// Draws single pixel points with <paramref name="color"/> onto the OS window.
		/// Their <paramref name="positions"/> are relative to the previously drawn tilemap.
		/// </summary>
		public static void DrawPoints(byte color, params (float, float)[] positions)
		{
			if(positions == null || positions.Length == 0)
				return;

			var verts = GetPointsVertices(color, positions);
			window.Draw(verts, PrimitiveType.Quads);
		}
		/// <summary>
		/// Draws a rectangle with <paramref name="color"/> onto the OS window.
		/// Its <paramref name="position"/> and <paramref name="size"/> are relative
		/// to the previously drawn tilemap.
		/// </summary>
		public static void DrawRectangle((float, float) position, (float, float) size, byte color)
		{
			var verts = GetRectangleVertices(position, size, color);
			window.Draw(verts, PrimitiveType.Quads);
		}
		/// <summary>
		/// Draws a line between <paramref name="pointA"/> and <paramref name="pointB"/> with
		/// <paramref name="color"/> onto the OS window.
		/// Its points are relative to the previously drawn tilemap.
		/// </summary>
		public static void DrawLine((float, float) pointA, (float, float) pointB, byte color)
		{
			var verts = GetLineVertices(pointA, pointB, color);
			window.Draw(verts, PrimitiveType.Quads);
		}

		#region Backend
		private const int LINE_MAX_ITERATIONS = 10000;

		private static bool isHidden;
		private static string title;
		private static string? prevDrawTilemapGfxPath;
		private static (uint, uint) prevDrawTilemapTileSz;
		private static (float, float) prevDrawTilemapCellSz;
		private static (uint, uint) prevDrawTilemapCellCount;

		private static readonly List<(byte, byte, byte)> colorLookup = new()
		{
			(0,0,0),(0,0,85),(0,0,170),(0,0,255),(0,36,0),(0,36,85),(0,36,170),(0,36,255),
			(0,72,0),(0,72,85),(0,72,170),(0,72,255),(0,109,0),(0,109,85),(0,109,170),
			(0,109,255),(0,145,0),(0,145,85),(0,145,170),(0,145,255),(0,182,0),(0,182,85),
			(0,182,170),(0,182,255),(0,218,0),(0,218,85),(0,218,170),(0,218,255),(0,255,0),
			(0,255,85),(0,255,170),(0,255,255),(36,0,0),(36,0,85),(36,0,170),(36,0,255),
			(36,36,0),(36,36,85),(36,36,170),(36,36,255),(36,72,0),(36,72,85),(36,72,170),
			(36,72,255),(36,109,0),(36,109,85),(36,109,170),(36,109,255),(36,145,0),(36,145,85),
			(36,145,170),(36,145,255),(36,182,0),(36,182,85),(36,182,170),(36,182,255),
			(36,218,0),(36,218,85),(36,218,170),(36,218,255),(36,255,0),(36,255,85),(36,255,170),
			(36,255,255),(72,0,0),(72,0,85),(72,0,170),(72,0,255),(72,36,0),(72,36,85),
			(72,36,170),(72,36,255),(72,72,0),(72,72,85),(72,72,170),(72,72,255),(72,109,0),
			(72,109,85),(72,109,170),(72,109,255),(72,145,0),(72,145,85),(72,145,170),
			(72,145,255),(72,182,0),(72,182,85),(72,182,170),(72,182,255),(72,218,0),(72,218,85),
			(72,218,170),(72,218,255),(72,255,0),(72,255,85),(72,255,170),(72,255,255),(109,0,0),
			(109,0,85),(109,0,170),(109,0,255),(109,36,0),(109,36,85),(109,36,170),(109,36,255),
			(109,72,0),(109,72,85),(109,72,170),(109,72,255),(109,109,0),(109,109,85),
			(109,109,170),(109,109,255),(109,145,0),(109,145,85),(109,145,170),(109,145,255),
			(109,182,0),(109,182,85),(109,182,170),(109,182,255),(109,218,0),(109,218,85),
			(109,218,170),(109,218,255),(109,255,0),(109,255,85),(109,255,170),(109,255,255),
			(145,0,0),(145,0,85),(145,0,170),(145,0,255),(145,36,0),(145,36,85),(145,36,170),
			(145,36,255),(145,72,0),(145,72,85),(145,72,170),(145,72,255),(145,109,0),
			(145,109,85),(145,109,170),(145,109,255),(145,145,0),(145,145,85),(145,145,170),
			(145,145,255),(145,182,0),(145,182,85),(145,182,170),(145,182,255),(145,218,0),
			(145,218,85),(145,218,170),(145,218,255),(145,255,0),(145,255,85),(145,255,170),
			(145,255,255),(182,0,0),(182,0,85),(182,0,170),(182,0,255),(182,36,0),(182,36,85),
			(182,36,170),(182,36,255),(182,72,0),(182,72,85),(182,72,170),(182,72,255),
			(182,109,0),(182,109,85),(182,109,170),(182,109,255),(182,145,0),(182,145,85),
			(182,145,170),(182,145,255),(182,182,0),(182,182,85),(182,182,170),(182,182,255),
			(182,218,0),(182,218,85),(182,218,170),(182,218,255),(182,255,0),(182,255,85),
			(182,255,170),(182,255,255),(218,0,0),(218,0,85),(218,0,170),(218,0,255),(218,36,0),
			(218,36,85),(218,36,170),(218,36,255),(218,72,0),(218,72,85),(218,72,170),
			(218,72,255),(218,109,0),(218,109,85),(218,109,170),(218,109,255),(218,145,0),
			(218,145,85),(218,145,170),(218,145,255),(218,182,0),(218,182,85),(218,182,170),
			(218,182,255),(218,218,0),(218,218,85),(218,218,170),(218,218,255),(218,255,0),
			(218,255,85),(218,255,170),(218,255,255),(255,0,0),(255,0,85),(255,0,170),
			(255,0,255),(255,36,0),(255,36,85),(255,36,170),(255,36,255),(255,72,0),(255,72,85),
			(255,72,170),(255,72,255),(255,109,0),(255,109,85),(255,109,170),(255,109,255),
			(255,145,0),(255,145,85),(255,145,170),(255,145,255),(255,182,0),(255,182,85),
			(255,182,170),(255,182,255),(255,218,0),(255,218,85),(255,218,170),(255,218,255),
			(255,255,0),(255,255,85),(255,255,170),(255,255,255),
		};
		internal static readonly Dictionary<string, Texture> graphics = new();
		internal static readonly RenderWindow window;

		static Window()
		{
			//var str = DefaultGraphics.PNGToBase64String("graphics.png");

			graphics["default"] = DefaultGraphics.CreateTexture();

			var desktopW = VideoMode.DesktopMode.Width;
			var desktopH = VideoMode.DesktopMode.Height;
			title = "";

			var width = (uint)RoundToMultipleOfTwo((int)(desktopW * 0.6f));
			var height = (uint)RoundToMultipleOfTwo((int)(desktopH * 0.6f));

			window = new(new VideoMode(width, height), title);
			window.Closed += (s, e) => window.Close();
			window.Resized += (s, e) => UpdateWindowAndView();
			window.LostFocus += (s, e) =>
			{
				MouseButton.CancelInput();
				KeyboardKey.CancelInput();
			};

			window.DispatchEvents();
			window.Clear();
			window.Display();
			UpdateWindowAndView();
		}

		private static void TryLoadGraphics(string path)
		{
			if(graphics.ContainsKey(path))
				return;

			graphics[path] = new(path);
		}
		private static void UpdateWindowAndView()
		{
			var view = window.GetView();
			var (w, h) = (RoundToMultipleOfTwo((int)Size.Item1), RoundToMultipleOfTwo((int)Size.Item2));
			view.Size = new(w, h);
			view.Center = new(RoundToMultipleOfTwo((int)(Size.Item1 / 2f)), RoundToMultipleOfTwo((int)(Size.Item2 / 2f)));
			window.SetView(view);
			window.Size = new((uint)w, (uint)h);
		}

		private static Vertex[] GetRectangleVertices((float, float) position, (float, float) size, byte color)
		{
			if(prevDrawTilemapGfxPath == null)
				return Array.Empty<Vertex>();

			var verts = new Vertex[4];
			var cellCount = prevDrawTilemapCellCount;
			var (cellWidth, cellHeight) = prevDrawTilemapCellSz;
			var (tileWidth, tileHeight) = prevDrawTilemapTileSz;

			var (w, h) = size;
			var x = Map(position.Item1, 0, cellCount.Item1, 0, window.Size.X);
			var y = Map(position.Item2, 0, cellCount.Item2, 0, window.Size.Y);
			var c = ByteToColor(color);
			var (gridX, gridY) = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
			var tl = new Vector2f(gridX, gridY);
			var br = new Vector2f(gridX + cellWidth * w, gridY + cellHeight * h);

			verts[0] = new(new(tl.X, tl.Y), c);
			verts[1] = new(new(br.X, tl.Y), c);
			verts[2] = new(new(br.X, br.Y), c);
			verts[3] = new(new(tl.X, br.Y), c);
			return verts;
		}
		private static Vertex[] GetLineVertices((float, float) a, (float, float) b, byte color)
		{
			var (tileW, tileH) = prevDrawTilemapTileSz;
			var (x0, y0) = a;
			var (x1, y1) = b;
			var dx = MathF.Abs(x1 - x0);
			var dy = -MathF.Abs(y1 - y0);
			var (stepX, stepY) = (1f / tileW * 0.999f, 1f / tileH * 0.999f);
			var sx = x0 < x1 ? stepX : -stepY;
			var sy = y0 < y1 ? stepX : -stepY;
			var err = dx + dy;
			var points = new List<(float, float)>();
			float e2;

			for(int i = 0; i < LINE_MAX_ITERATIONS; i++)
			{
				points.Add((x0, y0));

				if(IsWithin(x0, x1, stepX) && IsWithin(y0, y1, stepY))
					break;

				e2 = 2f * err;

				if(e2 > dy)
				{
					err += dy;
					x0 += sx;
				}
				if(e2 < dx)
				{
					err += dx;
					y0 += sy;
				}
			}
			return GetPointsVertices(color, points.ToArray());
		}
		private static Vertex[] GetSpriteVertices((float, float) position, int cell, byte color)
		{
			if(prevDrawTilemapGfxPath == null)
				return Array.Empty<Vertex>();

			var verts = new Vertex[4];
			var (cellWidth, cellHeight) = prevDrawTilemapCellSz;
			var cellCount = prevDrawTilemapCellCount;
			var (tileWidth, tileHeight) = prevDrawTilemapTileSz;
			var texture = graphics[prevDrawTilemapGfxPath];

			var tileCount = (texture.Size.X / tileWidth, texture.Size.Y / tileHeight);
			var texCoords = IndexToCoords(cell, tileCount);
			var tx = new Vector2f(texCoords.Item1 * tileWidth, texCoords.Item2 * tileHeight);
			var x = Map(position.Item1, 0, cellCount.Item1, 0, window.Size.X);
			var y = Map(position.Item2, 0, cellCount.Item2, 0, window.Size.Y);
			var c = ByteToColor(color);
			var grid = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
			var tl = new Vector2f(grid.Item1, grid.Item2);
			var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

			verts[0] = new(new(tl.X, tl.Y), c, tx);
			verts[1] = new(new(br.X, tl.Y), c, tx + new Vector2f(tileWidth, 0));
			verts[2] = new(new(br.X, br.Y), c, tx + new Vector2f(tileWidth, tileHeight));
			verts[3] = new(new(tl.X, br.Y), c, tx + new Vector2f(0, tileHeight));
			return verts;
		}
		private static Vertex[] GetTilemapVertices(int[,] tiles, byte[,] colors,
			(uint, uint) tileSz, (uint, uint) tileOff, string path)
		{
			if(tiles == null || window == null)
				return Array.Empty<Vertex>();

			var cellWidth = (float)window.Size.X / tiles.GetLength(0);
			var cellHeight = (float)window.Size.Y / tiles.GetLength(1);
			var texture = graphics[path];
			var tileCount = (texture.Size.X / tileSz.Item1, texture.Size.Y / tileSz.Item2);
			var verts = new Vertex[tiles.Length * 4];

			// this cache is used for a potential sprite draw
			prevDrawTilemapGfxPath = path;
			prevDrawTilemapCellSz = (cellWidth, cellHeight);
			prevDrawTilemapTileSz = tileSz;
			prevDrawTilemapCellCount = ((uint)tiles.GetLength(0), (uint)tiles.GetLength(1));

			for(uint y = 0; y < tiles.GetLength(1); y++)
				for(uint x = 0; x < tiles.GetLength(0); x++)
				{
					var cell = tiles[x, y];
					var color = ByteToColor(colors[x, y]);
					var i = GetIndex(x, y, (uint)tiles.GetLength(0)) * 4;
					var tl = new Vector2f(x * cellWidth, y * cellHeight);
					var tr = new Vector2f((x + 1) * cellWidth, y * cellHeight);
					var br = new Vector2f((x + 1) * cellWidth, (y + 1) * cellHeight);
					var bl = new Vector2f(x * cellWidth, (y + 1) * cellHeight);

					var w = tileSz.Item1;
					var h = tileSz.Item2;
					var texCoords = IndexToCoords(cell, tileCount);
					var tx = new Vector2f(
						texCoords.Item1 * (w + tileOff.Item1),
						texCoords.Item2 * (h + tileOff.Item2));
					var texTr = new Vector2f(tx.X + w, tx.Y);
					var texBr = new Vector2f(tx.X + w, tx.Y + h);
					var texBl = new Vector2f(tx.X, tx.Y + h);

					verts[i + 0] = new(tl, color, tx);
					verts[i + 1] = new(tr, color, texTr);
					verts[i + 2] = new(br, color, texBr);
					verts[i + 3] = new(bl, color, texBl);
				}
			return verts;
		}
		private static Vertex[] GetPointsVertices(byte color, (float, float)[] positions)
		{
			var verts = new Vertex[positions.Length * 4];
			var tileSz = prevDrawTilemapTileSz;
			var cellWidth = prevDrawTilemapCellSz.Item1 / tileSz.Item1;
			var cellHeight = prevDrawTilemapCellSz.Item2 / tileSz.Item2;
			var cellCount = prevDrawTilemapCellCount;

			for(int i = 0; i < positions.Length; i++)
			{
				var x = Map(positions[i].Item1, 0, cellCount.Item1, 0, window.Size.X);
				var y = Map(positions[i].Item2, 0, cellCount.Item2, 0, window.Size.Y);
				var c = ByteToColor(color);
				var grid = ToGrid((x, y), (cellWidth, cellHeight));
				var tl = new Vector2f(grid.Item1, grid.Item2);
				var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

				var index = i * 4;
				verts[index + 0] = new(new(tl.X, tl.Y), c);
				verts[index + 1] = new(new(br.X, tl.Y), c);
				verts[index + 2] = new(new(br.X, br.Y), c);
				verts[index + 3] = new(new(tl.X, br.Y), c);
			}
			return verts;
		}

		private static (int, int) IndexToCoords(int index, (uint, uint) fieldSize)
		{
			index = index < 0 ? 0 : index;
			index = index > fieldSize.Item1 * fieldSize.Item2 - 1 ?
				(int)(fieldSize.Item1 * fieldSize.Item2 - 1) : index;

			return (index % (int)fieldSize.Item1, index / (int)fieldSize.Item1);
		}
		private static uint GetIndex(uint x, uint y, uint width)
		{
			return y * width + x;
		}
		private static Color ByteToColor(byte color)
		{
			var (r, g, b) = colorLookup[color];
			return new(r, g, b);
		}
		private static (float, float) ToGrid((float, float) pos, (float, float) gridSize)
		{
			if(gridSize == default)
				return pos;

			var X = pos.Item1;
			var Y = pos.Item2;

			// this prevents -0 cells
			var x = X - (X < 0 ? gridSize.Item1 : 0);
			var y = Y - (Y < 0 ? gridSize.Item2 : 0);

			x -= X % gridSize.Item1;
			y -= Y % gridSize.Item2;
			return new(x, y);
		}
		private static float Map(float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		private static bool IsBetween(float number, float rangeA, float rangeB)
		{
			if(rangeA > rangeB)
				(rangeA, rangeB) = (rangeB, rangeA);

			var l = rangeA <= number;
			var u = rangeB >= number;
			return l && u;
		}
		private static bool IsWithin(float number, float targetNumber, float range)
		{
			return IsBetween(number, targetNumber - range, targetNumber + range);
		}
		private static int RoundToMultipleOfTwo(int n)
		{
			var rem = n % 2;
			var result = n - rem;
			if(rem >= 1)
				result += 2;
			return result;
		}
		internal static (float, float) PositionFrom((int, int) screenPixel)
		{
			var x = Map(screenPixel.Item1, 0, Size.Item1, 0, prevDrawTilemapCellCount.Item1);
			var y = Map(screenPixel.Item2, 0, Size.Item2, 0, prevDrawTilemapCellCount.Item2);

			return (x, y);
		}
		#endregion
	}
}
