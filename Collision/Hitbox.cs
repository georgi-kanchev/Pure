namespace Pure.Collision;

using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// A collection of <see cref="Rectangle"/>s which can be positioned and scaled, affecting all
/// <see cref="Rectangle"/>s accordingly. It checks whether it contains a point in the world or
/// overlaps with another <see cref="Hitbox"/>, <see cref="Rectangle"/> or <see cref="Line"/>.
/// </summary>
public class Hitbox
{
	/// <summary>
	/// The raw <see cref="Rectangle"/> collection.
	/// </summary>
	protected readonly List<Rectangle> rectangles = new();

	/// <summary>
	/// Applies to all <see cref="Rectangle"/>s in the collection accordingly.
	/// </summary>
	public (float x, float y) Position { get; set; }
	/// <summary>
	/// Applies to all <see cref="Rectangle"/>s in the collection accordingly.
	/// </summary>
	public float Scale { get; set; } = 1f;
	/// <summary>
	/// The amount of <see cref="Rectangle"/>s in the collection.
	/// </summary>
	public virtual int RectangleCount => rectangles.Count;

	/// <summary>
	/// Get: returns the <see cref="Rectangle"/> at <paramref name="index"/>.<br></br>
	/// Set: replaces the <see cref="Rectangle"/> at <paramref name="index"/>.
	/// </summary>
	public virtual Rectangle this[int index]
	{
		get => rectangles[index];
		set => rectangles[index] = value;
	}

	public Hitbox(string path, (float x, float y) position = default, float scale = 1f)
		: this(position, scale)
	{
		try
		{
			var bytes = Decompress(File.ReadAllBytes(path));
			var bCount = new byte[4];

			Array.Copy(bytes, 0, bCount, 0, bCount.Length);
			var count = BitConverter.ToInt32(bCount);

			var xs = new int[count];
			var ys = new int[count];
			var ws = new int[count];
			var hs = new int[count];
			var bXs = new byte[xs.Length * Marshal.SizeOf(typeof(int))];
			var bYs = new byte[ys.Length * Marshal.SizeOf(typeof(int))];
			var bWs = new byte[ws.Length * Marshal.SizeOf(typeof(int))];
			var bHs = new byte[hs.Length * Marshal.SizeOf(typeof(int))];

			Array.Copy(bytes, bCount.Length, bXs, 0, bXs.Length);
			Array.Copy(bytes, bCount.Length + bXs.Length, bYs, 0, bYs.Length);
			Array.Copy(bytes, bCount.Length + bXs.Length + bYs.Length, bWs, 0, bWs.Length);
			Array.Copy(bytes, bCount.Length + bXs.Length + bYs.Length + bWs.Length, bHs, 0, bHs.Length);

			FromBytes(xs, bXs);
			FromBytes(ys, bYs);
			FromBytes(ws, bWs);
			FromBytes(hs, bHs);
		}
		catch (Exception)
		{
			throw new Exception($"Could not load {nameof(Hitbox)} from '{path}'.");
		}
	}
	/// <summary>
	/// Creates an empty <see cref="Rectangle"/> collection with a
	/// <paramref name="position"/> and <paramref name="scale"/>.
	/// </summary>
	public Hitbox((float x, float y) position = default, float scale = 1f)
	{
		Position = position;
		Scale = scale;
	}
	/// <summary>
	/// Copies the contents of an existing <see cref="Rectangle"/> collection with a
	/// <paramref name="position"/> and <paramref name="scale"/>.
	/// </summary>
	public Hitbox(Rectangle[] rectangles, (float x, float y) position = default, float scale = 1f)
		: this(position, scale)
	{
		for (int i = 0; i < rectangles?.Length; i++)
			AddRectangle(rectangles[i]);
	}

	public virtual void Save(string path)
	{
		var c = rectangles.Count;
		var bCount = BitConverter.GetBytes(c);
		var xs = new int[c];
		var ys = new int[c];
		var ws = new int[c];
		var hs = new int[c];
		var bXs = ToBytes(xs);
		var bYs = ToBytes(ys);
		var bWs = ToBytes(ws);
		var bHs = ToBytes(hs);
		var result = new byte[bCount.Length + bXs.Length + bYs.Length + bWs.Length + bHs.Length];

		Array.Copy(bXs, 0, result, 0, bXs.Length);
		Array.Copy(bYs, 0, result, bXs.Length, bYs.Length);
		Array.Copy(bWs, 0, result, bXs.Length + bYs.Length, bWs.Length);
		Array.Copy(bHs, 0, result, bXs.Length + bYs.Length + bWs.Length, bHs.Length);

		File.WriteAllBytes(path, Compress(result));
	}

	/// <summary>
	/// Expands the <see cref="Rectangle"/> collection with a new <paramref name="rectangle"/>.
	/// </summary>
	public virtual void AddRectangle(Rectangle rectangle)
	{
		rectangles.Add(LocalToGlobalRectangle(rectangle));
	}

	/// <summary>
	/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection overlaps
	/// at least one of the <see cref="Rectangle"/>s in a <paramref name="hitbox"/>.
	/// Then the result is returned.
	/// </summary>
	public virtual bool IsOverlapping(Hitbox hitbox)
	{
		for (int i = 0; i < RectangleCount; i++)
			if (hitbox.IsOverlapping(this[i]))
				return true;

		return false;
	}
	/// <summary>
	/// Checks whether a <paramref name="rectangle"/> overlaps at least one of the
	/// <see cref="Rectangle"/>s in the collection and returns the result.
	/// </summary>
	public virtual bool IsOverlapping(Rectangle rectangle)
	{
		for (int i = 0; i < RectangleCount; i++)
			if (this[i].IsOverlapping(rectangle))
				return true;

		return false;
	}
	/// <summary>
	/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection is
	/// overlapping a <paramref name="line"/> and returns the result.
	/// </summary>
	public virtual bool IsOverlapping(Line line)
	{
		for (int i = 0; i < rectangles.Count; i++)
			if (rectangles[i].IsOverlapping(line))
				return true;

		return false;
	}
	/// <summary>
	/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection contains a
	/// <paramref name="point"/> and returns the result.
	/// </summary>
	public virtual bool IsOverlapping((float x, float y) point)
	{
		for (int i = 0; i < RectangleCount; i++)
			if (this[i].IsOverlapping(point))
				return true;

		return false;
	}

	/// <summary>
	/// Returns a new <see cref="Hitbox"/> created from a collection of
	/// <paramref name="rectangles"/>.
	/// </summary>
	public static implicit operator Hitbox(Rectangle[] rectangles) => new(rectangles);
	/// <summary>
	/// Returns a copy of all the values of a <paramref name="hitbox"/> collection.
	/// </summary>
	public static implicit operator Rectangle[](Hitbox hitbox) => hitbox.rectangles.ToArray();

	#region Backend
	// save format
	// [amount of bytes]	- data
	// --------------------------------
	// [4]					- count
	// [count * 4]			- Xs
	// [count * 4]			- Ys
	// [count * 4]			- Widths
	// [count * 4]			- Heights

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

	internal Rectangle LocalToGlobalRectangle(Rectangle localRect)
	{
		var (x, y) = localRect.Position;
		var (w, h) = localRect.Size;
		localRect.Position = (x * Scale, y * Scale);
		localRect.Size = (w * Scale, h * Scale);
		return localRect;
	}
	#endregion
}
