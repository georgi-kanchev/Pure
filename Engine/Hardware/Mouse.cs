using System.Diagnostics;
using SFML.Window;

namespace Pure.Engine.Hardware;

/// <summary>
/// Handles mouse input.
/// </summary>
[DoNotSave]
public class Mouse
{
	/// <summary>
	/// The common mouse button types.
	/// </summary>
	public enum Button { Left, Right, Middle, Extra1, Extra2 }

	/// <summary>
	/// The types of mouse cursor graphics that can be displayed on the window.
	/// </summary>
	public enum Cursor
	{
		Arrow, ArrowWait, Wait, Text, Hand,
		ResizeHorizontal, ResizeVertical, ResizeTopLeftBottomRight, ResizeBottomLeftTopRight,
		Move, Crosshair, Help, Disable
	}

	/// <summary>
	/// Gets the current position of the mouse cursor.
	/// </summary>
	public VecI CursorPosition { get; private set; }
	public VecI CursorDelta { get; private set; }

	/// <summary>
	/// Gets or sets the graphics for the mouse cursor.
	/// </summary>
	public Cursor CursorCurrent
	{
		get => cursor;
		set
		{
			if (cursor == value)
				return;

			cursor = value;
			TryUpdateSystemCursor();
		}
	}

	/// <summary>
	/// Gets or sets whether the mouse cursor is restricted to the window.
	/// </summary>
	public bool IsCursorBounded
	{
		get => isCursorBounded;
		set
		{
			isCursorBounded = value;
			window.SetMouseCursorGrabbed(value);
		}
	}
	public bool IsCursorVisible
	{
		get => isCursorVisible;
		set
		{
			if (isCursorVisible == value)
				return;

			isCursorVisible = value;
			TryUpdateSystemCursor();
		}
	}
	public bool IsCursorInWindow { get; internal set; }

	/// <summary>
	/// Gets an array of currently pressed mouse buttons, in order.
	/// </summary>
	public Button[] ButtonsPressed
	{
		get => pressed.ToArray();
	}
	public int[] ButtonIdsPressed
	{
		get
		{
			var press = pressed;
			var result = new int[press.Count];
			for (var i = 0; i < press.Count; i++)
				result[i] = (int)press[i];

			return result;
		}
	}
	/// <summary>
	/// Gets the scroll delta of the mouse.
	/// </summary>
	public int ScrollDelta { get; private set; }

	public Mouse(IntPtr windowHandle)
	{
		window = new(windowHandle);

		window.Resized += (_, _) =>
		{
			var mousePos = SFML.Window.Mouse.GetPosition(window);
			OnMove(null, new(new() { X = mousePos.X, Y = mousePos.Y }));
		};
		window.MouseButtonPressed += OnButtonPressed;
		window.MouseButtonReleased += OnButtonReleased;
		window.MouseWheelScrolled += OnWheelScrolled;
		window.MouseMoved += OnMove;
		window.MouseEntered += OnEnter;
		window.MouseLeft += OnLeft;
		window.LostFocus += (_, _) => CancelInput();

		IsCursorBounded = isCursorBounded;
		TryUpdateSystemCursor();
	}

	public void Update()
	{
		if (IsAnyJustPressed())
			hold.Restart();

		isJustHeld = false;
		if (hold.Elapsed.TotalSeconds > HOLD_DELAY &&
		    holdTrigger.Elapsed.TotalSeconds > HOLD_INTERVAL)
		{
			holdTrigger.Restart();
			isJustHeld = true;
		}

		if (IsAnyJustPressedAndHeld())
		{
			onHoldAny?.Invoke(pressed[^1]);

			foreach (var key in pressed)
				if (IsJustPressedAndHeld(key) && onHold.TryGetValue(key, out var callback))
					callback.Invoke();
		}

		prevPressed.Clear();
		prevPressed.AddRange(pressed);

		CursorDelta = (CursorPosition.x - prevPos.x, CursorPosition.y - prevPos.y);
		prevPos = CursorPosition;
		ScrollDelta = 0;

		window.SetMouseCursorVisible(IsCursorInWindow == false || IsCursorVisible);

		foreach (var button in prevSimulatedPressed)
			if (simulatedPresses.Contains(button) == false)
				OnButtonReleased(null, new(new() { Button = (SFML.Window.Mouse.Button)button }));

		prevSimulatedPressed.Clear();
		prevSimulatedPressed.AddRange(simulatedPresses);
		simulatedPresses.Clear();
	}

	public void SimulatePress(Button button)
	{
		simulatedPresses.Add(button);

		if (pressed.Contains(button) == false)
			OnButtonPressed(null, new(new() { Button = (SFML.Window.Mouse.Button)button }));
	}
	public void SimulateScroll(bool up)
	{
		OnWheelScrolled(null, new(new() { Delta = up ? 1 : -1 }));
	}
	public void SimulateCursorMove(VecI position)
	{
		OnMove(null, new(new() { X = position.x, Y = position.y }));
	}
	public void CancelInput()
	{
		ScrollDelta = 0;
		pressed.Clear();
	}

