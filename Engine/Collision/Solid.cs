namespace Pure.Engine.Collision;

/// <summary>
/// Represents a solid in 2D space defined by its position and size.
/// </summary>
public struct Solid
{
    /// <summary>
    /// Gets or sets the position of the top-left corner of the solid.
    /// </summary>
    public (float x, float y) Position { get; set; }
    /// <summary>
    /// Gets or sets the size of the solid.
    /// </summary>
    public (float width, float height) Size { get; set; }
    /// <summary>
    /// Gets or sets the color of the solid.
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// Initializes a new solid instance with the specified 
    /// position, size and color.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the solid. 
    /// The default value is (0, 0).</param>
    /// <param name="size">The size of the solid.</param>
    /// <param name="color">The color of the solid.</param>
    public Solid(
        (float width, float height) size,
        (float x, float y) position = default,
        uint color = uint.MaxValue)
    {
        Position = position;
        Size = size;
        Color = color;
    }
    public Solid(float width, float height, float x = default, float y = default,
        uint color = uint.MaxValue)
        : this((width, height), (x, y), color)
    {
    }

    /// <param name="solidPack">
    /// The hitbox to test for overlap with.</param>
    /// <returns>True if this solid overlaps with the specified 
    /// hitbox; otherwise, false.</returns>
    public bool IsOverlapping(SolidPack solidPack)
    {
        return solidPack.IsOverlapping(this);
    }
    /// <summary>
    /// Determines whether this solid is overlapping with the specified 
    /// solid.
    /// </summary>
    /// <param name="solid">The solid to test for overlap with.</param>
    /// <returns>True if this solids overlap; otherwise, false.</returns>
    public bool IsOverlapping(Solid solid)
    {
        var (x1, y1) = Position;
        var (w1, h1) = Size;
        var (x2, y2) = solid.Position;
        var (w2, h2) = solid.Size;

        return x1 < x2 + w2 &&
               x1 + w1 > x2 &&
               y1 < y2 + h2 &&
               y1 + h1 > y2;
    }
    /// <param name="line"> 
    /// The line to test for overlap with.</param>
    /// <returns>True if this solid overlaps with the specified 
    /// line; otherwise, false.</returns>
    public bool IsOverlapping(Line line)
    {
        return line.IsCrossing(this);
    }
    /// <param name="point">
    /// The line to test for overlap with.</param>
    /// <returns>True if this solid overlaps with the specified 
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
    /// A bundle tuple containing the position, size and the color of the solid.</returns>
    public (float x, float y, float width, float height, uint color) ToBundle()
    {
        return this;
    }

    /// <returns>
    /// A string that represents this solid. 
    /// The string has the format: "Position[x y] Size[width height]".</returns>
    public override string ToString()
    {
        var (x, y) = Position;
        var (w, h) = Size;
        return $"{nameof(Position)}[{x} {y}] {nameof(Size)}[{w} {h}]";
    }

    /// <summary>
    /// Implicitly converts a bundle tuple of position, size and color into a solid.
    /// </summary>
    /// <param name="bundle">The bundle tuple to convert.</param>
    /// <returns>A new solid instance.</returns>
    public static implicit operator
        Solid((float x, float y, float width, float height, uint color) bundle)
    {
        return new((bundle.x, bundle.y), (bundle.width, bundle.height), bundle.color);
    }
    /// <summary>
    /// Implicitly converts a solid into a bundle tuple of position, size and color.
    /// </summary>
    /// <param name="solid">The solid to convert.</param>
    /// <returns>A bundle tuple containing the position, size and color of the solid.</returns>
    public static implicit operator (float x, float y, float width, float height, uint color
        )(Solid solid)
    {
        return (solid.Position.x, solid.Position.y, solid.Size.width, solid.Size.height,
            solid.Color);
    }
}