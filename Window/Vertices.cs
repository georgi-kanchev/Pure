namespace Pure.Window;

using SFML.Graphics;
using SFML.System;

internal static class Vertices
{
	public static Vertex[] GetRectangle((float, float) position, (float, float) size, uint tint)
	{
		if (prevDrawTilemapGfxPath == null)
			return Array.Empty<Vertex>();

		var verts = new Vertex[4];
		var cellCount = prevDrawTilemapCellCount;
		var (cellWidth, cellHeight) = prevDrawTilemapCellSz;
		var (tileWidth, tileHeight) = prevDrawTilemapTileSz;

		var (w, h) = size;
		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.window.Size.X);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.window.Size.Y);
		var c = new Color(tint);
		var (gridX, gridY) = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
		var tl = new Vector2f(gridX, gridY);
		var br = new Vector2f(gridX + cellWidth * w, gridY + cellHeight * h);

		verts[0] = new(new(tl.X, tl.Y), c);
		verts[1] = new(new(br.X, tl.Y), c);
		verts[2] = new(new(br.X, br.Y), c);
		verts[3] = new(new(tl.X, br.Y), c);
		return verts;
	}
	public static Vertex[] GetLine((float, float) a, (float, float) b, uint tint)
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

		for (int i = 0; i < LINE_MAX_ITERATIONS; i++)
		{
			points.Add((x0, y0));

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
		return GetPoints(tint, points.ToArray());
	}
	public static Vertex[] GetSprite((float, float) position, int cell, uint tint)
	{
		if (prevDrawTilemapGfxPath == null)
			return Array.Empty<Vertex>();

		var verts = new Vertex[4];
		var (cellWidth, cellHeight) = prevDrawTilemapCellSz;
		var cellCount = prevDrawTilemapCellCount;
		var (tileWidth, tileHeight) = prevDrawTilemapTileSz;
		var texture = Window.graphics[prevDrawTilemapGfxPath];

		var tileCount = (texture.Size.X / tileWidth, texture.Size.Y / tileHeight);
		var texCoords = IndexToCoords(cell, tileCount);
		var tx = new Vector2f(texCoords.Item1 * tileWidth, texCoords.Item2 * tileHeight);
		var x = Map(position.Item1, 0, cellCount.Item1, 0, Window.window.Size.X);
		var y = Map(position.Item2, 0, cellCount.Item2, 0, Window.window.Size.Y);
		var c = new Color(tint);
		var grid = ToGrid((x, y), (cellWidth / tileWidth, cellHeight / tileHeight));
		var tl = new Vector2f(grid.Item1, grid.Item2);
		var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

		verts[0] = new(new(tl.X, tl.Y), c, tx);
		verts[1] = new(new(br.X, tl.Y), c, tx + new Vector2f(tileWidth, 0));
		verts[2] = new(new(br.X, br.Y), c, tx + new Vector2f(tileWidth, tileHeight));
		verts[3] = new(new(tl.X, br.Y), c, tx + new Vector2f(0, tileHeight));
		return verts;
	}
	public static Vertex[] GetTilemap(int[,] tiles, uint[,] tints, (uint, uint) tileSz, (uint, uint) tileOff, string path)
	{
		if (tiles == null || Window.window == null)
			return Array.Empty<Vertex>();

		var cellWidth = (float)Window.window.Size.X / tiles.GetLength(0);
		var cellHeight = (float)Window.window.Size.Y / tiles.GetLength(1);
		var texture = Window.graphics[path];
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

		for (uint y = 0; y < tiles.GetLength(1); y++)
			for (uint x = 0; x < tiles.GetLength(0); x++)
			{
				var cell = tiles[x, y];
				var tint = new Color(tints[x, y]);
				var i = GetIndex(x, y, (uint)tiles.GetLength(0)) * 4;
				var tl = new Vector2f((int)(x * cellWidth), (int)(y * cellHeight));
				var tr = new Vector2f((int)((x + 1) * cellWidth), (int)(y * cellHeight));
				var br = new Vector2f((int)((x + 1) * cellWidth), (int)((y + 1) * cellHeight));
				var bl = new Vector2f((int)(x * cellWidth), (int)((y + 1) * cellHeight));

				var texCoords = IndexToCoords(cell, tileCount);
				var tx = new Vector2f(
					texCoords.Item1 * (tileW + tileOffW),
					texCoords.Item2 * (tileH + tileOffH));
				var texTr = new Vector2f((int)(tx.X + tileW), (int)tx.Y);
				var texBr = new Vector2f((int)(tx.X + tileW), (int)(tx.Y + tileH));
				var texBl = new Vector2f((int)tx.X, (int)(tx.Y + tileH));

				verts[i + 0] = new(tl, tint, tx);
				verts[i + 1] = new(tr, tint, texTr);
				verts[i + 2] = new(br, tint, texBr);
				verts[i + 3] = new(bl, tint, texBl);
			}
		return verts;
	}
	public static Vertex[] GetPoints(uint tint, (float, float)[] positions)
	{
		var verts = new Vertex[positions.Length * 4];
		var tileSz = prevDrawTilemapTileSz;
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

			var index = i * 4;
			verts[index + 0] = new(new(tl.X, tl.Y), c);
			verts[index + 1] = new(new(br.X, tl.Y), c);
			verts[index + 2] = new(new(br.X, br.Y), c);
			verts[index + 3] = new(new(tl.X, br.Y), c);
		}
		return verts;
	}

	#region Backend
	private const int LINE_MAX_ITERATIONS = 10000;

	public static string? prevDrawTilemapGfxPath;
	public static (uint, uint) prevDrawTilemapTileSz;
	public static (float, float) prevDrawTilemapCellSz;
	public static (uint, uint) prevDrawTilemapCellCount;

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
	#endregion
}
