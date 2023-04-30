using SFML.Window;

namespace Pure.Window;

/// <summary>
/// Handles mouse input.
/// </summary>
public static class Mouse
{
	/// <summary>
	/// Provides constant values for common mouse button identifiers.
	/// </summary>
	public static class Button
	{
		public const int LEFT = 0, RIGHT = 1, MIDDLE = 2, EXTRA_1 = 3, EXTRA_2 = 4;
		internal const int COUNT = 5;
	}
	/// <summary>
	/// Provides constant values for mouse cursor graphics identifiers.
	/// </summary>
	public static class Cursor
	{
		public const int TILE_ARROW = 0, TILE_ARROW_NO_TAIL = 1, TILE_HAND = 2, TILE_TEXT = 3,
			TILE_CROSSHAIR = 4, TILE_NO = 5, TILE_RESIZE = 6, TILE_RESIZE_DIAGONAL = 7,
			TILE_MOVE = 8, TILE_WAIT = 9,

			SYSTEM_ARROW = 10, SYSTEM_ARROW_WAIT = 11, SYSTEM_WAIT = 12, SYSTEM_TEXT = 13,
			SYSTEM_HAND = 14, SYSTEM_RESIZE_HORINZONTAL = 15, SYSTEM_RESIZE_VERTICAL = 16,
			SYSTEM_RESIZE_DIAGONAL_2 = 17, SYSTEM_RESIZE_DIAGONAL_1 = 18, SYSTEM_MOVE = 19,
			SYSTEM_CROSSHAIR = 20, SYSTEM_HELP = 21, SYSTEM_NO = 22,

			NONE = 23;
	}

	/// <summary>
	/// Gets the current position of the mouse cursor.
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
	/// Gets or sets the graphics for the mouse cursor.
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

			if (value != Cursor.NONE && value > Cursor.TILE_WAIT)
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
	/// Gets or sets the color of the mouse cursor (if tile).
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
	/// Gets or sets whether the mouse cursor is restricted to the window.
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
	/// Gets whether the mouse cursor is hovering over the window.
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
	/// Gets an array of currently pressed mouse buttons, in order.
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
	/// Gets the scroll delta of the mouse.
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
	/// Gets whether the specified mouse button is currently pressed.
	/// </summary>
	/// <param name="button">The button to check.</param>
	/// <returns>True if the button is currently pressed, false otherwise.</returns>
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

		if (cursor > Cursor.TILE_WAIT)
			return;

		var (x, y) = Window.PositionFrom(CursorPosition);
		var (offX, offY) = cursorOffsets[CursorGraphics];

		Vertices.graphicsPath = "default";
		Vertices.tileSize = (8, 8);

		(int id, uint tint, sbyte ang, bool h, bool v) tile = default;
		tile.id = 442 + CursorGraphics;
		tile.tint = CursorColor;
		Window.DrawTile((x - offX, y - offY), tile);
	}
	internal static void CancelInput() => pressed.Clear();
	#endregion
}
