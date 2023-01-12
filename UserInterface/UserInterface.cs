namespace Pure.UserInterface
{
	public abstract class UserInterface
	{
		public (int, int) Position { get; set; }
		public (int, int) Size
		{
			get => size;
			set
			{
				var (x, y) = value;
				size = (Math.Max(x, 1), Math.Max(y, 1));
			}
		}
		public string Text { get; set; } = "";

		public bool IsFocused
		{
			get => focusedObject == this;
			private set => focusedObject = value ? this : null;
		}
		public bool IsHovered { get; private set; }
		public bool IsPressed => IsHovered && Input.IsPressed;
		public bool IsClicked { get; private set; }

		public bool IsJustHovered => IsHovered && wasHovered == false;
		public bool IsJustUnovered => IsHovered == false && wasHovered;
		public bool IsJustTriggered { get; private set; }
		public bool IsJustPressed => IsPressed && Input.wasPressed == false;
		public bool IsJustReleased => IsPressed == false && Input.wasPressed;

		public static string? CopiedText { get; set; } = "";
		public static int MouseCursorTile { get; internal set; }
		public static int MouseCursorSystem { get; internal set; }

		public UserInterface((int, int) position, (int, int) size)
		{
			Position = position;
			Size = size;
		}

		public void Update()
		{
			wasFocused = IsFocused;
			wasHovered = IsHovered;

			UpdateHovered();

			if(Input.wasPressed == false && Input.IsPressed && IsHovered)
				IsFocused = true;

			if(Input.IsKeyJustPressed(ESCAPE))
				IsFocused = false;

			TryTrigger();
			OnUpdate();

		}

		public static void ApplyInput(Input input, (int, int) tilemapSize)
		{
			IsInputCanceled = false;

			MouseCursorTile = 0;
			MouseCursorSystem = 0;
			TilemapSize = (Math.Abs(tilemapSize.Item1), Math.Abs(tilemapSize.Item2));

			Input.wasPressed = Input.IsPressed;
			Input.prevTypedSymbols = Input.TypedSymbols;
			Input.prevPosition = Input.Position;
			Input.prevPressedKeys.Clear();
			Input.prevPressedKeys.AddRange(Input.pressedKeys);

			Input.IsPressed = input.IsPressed;
			Input.Position = input.Position;
			Input.PressedKeys = input.PressedKeys;
			Input.TypedSymbols = input.TypedSymbols;

			if(Input.wasPressed == false && Input.IsPressed)
				focusedObject = null;
		}

		#region Backend
		protected const int ESCAPE = 36, CONTROL_LEFT = 37, SHIFT_LEFT = 38, ALT_LEFT = 39,
			CONTROL_RIGHT = 41, SHIFT_RIGHT = 42, ALT_RIGHT = 43, ENTER = 58, RETURN = 58,
			BACKSPACE = 59, TAB = 60, PAGE_UP = 61, PAGE_DOWN = 62,
			END = 63, HOME = 64, INSERT = 65, DELETE = 66,
			ARROW_LEFT = 71,
			ARROW_RIGHT = 72,
			ARROW_UP = 73,
			ARROW_DOWN = 74;

		protected const int TILE_ARROW = 0, TILE_ARROW_NO_TAIL = 1, TILE_HAND = 2, TILE_TEXT = 3, TILE_CROSSHAIR = 4,
			TILE_NO = 5, TILE_RESIZE_HORIZONTAL = 6, TILE_RESIZE_VERTICAL = 7, TILE_RESIZE_DIAGONAL_1 = 8,
			TILE_RESIZE_DIAGONAL_2 = 9, TILE_MOVE = 10, TILE_WAIT_1 = 11, TILE_WAIT_2 = 12, TILE_WAIT_3 = 13,

			SYSTEM_ARROW = 14, SYSTEM_ARROW_WAIT = 15, SYSTEM_WAIT = 16, SYSTEM_TEXT = 17, SYSTEM_HAND = 18,
			SYSTEM_RESIZE_HORINZONTAL = 19, SYSTEM_RESIZE_VERTICAL = 20, SYSTEM_RESIZE_DIAGONAL_2 = 21,
			SYSTEM_RESIZE_DIAGONAL_1 = 22, SYSTEM_MOVE = 23, SYSTEM_CROSSHAIR = 24, SYSTEM_HELP = 25,
			SYSTEM_NO = 26,

			NONE = 27;

		private (int, int) size;
		private static UserInterface? focusedObject;

		private bool wasFocused, wasHovered;

		protected static bool IsInputCanceled { get; private set; }
		protected static (int, int) TilemapSize { get; private set; }

		protected static Input Input { get; } = new();

		protected abstract void OnUpdate();
		protected void TryTrigger()
		{
			IsJustTriggered = false;

			if(IsFocused == false)
			{
				IsClicked = false;
				return;
			}

			if(IsHovered && Input.IsReleased && IsClicked)
			{
				IsClicked = false;
				IsJustTriggered = true;
			}

			if(IsHovered && Input.IsPressed && Input.wasPressed == false)
				IsClicked = true;

			if(Input.IsReleased)
				IsClicked = false;
		}

		private void UpdateHovered()
		{
			var (ix, iy) = Input.Position;
			var (x, y) = Position;
			var (w, h) = Size;
			var isHoveredX = ix >= x && ix < x + w;
			var isHoveredY = iy >= y && iy < y + h;
			if(w < 0)
				isHoveredX = ix > x + w && ix <= x;
			if(h < 0)
				isHoveredY = iy > y + h && iy <= y;

			IsHovered = isHoveredX && isHoveredY;
		}

		protected static void SetTileAndSystemCursor(int tileCursor)
		{
			MouseCursorTile = tileCursor;
			MouseCursorSystem = tileCursor + SYSTEM_ARROW;
		}
		#endregion
	}
}
