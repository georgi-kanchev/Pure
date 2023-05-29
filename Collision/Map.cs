using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Collision;

/// <summary>
/// Represents a map hitbox that contains a collection of rectangles that 
/// define the solid areas of a map.
/// </summary>
public class Map
{
	/// <summary>
	/// Gets the total number of rectangles at all cells.
	/// </summary>
	public int RectangleCount => count;

	/// <summary>
	/// Initializes a new empty map instance.
	/// </summary>
	public Map() { }
	/// <summary>
	/// Initializes a new map instance from the specified file path.
	/// </summary>
	/// <param name="path">The path to the file that contains the map hitbox data.</param>
	public Map(string path)
	{
		//var bytes = Decompress(File.ReadAllBytes(path));
		//// count (4) | xs * 4, ys * 4, ws * 4, hs * 4
		//var baseOffset = 4 + rectangles.Count * 4 * 4;
		//var bSectorCount = new byte[4];

		//Array.Copy(bytes, baseOffset, bSectorCount, 0, bSectorCount.Length);

		//var sectorCount = BitConverter.ToInt32(bSectorCount);

		//var off = baseOffset + bSectorCount.Length;
		//for (int i = 0; i < sectorCount; i++)
		//{
		//	var bTile = new byte[4];
		//	var bRectAmount = new byte[4];

		//	Array.Copy(bytes, off, bTile, 0, bTile.Length);
		//	Array.Copy(bytes, off + bTile.Length, bRectAmount, 0, bRectAmount.Length);

		//	off += bTile.Length + bRectAmount.Length;
		//	var tile = BitConverter.ToInt32(bTile);
		//	var rectAmount = BitConverter.ToInt32(bRectAmount);
		//	var bXs = new byte[rectAmount * 4];
		//	var bYs = new byte[rectAmount * 4];
		//	var bWs = new byte[rectAmount * 4];
		//	var bHs = new byte[rectAmount * 4];

		//	Array.Copy(bytes, off, bXs, 0, bXs.Length);
		//	Array.Copy(bytes, off + bXs.Length, bYs, 0, bYs.Length);
		//	Array.Copy(bytes, off + bXs.Length + bYs.Length, bWs, 0, bWs.Length);
		//	Array.Copy(bytes, off + bXs.Length + bYs.Length + bWs.Length, bHs, 0, bHs.Length);

		//	for (int j = 0; j < rectAmount; j++)
		//{
		//	var bX = new byte[4];
		//	var bY = new byte[4];
		//	var bW = new byte[4];
		//	var bH = new byte[4];

		//	Array.Copy(bytes, off, bX, 0, bX.Length);
		//	Array.Copy(bytes, off + bX.Length, bY, 0, bY.Length);
		//	Array.Copy(bytes, off + bX.Length + bY.Length, bW, 0, bW.Length);
		//	Array.Copy(bytes, off + bX.Length + bW.Length, bH, 0, bH.Length);

		//	var x = BitConverter.ToInt32(bX);
		//	var y = BitConverter.ToInt32(bY);
		//	var w = BitConverter.ToSingle(bW);
		//	var h = BitConverter.ToSingle(bH);
		//	AddRectangle(new((w, h), (x, y)), tile);

		//	off += bX.Length + bY.Length + bW.Length + bH.Length;
		//}
		//}
	}

