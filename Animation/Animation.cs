namespace Purity.Animation
{
	/// <summary>
	/// A collection of <typeparamref name="T"/>.
	/// Continuously iterates over its items in a set time period.
	/// </summary>
	public class Animation<T>
	{
		/// <summary>
		/// The current value from the <see cref="values"/> retrieved by the <see cref="CurrentIndex"/>.
		/// </summary>
		public T CurrentValue => values[CurrentIndex];
		/// <summary>
		/// The current index the <see cref="Animation{T}"/> is at.
		/// </summary>
		public int CurrentIndex => (int)MathF.Round(RawIndex);
		/// <summary>
		/// The amount of seconds needed to iterate over the <see cref="values"/>.
		/// </summary>
		public float TotalDuration { get; set; }
		/// <summary>
		/// The amount of seconds the iteration stays on a particular value.
		/// </summary>
		public float ValueDuration
		{
			get => TotalDuration / values.Length;
			set => TotalDuration = value * values.Length;
		}
		/// <summary>
		/// Whether the iteration over the collection starts over when the end is reached.
		/// </summary>
		public bool IsRepeating { get; set; }
		/// <summary>
		/// Whether the iteration over the collection is currently paused.
		/// </summary>
		public bool IsPaused { get; set; }

		/// <summary>
		/// Get: returns the value at <paramref name="index"/>.<br></br>
		/// Set: replaces the value at <paramref name="index"/>.
		/// </summary>
		public T this[int index]
		{
			get => values[index];
			set => values[index] = value;
		}

		/// <summary>
		/// Creates the <see cref="Animation{T}"/> from a collection of <paramref name="values"/>
		/// with <paramref name="totalDuration"/> in seconds while it <paramref name="isRepeating"/>.
		/// </summary>
		public Animation(float totalDuration, bool isRepeating, params T[] values)
		{
			this.values = values ?? throw new ArgumentNullException(nameof(values));
			if(values.Length < 1)
				throw new ArgumentException("Total values cannot be < 1.", nameof(values));

			rawIndex = 0;
			TotalDuration = totalDuration;
			IsRepeating = isRepeating;
			RawIndex = LOWER_BOUND;
		}
		/// <summary>
		/// Creates the <see cref="Animation{T}"/> from a collection of <paramref name="values"/>
		/// with <paramref name="valueDuration"/> in values per second while it <paramref name="isRepeating"/>.
		/// </summary>
		public Animation(bool isRepeating, float valueDuration, params T[] values)
		{
			this.values = values ?? throw new ArgumentNullException(nameof(values));
			if(values.Length < 1)
				throw new ArgumentException("Total values cannot be < 1.", nameof(values));

			rawIndex = 0;
			ValueDuration = valueDuration;
			IsRepeating = isRepeating;
			RawIndex = LOWER_BOUND;
		}
		/// <summary>
		/// Creates the <see cref="Animation{T}"/> from a collection of <paramref name="values"/>.
		/// </summary>
		public Animation(params T[] values) : this(1f, false, values) { }

		/// <summary>
		/// Advances the <see cref="Animation{T}"/> by <paramref name="deltaTime"/> seconds, unless
		/// <see cref="Animation{T}.IsPaused"/>.
		/// </summary>
		public void Update(float deltaTime)
		{
			if(values == default || IsPaused)
				return;

			RawIndex += deltaTime / ValueDuration;
			if((int)MathF.Round(RawIndex) >= values.Length)
				RawIndex = IsRepeating ? LOWER_BOUND : values.Length - 1;
		}

		/// <summary>
		/// Returns a new <see cref="Animation{T}"/> created from a collection of
		/// <paramref name="values"/>.
		/// </summary>
		public static implicit operator Animation<T>(T[] values) => new(values);
		/// <summary>
		/// Returns the values of a <paramref name="animation"/> - <see cref="values"/>.
		/// </summary>
		public static implicit operator T[](Animation<T> animation) => animation.values;

		#region Backend
		private readonly T[] values;

		private float rawIndex;
		private const float LOWER_BOUND = -0.49f;

		private float RawIndex
		{
			get => rawIndex;
			set => rawIndex = Math.Clamp(value, LOWER_BOUND, values.Length);
		}
		#endregion
	}
}