	/// <summary>
	/// Gets whether the specified mouse button is currently pressed.
	/// </summary>
	/// <param name="button">The button to check.</param>
	/// <returns>True if the button is currently pressed, false otherwise.</returns>
	public bool IsPressed(Button button)
	{
		return pressed.Contains(button);
	}
	public bool IsJustPressed(Button button)
	{
		return IsPressed(button) && prevPressed.Contains(button) == false;
	}
	public bool IsJustReleased(Button button)
	{
		return IsPressed(button) == false && prevPressed.Contains(button);
	}
	public bool IsJustPressedAndHeld(Button button)
	{
		return IsJustPressed(button) || (IsPressed(button) && isJustHeld);
	}

	public bool IsAnyPressed()
	{
		return pressed.Count > 0;
	}
	public bool IsAnyJustPressed()
	{
		return pressed.Count > prevPressed.Count;
	}
	public bool IsAnyJustReleased()
	{
		return pressed.Count < prevPressed.Count;
	}
	public bool IsAnyJustPressedAndHeld()
	{
		return IsAnyJustPressed() || isJustHeld;
	}

	public void OnPress(Button button, Action method)
	{
		if (onPress.TryAdd(button, method) == false)
			onPress[button] += method;
	}
	public void OnRelease(Button button, Action method)
	{
		if (onRelease.TryAdd(button, method) == false)
			onRelease[button] += method;
	}
	public void OnPressAndHold(Button key, Action method)
	{
		if (onHold.TryAdd(key, method) == false)
			onHold[key] += method;
	}

	public void OnPressAny(Action<Button> method)
	{
		onPressAny += method;
	}
	public void OnReleaseAny(Action<Button> method)
	{
		onReleaseAny += method;
	}
	public void OnPressAndHoldAny(Action<Button> method)
	{
		onHoldAny += method;
	}

	public void OnCursorMove(Action method)
	{
		move += method;
	}
	public void OnWheelScroll(Action method)
	{
		scroll += method;
	}

#region Backend
	private const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f;

	private readonly Window window;
	private readonly List<Button> simulatedPresses = [], prevSimulatedPressed = [];
	private Action<Button>? onPressAny, onReleaseAny, onHoldAny;
	private readonly Dictionary<Button, Action> onPress = new(), onRelease = new(), onHold = new();
	private Action? scroll, move;
	private readonly List<Button> pressed = [], prevPressed = [];
	private readonly Stopwatch hold = new(), holdTrigger = new();
	private bool isJustHeld, isCursorBounded;
	private Cursor cursor;
	private SFML.Window.Cursor sysCursor = new(SFML.Window.Cursor.CursorType.Arrow);
	private VecI prevPos;
	private bool isCursorVisible;

	internal void OnMove(object? s, MouseMoveEventArgs e)
	{
		IsCursorInWindow = true;
		CursorPosition = (e.X, e.Y);
		move?.Invoke();
	}
	internal void OnButtonPressed(object? s, MouseButtonEventArgs e)
	{
		hold.Restart();
		holdTrigger.Restart();

		IsCursorInWindow = true;
		var btn = (Button)e.Button;
		var contains = pressed.Contains(btn);

		if (contains)
			return;

		pressed.Add(btn);

		if (onPress.TryGetValue(btn, out var value))
			value.Invoke();

		onPressAny?.Invoke(btn);
	}
	internal void OnButtonReleased(object? s, MouseButtonEventArgs e)
	{
		IsCursorInWindow = true;
		var btn = (Button)e.Button;
		var contains = pressed.Contains(btn);

		if (contains == false)
			return;

		pressed.Remove(btn);

		if (onRelease.TryGetValue(btn, out var value))
			value.Invoke();

		onReleaseAny?.Invoke(btn);
	}
	internal void OnWheelScrolled(object? s, MouseWheelScrollEventArgs e)
	{
		IsCursorInWindow = true;
		ScrollDelta = e.Delta < 0 ? -1 : 1;
		scroll?.Invoke();
	}
	internal void OnEnter(object? s, EventArgs e)
	{
		IsCursorInWindow = true;
	}
	internal void OnLeft(object? s, EventArgs e)
	{
		IsCursorInWindow = false;
	}

	internal void TryUpdateSystemCursor()
	{
		if (sysCursor.CPointer == IntPtr.Zero)
			sysCursor.Dispose();
		else if (IsCursorVisible)
		{
			var cursorType = (SFML.Window.Cursor.CursorType)cursor;
			sysCursor.Dispose();
			sysCursor = new(cursorType);
			if (sysCursor.CPointer != IntPtr.Zero)
				window.SetMouseCursor(sysCursor);
		}
	}
#endregion
}