﻿namespace Pure.Engine.Window;

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
        ResizeTopLeftBottomRight,
        ResizeBottomLeftTopRight,
        Move,
        Crosshair,
        Help,
        Disable
    }

    /// <summary>
    /// Gets the current position of the mouse cursor.
    /// </summary>
    public static (int x, int y) CursorPosition { get; private set; }

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

    /// <summary>
    /// Gets an array of currently pressed mouse buttons, in order.
    /// </summary>
    public static Button[] ButtonsPressed
    {
        get => pressed.ToArray();
    }
    /// <summary>
    /// Gets the scroll delta of the mouse.
    /// </summary>
    public static int ScrollDelta { get; private set; }

    public static bool IsHovering(Layer layer)
    {
        return layer.IsOverlapping(layer.PixelToWorld(CursorPosition));
    }

    /// <summary>
    /// Gets whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button is currently pressed, false otherwise.</returns>
    public static bool IsButtonPressed(Button button)
    {
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

    private static Cursor cursor;
    private static SFML.Window.Cursor sysCursor = new(SFML.Window.Cursor.CursorType.Arrow);
    private static bool isGrabbed;

    internal static bool isOverWindow;

    internal static void OnMove(object? s, MouseMoveEventArgs e)
    {
        isOverWindow = true;
        CursorPosition = (e.X, e.Y);
    }
    internal static void OnButtonPressed(object? s, MouseButtonEventArgs e)
    {
        isOverWindow = true;
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
        isOverWindow = true;
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
        isOverWindow = true;
        ScrollDelta = e.Delta < 0 ? -1 : 1;
        actionScroll?.Invoke();
    }
    internal static void OnEnter(object? sender, EventArgs e)
    {
        isOverWindow = true;
    }
    internal static void OnLeft(object? sender, EventArgs e)
    {
        isOverWindow = false;
    }

    internal static void Update()
    {
        ScrollDelta = 0;
        Window.window?.SetMouseCursorVisible(isOverWindow == false || IsCursorVisible);
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