namespace Purity.Input
{
	/// <summary>
	/// The physical buttons on a mouse.
	/// </summary>
#pragma warning disable CS1591
	public enum Button { Left, Right, Middle, Extra1, Extra2 }
#pragma warning restore CS1591

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
		public static Button[] Pressed => i.Pressed;
		/// <summary>
		/// A collection of each newly pressed <see cref="Button"/>.
		/// </summary>
		public static Button[] JustPressed => i.JustPressed;
		/// <summary>
		/// A collection of each newly no longer pressed <see cref="Button"/>.
		/// </summary>
		public static Button[] JustReleased => i.JustReleased;

		/// <summary>
		/// Triggers events and provides each <see cref="Button"/> to the collections
		/// accordingly.
		/// </summary>
		public static void Update() => i.Update();

		/// <summary>
		/// Checks whether an <paramref name="input"/> is pressed and returns a result.
		/// </summary>
		public static bool IsPressed(Button input) => i.IsPressed(input);
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is pressed
		/// and returns a result.
		/// </summary>
		public static bool IsJustPressed(Button input) => i.IsJustPressed(input);
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public static bool IsJustReleased(Button input) => i.IsJustReleased(input);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> press.
		/// </summary>
		public static void OnPressed(Button input, Action method) => i.OnPressed(input, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> release.
		/// </summary>
		public static void OnReleased(Button input, Action method) => i.OnReleased(input, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> hold.
		/// </summary>
		public static void WhilePressed(Button input, Action method) => i.WhilePressed(input, method);

		#region Backend
		private static readonly MouseInstance i = new();
		#endregion
	}

	#region Backend
	internal class MouseInstance : Device<Button>
	{
		protected override bool IsPressedRaw(Button input)
		{
			var btn = (SFML.Window.Mouse.Button)input;
			return SFML.Window.Mouse.IsButtonPressed(btn);
		}
	}
	#endregion
}
