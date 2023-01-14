namespace Pure.UserInterface
{
	public abstract class UserInterface
	{
		public enum When
		{
			Trigger, Focus, Unfocus, Hover, Unhover, Press, Release, Drag
		}

		protected class Input
		{
			public bool IsPressed { get; internal set; }
			public bool IsReleased => IsPressed == false && wasPressed;
			public bool IsJustPressed => wasPressed == false && IsPressed;
			public bool IsJustReleased => wasPressed && IsPressed == false;

			public (float, float) Position { get; internal set; }
			public (float, float) PositionPrevious { get; internal set; }
			public string? Typed { get; internal set; }
			public string? TypedPrevious { get; internal set; }
			public int ScrollDelta { get; internal set; }

			public int[]? PressedKeys
			{
				get => pressedKeys.ToArray();
				internal set
				{
					pressedKeys.Clear();

					if(value != null && value.Length != 0)
						pressedKeys.AddRange(value);
				}
			}

			public bool IsKeyPressed(int key)
			{
				return pressedKeys.Contains(key);
			}
			public bool IsKeyJustPressed(int key)
			{
				return IsKeyPressed(key) && prevPressedKeys.Contains(key) == false;
			}
			public bool IsKeyJustReleased(int key)
			{
				return IsKeyPressed(key) == false && prevPressedKeys.Contains(key);
			}

			#region Backend
			internal readonly List<int> pressedKeys = new(), prevPressedKeys = new();
			internal bool wasPressed;
			#endregion
		}

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
		public bool IsPressed => IsHovered && CurrentInput.IsPressed;
		public bool IsClicked { get; private set; }

		public static string? CopiedText { get; set; } = "";
		public static int MouseCursorTile { get; internal set; }
		public static int MouseCursorSystem { get; internal set; }

		public UserInterface((int, int) position, (int, int) size)
		{
			Position = position;
			Size = size;
		}

		public void InCaseOf(When when, Action callMethod)
		{
			if(events.ContainsKey(when) == false)
				events[when] = new();

			events[when].Add(callMethod);
		}

		public void Update()
		{
			wasFocused = IsFocused;
			wasHovered = IsHovered;

			UpdateHovered();

			if(CurrentInput.wasPressed == false && CurrentInput.IsPressed && IsHovered)
				IsFocused = true;

			if(CurrentInput.IsKeyJustPressed(ESCAPE))
				IsFocused = false;

			TryTrigger();

			if(IsFocused && wasFocused == false)
				TriggerEvent(When.Focus);
			if(IsFocused == false && wasFocused)
				TriggerEvent(When.Unfocus);
			if(IsHovered && wasHovered == false)
				TriggerEvent(When.Hover);
			if(IsHovered == false && wasHovered)
				TriggerEvent(When.Unhover);
			if(IsPressed && CurrentInput.wasPressed == false)
				TriggerEvent(When.Press);
			if(IsPressed == false && CurrentInput.wasPressed)
				TriggerEvent(When.Release);

			OnUpdate();

		}
		public void Trigger()
		{
			TriggerEvent(When.Trigger);
		}

		public static void ApplyInput(bool isPressed, (float, float) position, int scrollDelta,
			int[] keysPressed, string keysTyped, (int, int) tilemapSize)
		{
			IsInputCanceled = false;

			MouseCursorTile = 0;
			MouseCursorSystem = 0;
			TilemapSize = (Math.Abs(tilemapSize.Item1), Math.Abs(tilemapSize.Item2));

			CurrentInput.wasPressed = CurrentInput.IsPressed;
			CurrentInput.TypedPrevious = CurrentInput.Typed;
			CurrentInput.PositionPrevious = CurrentInput.Position;
			CurrentInput.prevPressedKeys.Clear();
			CurrentInput.prevPressedKeys.AddRange(CurrentInput.pressedKeys);

			CurrentInput.IsPressed = isPressed;
			CurrentInput.Position = position;
			CurrentInput.PressedKeys = keysPressed;
			CurrentInput.Typed = keysTyped
				.Replace("\n", "")
				.Replace("\t", "")
				.Replace("\r", "");
			CurrentInput.ScrollDelta = scrollDelta;

			if(CurrentInput.wasPressed == false && CurrentInput.IsPressed)
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

		private readonly Dictionary<When, List<Action>> events = new();
		protected static bool IsInputCanceled { get; private set; }
		protected static (int, int) TilemapSize { get; private set; }

		protected static Input CurrentInput { get; } = new();

		protected void TryTrigger()
		{
			if(IsFocused == false)
			{
				IsClicked = false;
				return;
			}

			if(IsHovered && CurrentInput.IsReleased && IsClicked)
			{
				IsClicked = false;
				TriggerEvent(When.Trigger);
			}

			if(IsHovered && CurrentInput.IsPressed && CurrentInput.wasPressed == false)
				IsClicked = true;

			if(CurrentInput.IsReleased)
				IsClicked = false;
		}
		private void UpdateHovered()
		{
			var (ix, iy) = CurrentInput.Position;
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

		protected void TriggerEvent(When when)
		{
			OnEvent(when);

			if(events.ContainsKey(when) == false)
				return;

			for(int i = 0; i < events[when].Count; i++)
				events[when][i].Invoke();
		}
		protected virtual void OnEvent(When when) { }

		protected abstract void OnUpdate();

		protected static void SetTileAndSystemCursor(int tileCursor)
		{
			MouseCursorTile = tileCursor;
			MouseCursorSystem = tileCursor + SYSTEM_ARROW;
		}
		#endregion
	}
}
