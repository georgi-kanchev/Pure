namespace Pure.Window;

/// <summary>
/// Handles the physical buttons on a mouse.
/// </summary>
public static class MouseButton
{
	public const int LEFT = 0, RIGHT = 1, MIDDLE = 2, EXTRA_1 = 3, EXTRA_2 = 4;
	internal const int COUNT = 5;

	public static int[] Pressed => pressed.ToArray();
	public static int ScrollDelta { get; private set; }

	public static bool IsPressed(int button) => pressed.Contains(button);

	public static void OnPressed(int button, Action method)
	{
		Window.window.MouseButtonPressed += (s, e) =>
		{
			if (button == (int)e.Button)
				method?.Invoke();
		};
	}
	public static void OnReleased(int button, Action method)
	{
		Window.window.MouseButtonReleased += (s, e) =>
		{
			if (button == (int)e.Button)
				method?.Invoke();
		};
	}
	public static void OnScrolled(Action method)
	{
		Window.window.MouseWheelScrolled += (s, e) => method?.Invoke();
	}

	#region Backend
	private static readonly List<int> pressed = new();

	static MouseButton()
	{
		Window.window.MouseButtonPressed += (s, e) => pressed.Add((int)e.Button);
		Window.window.MouseButtonReleased += (s, e) => pressed.Remove((int)e.Button);
		Window.window.MouseWheelScrolled += (s, e) => ScrollDelta = e.Delta < 0 ? -1 : 1;
	}

	internal static void Update()
	{
		ScrollDelta = 0;
	}
	internal static void CancelInput() => pressed.Clear();
	#endregion
}
