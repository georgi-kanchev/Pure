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

	public Map(byte[] bytes)
	{
		var b = Decompress(bytes);
		var offset = 0;

		var sectorCount = BitConverter.ToInt32(Get<int>());

		for(int i = 0; i < sectorCount; i++)
		{
			var tile = BitConverter.ToInt32(Get<int>());
			var rectAmount = BitConverter.ToInt32(Get<int>());

			for(int j = 0; j < rectAmount; j++)
			{
				var x = BitConverter.ToSingle(Get<float>());
				var y = BitConverter.ToSingle(Get<float>());
				var w = BitConverter.ToSingle(Get<float>());
				var h = BitConverter.ToSingle(Get<float>());
				var color = BitConverter.ToUInt32(Get<uint>());

				AddRectangle(new((w, h), (x, y), color), tile);
			}
		}

		byte[] Get<T>() => GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
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
		if(cellRects.ContainsKey(tileID) == false)
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
		if(tileIndices.ContainsKey(cell) == false)
			return Array.Empty<Rectangle>();

		var id = tileIndices[cell];
		var rects = cellRects[id];
		var result = new List<Rectangle>();

		for(int r = 0; r < rects.Count; r++)
			result.Add(rects[r]);

		return result.ToArray();
	}
	public Rectangle[] GetRectangles(int tile)
	{
		return cellRects.ContainsKey(tile) == false ? Array.Empty<Rectangle>() : cellRects[tile].ToArray();
	}
	public Rectangle[] GetRectangles()
	{
		var result = new List<Rectangle>();
		foreach(var kvp in tileIndices)
		{
			var rects = cellRects[kvp.Value];
			var (cellX, cellY) = kvp.Key;
			for(int i = 0; i < rects.Count; i++)
			{
				var rect = rects[i];
				var (x, y) = rect.Position;
				rect.Position = (cellX + x, cellY + y);
				result.Add(rect);
			}
		}

		return result.ToArray();
	}

	public void ClearRectangles()
	{
		count = 0;
		cellRects.Clear();
	}
	public void ClearRectangles(int tile)
	{
		if(cellRects.ContainsKey(tile) == false)
			return;

		count -= cellRects[tile].Count;
		cellRects.Remove(tile);
	}

	/// <summary>
	/// Updates the map hitbox with new tile identifiers data.
	/// </summary>
	/// <param name="tileIDs">The new tile identifiers data.</param>
	/// <exception cref="ArgumentNullException">Thrown if tiles is null.</exception>
	public void Update(int[,] tileIDs)
	{
		if(tileIDs == null)
			throw new ArgumentNullException(nameof(tileIDs));

		tileIndices.Clear();

		for(int y = 0; y < tileIDs.GetLength(1); y++)
			for(int x = 0; x < tileIDs.GetLength(0); x++)
			{
				var tile = tileIDs[x, y];
				if(cellRects.ContainsKey(tile) == false)
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
		for(int i = 0; i < hitbox.RectangleCount; i++)
			if(IsOverlapping(hitbox[i]))
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
		for(int i = 0; i < neighborRects.Count; i++)
			if(neighborRects[i].IsOverlapping(rectangle))
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
		for(int i = 0; i < neighborRects.Count; i++)
			if(neighborRects[i].IsOverlapping(point))
				return true;

		return false;
	}

	/// <returns>
	/// An array copy of the rectangles in this map hitbox collection, as a bundle tuple.</returns>
	public (float x, float y, float width, float height, uint color)[] ToBundle() => this;
	public byte[] ToBytes()
	{
		var result = new List<byte>();
		result.AddRange(BitConverter.GetBytes(cellRects.Count));

		foreach(var kvp in cellRects)
		{
			result.AddRange(BitConverter.GetBytes(kvp.Key));
			result.AddRange(BitConverter.GetBytes(kvp.Value.Count));

			for(int i = 0; i < kvp.Value.Count; i++)
			{
				var r = kvp.Value[i];
				result.AddRange(BitConverter.GetBytes(r.Position.x));
				result.AddRange(BitConverter.GetBytes(r.Position.y));
				result.AddRange(BitConverter.GetBytes(r.Size.width));
				result.AddRange(BitConverter.GetBytes(r.Size.height));
				result.AddRange(BitConverter.GetBytes(r.Color));
			}
		}

		return Compress(result.ToArray());
	}

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
		for(int i = 0; i < rectangles.Length; i++)
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
	// [rect amount * 4]		- colors
	// = = = = = = (sector 2)
	// [4]						- tile
	// [4]						- rect amount
	// [rect amount * 4]		- xs
	// [rect amount * 4]		- ys
	// [rect amount * 4]		- widths
	// [rect amount * 4]		- heights
	// [rect amount * 4]		- colors
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
		var (chW, chH) = GetChunkSizeForRect(rect);

		for(int j = -chW; j < chW; j++)
			for(int i = -chH; i < chH; i++)
			{
				var cell = ((int)x + i, (int)y + j);

				if(tileIndices.ContainsKey(cell) == false)
					continue;

				var id = tileIndices[cell];
				var rects = cellRects[id];
				for(int r = 0; r < rects.Count; r++)
				{
					var curRect = rects[r];
					var (rx, ry) = curRect.Position;
					curRect.Position = (cell.Item1 + rx, cell.Item2 + ry);
					result.Add(curRect);
				}
			}
		return result;
	}
	private static (int, int) GetChunkSizeForRect(Rectangle globalRect)
	{
		var (w, h) = globalRect.Size;
		var resultW = Math.Max((int)MathF.Ceiling(w * 2f), 1);
		var resultH = Math.Max((int)MathF.Ceiling(h * 2f), 1);
		return (resultW, resultH);
	}

	private static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		using(var stream = new DeflateStream(output, CompressionLevel.Optimal))
			stream.Write(data, 0, data.Length);

		return output.ToArray();
	}
	private static byte[] Decompress(byte[] data)
	{
		var input = new MemoryStream(data);
		var output = new MemoryStream();
		using(var stream = new DeflateStream(input, CompressionMode.Decompress))
			stream.CopyTo(output);

		return output.ToArray();
	}
	private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
	{
		var result = fromBytes[offset..(offset + amount)];
		offset += amount;
		return result;
	}

	#endregion
}
