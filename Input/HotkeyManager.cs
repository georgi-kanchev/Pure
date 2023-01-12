namespace Pure.Input
{
	/// <summary>
	/// Exposes an input device functionality through an event. This is used to combine
	/// multiple device inputs into a single one. Or in other words: ties multiple
	/// inputs to a single hotkey.
	/// </summary>
	public static class HotkeyManager
	{
		/// <summary>
		/// A collection of the currently pressed hotkeys.
		/// </summary>
		public static int[] Pressed => i.Pressed;
		/// <summary>
		/// A collection of the newly pressed hotkeys.
		/// </summary>
		public static int[] JustPressed => i.JustPressed;
		/// <summary>
		/// A collection of the newly no longer pressed hotkeys.
		/// </summary>
		public static int[] JustReleased => i.JustReleased;

		/// <summary>
		/// Triggers events and provides each hotkey (up to a
		/// <paramref name="hotkeyCount"/>) to the collections accordingly.
		/// </summary>
		public static void Update(int hotkeyCount)
		{
			count = hotkeyCount;
			i.Update();
		}

		/// <summary>
		/// Checks whether a <paramref name="hotkey"/> is pressed and returns a result.
		/// </summary>
		public static bool IsPressed(int hotkey) => i.IsPressed(hotkey);
		/// <summary>
		/// Checks whether this is the very moment a <paramref name="hotkey"/> is pressed
		/// and returns a result.
		/// </summary>
		public static bool IsJustPressed(int hotkey) => i.IsJustPressed(hotkey);
		/// <summary>
		/// Checks whether this is the very moment a <paramref name="hotkey"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public static bool IsJustReleased(int hotkey) => i.IsJustReleased(hotkey);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="hotkey"/> press.
		/// </summary>
		public static void OnPressed(int hotkey, Action method) => i.OnPressed(hotkey, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="hotkey"/> release.
		/// </summary>
		public static void OnReleased(int hotkey, Action method) => i.OnReleased(hotkey, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="hotkey"/> hold.
		/// </summary>
		public static void WhilePressed(int hotkey, Action method) => i.WhilePressed(hotkey, method);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="hotkey"/> press. Only
		/// one <paramref name="method"/> at a time can be subscribed to a <paramref name="hotkey"/>,
		/// any new subscription overwrites the previous one.
		/// </summary>
		public static void OnRequired(int hotkey, Func<bool> method) => i.OnRequired(hotkey, method);

		#region Backend
		internal static int count;

		private static readonly HotkeyManagerInstance i = new();
		#endregion
	}
	#region Backend
	internal class HotkeyManagerInstance : Device
	{
		public void OnRequired(int hotkey, Func<bool> method)
		{
			hotkeyEvents[hotkey] = method;
		}
		protected override bool IsPressedRaw(int input)
		{
			return hotkeyEvents.ContainsKey(input) && hotkeyEvents[input].Invoke();
		}

		private readonly Dictionary<int, Func<bool>> hotkeyEvents = new();

		protected override int GetInputCount() => HotkeyManager.count;
	}
	#endregion
}
