using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Collision;

/// <summary>
/// (Inherits <see cref="Hitbox"/>)<br></br><br></br>
/// An additional grid collection of <see cref="Rectangle"/>s on top of the
/// <see cref="Hitbox"/> one.<br></br><br></br>
/// 
/// Each <see cref="Map"/> cell has its own <see cref="Rectangle"/> collection and the
/// <see cref="Map"/> combines them. Unlike <see cref="Hitbox"/>, this collection is optimized
/// to do checks in chunks by picking only neighbouring cells rather than checking each cell's
/// collection.
/// </summary>
public class Map : Hitbox
{
	/// <summary>
	/// The amount of <see cref="Rectangle"/>s in the <see cref="Rectangle"/> collection and
	/// cell <see cref="Rectangle"/> collection, combined.
	/// </summary>
	public override int RectangleCount => rectangles.Count + cellRects.Count;

	/// <summary>
	/// Get: returns the <see cref="Rectangle"/> at <paramref name="index"/>.
	/// </summary>
	public override Rectangle this[int index]
	{
		get
		{
			var r = rectangles;
			return index < r.Count ? r[index] : cellRects[index - r.Count];
		}
	}

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
	public Map() : base((0, 0), 1) { }

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
	/// Expands the <see cref="Rectangle"/> collection with a new <paramref name="rectangle"/>.
	/// This <see cref="Rectangle"/> will be updated over each <paramref name="tile"/> upon
	/// <see cref="Update"/>. It will have no effect before that.
	/// </summary>
	public void AddRectangle(Rectangle rectangle, int tile)
	{
		if (cellRectsMap.ContainsKey(tile) == false)
			cellRectsMap[tile] = new List<Rectangle>();

		cellRectsMap[tile].Add(rectangle);
	}
	/// <summary>
	/// Retrieves the <see cref="Rectangle"/> collection at a certain <paramref name="cell"/> and
	/// returns it if the provided <paramref name="cell"/> is present.
	/// Returns an empty <see cref="Rectangle"/> <see cref="Array"/> otherwise. 
	/// </summary>
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
	/// Updates all rectangles added by <see cref="AddRectangle"/> according to <paramref name="tiles"/>.
	/// </summary>
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

	/// <summary>
	/// Checks whether a <paramref name="line"/> overlaps at least one of the
	/// <see cref="Rectangle"/>s in the collection and returns the result.
	/// </summary>
	public override bool IsOverlapping(Line line)
	{
		return line.CrossPoints(this).Length > 0;
	}
	/// <summary>
	/// Checks whether a <paramref name="rectangle"/> overlaps at least one of the
	/// <see cref="Rectangle"/>s in the collection and returns the result.
	/// </summary>
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
	/// <summary>
	/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection contains
	/// a <paramref name="point"/> and returns the result.
	/// </summary>
	public override bool IsOverlapping((float x, float y) point)
	{
		var neighborRects = GetNeighborRects(point, (1, 1));
		for (int i = 0; i < neighborRects.Count; i++)
			if (neighborRects[i].IsOverlapping(point))
				return true;

		return false;
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
