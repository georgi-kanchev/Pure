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
		/// and <paramref name="tints"/>.
		/// </summary>
		public static void DrawTilemap(int[,] tiles, byte[,] tints, (uint, uint) tileSize,
			(uint, uint) tileMargin = default, string? path = default)
		{
			if(tiles == null || tints == null || tiles.Length != tints.Length)
				return;

			path ??= "default";

			TryLoadGraphics(path);
			var verts = GetTilemapVertices(tiles, tints, tileSize, tileMargin, path);
			window.Draw(verts, PrimitiveType.Quads, new(graphics[path]));
		}
		/// <summary>
		/// Draws a sprite onto the OS window. Its graphics are decided by a <paramref name="tile"/>
		/// from the last <see cref="DrawTilemap"/> call and a <paramref name="tint"/>. The sprite's
		/// <paramref name="position"/> is also relative to the previously drawn tilemap.
		/// </summary>
		public static void DrawSprite((float, float) position, int tile, byte tint = byte.MaxValue)
		{
			if(prevDrawTilemapGfxPath == null)
				return;

			var verts = GetSpriteVertices(position, tile, tint);
			window.Draw(verts, PrimitiveType.Quads, new(graphics[prevDrawTilemapGfxPath]));
		}
		/// <summary>
		/// Draws single pixel points with <paramref name="tint"/> onto the OS window.
		/// Their <paramref name="positions"/> are relative to the previously drawn tilemap.
		/// </summary>
		public static void DrawPoints(byte tint, params (float, float)[] positions)
		{
			if(positions == null || positions.Length == 0)
				return;

			var verts = GetPointsVertices(tint, positions);
			window.Draw(verts, PrimitiveType.Quads);
		}
		/// <summary>
		/// Draws a rectangle with <paramref name="tint"/> onto the OS window.
		/// Its <paramref name="position"/> and <paramref name="size"/> are relative
		/// to the previously drawn tilemap.
		/// </summary>
		public static void DrawRectangle((float, float) position, (float, float) size,
			byte tint = byte.MaxValue)
		{
			var verts = GetRectangleVertices(position, size, tint);
			window.Draw(verts, PrimitiveType.Quads);
		}
		/// <summary>
		/// Draws a line between <paramref name="pointA"/> and <paramref name="pointB"/> with
		/// <paramref name="tint"/> onto the OS window.
		/// Its points are relative to the previously drawn tilemap.
		/// </summary>
		public static void DrawLine((float, float) pointA, (float, float) pointB,
			byte tint = byte.MaxValue)
		{
			var verts = GetLineVertices(pointA, pointB, tint);
			window.Draw(verts, PrimitiveType.Quads);
		}

		#region Backend
		private const int LINE_MAX_ITERATIONS = 10000;

		private static bool isHidden;
		private static string title;
		internal static string? prevDrawTilemapGfxPath;
		internal static (uint, uint) prevDrawTilemapTileSz;
		internal static (float, float) prevDrawTilemapCellSz;
		internal static (uint, uint) prevDrawTilemapCellCount;

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
			var (tileOffW, tileOffH) = tileOff;
			var (tileW, tileH) = tileSz;
			var texSz = texture.Size;
			var tileCount = (texSz.X / (tileW + tileOffW), texSz.Y / (tileH + tileOffH));
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

					var texCoords = IndexToCoords(cell, tileCount);
					var tx = new Vector2f(
						texCoords.Item1 * (tileW + tileOffW),
						texCoords.Item2 * (tileH + tileOffH));
					var texTr = new Vector2f(tx.X + tileW, tx.Y);
					var texBr = new Vector2f(tx.X + tileW, tx.Y + tileH);
					var texBl = new Vector2f(tx.X, tx.Y + tileH);

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
			var r = (byte)((color >> 5) * 255 / 7);
			var g = (byte)(((color >> 2) & 0x07) * 255 / 7);
			var b = (byte)((color & 0x03) * 255 / 3);
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