	/// <summary>
	/// Saves the current state of the map to a compressed binary file at the given path.
	/// </summary>
	/// <param name="path">The path to save the file to.</param>
	public void Save(string path)
	{
		var baseSavedBytes = Decompress(File.ReadAllBytes(path));
		var sectorCount = cellRects.Count;
		var bSectorCount = BitConverter.GetBytes(sectorCount);

		var result = new List<byte>();
		result.AddRange(baseSavedBytes);
		result.AddRange(bSectorCount);

		foreach (var kvp in cellRects)
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
	/// Adds a rectangle to the cell corresponding to the 
	/// specified tile identifier.
	/// </summary>
	/// <param name="rectangle">The rectangle to add.</param>
	/// <param name="tileID">The tile identifier corresponding to the cell to add the 
	/// rectangle to.</param>
	public void AddRectangle(Rectangle rectangle, int tileID)
	{
		if (cellRects.ContainsKey(tileID) == false)
			cellRects[tileID] = new List<Rectangle>();

		cellRects[tileID].Add(rectangle);
		count++;
	}
	/// <summary>
	/// Gets an array of rectangles at the specified cell.
	/// </summary>
	/// <param name="cell">The (x, y) cell coordinates.</param>
	/// <returns>An array of rectangles at the cell.</returns>
	public Rectangle[] GetRectangles((int x, int y) cell)
	{
		if (tileIndices.ContainsKey(cell) == false)
			return Array.Empty<Rectangle>();

		var id = tileIndices[cell];
		var rects = cellRects[id];
		var result = new List<Rectangle>();
		var (x, y) = cell;

		for (int r = 0; r < rects.Count; r++)
			result.Add(rects[r]);

		return result.ToArray();
	}
	public Rectangle[] GetRectangles(int tile)
	{
		if (cellRects.ContainsKey(tile) == false)
			return Array.Empty<Rectangle>();

		return cellRects[tile].ToArray();
	}
	public Rectangle[] GetRectangles()
	{
		var result = new List<Rectangle>();
		foreach (var kvp in tileIndices)
			result.AddRange(cellRects[kvp.Value]);

		return result.ToArray();
	}

	/// <summary>
	/// Updates the map hitbox with new tile identifiers data.
	/// </summary>
	/// <param name="tileIDs">The new tile identifiers data.</param>
	/// <exception cref="ArgumentNullException">Thrown if tiles is null.</exception>
	public void Update(int[,] tileIDs)
	{
		if (tileIDs == null)
			throw new ArgumentNullException(nameof(tileIDs));

		tileIndices.Clear();

		for (int y = 0; y < tileIDs.GetLength(1); y++)
			for (int x = 0; x < tileIDs.GetLength(0); x++)
			{
				var tile = tileIDs[x, y];
				if (cellRects.ContainsKey(tile) == false)
					continue;

				tileIndices[(x, y)] = tile;
			}
	}

	/// <param name="hitbox">
	/// The hitbox to check.</param>
	/// <returns>True if the specified hitbox overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public bool IsOverlapping(Hitbox hitbox)
	{
		for (int i = 0; i < hitbox.RectangleCount; i++)
			if (IsOverlapping(hitbox[i]))
				return true;

		return false;
	}
	/// <param name="line">
	/// The line to check.</param>
	/// <returns>True if the specified line overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public bool IsOverlapping(Line line)
	{
		return line.CrossPoints(this).Length > 0;
	}
	/// <param name="rectangle">
	/// The rectangle to check.</param>
	/// <returns>True if the specified rectangle overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public bool IsOverlapping(Rectangle rectangle)
	{
		var neighborRects = GetNeighborRects(rectangle);
		for (int i = 0; i < neighborRects.Count; i++)
			if (neighborRects[i].IsOverlapping(rectangle))
				return true;

		return false;
	}
	/// <param name="point">
	/// The point to check.</param>
	/// <returns>True if the specified point overlaps with the map hitbox, 
	/// false otherwise.</returns>
	public bool IsOverlapping((float x, float y) point)
	{
		var neighborRects = GetNeighborRects(new(point, (1, 1)));
		for (int i = 0; i < neighborRects.Count; i++)
			if (neighborRects[i].IsOverlapping(point))
				return true;

		return false;
	}

	/// <returns>
	/// An array copy of the rectangles in this map hitbox collection, as a bundle tuple.</returns>
	public (float x, float y, float width, float height, uint color)[] ToBundle() => this;

	/// <summary>
	/// Implicitly converts a map object to a rectangle array.
	/// </summary>
	/// <param name="map">The map object to convert.</param>
	public static implicit operator Rectangle[](Map map) => map.GetRectangles();
	/// <summary>
	/// Implicitly converts a map object to an array of rectangle bundles.
	/// </summary>
	/// <param name="map">The map object to convert.</param>
	public static implicit operator (float x, float y, float width, float height, uint color)[](Map map)
	{
		var rectangles = map.GetRectangles();
		var result = new (float x, float y, float width, float height, uint color)[rectangles.Length];
		for (int i = 0; i < rectangles.Length; i++)
			result[i] = rectangles[i];
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
	private int count;

	// to not repeat rectangles for each tile
	// saving map of tiles [(x, y), tile]
	// and rectangles for each tile [tile, list of rectangles]
	private readonly Dictionary<(int, int), int> tileIndices = new();
	private readonly Dictionary<int, List<Rectangle>> cellRects = new();

	internal List<Rectangle> GetNeighborRects(Rectangle rect)
	{
		var result = new List<Rectangle>();
		var (x, y) = rect.Position;
		var (w, h) = rect.Size;
		var (chW, chH) = GetChunkSizeForRect(rect);

		for (int j = -chW; j < chW; j++)
			for (int i = -chH; i < chH; i++)
			{
				var cell = ((int)x + i, (int)y + j);

				if (tileIndices.ContainsKey(cell) == false)
					continue;

				var id = tileIndices[cell];
				var rects = cellRects[id];
				for (int r = 0; r < rects.Count; r++)
					result.Add(rects[r]);
			}
		return result;
	}
	private (int, int) GetChunkSizeForRect(Rectangle globalRect)
	{
		var (w, h) = globalRect.Size;
		var resultW = Math.Max((int)MathF.Ceiling(w * 2f), 1);
		var resultH = Math.Max((int)MathF.Ceiling(h * 2f), 1);
		return (resultW, resultH);
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
