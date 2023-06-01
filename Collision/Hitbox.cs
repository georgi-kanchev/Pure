namespace Pure.Collision;

using System.IO.Compression;

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

	/// <summary>
	/// Initializes a new hitbox instance from a file.
	/// </summary>
	/// <param name="path">The path to the file to load the hitbox from.</param>
	/// <param name="position">The position of the hitbox.</param>
	/// <param name="scale">The scale of the hitbox.</param>
	public Hitbox(string path) : this((0, 0), (0, 0))
	{
		try
		{
			var bytes = Decompress(File.ReadAllBytes(path));
			var bX = new byte[4];
			var bY = new byte[4];
			var bScW = new byte[4];
			var bScH = new byte[4];
			var bCount = new byte[4];
			var offset = 0;

			Add(bX); Add(bY);
			Add(bScW); Add(bScH);
			Add(bCount);
			var count = BitConverter.ToInt32(bCount);

			Position = (BitConverter.ToSingle(bX), BitConverter.ToSingle(bY));
			Scale = (BitConverter.ToSingle(bScW), BitConverter.ToSingle(bScH));

			for(int i = 0; i < count; i++)
			{
				var bXs = new byte[4];
				var bYs = new byte[4];
				var bWs = new byte[4];
				var bHs = new byte[4];
				var bColors = new byte[4];

				Add(bXs); Add(bYs);
				Add(bWs); Add(bHs);
				Add(bColors);

				var x = BitConverter.ToSingle(bXs);
				var y = BitConverter.ToSingle(bYs);
				var w = BitConverter.ToSingle(bWs);
				var h = BitConverter.ToSingle(bHs);
				var color = BitConverter.ToUInt32(bColors);

				AddRectangle(new((w, h), (x, y), color));
			}

			void Add(Array array)
			{
				Array.Copy(bytes, offset, array, 0, array.Length);
				offset += array.Length;
			}
		}
		catch(Exception)
		{
			throw new Exception($"Could not load {nameof(Hitbox)} from '{path}'.");
		}
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
		for(int i = 0; i < rectangles?.Length; i++)
			AddRectangle(rectangles[i]);
	}

	/// <summary>
	/// Saves the hitbox as a compressed binary file to the specified path.
	/// </summary>
	/// <param name="path">The path to save the hitbox to.</param>
	public void Save(string path)
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
		for(int i = 0; i < rectangles.Count; i++)
		{
			var r = rectangles[i];
			result.AddRange(BitConverter.GetBytes(r.Position.x));
			result.AddRange(BitConverter.GetBytes(r.Position.y));
			result.AddRange(BitConverter.GetBytes(r.Size.width));
			result.AddRange(BitConverter.GetBytes(r.Size.height));
			result.AddRange(BitConverter.GetBytes(r.Color));
		}

		File.WriteAllBytes(path, Compress(result.ToArray()));
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
		for(int i = 0; i < RectangleCount; i++)
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
		for(int i = 0; i < RectangleCount; i++)
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
		for(int i = 0; i < rectangles.Count; i++)
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
		for(int i = 0; i < RectangleCount; i++)
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
		for(int i = 0; i < result.Length; i++)
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
		for(int i = 0; i < result.Length; i++)
			result[i] = rectangles[i];
		return result;
	}

	#region Backend
	// save format
	// [amount of bytes]	- data
	// --------------------------------
	// [4]					- x
	// [4]					- y
	// [4]					- scale width
	// [4]					- scale height
	// [4]					- count
	// [count * 4]			- xs
	// [count * 4]			- ys
	// [count * 4]			- widths
	// [count * 4]			- heights
	// [count * 4]			- colors

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
