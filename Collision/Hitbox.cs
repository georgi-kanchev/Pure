namespace Pure.Collision
{
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
		public (float, float) Position { get; set; }
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

		/// <summary>
		/// Creates an empty <see cref="Rectangle"/> collection with a
		/// <paramref name="position"/> and <paramref name="scale"/>.
		/// </summary>
		public Hitbox((float, float) position = default, float scale = 1f)
		{
			Position = position;
			Scale = scale;
		}
		/// <summary>
		/// Copies the contents of an existing <see cref="Rectangle"/> collection with a
		/// <paramref name="position"/> and <paramref name="scale"/>.
		/// </summary>
		public Hitbox(Rectangle[] rectangles, (float, float) position = default, float scale = 1f)
			: this(position, scale)
		{
			for(int i = 0; i < rectangles?.Length; i++)
				AddRectangle(rectangles[i]);
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
			for(int i = 0; i < RectangleCount; i++)
				if(hitbox.IsOverlapping(this[i]))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether a <paramref name="rectangle"/> overlaps at least one of the
		/// <see cref="Rectangle"/>s in the collection and returns the result.
		/// </summary>
		public virtual bool IsOverlapping(Rectangle rectangle)
		{
			for(int i = 0; i < RectangleCount; i++)
				if(this[i].IsOverlapping(rectangle))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection is
		/// overlapping a <paramref name="line"/> and returns the result.
		/// </summary>
		public virtual bool IsOverlapping(Line line)
		{
			for(int i = 0; i < rectangles.Count; i++)
				if(rectangles[i].IsOverlapping(line))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection contains a
		/// <paramref name="point"/> and returns the result.
		/// </summary>
		public virtual bool IsOverlapping((float, float) point)
		{
			for(int i = 0; i < RectangleCount; i++)
				if(this[i].IsOverlapping(point))
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
}
