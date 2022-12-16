namespace Purity.Input
{
	/// <summary>
	/// A template for a physical input device. All the available inputs are represented by the
	/// <see cref="Enum"/> <typeparamref name="T"/>. Checks may be performed for whether those
	/// inputs are pressed.
	/// </summary>
	public abstract class Device<T> where T : Enum
	{
		/// <summary>
		/// A collection of each currently pressed <typeparamref name="T"/>.
		/// </summary>
		public T[] Pressed { get; private set; } = Array.Empty<T>();
		/// <summary>
		/// A collection of each newly pressed <typeparamref name="T"/>.
		/// </summary>
		public T[] JustPressed { get; private set; } = Array.Empty<T>();
		/// <summary>
		/// A collection of each newly no longer pressed <typeparamref name="T"/>.
		/// </summary>
		public T[] JustReleased { get; private set; } = Array.Empty<T>();

		/// <summary>
		/// Triggers events and provides each <typeparamref name="T"/> to the collections
		/// accordingly.
		/// </summary>
		public virtual void Update()
		{
			var prevPressed = new List<T>(pressed);
			pressed.Clear();
			justPressed.Clear();
			justReleased.Clear();

			var count = Enum.GetNames(typeof(T)).Length;
			for(int i = 0; i < count; i++)
			{
				var cur = (T)(object)i;
				var isPr = IsPressedRaw(cur);
				var wasPr = prevPressed.Contains(cur);

				if(wasPr && isPr == false)
					justReleased.Add(cur);
				else if(wasPr == false && isPr)
					justPressed.Add(cur);

				if(isPr)
					pressed.Add(cur);
			}

			Pressed = pressed.ToArray();
			JustPressed = justPressed.ToArray();
			JustReleased = justReleased.ToArray();
		}

		/// <summary>
		/// Checks whether an <paramref name="input"/> is pressed and returns a result.
		/// </summary>
		public bool IsPressed(T input)
		{
			return pressed.Contains(input);
		}
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is pressed
		/// and returns a result.
		/// </summary>
		public bool IsJustPressed(T input)
		{
			return justPressed.Contains(input);
		}
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public bool IsJustReleased(T input)
		{
			return justReleased.Contains(input);
		}

		/// <summary>
		/// The raw logic that handles whether a physical <paramref name="input"/> is pressed.
		/// </summary>
		protected abstract bool IsPressedRaw(T input);

		#region Backend
		private static readonly List<T>
			pressed = new(),
			justPressed = new(),
			justReleased = new();
		#endregion
	}
}
