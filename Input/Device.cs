﻿namespace Pure.Input
{
	internal abstract class Device
	{
		public int[] Pressed { get; private set; } = Array.Empty<int>();
		public int[] JustPressed { get; private set; } = Array.Empty<int>();
		public int[] JustReleased { get; private set; } = Array.Empty<int>();

		public void Update()
		{
			var prevPressed = new List<int>(pressed);
			pressed.Clear();
			justPressed.Clear();
			justReleased.Clear();

			var count = GetInputCount();
			for(int i = 0; i < count; i++)
			{
				var isPr = IsPressedRaw(i);
				var wasPr = prevPressed.Contains(i);

				if(wasPr && isPr == false)
				{
					justReleased.Add(i);
					Trigger(i, releasedEvents);
					OnReleased(i);
				}
				else if(wasPr == false && isPr)
				{
					justPressed.Add(i);
					Trigger(i, pressedEvents);
					OnPressed(i);
				}

				if(isPr)
				{
					pressed.Add(i);
					Trigger(i, whilePressedEvents);
					WhilePressed(i);
				}
			}

			Pressed = pressed.ToArray();
			JustPressed = justPressed.ToArray();
			JustReleased = justReleased.ToArray();
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
		private readonly List<int>
			pressed = new(),
			justPressed = new(),
			justReleased = new();

		private readonly Dictionary<int, List<Action>> pressedEvents = new(), releasedEvents = new(),
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
		private static void Trigger(int input, Dictionary<int, List<Action>> events)
		{
			if(events.ContainsKey(input) == false)
				return;

			for(int i = 0; i < events[input].Count; i++)
				events[input][i].Invoke();
		}
		#endregion
	}
}
