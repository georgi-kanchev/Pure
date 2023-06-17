namespace Pure.Collision;

using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// A collection of rectangles representing an area in 2D space that defines a collision zone.
/// </summary>
public class Hitbox
{
	/// <summary>
	/// Gets or sets the position of the hitbox.
	/// </summary>
	public (float x, float y) Position { get; set; }
	/// <summary>
	/// Gets or sets the scale of the hitbox.
	/// </summary>
	public (float width, float height) Scale { get; set; } = (1f, 1f);
	/// <summary>
	/// Gets the number of rectangles that make up the hitbox.
	/// </summary>
	public int RectangleCount => rectangles.Count;

	/// <summary>
	/// Gets or sets the rectangle at the specified index.
	/// </summary>
	/// <param name="index">The index of the rectangle to get or set.</param>
	/// <returns>The rectangle at the specified index.</returns>
	public Rectangle this[int index]
	{
		get => LocalToGlobalRectangle(rectangles[index]);
		set => rectangles[index] = value;
	}

	public Hitbox(byte[] bytes) : this((0, 0), (0, 0))
	{
		var b = Decompress(bytes);
		var offset = 0;

		var count = BitConverter.ToInt32(Get<int>());

		Position = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
		Scale = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));

		for(var i = 0; i < count; i++)
		{
			var x = BitConverter.ToSingle(Get<float>());
			var y = BitConverter.ToSingle(Get<float>());
			var w = BitConverter.ToSingle(Get<float>());
			var h = BitConverter.ToSingle(Get<float>());
			var color = BitConverter.ToUInt32(Get<uint>());

			AddRectangle(new((w, h), (x, y), color));
		}

		byte[] Get<T>() => GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
	}
	/// <summary>
	/// Initializes a new empty hitbox instance (with no rectangles in it) with the specified 
	/// position and scale.
	/// </summary>
	/// <param name="position">The position of the hitbox.</param>
	/// <param name="scale">The scale of the hitbox.</param>
	public Hitbox((float x, float y) position, (float width, float height) scale = default)
	{
		scale = scale == default ? (1f, 1f) : scale;

		Position = position;
		Scale = scale;
	}
	/// <summary>
	/// Initializes aa new hitbox instance with the specified 
	/// position, scale and rectangles.
	/// </summary>
	/// <param name="position">The position of the hitbox.</param>
	/// <param name="scale">The scale of the hitbox.</param>
	/// <param name="rectangles">The rectangles to add to the hitbox.</param>
	public Hitbox((float x, float y) position, (float width, float height) scale = default, params Rectangle[] rectangles)
		: this(position, scale)
	{
		foreach (var r in rectangles)
			AddRectangle(r);
	}

	public byte[] ToBytes()
	{
		var c = rectangles.Count;
		var bX = BitConverter.GetBytes(Position.x);
		var bY = BitConverter.GetBytes(Position.y);
		var bScW = BitConverter.GetBytes(Scale.width);
		var bScH = BitConverter.GetBytes(Scale.height);
		var bCount = BitConverter.GetBytes(c);
		var result = new List<byte>();

		result.AddRange(bX);
		result.AddRange(bY);
		result.AddRange(bScW);
		result.AddRange(bScH);
		result.AddRange(bCount);
		foreach (var r in rectangles)
		{
			result.AddRange(BitConverter.GetBytes(r.Position.x));
			result.AddRange(BitConverter.GetBytes(r.Position.y));
			result.AddRange(BitConverter.GetBytes(r.Size.width));
			result.AddRange(BitConverter.GetBytes(r.Size.height));
			result.AddRange(BitConverter.GetBytes(r.Color));
		}

		return Compress(result.ToArray());
	}

	/// <summary>
	/// Adds a rectangle to the hitbox.
	/// </summary>
	/// <param name="rectangle">The rectangle to add.</param>
	public void AddRectangle(Rectangle rectangle)
	{
		rectangles.Add(rectangle);
	}

	/// <summary>
	/// Checks if the hitbox overlaps with another hitbox.
	/// </summary>
	/// <param name="hitbox">The hitbox to check for overlap with.</param>
	/// <returns>True if the hitboxes overlap, false otherwise.</returns>
	public bool IsOverlapping(Hitbox hitbox)
	{
		for(var i = 0; i < RectangleCount; i++)
			if(hitbox.IsOverlapping(this[i]))
				return true;

		return false;
	}
	/// <param name="rectangle">
	/// The rectangle to check for overlap with.</param>
	/// <returns>True if the hitbox overlaps with the rectangle, 
	/// false otherwise.</returns>
	public bool IsOverlapping(Rectangle rectangle)
	{
		for(var i = 0; i < RectangleCount; i++)
			if(this[i].IsOverlapping(rectangle))
				return true;

		return false;
	}
	/// <param name="line">
	/// The line to check for overlap with.</param>
	/// <returns>True if the hitbox overlaps with the line, 
	/// false otherwise.</returns>
	public bool IsOverlapping(Line line)
	{
		for(var i = 0; i < rectangles.Count; i++)
			if(this[i].IsOverlapping(line))
				return true;

		return false;
	}
	/// <param name="point">
	/// The point to check for overlap with.</param>
	/// <returns>True if the hitbox overlaps with the point, 
	/// false otherwise.</returns>
	public bool IsOverlapping((float x, float y) point)
	{
		for(var i = 0; i < RectangleCount; i++)
			if(this[i].IsOverlapping(point))
				return true;

		return false;
	}

	/// <returns>
	/// An array copy of the rectangles in this hitbox collection.</returns>
	public Rectangle[] ToArray() => this;
	/// <returns>
	/// An array copy of the rectangles in this hitbox collection, as a bundle tuple.</returns>
	public (float x, float y, float width, float height, uint color)[] ToBundle() => this;

	/// <summary>
	/// Implicitly converts a rectangle array to a hitbox object.
	/// </summary>
	/// <param name="rectangles">The rectangle array to convert.</param>
	public static implicit operator Hitbox(Rectangle[] rectangles) => new(default, default, rectangles);
	/// <summary>
	/// Implicitly converts a hitbox object to a rectangle array.
	/// </summary>
	/// <param name="hitbox">The hitbox object to convert.</param>
	public static implicit operator Rectangle[](Hitbox hitbox) => hitbox.rectangles.ToArray();
	/// <summary>
	/// Implicitly converts a hitbox object to an array of rectangle bundle tuples.
	/// </summary>
	/// <param name="hitbox">The hitbox object to convert.</param>
	public static implicit operator (float x, float y, float width, float height, uint color)[](Hitbox hitbox)
	{
		var result = new (float x, float y, float width, float height, uint color)[hitbox.rectangles.Count];
		for(var i = 0; i < result.Length; i++)
			result[i] = hitbox[i];
		return result;
	}
	/// <summary>
	/// Implicitly converts an array of rectangle bundle tuples to a hitbox object.
	/// </summary>
	/// <param name="rectangles">The array of rectangle bundles to convert.</param>
	public static implicit operator Hitbox((float x, float y, float width, float height, uint color)[] rectangles)
	{
		var result = new Rectangle[rectangles.Length];
		for(var i = 0; i < result.Length; i++)
			result[i] = rectangles[i];
		return result;
	}

	#region Backend
	// save format in sectors
	// [amount of bytes]	- data
	// --------------------------------
	// [4]					- x
	// [4]					- y
	// [4]					- scale width
	// [4]					- scale height
	// [4]					- count
	// = = = = = = (sector 1)
	// [4]					- x
	// [4]					- y
	// [4]					- width
	// [4]					- height
	// [4]					- color
	// = = = = = = (sector 2)
	// [4]					- x
	// [4]					- y
	// [4]					- width
	// [4]					- height
	// [4]					- color
	// = = = = = = (sector 3)
	// ... up to sector [count]

	private readonly List<Rectangle> rectangles = new();

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

	internal Rectangle LocalToGlobalRectangle(Rectangle localRect)
	{
		var (x, y) = localRect.Position;
		var (w, h) = localRect.Size;
		localRect.Position = (Position.x + x * Scale.width, Position.y + y * Scale.height);
		localRect.Size = (w * Scale.width, h * Scale.height);
		return localRect;
	}
	#endregion
}
