namespace Pure.Engine.Window;

/// <summary>
/// Handles mouse input.
/// </summary>
public static class Mouse
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
    public static (int x, int y) CursorPosition { get; private set; }
    public static (int x, int y) CursorDelta { get; private set; }

    /// <summary>
    /// Gets or sets the graphics for the mouse cursor.
    /// </summary>
    public static Cursor CursorCurrent
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
    public static bool IsCursorBounded
    {
        get => Window.window != null && isGrabbed;
        set
        {
            isGrabbed = value;
            Window.window?.SetMouseCursorGrabbed(value);
        }
    }
    public static bool IsCursorVisible { get; set; }
    public static bool IsCursorInWindow { get; internal set; }

    /// <summary>
    /// Gets an array of currently pressed mouse buttons, in order.
    /// </summary>
    public static Button[] ButtonsPressed
    {
        get => pressed.ToArray();
    }
    public static int[] ButtonIdsPressed
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
    public static int ScrollDelta { get; private set; }

    public static void SimulatePress(this Button button)
    {
        simulatedPresses.Add(button);

        if (pressed.Contains(button) == false)
            OnButtonPressed(null, new(new() { Button = (SFML.Window.Mouse.Button)button }));
    }
    public static void SimulateScroll(bool up)
    {
        OnWheelScrolled(null, new(new() { Delta = up ? 1 : -1 }));
    }
    public static void SimulateCursorMove((int x, int y) position)
    {
        OnMove(null, new(new() { X = position.x, Y = position.y }));
    }
    public static void CancelInput()
    {
        ScrollDelta = 0;
        pressed.Clear();
    }

    /// <summary>
    /// Gets whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button is currently pressed, false otherwise.</returns>
    public static bool IsPressed(this Button button)
    {
        return pressed.Contains(button);
    }
    public static bool IsJustPressed(this Button button)
    {
        return IsPressed(button) && prevPressed.Contains(button) == false;
    }
    public static bool IsJustReleased(this Button button)
    {
        return IsPressed(button) == false && prevPressed.Contains(button);
    }
    public static bool IsJustPressedAndHeld(this Button button)
    {
        return IsJustPressed(button) || (IsPressed(button) && isJustHeld);
    }

    public static bool IsAnyPressed()
    {
        return pressed.Count > 0;
    }
    public static bool IsAnyJustPressed()
    {
        return pressed.Count > prevPressed.Count;
    }
    public static bool IsAnyJustReleased()
    {
        return pressed.Count < prevPressed.Count;
    }
    public static bool IsAnyJustPressedAndHeld()
    {
        return IsAnyJustPressed() || isJustHeld;
    }

    public static void OnPress(this Button button, Action method)
    {
        if (onPress.TryAdd(button, method) == false)
            onPress[button] += method;
    }
    public static void OnRelease(this Button button, Action method)
    {
        if (onRelease.TryAdd(button, method) == false)
            onRelease[button] += method;
    }
    public static void OnPressAndHold(this Button key, Action method)
    {
        if (onHold.TryAdd(key, method) == false)
            onHold[key] += method;
    }

    public static void OnPressAny(Action<Button> method)
    {
        onPressAny += method;
    }
    public static void OnReleaseAny(Action<Button> method)
    {
        onReleaseAny += method;
    }
    public static void OnPressAndHoldAny(Action<Button> method)
    {
        onHoldAny += method;
    }

    public static void OnCursorMove(Action method)
    {
        move += method;
    }
    public static void OnWheelScroll(Action method)
    {
        scroll += method;
    }

#region Backend
    private const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f;

    private static readonly List<Button> simulatedPresses = [], prevSimulatedPressed = [];
    private static Action<Button>? onPressAny, onReleaseAny, onHoldAny;
    private static readonly Dictionary<Button, Action> onPress = new(), onRelease = new(), onHold = new();
    private static Action? scroll, move;
    private static readonly List<Button> pressed = [], prevPressed = [];
    private static readonly Stopwatch hold = new(), holdTrigger = new();
    private static bool isJustHeld, isGrabbed;
    private static Cursor cursor;
    private static SFML.Window.Cursor sysCursor = new(SFML.Window.Cursor.CursorType.Arrow);
    private static (int x, int y) prevPos;

    private static bool IsOverRender
    {
        get
        {
            var (ww, wh) = Window.Size;
            var (_, _, ow, oh) = Window.GetRenderOffset();
            var (x, y) = CursorPosition;

            return x > ow && x < ww - ow && y > oh && y < wh - oh;
        }
    }

    internal static void OnMove(object? s, MouseMoveEventArgs e)
    {
        IsCursorInWindow = true;
        CursorPosition = (e.X, e.Y);
        move?.Invoke();
    }
    internal static void OnButtonPressed(object? s, MouseButtonEventArgs e)
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
    internal static void OnButtonReleased(object? s, MouseButtonEventArgs e)
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
    internal static void OnWheelScrolled(object? s, MouseWheelScrollEventArgs e)
    {
        IsCursorInWindow = true;
        ScrollDelta = e.Delta < 0 ? -1 : 1;
        scroll?.Invoke();
    }
    internal static void OnEnter(object? sender, EventArgs e)
    {
        IsCursorInWindow = true;
    }
    internal static void OnLeft(object? sender, EventArgs e)
    {
        IsCursorInWindow = false;
    }

    internal static void Update()
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
        Window.window?.SetMouseCursorVisible(IsCursorInWindow == false ||
                                             IsCursorVisible ||
                                             IsOverRender == false);

        foreach (var button in prevSimulatedPressed)
            if (simulatedPresses.Contains(button) == false)
                OnButtonReleased(null, new(new() { Button = (SFML.Window.Mouse.Button)button }));

        prevSimulatedPressed.Clear();
        prevSimulatedPressed.AddRange(simulatedPresses);
        simulatedPresses.Clear();
    }

    internal static void TryUpdateSystemCursor()
    {
        if (Window.window == null)
            return;

        if (sysCursor.CPointer == IntPtr.Zero)
            sysCursor.Dispose();
        else if (IsCursorVisible)
        {
            var cursorType = (SFML.Window.Cursor.CursorType)cursor;
            sysCursor.Dispose();
            sysCursor = new(cursorType);
            if (sysCursor.CPointer != IntPtr.Zero)
                Window.window.SetMouseCursor(sysCursor);
        }
    }
#endregion
}