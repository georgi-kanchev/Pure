namespace Pure.Input
{
	internal class HotkeyManagerInstance<T> : Device<T> where T : Enum
	{
		public void OnRequired(T hotkey, Func<bool> method)
		{
			hotkeyEvents[hotkey] = method;
		}
		protected override bool IsPressedRaw(T input)
		{
			return hotkeyEvents.ContainsKey(input) && hotkeyEvents[input].Invoke();
		}

		private readonly Dictionary<T, Func<bool>> hotkeyEvents = new();
	}

	/// <summary>
	/// Exposes an input device functionality through an event. This is used to combine
	/// multiple device inputs into a single one. Or in other words: ties multiple
	/// inputs to a single <typeparamref name="T"/>.
	/// </summary>
	public static class HotkeyManager<T> where T : Enum
	{
		/// <summary>
		/// A collection of each currently pressed <typeparamref name="T"/>.
		/// </summary>
		public static T[] Pressed => i.Pressed;
		/// <summary>
		/// A collection of each newly pressed <typeparamref name="T"/>.
		/// </summary>
		public static T[] JustPressed => i.JustPressed;
		/// <summary>
		/// A collection of each newly no longer pressed <typeparamref name="T"/>.
		/// </summary>
		public static T[] JustReleased => i.JustReleased;

		/// <summary>
		/// Triggers events and provides each <typeparamref name="T"/> to the collections accordingly.
		/// </summary>
		public static void Update() => i.Update();

		/// <summary>
		/// Checks whether an <paramref name="input"/> is pressed and returns a result.
		/// </summary>
		public static bool IsPressed(T input) => i.IsPressed(input);
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is pressed
		/// and returns a result.
		/// </summary>
		public static bool IsJustPressed(T input) => i.IsJustPressed(input);
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public static bool IsJustReleased(T input) => i.IsJustReleased(input);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> press.
		/// </summary>
		public static void OnPressed(T input, Action method) => i.OnPressed(input, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> release.
		/// </summary>
		public static void OnReleased(T input, Action method) => i.OnReleased(input, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> hold.
		/// </summary>
		public static void WhilePressed(T input, Action method) => i.WhilePressed(input, method);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="hotkey"/> press. Only
		/// one <paramref name="method"/> can be subscribed at a time, any new subscription
		/// would overwrite the previous one.
		/// </summary>
		public static void OnRequired(T hotkey, Func<bool> method) => i.OnRequired(hotkey, method);

		#region Backend
		private static readonly HotkeyManagerInstance<T> i = new();
		#endregion
	}
}
