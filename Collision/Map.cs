using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Collision;

/// <summary>
/// Represents a map hitbox that contains a collection of rectangles that 
/// define the solid areas of a map.
/// </summary>
public class Map : Hitbox
{
	/// <summary>
	/// Gets the number of rectangles in the map hitbox, including the 
	/// rectangles that are part of the cells.
	/// </summary>
	public override int RectangleCount => rectangles.Count + cellRects.Count;

	/// <summary>
	/// Gets the rectangle at the specified <paramref name="index"/>, 
	/// including the rectangles that are part of the cells.
	/// </summary>
	/// <param name="index">The index of the rectangle to retrieve.</param>
	/// <returns>The rectangle at the specified <paramref name="index"/>.</returns>
	public override Rectangle this[int index]
	{
		get
		{
			var r = rectangles;
			return index < r.Count ? r[index] : cellRects[index - r.Count];
		}
	}

	/// <summary>
	/// Initializes a new map instance from the specified file <paramref name="path"/>.
	/// </summary>
	/// <param name="path">The path to the file that contains the map hitbox data.</param>
	public Map(string path) : base(path, (0, 0), 1)
	{
		var bytes = Decompress(File.ReadAllBytes(path));
		// count (4) | xs * 4, ys * 4, ws * 4, hs * 4
		var baseOffset = 4 + rectangles.Count * 4 * 4;
		var bSectorCount = new byte[4];

		Array.Copy(bytes, baseOffset, bSectorCount, 0, bSectorCount.Length);

		var sectorCount = BitConverter.ToInt32(bSectorCount);

		var off = baseOffset + bSectorCount.Length;
		for (int i = 0; i < sectorCount; i++)
		{
			var bTile = new byte[4];
			var bRectAmount = new byte[4];

			Array.Copy(bytes, off, bTile, 0, bTile.Length);
			Array.Copy(bytes, off + bTile.Length, bRectAmount, 0, bRectAmount.Length);

			off += bTile.Length + bRectAmount.Length;
			var tile = BitConverter.ToInt32(bTile);
			var rectAmount = BitConverter.ToInt32(bRectAmount);
			var bXs = new byte[rectAmount * 4];
			var bYs = new byte[rectAmount * 4];
			var bWs = new byte[rectAmount * 4];
			var bHs = new byte[rectAmount * 4];

			Array.Copy(bytes, off, bXs, 0, bXs.Length);
			Array.Copy(bytes, off + bXs.Length, bYs, 0, bYs.Length);
			Array.Copy(bytes, off + bXs.Length + bYs.Length, bWs, 0, bWs.Length);
			Array.Copy(bytes, off + bXs.Length + bYs.Length + bWs.Length, bHs, 0, bHs.Length);

			for (int j = 0; j < rectAmount; j++)
			{
				var bX = new byte[4];
				var bY = new byte[4];
				var bW = new byte[4];
				var bH = new byte[4];

				Array.Copy(bytes, off, bX, 0, bX.Length);
				Array.Copy(bytes, off + bX.Length, bY, 0, bY.Length);
				Array.Copy(bytes, off + bX.Length + bY.Length, bW, 0, bW.Length);
				Array.Copy(bytes, off + bX.Length + bW.Length, bH, 0, bH.Length);

				var x = BitConverter.ToInt32(bX);
				var y = BitConverter.ToInt32(bY);
				var w = BitConverter.ToSingle(bW);
				var h = BitConverter.ToSingle(bH);
				AddRectangle(new((w, h), (x, y)), tile);

				off += bX.Length + bY.Length + bW.Length + bH.Length;
			}
		}
	}
	/// <summary>
	/// Initializes a new map instance with an empty collection of rectangles.
	/// </summary>
	public Map() : base((0, 0), 1) { }

