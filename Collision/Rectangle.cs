namespace Purity.Collision
{
	/// <summary>
	/// A simple representation of an <see langword="Axis Aligned Bounding Box (AABB)"/>.
	/// Used for checking whether it contains a point in the world or overlaps another
	/// <see cref="Rectangle"/>. It can be positioned and resized.
	/// </summary>
	public struct Rectangle
	{
		/// <summary>
		/// The point of the top-left corner of this <see cref="Rectangle"/> in the world.
		/// </summary>
		public (float, float) Position { get; set; }
		/// <summary>
		/// The size between the top-left and bottom-right corner of this <see cref="Rectangle"/>
		/// in the world.
		/// </summary>
		public (float, float) Size { get; set; }

		/// <summary>
		/// Creates the <see cref="Rectangle"/> with a certain <paramref name="size"/> and
		/// <paramref name="position"/>.
		/// </summary>
		public Rectangle((float, float) size, (float, float) position = default)
		{
			Position = position;
			Size = size;
		}

		/// <summary>
		/// Checks whether the <see cref="Rectangle"/> overlaps a <paramref name="hitbox"/> and
		/// returns the result. See <see cref="Hitbox.IsOverlapping(Rectangle)"/> for more info.
		/// </summary>
		public bool IsOverlapping(Hitbox hitbox)
		{
			return hitbox.IsOverlapping(this);
		}
		/// <summary>
		/// Checks whether <see langword="this"/> and another <paramref name="rectangle"/> overlaps
		/// overlaps and returns the result.
		/// </summary>
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
		/// <summary>
		/// Checks whether the <see cref="Rectangle"/> overlaps a <paramref name="line"/> and
		/// returns the result.
		/// </summary>
		public bool IsOverlapping(Line line)
		{
			return line.IsCrossing(this) || IsContaining(line);
		}
		/// <summary>
		/// Checks whether the <see cref="Rectangle"/> overlaps a <paramref name="point"/>
		/// and returns the result.
		/// </summary>
		public bool IsOverlapping((float, float) point)
		{
			var (x, y) = Position;
			var (w, h) = Size;
			var (px, py) = point;

			var containsX = x <= px && px <= x + w;
			var containsY = y <= py && py <= y + h;
			return containsX && containsY;
		}

		/// <summary>
		/// Checks whether the <see cref="Rectangle"/> contains a <paramref name="line"/> and
		/// returns the result.
		/// </summary>
		public bool IsContaining(Line line)
		{
			return IsOverlapping(line.A) || IsOverlapping(line.B);
		}

		/// <summary>
		/// Returns a text representation of this <see cref="Rectangle"/> in the format:
		/// <see langword="Position[x y] Size[w h]"/>
		/// </summary>
		public override string ToString()
		{
			var (x, y) = Position;
			var (w, h) = Size;
			return $"{nameof(Position)}[{x} {y}] {nameof(Size)}[{w} {h}]";
		}
	}
}