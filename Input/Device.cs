namespace Purity.Input
{
	public abstract class Device<T> where T : Enum
	{
		public T[] Pressed { get; private set; } = Array.Empty<T>();
		public T[] JustPressed { get; private set; } = Array.Empty<T>();
		public T[] JustReleased { get; private set; } = Array.Empty<T>();

		public void Update()
		{
			pressed.Clear();
			prevPressed.Clear();
			justPressed.Clear();
			justReleased.Clear();

			var keyCount = (int)SFML.Window.Keyboard.Key.KeyCount;
			for(int i = 0; i < keyCount; i++)
			{
				var cur = (T)(object)i;
				if(IsPressed(cur))
					pressed.Add(cur);

				var isPr = pressed.Contains(cur);
				var wasPr = prevPressed.Contains(cur);

				if(wasPr && isPr == false)
					justReleased.Add(cur);
				else if(wasPr == false && isPr)
					justPressed.Add(cur);
			}

			Pressed = pressed.ToArray();
			JustPressed = justPressed.ToArray();
			JustReleased = justReleased.ToArray();
		}
		public bool ArePressed(params T[] inputs)
		{
			for(int i = 0; i < inputs?.Length; i++)
				if(pressed.Contains(inputs[i]) == false)
					return false;

			return true;
		}
		public bool AreJustPressed(params T[] inputs)
		{
			for(int i = 0; i < inputs?.Length; i++)
				if(justPressed.Contains(inputs[i]) == false)
					return false;

			return true;
		}
		public bool AreJustReleased(params T[] inputs)
		{
			for(int i = 0; i < inputs?.Length; i++)
				if(justReleased.Contains(inputs[i]) == false)
					return false;

			return true;
		}

		protected abstract bool IsPressed(T input);

		#region Backend
		private static readonly List<T>
			pressed = new(),
			prevPressed = new(),
			justPressed = new(),
			justReleased = new();
		#endregion
	}
}