	/// <summary>
	/// Saves the current state of the map to a compressed binary 
	/// file at the given <paramref name="path"/>.
	/// Overrides the hitbox implementation to include additional data for each tile's cell rectangles.
	/// </summary>
	/// <param name="path">The path to save the file to.</param>
	public override void Save(string path)
	{
		base.Save(path);

		var baseSavedBytes = Decompress(File.ReadAllBytes(path));
		var sectorCount = cellRectsMap.Count;
		var bSectorCount = BitConverter.GetBytes(sectorCount);

		var result = new List<byte>();
		result.AddRange(baseSavedBytes);
		result.AddRange(bSectorCount);

		foreach (var kvp in cellRectsMap)
		{
			var rects = kvp.Value;
			var rectAmount = rects.Count;
			var bTile = BitConverter.GetBytes(kvp.Key);
			var bRectAmount = BitConverter.GetBytes(rectAmount);
			var bXs = new List<byte>();
			var bYs = new List<byte>();
			var bWs = new List<byte>();
			var bHs = new List<byte>();

			for (int i = 0; i < rects.Count; i++)
			{
				var r = rects[i];
				bXs.AddRange(BitConverter.GetBytes(r.Position.Item1));
				bYs.AddRange(BitConverter.GetBytes(r.Position.Item2));
				bWs.AddRange(BitConverter.GetBytes(r.Size.Item1));
				bHs.AddRange(BitConverter.GetBytes(r.Size.Item2));
			}
			result.AddRange(bTile);
			result.AddRange(bRectAmount);
			result.AddRange(bXs);
			result.AddRange(bYs);
			result.AddRange(bWs);
			result.AddRange(bHs);
		}

		File.WriteAllBytes(path, Compress(result.ToArray()));
	}

	/// <summary>
	/// Adds a <paramref name="rectangle"/> to the cell corresponding to the 
	/// specified <paramref name="tile"/>.
	/// </summary>
	/// <param name="rectangle">The rectangle to add.</param>
	/// <param name="tile">The tile corresponding to the cell to add the 
	/// <paramref name="rectangle"/> to.</param>
	public void AddRectangle(Rectangle rectangle, int tile)
	{
		if (cellRectsMap.ContainsKey(tile) == false)
			cellRectsMap[tile] = new List<Rectangle>();

		cellRectsMap[tile].Add(rectangle);
	}
	/// <summary>
	/// Gets an array of rectangles that intersect the specified <paramref name="cell"/>.
	/// </summary>
	/// <param name="cell">The (x, y) cell coordinates.</param>
	/// <returns>An array of rectangles that intersect the <paramref name="cell"/>.</returns>
	public Rectangle[] GetRectangles((int x, int y) cell)
	{
		if (tileIndices.ContainsKey(cell) == false)
			return Array.Empty<Rectangle>();

		var id = tileIndices[cell];
		var rects = cellRectsMap[id];
		var result = new List<Rectangle>();
		var (x, y) = cell;

		for (int r = 0; r < rects.Count; r++)
			result.Add(LocalToGlobalRectangle(rects[r], (x, y)));

		return result.ToArray();
	}

	/// <summary>
	/// Updates the map hitbox with new tile data.
	/// </summary>
	/// <param name="tiles">The new tile data.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="tiles"/> is null.</exception>
	public void Update(int[,] tiles)
	{
		if (tiles == null)
			throw new ArgumentNullException(nameof(tiles));

		cellRects.Clear();
		tileIndices.Clear();

		for (int y = 0; y < tiles.GetLength(1); y++)
			for (int x = 0; x < tiles.GetLength(0); x++)
			{
				var tile = tiles[x, y];
				if (cellRectsMap.ContainsKey(tile) == false)
					continue;

				var pos = (x, y);
				var localRects = cellRectsMap[tile];

				for (int i = 0; i < localRects.Count; i++)
					cellRects.Add(LocalToGlobalRectangle(localRects[i], pos));

				tileIndices[pos] = tile;
			}
	}

	/// <param name="line">
	/// The line to check.</param>
	/// <returns>True if the specified <paramref name="line"/> overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public override bool IsOverlapping(Line line)
	{
		return line.CrossPoints(this).Length > 0;
	}
	/// <param name="rectangle">
	/// The rectangle to check.</param>
	/// <returns>True if the specified <paramref name="rectangle"/> overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public override bool IsOverlapping(Rectangle rectangle)
	{
		var point = rectangle.Position;
		var chunkSize = GetChunkSizeForRect(rectangle);
		var neighborRects = GetNeighborRects(point, chunkSize);
		for (int i = 0; i < neighborRects.Count; i++)
			if (neighborRects[i].IsOverlapping(rectangle))
				return true;

		return false;
	}
	/// <param name="point">
	/// The point to check.</param>
	/// <returns>True if the specified <paramref name="point"/> overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public override bool IsOverlapping((float x, float y) point)
	{
		var neighborRects = GetNeighborRects(point, (1, 1));
		for (int i = 0; i < neighborRects.Count; i++)
			if (neighborRects[i].IsOverlapping(point))
				return true;

		return false;
	}

