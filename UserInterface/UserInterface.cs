namespace Pure.UserInterface;

using System.Diagnostics;

/// <summary>
/// Represents a user interface element that the user can interact with and receive some
/// results back.
/// </summary>
public abstract class UserInterface
{
	/// <summary>
	/// Contains static values for various user interface events that can be triggered by user input.
	/// </summary>
	protected static class UserEvent
	{
		public const int FOCUS = 0, UNFOCUS = 1, HOVER = 2, UNHOVER = 3,
			PRESS = 4, RELEASE = 5, TRIGGER = 6, DRAG = 7, DRAGGED = 8,
			HOLD = 9, SCROLL = 10;
	}
	/// <summary>
	/// Graphics resulted from a user interaction with the user interface.
	/// </summary>
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
	/// <summary>
	/// Represents user input for received by the user interface.
	/// </summary>
	protected class Input
	{
		/// <summary>
		/// Represents the physical keyboard keys used for input by the user interface.
		/// </summary>
		public static class Key
		{
			public const int ESCAPE = 36, CONTROL_LEFT = 37, SHIFT_LEFT = 38, ALT_LEFT = 39,
				CONTROL_RIGHT = 41, SHIFT_RIGHT = 42, ALT_RIGHT = 43, ENTER = 58, RETURN = 58,
				BACKSPACE = 59, TAB = 60, PAGE_UP = 61, PAGE_DOWN = 62,
				END = 63, HOME = 64, INSERT = 65, DELETE = 66,
				ARROW_LEFT = 71, ARROW_RIGHT = 72, ARROW_UP = 73, ARROW_DOWN = 74;
		}
		/// <summary>
		/// Gets a value indicating whether the input is currently pressed.
		/// </summary>
		public bool IsPressed { get; internal set; }
		/// <summary>
		/// Gets a value indicating whether the input has just been pressed.
		/// </summary>
		public bool IsJustPressed => wasPressed == false && IsPressed;
		/// <summary>
		/// Gets a value indicating whether the input has just been released.
		/// </summary>
		public bool IsJustReleased => wasPressed && IsPressed == false;
		/// <summary>
		/// Gets a value indicating whether the input has just been held.
		/// </summary>
		public bool IsJustHeld { get; internal set; }

		/// <summary>
		/// Gets the current position of the input.
		/// </summary>
		public (float, float) Position { get; internal set; }
		/// <summary>
		/// Gets the previous position of the input.
		/// </summary>
		public (float, float) PositionPrevious { get; internal set; }
		/// <summary>
		/// Gets the most recent typed text.
		/// </summary>
		public string? Typed { get; internal set; }
		/// <summary>
		/// Gets the previous typed text.
		/// </summary>
		public string? TypedPrevious { get; internal set; }
		/// <summary>
		/// Gets the scroll delta of the input.
		/// </summary>
		public int ScrollDelta { get; internal set; }

		/// <summary>
		/// Gets an array of currently pressed keys.
		/// </summary>
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

		/// <param name="key">
		/// The key to check.</param>
		/// <returns>True if the specified key is pressed, false otherwise.</returns>
		public bool IsKeyPressed(int key)
		{
			return pressedKeys.Contains(key);
		}
		/// <param name="key">
		/// The key to check.</param>
		/// <returns>True if the specified key has just been 
		/// pressed, false otherwise.</returns>
		public bool IsKeyJustPressed(int key)
		{
			return IsKeyPressed(key) && prevPressedKeys.Contains(key) == false;
		}
		/// <param name="key">
		/// The key to check.</param>
		/// <returns>True if the specified key has just been 
		/// released, false otherwise.</returns>

		public bool IsKeyJustReleased(int key)
		{
			return IsKeyPressed(key) == false && prevPressedKeys.Contains(key);
		}

		#region Backend
		internal readonly List<int> pressedKeys = new(), prevPressedKeys = new();
		internal bool wasPressed;
		#endregion
	}

	/// <summary>
	/// The currently focused user interface element.
	/// </summary>
	protected static UserInterface? Focused { get; private set; }
	/// <summary>
	/// Indicates whether user input has been cancelled.
	/// </summary>
	protected static bool IsInputCanceled { get; private set; }
	/// <summary>
	/// The last input received by the user interface.
	/// </summary>
	protected static Input CurrentInput { get; } = new();
	/// <summary>
	/// The size of the tilemap being used by the user interface.
	/// </summary>
	protected static (int width, int height) TilemapSize { get; private set; }

