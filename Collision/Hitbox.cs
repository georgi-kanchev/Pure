namespace Pure.Collision;

using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// A collection of rectangles representing an area in 2D space that defines a collision zone.
/// </summary>
public class Hitbox
{
	/// <summary>
	/// Stores the individual rectangles that make up the hitbox.
	/// </summary>
	protected readonly List<Rectangle> rectangles = new();

	/// <summary>
	/// Gets or sets the position of the hitbox.
	/// </summary>
	public (float x, float y) Position { get; set; }
	/// <summary>
	/// Gets or sets the scale of the hitbox.
	/// </summary>
	public float Scale { get; set; } = 1f;
	/// <summary>
	/// Gets the number of rectangles that make up the hitbox.
	/// </summary>
	public virtual int RectangleCount => rectangles.Count;

	/// <summary>
	/// Gets or sets the rectangle at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index">The index of the rectangle to get or set.</param>
	/// <returns>The rectangle at the specified <paramref name="index"/>.</returns>
	public virtual Rectangle this[int index]
	{
		get => rectangles[index];
		set => rectangles[index] = value;
	}

	/// <summary>
	/// Initializes a new hitbox instance from a file.
	/// </summary>
	/// <param name="path">The path to the file to load the hitbox from.</param>
	/// <param name="position">The position of the hitbox.</param>
	/// <param name="scale">The scale of the hitbox.</param>
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
	/// Initializes a new empty hitbox instance (with no rectangles in it) with the specified 
	/// <paramref name="position"/> and <paramref name="scale"/>.
	/// </summary>
	/// <param name="position">The position of the hitbox.</param>
	/// <param name="scale">The scale of the hitbox.</param>
	public Hitbox((float x, float y) position = default, float scale = 1f)
	{
		Position = position;
		Scale = scale;
	}
	/// <summary>
	/// Initializes aa new hitbox instance with the specified 
	/// <paramref name="rectangles"/>, <paramref name="position"/>, and <paramref name="scale"/>.
	/// </summary>
	/// <param name="rectangles">The rectangles to add to the hitbox.</param>
	/// <param name="position">The position of the hitbox.</param>
	/// <param name="scale">The scale of the hitbox.</param>
	public Hitbox(Rectangle[] rectangles, (float x, float y) position = default, float scale = 1f)
		: this(position, scale)
	{
		for (int i = 0; i < rectangles?.Length; i++)
			AddRectangle(rectangles[i]);
	}

	/// <summary>
	/// Saves the hitbox as a compressed binary file to the specified <paramref name="path"/>.
	/// </summary>
	/// <param name="path">The path to save the hitbox to.</param>
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
	/// Adds a <paramref name="rectangle"/> to the hitbox.
	/// </summary>
	/// <param name="rectangle">The rectangle to add.</param>
	public virtual void AddRectangle(Rectangle rectangle)
	{
		rectangles.Add(LocalToGlobalRectangle(rectangle));
	}

	/// <summary>
	/// Checks if the hitbox overlaps with another <paramref name="hitbox"/>.
	/// </summary>
	/// <param name="hitbox">The hitbox to check for overlap with.</param>
	/// <returns>True if the hitboxes overlap, false otherwise.</returns>
	public virtual bool IsOverlapping(Hitbox hitbox)
	{
		for (int i = 0; i < RectangleCount; i++)
			if (hitbox.IsOverlapping(this[i]))
				return true;

		return false;
	}
	/// <param name="rectangle">
	/// The rectangle to check for overlap with.</param>
	/// <returns>True if the hitbox overlaps with the <paramref name="rectangle"/>, 
	/// false otherwise.</returns>
	public virtual bool IsOverlapping(Rectangle rectangle)
	{
		for (int i = 0; i < RectangleCount; i++)
			if (this[i].IsOverlapping(rectangle))
				return true;

		return false;
	}
	/// <param name="line">
	/// The line to check for overlap with.</param>
	/// <returns>True if the hitbox overlaps with the <paramref name="line"/>, 
	/// false otherwise.</returns>
	public virtual bool IsOverlapping(Line line)
	{
		for (int i = 0; i < rectangles.Count; i++)
			if (rectangles[i].IsOverlapping(line))
				return true;

		return false;
	}
	/// <param name="point">
	/// The point to check for overlap with.</param>
	/// <returns>True if the hitbox overlaps with the <paramref name="point"/>, 
	/// false otherwise.</returns>
	public virtual bool IsOverlapping((float x, float y) point)
	{
		for (int i = 0; i < RectangleCount; i++)
			if (this[i].IsOverlapping(point))
				return true;

		return false;
	}

	/// <summary>
	/// Implicitly converts a Rectangle array to a Hitbox object.
	/// </summary>
	/// <param name="rectangles">The Rectangle array to convert.</param>
	public static implicit operator Hitbox(Rectangle[] rectangles) => new(rectangles);
	/// <summary>
	/// Implicitly converts a Hitbox object to a Rectangle array.
	/// </summary>
	/// <param name="hitbox">The Hitbox object to convert.</param>
	public static implicit operator Rectangle[](Hitbox hitbox) => hitbox.rectangles.ToArray();

	/// <returns>
	/// An array copy of the rectangles in this hitbox collection.</returns>
	public Rectangle[] ToArray() => this;

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
