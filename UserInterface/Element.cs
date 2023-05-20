namespace Pure.UserInterface;

using System.Diagnostics;

/// <summary>
/// The various user interface events that can be triggered by user input.
/// </summary>
public enum UserEvent
{
	Focus, Unfocus, Hover, Unhover, Press, Release, Trigger, Drag, Dragged, PressAndHold, Scroll
}
/// <summary>
/// The type of mouse cursor result from a user interaction with the user interface.
/// </summary>
public enum MouseCursor
{
	None = -1, Arrow, ArrowWait, Wait, Text, Hand, ResizeHorizontal, ResizeVertical,
	ResizeDiagonal1, ResizeDiagonal2, Move, Crosshair, Help, Disable
}
/// <summary>
/// Represents the keyboard keys used for input by the user interface.
/// </summary>
public enum Key
{
	Escape = 36, ControlLeft = 37, ShiftLeft = 38, AltLeft = 39,
	ControlRight = 41, ShiftRight = 42, AltRight = 43, Enter = 58, Return = 58,
	Backspace = 59, Tab = 60, PageUp = 61, PageDown = 62,
	End = 63, Home = 64, Insert = 65, Delete = 66,
	ArrowLeft = 71, ArrowRight = 72, ArrowUp = 73, ArrowDown = 74
}

/// <summary>
/// Represents a user interface element that the user can interact with and receive some
/// results back.
/// </summary>
public abstract partial class Element
{
	/// <summary>
	/// The currently focused user interface element.
	/// </summary>
	protected static Element? Focused { get; private set; }
	/// <summary>
	/// The size of the tilemap being used by the user interface.
	/// </summary>
	protected static (int width, int height) TilemapSize { get; private set; }

	/// <summary>
	/// Gets or sets the position of the user interface element.
	/// </summary>
	public (int x, int y) Position
	{
		get => position;
		set { if (hasParent == false) position = value; }
	}
	/// <summary>
	/// Gets or sets the size of the user interface element.
	/// </summary>
	public (int width, int height) Size
	{
		get => size;
		set { if (hasParent == false) size = (Math.Max(value.width, 1), Math.Max(value.height, 1)); }
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
	/// Gets a value indicating whether the user interface element is currently focused by
	/// the user input.
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
	/// being pressed and hovered by the input.
	/// </summary>
	public bool IsPressed => IsHovered && Input.Current.IsPressed;
	/// <summary>
	/// Gets a value indicating whether the user interface element is currently held by the input,
	/// regardless of being hovered or not.
	/// </summary>
	public bool IsPressedAndHeld { get; private set; }
	/// <summary>
	/// Gets or sets the mouse cursor graphics result. Usually set by each user interface element
	/// when the user interacts with that specific element.
	/// </summary>
	public static MouseCursor MouseCursorResult { get; protected set; }

	/// <summary>
	/// Initializes a new user interface element instance class with the specified 
	/// position and size.
	/// </summary>
	/// <param name="position">The position of the user interface element.</param>
	/// <param name="size">The size of the ser interface element.</param>
	public Element((int x, int y) position)
	{
		Size = (1, 1);
		Position = position;

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
				MouseCursorResult = MouseCursor.Disable;

			OnUpdate();
			return;
		}

		if (Input.Current.wasPressed == false && Input.Current.IsPressed && IsHovered)
			IsFocused = true;

		if (Input.Current.IsKeyJustPressed(Key.Escape))
			IsFocused = false;

		TryTrigger();

		if (IsFocused && wasFocused == false)
			TriggerUserEvent(UserEvent.Focus);
		if (IsFocused == false && wasFocused)
			TriggerUserEvent(UserEvent.Unfocus);
		if (IsHovered && wasHovered == false)
			TriggerUserEvent(UserEvent.Hover);
		if (IsHovered == false && wasHovered)
			TriggerUserEvent(UserEvent.Unhover);
		if (IsPressed && Input.Current.wasPressed == false)
			TriggerUserEvent(UserEvent.Press);
		if (IsPressed == false && Input.Current.wasPressed)
			TriggerUserEvent(UserEvent.Release);
		if (IsPressedAndHeld && Input.Current.IsJustHeld)
			TriggerUserEvent(UserEvent.PressAndHold);
		if (Input.Current.ScrollDelta != 0)
			TriggerUserEvent(UserEvent.Scroll);

		OnUpdate();
	}
	/// <summary>
	/// Simulates a user click over this user interface element.
	/// </summary>
	public void Trigger()
	{
		TriggerUserEvent(UserEvent.Trigger);
	}

