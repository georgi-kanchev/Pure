namespace Pure.Window;

using System.Numerics;
using SFML.Graphics;
using SFML.System;

internal static class Vertices
{
	public static string graphicsPath = "default";
	public static int layer;
	public static (int, int) tileSize = (8, 8);
	public static (int, int) tileGap;

	public static (float, float) MapCellSize
	{
		get
		{
			Window.TryNoWindowException();

			var w = (float)Window.window.Size.X / mapCellCount.Item1;
			var h = (float)Window.window.Size.Y / mapCellCount.Item2;
			return (w, h);
		}
	}
	public static (uint, uint) mapCellCount = (48, 27);

	public static Shader? retroScreen;
	public static readonly SortedDictionary<(int, string), VertexArray> vertexQueue = new();

	public static void QueueRectangle((float, float) position, (float, float) size, uint tint)
	{
		TryInitQueue();

		var cellCount = mapCellCount;
		var (cellWidth, cellHeight) = MapCellSize;
		var (tileWidth, tileHeight) = tileSize;

		var (w, h) = size;
		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.Size.Item1);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.Size.Item2);
		var c = new Color(tint);
		var (gridX, gridY) = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
		var tl = new Vector2f(gridX, gridY);
		var br = new Vector2f(gridX + cellWidth * w, gridY + cellHeight * h);
		var verts = vertexQueue[(layer, graphicsPath)];

		verts.Append(new(new((int)tl.X, (int)tl.Y), c));
		verts.Append(new(new((int)br.X, (int)tl.Y), c));
		verts.Append(new(new((int)br.X, (int)br.Y), c));
		verts.Append(new(new((int)tl.X, (int)br.Y), c));
	}
	public static void QueueLine((float, float) a, (float, float) b, uint tint)
	{
		TryInitQueue();

		var (tileW, tileH) = tileSize;
		var (x0, y0) = a;
		var (x1, y1) = b;
		var dx = MathF.Abs(x1 - x0);
		var dy = -MathF.Abs(y1 - y0);
		var (stepX, stepY) = (1f / tileW * 0.999f, 1f / tileH * 0.999f);
		var sx = x0 < x1 ? stepX : -stepY;
		var sy = y0 < y1 ? stepX : -stepY;
		var err = dx + dy;
		var e2 = 0f;

		for (int i = 0; i < LINE_MAX_ITERATIONS; i++)
		{
			QueuePoint((x0, y0), tint);

			if (IsWithin(x0, x1, stepX) && IsWithin(y0, y1, stepY))
				break;

			e2 = 2f * err;

			if (e2 > dy)
			{
				err += dy;
				x0 += sx;
			}
			if (e2 < dx)
			{
				err += dx;
				y0 += sy;
			}
		}
	}
	public static void QueueTile((float, float) position, int tile, uint tint, sbyte angle, (int, int) size, (bool, bool) flips)
	{
		TryInitQueue();

		var (w, h) = size;
		w = w == 0 ? 1 : w;
		h = h == 0 ? 1 : h;

		var tiles = new int[Math.Abs(w), Math.Abs(h)];
		var gfxSize = Window.graphics[graphicsPath].Size;
		var (gapX, gapY) = tileGap;
		var (tileW, tileH) = tileSize;
		var tilesetTileCount = ((int)gfxSize.X / (tileW + gapX), (int)gfxSize.Y / (tileH + gapY));
		var (tileX, tileY) = IndexToCoords(tile, tilesetTileCount);
		var (x, y) = position;

		for (int j = 0; j < Math.Abs(h); j++)
			for (int i = 0; i < Math.Abs(w); i++)
				tiles[i, j] = (int)CoordsToIndex(tileX + i, tileY + j, tilesetTileCount.Item1);

		if (w < 0)
			FlipVertically(tiles);
		if (h < 0)
			FlipHorizontally(tiles);

		QueueSingleSprite((x, y), tile, tint, angle, (w, h));
	}
	public static void QueueTilemap((int tile, uint tint, sbyte angle, (bool isFlippedH, bool isFlippedV) flips)[,] tilemap)
	{
		if (Window.window == null)
			return;

		TryInitQueue();

		var (tilemapW, tilemapH) = (tilemap.GetLength(0), tilemap.GetLength(1));
		var cellWidth = (float)Window.window.Size.X / tilemapW;
		var cellHeight = (float)Window.window.Size.Y / tilemapH;
		var texture = Window.graphics[graphicsPath];
		var (tileGapW, tileGapH) = tileGap;
		var (tileW, tileH) = tileSize;
		var texSz = texture.Size;
		var tileCount = ((int)texSz.X / (tileW + tileGapW), (int)texSz.Y / (tileH + tileGapH));
		var key = (layer, graphicsPath);
		var cellCount = ((uint)tilemapW, (uint)tilemapH);

		mapCellCount = cellCount;

		for (int y = 0; y < tilemapH; y++)
			for (int x = 0; x < tilemapW; x++)
			{
				var cell = tilemap[x, y].tile;
				var tint = new Color(tilemap[x, y].tint);
				var i = CoordsToIndex(x, y, tilemapW) * 4;
				var tl = new Vector2f(x * cellWidth, y * cellHeight);
				var tr = new Vector2f((x + 1) * cellWidth, y * cellHeight);
				var br = new Vector2f((x + 1) * cellWidth, (y + 1) * cellHeight);
				var bl = new Vector2f(x * cellWidth, (y + 1) * cellHeight);

				var texCoords = IndexToCoords(cell, tileCount);
				var tx = new Vector2f(
					texCoords.Item1 * (tileW + tileGapW),
					texCoords.Item2 * (tileH + tileGapH));
				var texTr = new Vector2f((int)(tx.X + tileW), (int)tx.Y);
				var texBr = new Vector2f((int)(tx.X + tileW), (int)(tx.Y + tileH));
				var texBl = new Vector2f((int)tx.X, (int)(tx.Y + tileH));
				var center = Vector2.Lerp(new(tl.X, tl.Y), new(br.X, br.Y), 0.5f);
				var rotated = GetRotatedPoints(tilemap[x, y].angle, tl, tr, br, bl);
				var (flipX, flipY) = tilemap[x, y].flips;

				if (flipX)
				{
					(tx, texTr) = (texTr, tx);
					(texBl, texBr) = (texBr, texBl);
				}
				if (flipY)
				{
					(tx, texBl) = (texBl, tx);
					(texTr, texBr) = (texBr, texTr);
				}

				tl = rotated[0];
				tr = rotated[1];
				br = rotated[2];
				bl = rotated[3];

				tl = new((int)tl.X, (int)tl.Y);
				tr = new((int)tr.X, (int)tr.Y);
				br = new((int)br.X, (int)br.Y);
				bl = new((int)bl.X, (int)bl.Y);

				vertexQueue[key].Append(new(tl, tint, tx));
				vertexQueue[key].Append(new(tr, tint, texTr));
				vertexQueue[key].Append(new(br, tint, texBr));
				vertexQueue[key].Append(new(bl, tint, texBl));
			}

	}
	public static void QueuePoint((float x, float y) position, uint color)
	{
		if (Window.window == null)
			return;

		TryInitQueue();

		var verts = vertexQueue[(layer, graphicsPath)];
		var tileSz = tileSize;
		var cellWidth = MapCellSize.Item1 / tileSz.Item1;
		var cellHeight = MapCellSize.Item2 / tileSz.Item2;
		var cellCount = mapCellCount;

		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.window.Size.X);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.window.Size.Y);
		var c = new Color(color);
		var grid = ToGrid((x, y), (cellWidth, cellHeight));
		var tl = new Vector2f(grid.Item1, grid.Item2);
		var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

		verts.Append(new(new(tl.X, tl.Y), c));
		verts.Append(new(new(br.X, tl.Y), c));
		verts.Append(new(new(br.X, br.Y), c));
		verts.Append(new(new(tl.X, br.Y), c));
	}

	public static void TryInitQueue()
	{
		var key = (layer, graphicsPath);
		if (vertexQueue.ContainsKey(key) == false)
			vertexQueue[key] = new(PrimitiveType.Quads);
	}
	public static void DrawQueue()
	{
		if (Window.window == null)
			return;

		foreach (var kvp in Vertices.vertexQueue)
		{
			var tex = Window.graphics[kvp.Key.Item2];
			var shader = Window.IsRetro ? retroScreen : null;
			var rend = new RenderStates(BlendMode.Alpha, Transform.Identity, tex, shader);
			var randVec = new Vector2f(retroRand.Next(0, 10) / 10f, retroRand.Next(0, 10) / 10f);

			if (Window.IsRetro)
			{
				shader?.SetUniform("time", retroScreenTimer.ElapsedTime.AsSeconds());
				shader?.SetUniform("randomVec", randVec);
				shader?.SetUniform("viewSize", Window.window.GetView().Size);

				if (Window.isClosing && retroTurnoffTime != null)
				{
					var timing = retroTurnoffTime.ElapsedTime.AsSeconds() / RETRO_TURNOFF_TIME;
					shader?.SetUniform("turnoffAnimation", timing);
				}
			}
			Window.window.Draw(kvp.Value, rend);
		}
	}
	public static void ClearQueue()
	{
		foreach (var kvp in vertexQueue)
			kvp.Value.Clear();
	}

	public static void StartRetroAnimation()
	{
		retroTurnoffTime = new();
		retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
		retroTurnoff.Start();
		retroTurnoff.Elapsed += (s, e) => Window.window?.Close();
	}

	#region Backend
	private const int LINE_MAX_ITERATIONS = 10_000;
	private static Random retroRand = new();
	private static RenderStates Rend => Window.IsRetro ? new(retroScreen) : default;
	private static readonly SFML.System.Clock retroScreenTimer = new();
	private static System.Timers.Timer? retroTurnoff;
	private static Clock? retroTurnoffTime;
	private const float RETRO_TURNOFF_TIME = 0.5f;

	private static bool IsWithin(float number, float targetNumber, float range)
	{
		return IsBetween(number, targetNumber - range, targetNumber + range);
	}
	private static bool IsBetween(float number, float rangeA, float rangeB)
	{
		if (rangeA > rangeB)
			(rangeA, rangeB) = (rangeB, rangeA);

		var l = rangeA <= number;
		var u = rangeB >= number;
		return l && u;
	}
	private static (int, int) IndexToCoords(int index, (int, int) fieldSize)
	{
		index = index < 0 ? 0 : index;
		index = index > fieldSize.Item1 * fieldSize.Item2 - 1 ?
			(int)(fieldSize.Item1 * fieldSize.Item2 - 1) : index;

		return (index % fieldSize.Item1, index / fieldSize.Item1);
	}
	private static int CoordsToIndex(int x, int y, int width)
	{
		return y * width + x;
	}
	private static (float, float) ToGrid((float, float) pos, (float, float) gridSize)
	{
		if (gridSize == default)
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
	private static int Wrap(int number, int targetNumber)
	{
		return ((number % targetNumber) + targetNumber) % targetNumber;
	}
	private static void Shift<T>(IList<T> collection, int offset)
	{
		if (offset == default)
			return;

		if (offset < 0)
		{
			offset = Math.Abs(offset);
			for (int j = 0; j < offset; j++)
			{
				var temp = new T[collection.Count];
				for (int i = 0; i < collection.Count - 1; i++)
					temp[i] = collection[i + 1];
				temp[temp.Length - 1] = collection[0];

				for (int i = 0; i < temp.Length; i++)
					collection[i] = temp[i];
			}
			return;
		}

		offset = Math.Abs(offset);
		for (int j = 0; j < offset; j++)
		{
			var tempp = new T[collection.Count];
			for (int i = 1; i < collection.Count; i++)
				tempp[i] = collection[i - 1];
			tempp[0] = collection[tempp.Length - 1];

			for (int i = 0; i < tempp.Length; i++)
				collection[i] = tempp[i];
		}
	}
	private static Vector2f[] GetRotatedPoints(sbyte angle, params Vector2f[] points)
	{
		Shift(points, Wrap(-((int)angle), 4));
		return points;
	}
	private static T[,] Rotate<T>(T[,] matrix, int direction)
	{
		var dir = Wrap(Math.Abs(direction), 4);
		if (dir == 0)
			return matrix;

		var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
		var rotated = new T[n, m];

		if (direction > 0)
		{
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					rotated[i, j] = matrix[m - j - 1, i];

			direction--;
			return Rotate(rotated, direction);
		}

		for (int i = 0; i < n; i++)
			for (int j = 0; j < m; j++)
				rotated[i, j] = matrix[j, n - i - 1];

		direction++;
		return Rotate(rotated, direction);
	}
	private static void FlipHorizontally<T>(T[,] matrix)
	{
		var rows = matrix.GetLength(0);
		var cols = matrix.GetLength(1);

		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols / 2; j++)
			{
				T temp = matrix[i, j];
				matrix[i, j] = matrix[i, cols - j - 1];
				matrix[i, cols - j - 1] = temp;
			}
	}
	private static void FlipVertically<T>(T[,] matrix)
	{
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);

		for (int i = 0; i < rows / 2; i++)
			for (int j = 0; j < cols; j++)
			{
				T temp = matrix[i, j];
				matrix[i, j] = matrix[rows - i - 1, j];
				matrix[rows - i - 1, j] = temp;
			}
	}
	private static void QueueSingleSprite((float, float) position, int cell, uint tint, sbyte angle, (int, int) size)
	{
		if (Window.window == null)
			return;

		var verts = vertexQueue[(layer, graphicsPath)];
		var (cellWidth, cellHeight) = MapCellSize;
		var cellCount = mapCellCount;
		var (tileWidth, tileHeight) = tileSize;
		var texture = Window.graphics[graphicsPath];
		var (tileGapW, tileGapH) = tileGap;
		var tileCount = ((int)texture.Size.X / tileWidth, (int)texture.Size.Y / tileHeight);
		var (w, h) = size;
		w = Math.Abs(w);
		h = Math.Abs(h);

		var (texX, texY) = IndexToCoords(cell, tileCount);
		var tx = new Vector2f(
			(texX) * (tileWidth + tileGapW),
			(texY) * (tileHeight + tileGapH));
		var texTr = tx + new Vector2f(tileWidth * w, 0);
		var texBr = tx + new Vector2f(tileWidth * w, tileHeight * h);
		var texBl = tx + new Vector2f(0, tileHeight * h);

		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.window.Size.X);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.window.Size.Y);
		var c = new Color(tint);
		var grid = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
		var tl = new Vector2f((int)grid.Item1, (int)grid.Item2);
		var br = new Vector2f((int)(tl.X + cellWidth * w), (int)(tl.Y + cellHeight * h));

		if (angle == 1 || angle == 3)
			br = new((int)(tl.X + cellHeight * h), (int)(tl.Y + cellWidth * w));

		var tr = new Vector2f(br.X, tl.Y);
		var bl = new Vector2f(tl.X, br.Y);
		var rotated = GetRotatedPoints((sbyte)-angle, tx, texTr, texBr, texBl);
		tx = rotated[0];
		texTr = rotated[1];
		texBr = rotated[2];
		texBl = rotated[3];

		if (size.Item1 < 0)
		{
			(tx, texTr) = (texTr, tx);
			(texBl, texBr) = (texBr, texBl);
		}
		if (size.Item2 < 0)
		{
			(tx, texBl) = (texBl, tx);
			(texTr, texBr) = (texBr, texTr);
		}

		verts.Append(new(tl, c, tx));
		verts.Append(new(tr, c, texTr));
		verts.Append(new(br, c, texBr));
		verts.Append(new(bl, c, texBl));
	}
	#endregion
}
