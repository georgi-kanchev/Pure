namespace Pure.UserInterface;

using System.Diagnostics;

public abstract class UserInterface
{
	protected static class UserEvent
	{
		public const int FOCUS = 0, UNFOCUS = 1, HOVER = 2, UNHOVER = 3,
			PRESS = 4, RELEASE = 5, TRIGGER = 6, DRAG = 7, DRAGGED = 8,
			HOLD = 9, SCROLL = 10;
	}
	protected static class MouseCursor
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
	protected class Input
	{
		public const int ESCAPE = 36, CONTROL_LEFT = 37, SHIFT_LEFT = 38, ALT_LEFT = 39,
			CONTROL_RIGHT = 41, SHIFT_RIGHT = 42, ALT_RIGHT = 43, ENTER = 58, RETURN = 58,
			BACKSPACE = 59, TAB = 60, PAGE_UP = 61, PAGE_DOWN = 62,
			END = 63, HOME = 64, INSERT = 65, DELETE = 66,
			ARROW_LEFT = 71, ARROW_RIGHT = 72, ARROW_UP = 73, ARROW_DOWN = 74;

		public bool IsPressed { get; internal set; }
		public bool IsReleased => IsPressed == false && wasPressed;
		public bool IsJustPressed => wasPressed == false && IsPressed;
		public bool IsJustReleased => wasPressed && IsPressed == false;
		public bool IsJustHeld { get; internal set; }

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

				if (value != null && value.Length != 0)
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

	public bool IsHidden { get; set; }
	public bool IsDisabled { get; set; }
	public bool IsFocused
	{
		get => Focused == this;
		protected set => Focused = value ? this : null;
	}
	public bool IsHovered { get; private set; }
	public bool IsPressed => IsHovered && CurrentInput.IsPressed;
	public bool IsClicked { get; private set; }

	public static string? TextCopied { get; set; } = "";

	protected static UserInterface? Focused { get; private set; }
	protected static bool IsInputCanceled { get; private set; }
	protected static Input CurrentInput { get; } = new();
	protected static (int, int) TilemapSize { get; private set; }

	public UserInterface((int, int) position, (int, int) size)
	{
		Position = position;
		Size = size;

		hold.Start();
		holdTrigger.Start();
	}

	public void Update()
	{
		wasFocused = IsFocused;
		wasHovered = IsHovered;

		UpdateHovered();

		if (IsDisabled)
		{
			if (IsHovered)
				SetMouseCursor(MouseCursor.TILE_NO);

			OnUpdate();
			return;
		}

		if (CurrentInput.wasPressed == false && CurrentInput.IsPressed && IsHovered)
			IsFocused = true;

		if (CurrentInput.IsKeyJustPressed(Input.ESCAPE))
			IsFocused = false;

		TryTrigger();

		if (IsFocused && wasFocused == false)
			TriggerUserEvent(UserEvent.FOCUS);
		if (IsFocused == false && wasFocused)
			TriggerUserEvent(UserEvent.UNFOCUS);
		if (IsHovered && wasHovered == false)
			TriggerUserEvent(UserEvent.HOVER);
		if (IsHovered == false && wasHovered)
			TriggerUserEvent(UserEvent.UNHOVER);
		if (IsPressed && CurrentInput.wasPressed == false)
			TriggerUserEvent(UserEvent.PRESS);
		if (IsPressed == false && CurrentInput.wasPressed)
			TriggerUserEvent(UserEvent.RELEASE);
		if (IsPressed && CurrentInput.IsJustHeld)
			TriggerUserEvent(UserEvent.HOLD);
		if (CurrentInput.ScrollDelta != 0)
			TriggerUserEvent(UserEvent.SCROLL);

		OnUpdate();
	}
	public void Trigger()
	{
		TriggerUserEvent(UserEvent.TRIGGER);
	}

	protected void SetMouseCursor(int tileCursor)
	{
		cursorTile = tileCursor;
		cursorSystem = tileCursor + MouseCursor.SYSTEM_ARROW;
	}

	protected void TriggerUserEvent(int userEvent)
	{
		OnUserEvent(userEvent);

		if (userEvents.ContainsKey(userEvent) == false)
			return;

		for (int i = 0; i < userEvents[userEvent].Count; i++)
			userEvents[userEvent][i].Invoke();
	}
	protected internal void SubscribeToUserEvent(int userEvent, Action method)
	{
		if (userEvents.ContainsKey(userEvent) == false)
			userEvents[userEvent] = new();

		userEvents[userEvent].Add(method);
	}
	protected virtual void OnUserEvent(int userEvent) { }

	protected abstract void OnUpdate();

	public static void ApplyInput(bool isPressed, (float, float) position, int scrollDelta,
		int[] keysPressed, string keysTyped, (int, int) tilemapSize)
	{
		IsInputCanceled = false;

		cursorTile = 0;
		cursorSystem = 0;
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

		if (CurrentInput.IsJustPressed)
			hold.Restart();

		CurrentInput.IsJustHeld = false;
		if (hold.Elapsed.TotalSeconds > 0.5f && holdTrigger.Elapsed.TotalSeconds > 0.05f)
		{
			holdTrigger.Restart();
			CurrentInput.IsJustHeld = true;
		}

		if (CurrentInput.wasPressed == false && CurrentInput.IsPressed)
			Focused = null;
	}
	public static void GetMouseCursorGraphics(out int tile, out int system)
	{
		tile = cursorTile;
		system = cursorSystem;
	}

	#region Backend
	private static int cursorTile, cursorSystem;
	private (int, int) size;
	private static readonly Stopwatch hold = new(), holdTrigger = new();

	private bool wasFocused, wasHovered;

	private readonly Dictionary<int, List<Action>> userEvents = new();

	public override string ToString()
	{
		return $"{GetType().Name} \"{Text}\"";
	}

	private void UpdateHovered()
	{
		var (ix, iy) = CurrentInput.Position;
		var (x, y) = Position;
		var (w, h) = Size;
		var isHoveredX = ix >= x && ix < x + w;
		var isHoveredY = iy >= y && iy < y + h;
		if (w < 0)
			isHoveredX = ix > x + w && ix <= x;
		if (h < 0)
			isHoveredY = iy > y + h && iy <= y;

		IsHovered = isHoveredX && isHoveredY;
	}
	private void TryTrigger()
	{
		if (IsFocused == false || IsDisabled)
		{
			IsClicked = false;
			return;
		}

		if (IsHovered && CurrentInput.IsReleased && IsClicked)
		{
			IsClicked = false;
			TriggerUserEvent(UserEvent.TRIGGER);
		}

		if (IsHovered && CurrentInput.IsJustPressed)
			IsClicked = true;

		if (CurrentInput.IsReleased)
			IsClicked = false;
	}
	#endregion
}
