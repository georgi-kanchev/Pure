namespace Pure.Input
{
	/// <summary>
	/// The physical buttons on a mouse.
	/// </summary>
	public static class Button
	{
		public const int LEFT = 0, RIGHT = 1, MIDDLE = 2, EXTRA_1 = 3, EXTRA_2 = 4;
		internal const int COUNT = 5;
	}

	/// <summary>
	/// Handles input from a physical mouse.
	/// </summary>
	public static class Mouse
	{
		/// <summary>
		/// The cursor position on the screen.
		/// </summary>
		public static (int, int) Position
		{
			get { var pos = SFML.Window.Mouse.GetPosition(); return (pos.X, pos.Y); }
			set => SFML.Window.Mouse.SetPosition(new(value.Item1, value.Item2));
		}

		/// <summary>
		/// A collection of each currently pressed <see cref="Button"/>.
		/// </summary>
		public static int[] Pressed => i.Pressed;
		/// <summary>
		/// A collection of each newly pressed <see cref="Button"/>.
		/// </summary>
		public static int[] JustPressed => i.JustPressed;
		/// <summary>
		/// A collection of each newly no longer pressed <see cref="Button"/>.
		/// </summary>
		public static int[] JustReleased => i.JustReleased;

		/// <summary>
		/// Checks whether a <paramref name="button"/> is pressed and returns a result.
		/// </summary>
		public static bool IsPressed(int button) => i.IsPressed(button);
		/// <summary>
		/// Checks whether this is the very moment a <paramref name="button"/> is pressed
		/// and returns a result.
		/// </summary>
		public static bool IsJustPressed(int button) => i.IsJustPressed(button);
		/// <summary>
		/// Checks whether this is the very moment a <paramref name="button"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public static bool IsJustReleased(int button) => i.IsJustReleased(button);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="button"/> press.
		/// </summary>
		public static void OnPressed(int button, Action method) => i.OnPressed(button, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="button"/> release.
		/// </summary>
		public static void OnReleased(int button, Action method) => i.OnReleased(button, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="button"/> hold.
		/// </summary>
		public static void WhilePressed(int button, Action method) => i.WhilePressed(button, method);

		#region Backend
		private static readonly MouseInstance i = new();
		#endregion
	}

	#region Backend
	internal class MouseInstance : Device
	{
		protected override bool IsPressedRaw(int input)
		{
			var btn = (SFML.Window.Mouse.Button)input;
			return SFML.Window.Mouse.IsButtonPressed(btn);
		}
		protected override int GetInputCount() => Button.COUNT;
	}
	#endregion
}
