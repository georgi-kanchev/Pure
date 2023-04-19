namespace Pure.Collision;

/// <summary>
/// Represents a rectangle in 2D space defined by its position and size.
/// </summary>
public struct Rectangle
{
	/// <summary>
	/// Gets or sets the position of the top-left corner of the rectangle.
	/// </summary>
	public (float x, float y) Position { get; set; }
	/// <summary>
	/// Gets or sets the size of the rectangle.
	/// </summary>
	public (float width, float height) Size { get; set; }

	/// <summary>
	/// Initializes a new rectangle instance with the specified 
	/// <paramref name="size"/> and <paramref name="position"/>.
	/// </summary>
	/// <param name="size">The size of the rectangle.</param>
	/// <param name="position">The position of the top-left corner of the rectangle. 
	/// The default value is (0, 0).</param>
	public Rectangle((float width, float height) size, (float x, float y) position = default)
	{
		Position = position;
		Size = size;
	}

	/// <param name="hitbox">
	/// The hitbox to test for overlap with.</param>
	/// <returns>True if this rectangle overlaps with the specified 
	/// <paramref name="hitbox"/>; otherwise, false.</returns>
	public bool IsOverlapping(Hitbox hitbox)
	{
		return hitbox.IsOverlapping(this);
	}
	/// <summary>
	/// Determines whether this rectangle is overlapping with the specified 
	/// <paramref name="rectangle"/>.
	/// </summary>
	/// <param name="rectangle">The rectangle to test for overlap with.</param>
	/// <returns>True if this rectangles overlap; otherwise, false.</returns>
	public bool IsOverlapping(Rectangle rectangle)
	{
		var (x1, y1) = Position;
		var (w1, h1) = Size;
		var (x2, y2) = rectangle.Position;
		var (w2, h2) = rectangle.Size;
		var tl1 = IsOverlapping((x2, y2));
		var tr1 = IsOverlapping((x2 + w2, y2));
		var br1 = IsOverlapping((x2 + w2, y2 + h2));
		var bl1 = IsOverlapping((x2, y2 + h2));
		var tl2 = rectangle.IsOverlapping((x1, y1));
		var tr2 = rectangle.IsOverlapping((x1 + w1, y1));
		var br2 = rectangle.IsOverlapping((x1 + w1, y1 + h1));
		var bl2 = rectangle.IsOverlapping((x1, y1 + h1));
		var overlap1 = tl1 || tr1 || br1 || bl1;
		var overlap2 = tl2 || tr2 || br2 || bl2;

		return overlap1 || overlap2;
	}
	/// <param name="line"> 
	/// The line to test for overlap with.</param>
	/// <returns>True if this rectangle overlaps with the specified 
	/// <paramref name="line"/>; otherwise, false.</returns>
	public bool IsOverlapping(Line line)
	{
		return line.IsCrossing(this) || IsContaining(line);
	}
	/// <param name="point">
	/// The line to test for overlap with.</param>
	/// <returns>True if this rectangle overlaps with the specified 
	/// <paramref name="point"/>; otherwise, false.</returns>
	public bool IsOverlapping((float x, float y) point)
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var (px, py) = point;

		var containsX = x <= px && px <= x + w;
		var containsY = y <= py && py <= y + h;
		return containsX && containsY;
	}

	/// <param name="line">
	/// The line to check.</param>
	/// <returns>True if the <paramref name="line"/> is contained within this
	/// rectangle; otherwise, false<.</returns>
	public bool IsContaining(Line line)
	{
		return IsOverlapping(line.A) || IsOverlapping(line.B);
	}

	/// <returns>
	/// A string that represents this rectangle. 
	/// The string has the format: "Position[x y] Size[width height]".</returns>
	public override string ToString()
	{
		var (x, y) = Position;
		var (w, h) = Size;
		return $"{nameof(Position)}[{x} {y}] {nameof(Size)}[{w} {h}]";
	}
}
