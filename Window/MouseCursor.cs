using SFML.Window;

namespace Pure.Window
{
	/// <summary>
	/// Handles the OS system cursor and the OS window tile cursor.
	/// </summary>
	public static class MouseCursor
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

		/// <summary>
		/// The mouse cursor position relative to the OS window.
		/// </summary>
		public static (int, int) Position
		{
			get
			{
				var p = Mouse.GetPosition(Window.window);
				return (p.X, p.Y);
			}
		}
		/// <summary>
		/// The mouse cursor type used by the OS window and <see cref="TryDrawCursor"/>.
		/// </summary>
		public static int Type
		{
			get => cursor;
			set
			{
				cursor = value;

				if(value != NONE && value > TILE_WAIT_3)
				{
					var arrow = SYSTEM_ARROW;
					var sfmlEnum = (SFML.Window.Cursor.CursorType)(value - arrow);
					sysCursor.Dispose();
					sysCursor = new(sfmlEnum);

					Window.window.SetMouseCursor(sysCursor);
				}
			}
		}
		/// <summary>
		/// The mouse cursor color used by <see cref="TryDrawCursor"/>.
		/// </summary>
		public static byte Color { get; set; } = 255;
		/// <summary>
		/// Whether the mouse cursor is restricted of leaving the OS window.
		/// </summary>
		public static bool IsRestriced
		{
			get => isMouseGrabbed;
			set { isMouseGrabbed = value; Window.window.SetMouseCursorGrabbed(value); }
		}

		#region Backend
		private static readonly List<(float, float)> cursorOffsets = new()
		{
			(0.0f, 0.0f), (0.0f, 0.0f), (0.2f, 0.0f), (0.3f, 0.4f), (0.3f, 0.3f),
			(0.4f, 0.4f), (0.4f, 0.3f), (0.3f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f),
			(0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f),
		};

		private static int cursor;
		private static Cursor sysCursor;
		private static bool isMouseGrabbed;

		static MouseCursor()
		{
			sysCursor = new SFML.Window.Cursor(SFML.Window.Cursor.CursorType.Arrow);
		}

		internal static void TryDrawCursor()
		{
			Window.window.SetMouseCursorVisible(IsHovering() == false);

			if(cursor > TILE_WAIT_3)
				return;

			var (x, y) = Window.PositionFrom(Position);
			var (offX, offY) = cursorOffsets[Type];

			Window.prevDrawTilemapGfxPath = "default";
			Window.prevDrawTilemapTileSz = (8, 8);
			Window.DrawSprite((x - offX, y - offY), 494 + Type, Color);
		}

		private static bool IsHovering()
		{
			var w = Window.window;
			var pos = SFML.Window.Mouse.GetPosition(w);
			return pos.X > 0 && pos.X < w.Size.X && pos.Y > 0 && pos.Y < w.Size.Y;
		}
		#endregion
	}
}