	public bool Contains((float x, float y) point)
	{
		return point.x >= Position.x && point.x <= Position.x + Size.width &&
			point.y >= Position.y && point.y <= Position.y + Size.height;
	}
	public bool Overlaps(Element element)
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var (ex, ey) = element.Position;
		var (ew, eh) = element.Size;

		return (x + w <= ex || x >= ex + ew || y + h <= ey || y >= ey + eh) == false;
	}

	/// <summary>
	/// Triggers the specified user event, invoking all the registered
	/// methods associated with it.
	/// Used internally by the user interface elements to notify subscribers of
	/// user interactions and state changes.
	/// </summary>
	/// <param name="userEvent">The identifier of the user event to trigger.</param>
	protected internal void TriggerUserEvent(UserEvent userEvent)
	{
		OnUserEvent(userEvent);

		if (userEvents.ContainsKey(userEvent) == false)
			return;

		for (int i = 0; i < userEvents[userEvent].Count; i++)
			userEvents[userEvent][i].Invoke();
	}
	/// <summary>
	/// Subscribes the specified method to the specified user event, 
	/// so that it will be invoked every time the event is triggered. Multiple methods can be 
	/// associated with the same event.
	/// </summary>
	/// <param name="userEvent">The identifier of the user event to subscribe to.</param>
	/// <param name="method">The method to subscribe.</param>
	protected internal void SubscribeToUserEvent(UserEvent userEvent, Action method)
	{
		if (userEvents.ContainsKey(userEvent) == false)
			userEvents[userEvent] = new();

		userEvents[userEvent].Add(method);
	}

	/// <summary>
	/// Called by <see cref="Update"/> to update the state and appearance of the user interface element. 
	/// Subclasses should override this method to implement their own behavior.
	/// </summary>
	protected virtual void OnUserEvent(UserEvent userEvent) { }
	/// <summary>
	/// Called by <see cref="Update"/> to update the state and appearance of the user interface element. 
	/// Subclasses should override this method to implement their own behavior.
	/// </summary>
	protected virtual void OnUpdate() { }

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
		Input.IsCanceled = false;

		MouseCursorResult = MouseCursor.Arrow;
		TilemapSize = (Math.Abs(tilemapSize.Item1), Math.Abs(tilemapSize.Item2));

		Input.Current.wasPressed = Input.Current.IsPressed;
		Input.Current.TypedPrevious = Input.Current.Typed;
		Input.Current.PositionPrevious = Input.Current.Position;
		Input.Current.prevPressedKeys.Clear();
		Input.Current.prevPressedKeys.AddRange(Input.Current.pressedKeys);

		Input.Current.IsPressed = isPressed;
		Input.Current.Position = position;

		var keys = new Key[keysPressed.Length];
		for (int i = 0; i < keysPressed.Length; i++)
			keys[i] = (Key)keysPressed[i];

		Input.Current.PressedKeys = keys;
		Input.Current.Typed = keysTyped
			.Replace("\n", "")
			.Replace("\t", "")
			.Replace("\r", "");
		Input.Current.ScrollDelta = scrollDelta;

		if (Input.Current.IsJustPressed)
			hold.Restart();

		Input.Current.IsJustHeld = false;
		if (hold.Elapsed.TotalSeconds > HOLD_DELAY && holdTrigger.Elapsed.TotalSeconds > HOLD_INTERVAL)
		{
			holdTrigger.Restart();
			Input.Current.IsJustHeld = true;
		}

		if (Input.Current.wasPressed == false && Input.Current.IsPressed)
			Focused = null;
	}

	#region Backend
	private const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f;
	internal (int, int) position, size, listSizeTrimOffset;
	internal bool hasParent;
	private static readonly Stopwatch hold = new(), holdTrigger = new();

	private bool wasFocused, wasHovered;

	private readonly Dictionary<UserEvent, List<Action>> userEvents = new();

	public override string ToString()
	{
		return $"{GetType().Name} \"{Text}\"";
	}

	private void UpdateHovered()
	{
		var (ix, iy) = Input.Current.Position;
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
			IsPressedAndHeld = false;
			return;
		}

		if (IsHovered && Input.Current.IsJustReleased && IsPressedAndHeld)
		{
			IsPressedAndHeld = false;
			TriggerUserEvent(UserEvent.Trigger);
		}

		if (IsHovered && Input.Current.IsJustPressed)
			IsPressedAndHeld = true;

		if (Input.Current.IsJustReleased)
			IsPressedAndHeld = false;
	}
	#endregion
}
