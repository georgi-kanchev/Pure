namespace Pure.Animation
{
	/// <summary>
	/// A collection of <see cref="Animation{T}"/>.
	/// </summary>
	public class AnimationManager<T>
	{
		/// <summary>
		/// Each name of each <see cref="Animation{T}"/> in the collection.
		/// </summary>
		public string[] Names { get; private set; } = Array.Empty<string>();

		/// <summary>
		/// Get: returns <see cref="Animation{T}"/>[<paramref name="name"/>]
		/// (<see langword="null"/> if it doesn't exist).<br></br>
		/// Set:<br></br>
		/// - replaces <see cref="Animation{T}"/>[<paramref name="name"/>] if it exists in the
		/// collection<br></br>
		/// - adds it otherwise<br></br>
		/// - providing <see langword="null"/> value deletes the
		/// <see cref="Animation{T}"/>[<paramref name="name"/>] if it exist
		/// </summary>
		public Animation<T>? this[string name]
		{
			get => animations.ContainsKey(name) ? animations[name] : default;
			set
			{
				if(value == null)
				{
					animations.Remove(name);
					names.Remove(name);
					Names = names.ToArray();
					return;
				}

				if(names.Contains(name) == false)
					names.Add(name);

				animations[name] = value;
				Names = names.ToArray();
			}
		}

		/// <summary>
		/// Calls <see cref="Animation{T}.Update"/> on each <see cref="Animation{T}"/> in the
		/// collection.
		/// </summary>
		public void Update(float deltaTime)
		{
			foreach(var kvp in animations)
				kvp.Value.Update(deltaTime);
		}

		#region Backend
		private readonly Dictionary<string, Animation<T>> animations = new();
		private readonly List<string> names = new();
		#endregion
	}
}
