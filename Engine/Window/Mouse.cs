namespace Pure.Engine.Window;

/// <summary>
/// Handles mouse input.
/// </summary>
public static class Mouse
{
    /// <summary>
    /// The common mouse button types.
    /// </summary>
    public enum Button
    {
        Left,
        Right,
        Middle,
        Extra1,
        Extra2
    }

    /// <summary>
    /// The types of mouse cursor graphics that can be displayed on the window.
    /// </summary>
    public enum Cursor
    {
        Arrow,
        ArrowWait,
        Wait,
        Text,
        Hand,
        ResizeHorizontal,
        ResizeVertical,
        ResizeDiagonal1,
        ResizeDiagonal2,
        Move,
        Crosshair,
        Help,
        Disable
    }

    /// <summary>
    /// Gets the current position of the mouse cursor.
    /// </summary>
    public static (int x, int y) CursorPosition
    {
        get
        {
            Window.TryNoWindowException();
            return cursorPos;
        }
    }
    /// <summary>
    /// Gets or sets the graphics for the mouse cursor.
    /// </summary>
    public static Cursor CursorCurrent
    {
        get
        {
            Window.TryNoWindowException();
            return cursor;
        }
        set
        {
            Window.TryNoWindowException();
            if (cursor == value)
                return;

            cursor = value;
            TryUpdateSystemCursor();
        }
    }
    /// <summary>
    /// Gets or sets whether the mouse cursor is restricted to the window.
    /// </summary>
    public static bool IsCursorRestricted
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
            // this -1 -1 padding works on windows
            return pos.X > -1 && pos.X < w.Size.X && pos.Y > -1 && pos.Y < w.Size.Y;
        }
    }
    public static bool IsCursorVisible
    {
        get => isCursorVisible;
        set
        {
            Window.TryNoWindowException();
            isCursorVisible = value;
            Window.window.SetMouseCursorVisible(value);
        }
    }

    /// <summary>
    /// Gets an array of currently pressed mouse buttons, in order.
    /// </summary>
    public static Button[] ButtonsPressed
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
    public static bool IsButtonPressed(Button button)
    {
        Window.TryNoWindowException();
        return pressed.Contains(button);
    }

    public static void CancelInput()
    {
        ScrollDelta = 0;
        pressed.Clear();
    }

    public static void OnButtonPress(Button button, Action method)
    {
        if (actionsPress.TryAdd(button, method) == false)
            actionsPress[button] += method;
    }
    public static void OnButtonRelease(Button button, Action method)
    {
        if (actionsRelease.TryAdd(button, method) == false)
            actionsRelease[button] += method;
    }
    public static void OnWheelScroll(Action method)
    {
        actionScroll += method;
    }

#region Backend
    private static readonly Dictionary<Button, Action> actionsPress = new(), actionsRelease = new();
    private static Action? actionScroll;
    private static readonly List<Button> pressed = new();

    private static (int x, int y) cursorPos;
    private static int scrollData;
    private static Cursor cursor;
    private static SFML.Window.Cursor sysCursor = new(SFML.Window.Cursor.CursorType.Arrow);
    private static bool isMouseGrabbed;
    private static bool isCursorVisible;

    internal static void OnMove(object? s, MouseMoveEventArgs e)
    {
        cursorPos = (e.X, e.Y);
    }
    internal static void OnButtonPressed(object? s, MouseButtonEventArgs e)
    {
        var btn = (Button)e.Button;
        var contains = pressed.Contains(btn);

        if (contains)
            return;

        pressed.Add(btn);

        if (actionsPress.TryGetValue(btn, out var value))
            value.Invoke();
    }
    internal static void OnButtonReleased(object? s, MouseButtonEventArgs e)
    {
        var btn = (Button)e.Button;
        var contains = pressed.Contains(btn);

        if (contains == false)
            return;

        pressed.Remove(btn);

        if (actionsRelease.TryGetValue(btn, out var value))
            value.Invoke();
    }
    internal static void OnWheelScrolled(object? s, MouseWheelScrollEventArgs e)
    {
        ScrollDelta = e.Delta < 0 ? -1 : 1;
        actionScroll?.Invoke();
    }

    internal static void Update()
    {
        ScrollDelta = 0;
    }

    private static void TryUpdateSystemCursor()
    {
        Window.TryNoWindowException();

        if (sysCursor.CPointer == IntPtr.Zero)
            sysCursor.Dispose();
        else
        {
            var sfmlEnum = (SFML.Window.Cursor.CursorType)cursor;
            sysCursor.Dispose();
            sysCursor = new(sfmlEnum);
            Window.window.SetMouseCursor(sysCursor);
        }
    }
#endregion
}