	/// <summary>
	/// Gets or sets the position of the user interface element.
	/// </summary>
	public (int x, int y) Position { get; set; }
	/// <summary>
	/// Gets or sets the size of the user interface element.
	/// </summary>
	public (int width, int height) Size
	{
		get => size;
		set
		{
			var (x, y) = value;
			size = (Math.Max(x, 1), Math.Max(y, 1));
		}
	}
	/// <summary>
	/// Gets or sets the text displayed (if any) by the user interface element.
	/// </summary>
	public string Text { get; set; } = "";
	/// <summary>
	/// Gets or sets the text that has been copied to the user interface clipboard.
	/// </summary>
	public static string? TextCopied { get; set; } = "";
	/// <summary>
	/// Gets or sets a value indicating whether the user interface element is hidden.
	/// </summary>
	public bool IsHidden { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether the user interface element is disabled.
	/// </summary>
	public bool IsDisabled { get; set; }
	/// <summary>
	/// Gets a value indicating whether the user interface element is currently focused.
	/// </summary>
	public bool IsFocused
	{
		get => Focused == this;
		protected set => Focused = value ? this : null;
	}
	/// <summary>
	/// Gets a value indicating whether the input position is currently hovering 
	/// over the user interface element.
	/// </summary>
	public bool IsHovered { get; private set; }
	/// <summary>
	/// Gets a value indicating whether the user interface element is currently 
	/// being pressed by the input.
	/// </summary>
	public bool IsPressed => IsHovered && CurrentInput.IsPressed;
	/// <summary>
	/// Gets a value indicating whether the user interface element has been clicked by the input
	/// (as a button - press and release on it).
	/// </summary>
	public bool IsClicked { get; private set; }

	/// <summary>
	/// Initializes a new user interface element instance class with the specified 
	/// position and size.
	/// </summary>
	/// <param name="position">The position of the user interface element.</param>
	/// <param name="size">The size of the ser interface element.</param>
	public UserInterface((int x, int y) position, (int width, int height) size)
	{
		Position = position;
		Size = size;

		hold.Start();
		holdTrigger.Start();
	}

	/// <summary>
	/// Updates the user interface element, detecting changes in focus, hover, and user input,
	/// and storing the results. This method should be called
	/// once per application update cycle for each user interface element.
	/// </summary>
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

		if (CurrentInput.IsKeyJustPressed(Input.Key.ESCAPE))
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
	/// <summary>
	/// Simulates a user click over this user interface element.
	/// </summary>
	public void Trigger()
	{
		TriggerUserEvent(UserEvent.TRIGGER);
	}

	/// <summary>
	/// Sets the mouse cursor graphics result to the specified tile cursor identifier.
	/// Also sets the corresponding system cursor identifier as well. 
	/// Usually used internally when the user interacts with the user interface element.
	/// </summary>
	protected void SetMouseCursor(int tileIdCursor)
	{
		cursorTile = tileIdCursor;
		cursorSystem = tileIdCursor + MouseCursor.SYSTEM_ARROW;
	}

	/// <summary>
	/// Triggers the specified user event, invoking all the registered
	/// methods associated with it.
	/// Used internally by the user interface elements to notify subscribers of
	/// user interactions and state changes.
	/// </summary>
	/// <param name="userEventId">The identifier of the user event to trigger.</param>
	protected void TriggerUserEvent(int userEventId)
	{
		OnUserEvent(userEventId);

		if (userEvents.ContainsKey(userEventId) == false)
			return;

		for (int i = 0; i < userEvents[userEventId].Count; i++)
			userEvents[userEventId][i].Invoke();
	}
	/// <summary>
	/// Subscribes the specified method to the specified user event, 
	/// so that it will be invoked every time the event is triggered. Multiple methods can be 
	/// associated with the same event.
	/// </summary>
	/// <param name="userEventId">The identifier of the user event to subscribe to.</param>
	/// <param name="method">The method to subscribe.</param>
	protected internal void SubscribeToUserEvent(int userEventId, Action method)
	{
		if (userEvents.ContainsKey(userEventId) == false)
			userEvents[userEventId] = new();

		userEvents[userEventId].Add(method);
	}
	/// <summary>
	/// Called by <see cref="Update"/> to update the state and appearance of the user interface element. 
	/// Subclasses should override this method to implement their own behavior.
	/// </summary>
	protected virtual void OnUserEvent(int userEventId) { }

	/// <summary>
	/// Called by <see cref="Update"/> to update the state and appearance of the user interface element. 
	/// Subclasses should override this method to implement their own behavior.
	/// </summary>
	protected abstract void OnUpdate();

	/// <summary>
	/// Applies input to the user interface element, updating its state accordingly.
	/// </summary>
	/// <param name="isPressed">Whether an input is currently pressed.</param>
	/// <param name="position">The current position of the input.</param>
	/// <param name="scrollDelta">The amount the mouse wheel has been scrolled.</param>
	/// <param name="keysPressed">An array of currently pressed keys on the keyboard.</param>
	/// <param name="keysTyped">A string containing characters typed on the keyboard.</param>
	/// <param name="tilemapSize">The size of the tilemap used by the user interface element.</param>
	public static void ApplyInput(bool isPressed, (float x, float y) position, int scrollDelta,
		int[] keysPressed, string keysTyped, (int width, int height) tilemapSize)
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
	/// <summary>
	/// Gets the tile and system identifiers of the mouse cursor graphics.
	/// </summary>
	/// <param name="tileId">The identifier of the current tile cursor graphics.</param>
	/// <param name="systemId">The identifier of the current system cursor graphics.</param>
	public static void GetMouseCursorGraphics(out int tileId, out int systemId)
	{
		tileId = cursorTile;
		systemId = cursorSystem;
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

		if (IsHovered && CurrentInput.IsJustReleased && IsClicked)
		{
			IsClicked = false;
			TriggerUserEvent(UserEvent.TRIGGER);
		}

		if (IsHovered && CurrentInput.IsJustPressed)
			IsClicked = true;

		if (CurrentInput.IsJustReleased)
			IsClicked = false;
	}
	#endregion
}
