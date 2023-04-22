using SFML.Window;

namespace Pure.Window;

/// <summary>
/// Handles the physical button presses on a mouse and the on-screen OS cursor.
/// </summary>
public static class Mouse
{
	/// <summary>
	/// Each physical button on a standard mouse.
	/// </summary>
	public static class Button
	{
		public const int LEFT = 0, RIGHT = 1, MIDDLE = 2, EXTRA_1 = 3, EXTRA_2 = 4;
		internal const int COUNT = 5;
	}
	/// <summary>
	/// Each tile or OS cursor available.
	/// </summary>
	public static class Cursor
	{
		public const int TILE_ARROW = 0, TILE_ARROW_NO_TAIL = 1, TILE_HAND = 2, TILE_TEXT = 3,
			TILE_CROSSHAIR = 4, TILE_NO = 5, TILE_RESIZE_HORIZONTAL = 6, TILE_RESIZE_VERTICAL = 7,
			TILE_RESIZE_DIAGONAL_1 = 8, TILE_RESIZE_DIAGONAL_2 = 9, TILE_MOVE = 10, TILE_WAIT_1 = 11,
			TILE_WAIT_2 = 12, TILE_WAIT_3 = 13,

			SYSTEM_ARROW = 14, SYSTEM_ARROW_WAIT = 15, SYSTEM_WAIT = 16, SYSTEM_TEXT = 17,
			SYSTEM_HAND = 18, SYSTEM_RESIZE_HORINZONTAL = 19, SYSTEM_RESIZE_VERTICAL = 20,
			SYSTEM_RESIZE_DIAGONAL_2 = 21, SYSTEM_RESIZE_DIAGONAL_1 = 22, SYSTEM_MOVE = 23,
			SYSTEM_CROSSHAIR = 24, SYSTEM_HELP = 25, SYSTEM_NO = 26,

			NONE = 27;
	}

	/// <summary>
	/// The mouse cursor position relative to the OS window.
	/// </summary>
	public static (int x, int y) CursorPosition
	{
		get
		{
			Window.TryNoWindowException();
			var p = SFML.Window.Mouse.GetPosition(Window.window);
			return (p.X, p.Y);
		}
	}
	/// <summary>
	/// The mouse cursor type used to display the tile or OS cursor.
	/// </summary>
	public static int CursorGraphics
	{
		get
		{
			Window.TryNoWindowException();
			return cursor;
		}
		set
		{
			Window.TryNoWindowException();

			cursor = value;

			if (value != Cursor.NONE && value > Cursor.TILE_WAIT_3)
			{
				var arrow = Cursor.SYSTEM_ARROW;
				var sfmlEnum = (SFML.Window.Cursor.CursorType)(value - arrow);
				sysCursor.Dispose();
				sysCursor = new(sfmlEnum);

				Window.window.SetMouseCursor(sysCursor);
			}
		}
	}
	/// <summary>
	/// The mouse cursor color used to draw the tile cursor.
	/// </summary>
	public static uint CursorColor
	{
		get
		{
			Window.TryNoWindowException();
			return cursorColor;
		}
		set
		{
			Window.TryNoWindowException();
			cursorColor = value;
		}
	}
	/// <summary>
	/// Whether the mouse cursor is restricted of leaving the OS window.
	/// </summary>
	public static bool IsCursorRestriced
	{
		get
		{
			Window.TryNoWindowException();
			return isMouseGrabbed;
		}
		set
		{
			Window.TryNoWindowException();
			isMouseGrabbed = value;
			Window.window.SetMouseCursorGrabbed(value);
		}
	}
	/// <summary>
	/// Whether the mouse cursor hovers the OS window.
	/// </summary>
	public static bool IsCursorHoveringWindow
	{
		get
		{
			Window.TryNoWindowException();
			var w = Window.window;
			var pos = SFML.Window.Mouse.GetPosition(w);
			return pos.X > 0 && pos.X < w.Size.X && pos.Y > 0 && pos.Y < w.Size.Y;
		}
	}

	/// <summary>
	/// The currently pressed mouse buttons, in the order they were pressed.
	/// </summary>
	public static int[] ButtonsPressed
	{
		get
		{
			Window.TryNoWindowException();
			return pressed.ToArray();
		}
	}
	/// <summary>
	/// Returns the mouse scroll direction during the current <see cref="Window"/> activation.
	/// </summary>
	public static int ScrollDelta
	{
		get
		{
			Window.TryNoWindowException();
			return scrollData;
		}
		private set
		{
			Window.TryNoWindowException();
			scrollData = value;
		}
	}
	/// <summary>
	/// Returns whether a <paramref name="key"/> is among the <see cref="ButtonsPressed"/>.
	/// </summary>
	public static bool IsButtonPressed(int button)
	{
		Window.TryNoWindowException();
		return pressed.Contains(button);
	}

	#region Backend
	private static readonly List<int> pressed = new();
	private static readonly List<(float, float)> cursorOffsets = new()
		{
			(0.0f, 0.0f), (0.0f, 0.0f), (0.2f, 0.0f), (0.3f, 0.4f), (0.3f, 0.3f),
			(0.4f, 0.4f), (0.4f, 0.3f), (0.3f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f),
			(0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f),
		};

	private static int cursor, scrollData;
	private static uint cursorColor = uint.MaxValue;
	private static SFML.Window.Cursor sysCursor = new SFML.Window.Cursor(SFML.Window.Cursor.CursorType.Arrow);
	private static bool isMouseGrabbed;

	internal static void OnButtonPressed(object? s, MouseButtonEventArgs e)
	{
		pressed.Add((int)e.Button);
	}
	internal static void OnButtonReleased(object? s, MouseButtonEventArgs e)
	{
		pressed.Remove((int)e.Button);
	}
	internal static void OnWheelScrolled(object? s, MouseWheelScrollEventArgs e)
	{
		ScrollDelta = e.Delta < 0 ? -1 : 1;
	}

	internal static void Update()
	{
		ScrollDelta = 0;

		if (cursor > Cursor.TILE_WAIT_3)
			return;

		var (x, y) = Window.PositionFrom(CursorPosition);
		var (offX, offY) = cursorOffsets[CursorGraphics];

		Vertices.graphicsPath = "default";
		Vertices.tileSize = (8, 8);
		Window.DrawBasicTile((x - offX, y - offY), 442 + CursorGraphics, CursorColor);
	}
	internal static void CancelInput() => pressed.Clear();
	#endregion
}
