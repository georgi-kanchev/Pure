namespace Pure.Input
{
	internal abstract class Device<T> where T : Enum
	{
		public T[] Pressed { get; private set; } = Array.Empty<T>();
		public T[] JustPressed { get; private set; } = Array.Empty<T>();
		public T[] JustReleased { get; private set; } = Array.Empty<T>();

		public void Update()
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
				{
					justReleased.Add(cur);
					Trigger(cur, releasedEvents);
					OnReleased(cur);
				}
				else if(wasPr == false && isPr)
				{
					justPressed.Add(cur);
					Trigger(cur, pressedEvents);
					OnPressed(cur);
				}

				if(isPr)
				{
					pressed.Add(cur);
					Trigger(cur, whilePressedEvents);
					WhilePressed(cur);
				}
			}

			Pressed = pressed.ToArray();
			JustPressed = justPressed.ToArray();
			JustReleased = justReleased.ToArray();
		}

		public bool IsPressed(T input)
		{
			return pressed.Contains(input);
		}
		public bool IsJustPressed(T input)
		{
			return justPressed.Contains(input);
		}
		public bool IsJustReleased(T input)
		{
			return justReleased.Contains(input);
		}

		public void OnPressed(T input, Action method)
		{
			Subscribe(input, pressedEvents, method);
		}
		public void OnReleased(T input, Action method)
		{
			Subscribe(input, releasedEvents, method);
		}
		public void WhilePressed(T input, Action method)
		{
			Subscribe(input, whilePressedEvents, method);
		}

		protected abstract bool IsPressedRaw(T input);

		#region Backend
		private readonly List<T>
			pressed = new(),
			justPressed = new(),
			justReleased = new();

		private readonly Dictionary<T, List<Action>> pressedEvents = new(), releasedEvents = new(),
			whilePressedEvents = new();

		protected virtual void OnPressed(T input) { }
		protected virtual void OnReleased(T input) { }
		protected virtual void WhilePressed(T input) { }

		private static void Subscribe(T input, Dictionary<T, List<Action>> events, Action method)
		{
			if(events.ContainsKey(input) == false)
				events[input] = new();

			events[input].Add(method);
		}
		private static void Trigger(T input, Dictionary<T, List<Action>> events)
		{
			if(events.ContainsKey(input) == false)
				return;

			for(int i = 0; i < events[input].Count; i++)
				events[input][i].Invoke();
		}
		#endregion
	}
}
