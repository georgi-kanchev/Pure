namespace Purity.Collision
{
	/// <summary>
	/// A collection of <see cref="Rectangle"/>s which can be offset by a position, affecting all
	/// <see cref="Rectangles"/> accordingly. It checks whether it contains a point in the world or
	/// overlaps with another <see cref="Hitbox"/> or <see cref="Rectangle"/>.
	/// </summary>
	public class Hitbox
	{
		/// <summary>
		/// Applies to all <see cref="Rectangles"/> accordingly.
		/// </summary>
		public (float, float) Position { get; set; }
		/// <summary>
		/// A copy of the <see cref="Rectangle"/> collection.
		/// </summary>
		public Rectangle[] Rectangles => globalRectsCopy;

		/// <summary>
		/// Creates an empty <see cref="Rectangle"/> collection with a certain
		/// <paramref name="position"/>.
		/// </summary>
		public Hitbox((float, float) position)
		{
			Position = position;
		}
		/// <summary>
		/// Copies the contents of an existing <see cref="Rectangle"/> collection with a
		/// certain <paramref name="position"/> and <paramref name="scale"/>.
		/// </summary>
		public Hitbox(Rectangle[] rectangles, (float, float) position = default) : this(position)
		{
			for(int i = 0; i < rectangles?.Length; i++)
				AddRectangle(rectangles[i]);
		}

		/// <summary>
		/// Expands the <see cref="Rectangle"/> collection with a new <paramref name="rectangle"/>.
		/// </summary>
		public virtual void AddRectangle(Rectangle rectangle)
		{
			globalRects.Add(rectangle);
			globalRectsCopy = globalRects.ToArray();
		}
		/// <summary>
		/// Checks whether at least one of the <see cref="Rectangles"/> of this collection overlaps
		/// at least one of the <see cref="Rectangles"/> of a certain <paramref name="hitbox"/>.
		/// Then the result is returned.
		/// </summary>
		public virtual bool IsOverlapping(Hitbox hitbox)
		{
			for(int i = 0; i < Rectangles.Length; i++)
				if(hitbox.IsOverlapping(Rectangles[i]))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether a <paramref name="rectangle"/> overlaps at least one of the
		/// <see cref="Rectangles"/> and returns the result.
		/// </summary>
		public virtual bool IsOverlapping(Rectangle rectangle)
		{
			for(int i = 0; i < Rectangles.Length; i++)
				if(Rectangles[i].IsOverlapping(rectangle))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether at least one of the <see cref="Rectangles"/> contains a
		/// <paramref name="point"/> and returns the result.
		/// </summary>
		public virtual bool IsContaining((float, float) point)
		{
			for(int i = 0; i < Rectangles.Length; i++)
				if(Rectangles[i].IsContaining(point))
					return true;

			return false;
		}

		public static implicit operator Hitbox(Rectangle[] rectangles) => new(rectangles);
		public static implicit operator Rectangle[](Hitbox hitbox) => hitbox.Rectangles;

		#region Backend
		private Rectangle[] globalRectsCopy = Array.Empty<Rectangle>();
		private readonly List<Rectangle> globalRects = new();
		#endregion
	}
}