	/// <summary>
	/// Implicitly converts a map object to a rectangle array.
	/// </summary>
	/// <param name="map">The map object to convert.</param>
	public static implicit operator Rectangle[](Map map)
	{
		var result = new Rectangle[map.RectangleCount];
		Array.Copy(map.rectangles.ToArray(), 0, result, 0, map.rectangles.Count);
		Array.Copy(map.cellRects.ToArray(), 0, result, map.rectangles.Count, map.cellRects.Count);

		return map.rectangles.ToArray();
	}
	/// <summary>
	/// Implicitly converts a map object to an array of rectangle bundles.
	/// </summary>
	/// <param name="hitbox">The map object to convert.</param>
	public static implicit operator ((float x, float y) position, (float width, float height) size, uint color)[](Map map)
	{
		var result = new ((float x, float y) position, (float width, float height) size, uint color)[map.RectangleCount];
		for (int i = 0; i < result.Length; i++)
			result[i] = map[i];
		return result;
	}

	#region Backend
	// save format in sectors
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- amount of sectors
	// = = = = = = (sector 1)
	// [4]						- tile
	// [4]						- rect amount
	// [rect amount * 4]		- xs
	// [rect amount * 4]		- ys
	// [rect amount * 4]		- widths
	// [rect amount * 4]		- heights
	// = = = = = = (sector 2)
	// [4]						- tile
	// [4]						- rect amount
	// [rect amount * 4]		- xs
	// [rect amount * 4]		- ys
	// [rect amount * 4]		- widths
	// [rect amount * 4]		- heights
	// = = = = = = (sector 3)
	// ...

	private readonly List<Rectangle> cellRects = new(); // final result, recreated on each update
	private readonly Dictionary<int, List<Rectangle>> cellRectsMap = new(); // to not repeat the lists
	private readonly Dictionary<(int, int), int> tileIndices = new(); // for faster pick

	private Rectangle LocalToGlobalRectangle(Rectangle localRect, (float, float) tilePos)
	{
		var sc = Scale;
		var (x, y) = tilePos;
		var (rx, ry) = localRect.Position;
		var (rw, rh) = localRect.Size;
		var (offX, offY) = Position;

		rx *= sc;
		ry *= sc;
		rw *= sc;
		rh *= sc;

		rx += offX * sc;
		ry += offY * sc;

		return new((rw, rh), (x + rx, y + ry));
	}
	private Rectangle GlobalToLocal(Rectangle globalRect)
	{
		var (w, h) = globalRect.Size;
		var (x, y) = globalRect.Position;
		var sc = Scale;
		var (offX, offY) = Position;

		x -= offX * sc;
		y -= offY * sc;

		x /= sc;
		y /= sc;
		w /= sc;
		h /= sc;

		return new(((int)w, (int)h), ((int)x, (int)y));
	}
	internal List<Rectangle> GetNeighborRects((float, float) point, (int, int) chunkSize)
	{
		var result = new List<Rectangle>(cellRects);
		var localRect = GlobalToLocal(new(point, (0, 0)));
		var (x, y) = localRect.Position;
		var (w, h) = localRect.Size;
		var xStep = w < 0 ? -1 : 1;
		var yStep = h < 0 ? -1 : 1;
		var (chW, chH) = chunkSize;

		for (int j = (int)y - yStep * (chH - 1); j != y + h + yStep * chH; j += yStep)
			for (int i = (int)x - xStep * (chW - 1); i != x + w + xStep * chW; i += xStep)
			{
				var pos = (i, j);

				if (tileIndices.ContainsKey(pos) == false)
					continue;

				var id = tileIndices[(i, j)];
				var rects = cellRectsMap[id];
				for (int r = 0; r < rects.Count; r++)
				{
					var rect = LocalToGlobalRectangle(rects[r], (i, j));
					result.Add(rect);
				}
			}
		return result;
	}
	private (int, int) GetChunkSizeForRect(Rectangle globalRect)
	{
		var (w, h) = globalRect.Size;

		w /= Scale;
		h /= Scale;
		return ((int)w * 2, (int)h * 2);
	}

	private static byte[] ToBytes<T>(T[] array) where T : struct
	{
		var size = array.Length * Marshal.SizeOf(typeof(T));
		var buffer = new byte[size];
		Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
		return buffer;
	}
	private static void FromBytes<T>(T[] array, byte[] buffer) where T : struct
	{
		var size = array.Length;
		var len = Math.Min(size * Marshal.SizeOf(typeof(T)), buffer.Length);
		Buffer.BlockCopy(buffer, 0, array, 0, len);
	}

	private static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
			stream.Write(data, 0, data.Length);

		return output.ToArray();
	}
	private static byte[] Decompress(byte[] data)
	{
		var input = new MemoryStream(data);
		var output = new MemoryStream();
		using (var stream = new DeflateStream(input, CompressionMode.Decompress))
			stream.CopyTo(output);

		return output.ToArray();
	}

	#endregion
}
