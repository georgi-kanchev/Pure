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
    /// Gets or sets the color of the rectangle.
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// Initializes a new rectangle instance with the specified 
    /// position, size and color.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the rectangle. 
    /// The default value is (0, 0).</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="color">The color of the rectangle.</param>
    public Rectangle((float width, float height) size, (float x, float y) position = default,
        uint color = uint.MaxValue)
    {
        Position = position;
        Size = size;
        Color = color;
    }

    /// <param name="hitbox">
    /// The hitbox to test for overlap with.</param>
    /// <returns>True if this rectangle overlaps with the specified 
    /// hitbox; otherwise, false.</returns>
    public bool IsOverlapping(Hitbox hitbox)
    {
        return hitbox.IsOverlapping(this);
    }
    /// <summary>
    /// Determines whether this rectangle is overlapping with the specified 
    /// rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle to test for overlap with.</param>
    /// <returns>True if this rectangles overlap; otherwise, false.</returns>
    public bool IsOverlapping(Rectangle rectangle)
    {
        var (x1, y1) = Position;
        var (w1, h1) = Size;
        var (x2, y2) = rectangle.Position;
        var (w2, h2) = rectangle.Size;

        return x1 < x2 + w2 &&
               x1 + w1 > x2 &&
               y1 < y2 + h2 &&
               y1 + h1 > y2;
    }
    /// <param name="line"> 
    /// The line to test for overlap with.</param>
    /// <returns>True if this rectangle overlaps with the specified 
    /// line; otherwise, false.</returns>
    public bool IsOverlapping(Line line)
    {
        return line.IsCrossing(this);
    }
    /// <param name="point">
    /// The line to test for overlap with.</param>
    /// <returns>True if this rectangle overlaps with the specified 
    /// point; otherwise, false.</returns>
    public bool IsOverlapping((float x, float y) point)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (px, py) = point;

        var containsX = x < px && px < x + w;
        var containsY = y < py && py < y + h;
        return containsX && containsY;
    }

    /// <returns>
    /// A bundle tuple containing the position, size and the color of the rectangle.</returns>
    public (float x, float y, float width, float height, uint color) ToBundle() => this;

    /// <returns>
    /// A string that represents this rectangle. 
    /// The string has the format: "Position[x y] Size[width height]".</returns>
    public override string ToString()
    {
        var (x, y) = Position;
        var (w, h) = Size;
        return $"{nameof(Position)}[{x} {y}] {nameof(Size)}[{w} {h}]";
    }

    /// <summary>
    /// Implicitly converts a bundle tuple of position, size and color into a rectangle.
    /// </summary>
    /// <param name="bundle">The bundle tuple to convert.</param>
    /// <returns>A new rectangle instance.</returns>
    public static implicit operator
        Rectangle((float x, float y, float width, float height, uint color) bundle) =>
        new((bundle.x, bundle.y), (bundle.width, bundle.height), bundle.color);
    /// <summary>
    /// Implicitly converts a rectangle into a bundle tuple of position, size and color.
    /// </summary>
    /// <param name="rectangle">The rectangle to convert.</param>
    /// <returns>A bundle tuple containing the position, size and color of the rectangle.</returns>
    public static implicit operator (float x, float y, float width, float height, uint color
        )(Rectangle rectangle) =>
        (rectangle.Position.x, rectangle.Position.y, rectangle.Size.width, rectangle.Size.height,
            rectangle.Color);
}