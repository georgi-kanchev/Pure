using SharpHook;

namespace Pure.Input
{
	internal abstract class Device
	{
		public int[] Pressed { get; private set; } = Array.Empty<int>();
		public int[] JustPressed { get; private set; } = Array.Empty<int>();
		public int[] JustReleased { get; private set; } = Array.Empty<int>();

		static Device()
		{
			thread = new(() => input.Run());
			thread.Start();
		}

		public bool IsPressed(int input)
		{
			return pressed.Contains(input);
		}
		public bool IsJustPressed(int input)
		{
			return justPressed.Contains(input);
		}
		public bool IsJustReleased(int input)
		{
			return justReleased.Contains(input);
		}

		public void OnPressed(int input, Action method)
		{
			Subscribe(input, pressedEvents, method);
		}
		public void OnReleased(int input, Action method)
		{
			Subscribe(input, releasedEvents, method);
		}
		public void WhilePressed(int input, Action method)
		{
			Subscribe(input, whilePressedEvents, method);
		}

		protected abstract bool IsPressedRaw(int input);
		protected abstract int GetInputCount();

		#region Backend
		internal readonly List<int>
			pressed = new(),
			justPressed = new(),
			justReleased = new();

		private static readonly Thread thread;
		internal static readonly SimpleGlobalHook input = new();
		internal readonly Dictionary<int, List<Action>> pressedEvents = new(), releasedEvents = new(),
			whilePressedEvents = new();

		protected virtual void OnPressed(int input) { }
		protected virtual void OnReleased(int input) { }
		protected virtual void WhilePressed(int input) { }

		private static void Subscribe(int input, Dictionary<int, List<Action>> events, Action method)
		{
			if(events.ContainsKey(input) == false)
				events[input] = new();

			events[input].Add(method);
		}
		internal static void Trigger(int input, Dictionary<int, List<Action>> events)
		{
			if(events.ContainsKey(input) == false)
				return;

			for(int i = 0; i < events[input].Count; i++)
				events[input][i].Invoke();
		}
		#endregion
	}
}
