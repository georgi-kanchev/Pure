namespace Pure.Window;

using System.Numerics;
using SFML.Graphics;
using SFML.System;

internal static class Vertices
{
	public static string? prevDrawTilesetGfxPath;
	public static (uint, uint) prevDrawTilesetTileSz;
	public static (uint, uint) prevDrawTilesetTileGap;
	public static (float, float) prevDrawTilemapCellSz;
	public static (uint, uint) prevDrawTilemapCellCount;

	public static void QueueRectangle((float, float) position, (float, float) size, uint tint)
	{
		if (Window.window == null || prevDrawTilesetGfxPath == null)
			return;

		var cellCount = prevDrawTilemapCellCount;
		var (cellWidth, cellHeight) = prevDrawTilemapCellSz;
		var (tileWidth, tileHeight) = prevDrawTilesetTileSz;

		var (w, h) = size;
		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.Size.Item1);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.Size.Item2);
		var c = new Color(tint);
		var (gridX, gridY) = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
		var tl = new Vector2f(gridX, gridY);
		var br = new Vector2f(gridX + cellWidth * w, gridY + cellHeight * h);
		var verts = vertexQueue[prevDrawTilesetGfxPath];

		verts.Append(new(new((int)tl.X, (int)tl.Y), c));
		verts.Append(new(new((int)br.X, (int)tl.Y), c));
		verts.Append(new(new((int)br.X, (int)br.Y), c));
		verts.Append(new(new((int)tl.X, (int)br.Y), c));
	}
	public static void QueueLine((float, float) a, (float, float) b, uint tint)
	{
		var (tileW, tileH) = prevDrawTilesetTileSz;
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
			QueuePoints(tint, new (float, float)[] { (x0, y0) });

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
	public static void QueueSprite((float, float) position, int cell, uint tint, sbyte angle, (uint, uint) size, (bool, bool) flip)
	{
		if (Window.window == null || prevDrawTilesetGfxPath == null)
			return;

		var verts = vertexQueue[prevDrawTilesetGfxPath];
		var (cellWidth, cellHeight) = prevDrawTilemapCellSz;
		var cellCount = prevDrawTilemapCellCount;
		var (tileWidth, tileHeight) = prevDrawTilesetTileSz;
		var texture = Window.graphics[prevDrawTilesetGfxPath];
		var (tileGapW, tileGapH) = prevDrawTilesetTileGap;
		var tileCount = (texture.Size.X / tileWidth, texture.Size.Y / tileHeight);
		var (texX, texY) = IndexToCoords(cell, tileCount);
		var tx = new Vector2f(texX * (tileWidth + tileGapW), texY * (tileHeight + tileGapH));
		var texTr = tx + new Vector2f(tileWidth, 0);
		var texBr = tx + new Vector2f(tileWidth, tileHeight);
		var texBl = tx + new Vector2f(0, tileHeight);
		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.window.Size.X);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.window.Size.Y);
		var c = new Color(tint);
		var grid = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
		var tl = new Vector2f((int)grid.Item1, (int)grid.Item2);
		var br = new Vector2f((int)(grid.Item1 + cellWidth), (int)(grid.Item2 + cellHeight));
		var tr = new Vector2f(br.X, tl.Y);
		var bl = new Vector2f(tl.X, br.Y);
		var rotated = GetRotatedPoints(angle, tl, tr, br, bl);
		var (flipX, flipY) = flip;

		size.Item1 = Math.Max(1, size.Item1);
		size.Item2 = Math.Max(1, size.Item2);

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

		verts.Append(new(tl, c, tx));
		verts.Append(new(tr, c, texTr));
		verts.Append(new(br, c, texBr));
		verts.Append(new(bl, c, texBl));
	}
	public static void QueueTilemap(int[,] tiles, uint[,] tints, sbyte[,] angles, (bool, bool)[,] flips, (uint, uint) tileSz, (uint, uint) tileOff, string path)
	{
		if (tiles == null || Window.window == null)
			return;

		var cellWidth = (float)Window.window.Size.X / tiles.GetLength(0);
		var cellHeight = (float)Window.window.Size.Y / tiles.GetLength(1);
		var texture = Window.graphics[path];
		var (tileOffW, tileOffH) = tileOff;
		var (tileW, tileH) = tileSz;
		var texSz = texture.Size;
		var tileCount = (texSz.X / (tileW + tileOffW), texSz.Y / (tileH + tileOffH));

		// this cache is used for a potential sprite draw
		prevDrawTilesetGfxPath = path;
		prevDrawTilemapCellSz = (cellWidth, cellHeight);
		prevDrawTilesetTileSz = tileSz;
		prevDrawTilesetTileGap = tileOff;
		prevDrawTilemapCellCount = ((uint)tiles.GetLength(0), (uint)tiles.GetLength(1));

		if (vertexQueue.ContainsKey(path) == false)
			vertexQueue[path] = new(PrimitiveType.Quads);

		for (uint y = 0; y < tiles.GetLength(1); y++)
			for (uint x = 0; x < tiles.GetLength(0); x++)
			{
				var cell = tiles[x, y];
				var tint = new Color(tints[x, y]);
				var i = GetIndex(x, y, (uint)tiles.GetLength(0)) * 4;
				var tl = new Vector2f(x * cellWidth, y * cellHeight);
				var tr = new Vector2f((x + 1) * cellWidth, y * cellHeight);
				var br = new Vector2f((x + 1) * cellWidth, (y + 1) * cellHeight);
				var bl = new Vector2f(x * cellWidth, (y + 1) * cellHeight);

				var texCoords = IndexToCoords(cell, tileCount);
				var tx = new Vector2f(
					texCoords.Item1 * (tileW + tileOffW),
					texCoords.Item2 * (tileH + tileOffH));
				var texTr = new Vector2f((int)(tx.X + tileW), (int)tx.Y);
				var texBr = new Vector2f((int)(tx.X + tileW), (int)(tx.Y + tileH));
				var texBl = new Vector2f((int)tx.X, (int)(tx.Y + tileH));
				var center = Vector2.Lerp(new(tl.X, tl.Y), new(br.X, br.Y), 0.5f);
				var rotated = GetRotatedPoints(angles[x, y], tl, tr, br, bl);
				var (flipX, flipY) = flips[x, y];

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

				vertexQueue[path].Append(new(tl, tint, tx));
				vertexQueue[path].Append(new(tr, tint, texTr));
				vertexQueue[path].Append(new(br, tint, texBr));
				vertexQueue[path].Append(new(bl, tint, texBl));
			}

	}
	public static void QueuePoints(uint tint, (float, float)[] positions)
	{
		if (Window.window == null || positions == null || positions.Length == 0 || prevDrawTilesetGfxPath == null)
			return;

		var verts = vertexQueue[prevDrawTilesetGfxPath];
		var tileSz = prevDrawTilesetTileSz;
		var cellWidth = prevDrawTilemapCellSz.Item1 / tileSz.Item1;
		var cellHeight = prevDrawTilemapCellSz.Item2 / tileSz.Item2;
		var cellCount = prevDrawTilemapCellCount;

		for (int i = 0; i < positions.Length; i++)
		{
			var x = Map(positions[i].Item1, 0, cellCount.Item1, 0, Window.window.Size.X);
			var y = Map(positions[i].Item2, 0, cellCount.Item2, 0, Window.window.Size.Y);
			var c = new Color(tint);
			var grid = ToGrid((x, y), (cellWidth, cellHeight));
			var tl = new Vector2f(grid.Item1, grid.Item2);
			var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

			verts.Append(new(new(tl.X, tl.Y), c));
			verts.Append(new(new(br.X, tl.Y), c));
			verts.Append(new(new(br.X, br.Y), c));
			verts.Append(new(new(tl.X, br.Y), c));
		}
	}

	public static VertexArray GetFromQueue(string texturePath)
	{
		return vertexQueue[texturePath];
	}
	public static void ClearQueue()
	{
		foreach (var kvp in vertexQueue)
			kvp.Value.Clear();
	}

	#region Backend
	private const int LINE_MAX_ITERATIONS = 10_000;
	private static readonly Dictionary<string, VertexArray> vertexQueue = new();

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
	internal static T[,] Rotate<T>(T[,] matrix, int direction)
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
	#endregion
}
