namespace Pure.Engine.UserInterface;

using System.Diagnostics;

/// <summary>
/// Represents user input for received by the user interface.
/// </summary>
public static class Input
{
    /// <summary>
    /// Gets or sets the mouse cursor graphics result. Usually set by each user interface block
    /// when the user interacts with that specific block.
    /// </summary>
    public static MouseCursor MouseCursorResult
    {
        get;
        set;
    }

    /// <summary>
    /// The currently focused user interface block.
    /// </summary>
    public static Block? Focused
    {
        get;
        set;
    }

    /// <summary>
    /// The size of the tilemap being used by the user interface.
    /// </summary>
    public static (int width, int height) TilemapSize
    {
        get => tilemapSize;
        set => tilemapSize = (Math.Abs(value.width), Math.Abs(value.height));
    }

    /// <summary>
    /// Applies input to all user interface blocks, updating their state accordingly.
    /// </summary>
    /// <param name="isPressed">Whether an input is currently pressed.</param>
    /// <param name="position">The current position of the input.</param>
    /// <param name="scrollDelta">The amount the mouse wheel has been scrolled.</param>
    /// <param name="keysPressed">An array of currently pressed keys on the keyboard.</param>
    /// <param name="keysTyped">A string containing characters typed on the keyboard.</param>
    public static void Update(
        bool isPressed = default,
        (float x, float y) position = default,
        int scrollDelta = default,
        int[]? keysPressed = default,
        string? keysTyped = default)
    {
        IsCanceled = false;

        MouseCursorResult = MouseCursor.Arrow;

        WasPressed = IsPressed;
        TypedPrevious = Typed;
        PositionPrevious = Position;
        prevPressedKeys.Clear();
        prevPressedKeys.AddRange(pressedKeys);

        IsPressed = isPressed;
        Position = position;

        if (keysPressed != null)
        {
            var keys = new Key[keysPressed.Length];
            for (var i = 0; i < keysPressed.Length; i++)
                keys[i] = (Key)keysPressed[i];

            PressedKeys = keys;
        }

        Typed = keysTyped?.Replace("\n", "").Replace("\t", "").Replace("\r", "");

        ScrollDelta = scrollDelta;

        if (IsJustPressed)
            hold.Restart();

        IsJustHeld = false;
        if (hold.Elapsed.TotalSeconds > HOLD_DELAY &&
            holdTrigger.Elapsed.TotalSeconds > HOLD_INTERVAL)
        {
            holdTrigger.Restart();
            IsJustHeld = true;
        }

        FocusedPrevious = Focused;
        if (WasPressed == false && IsPressed)
            Focused = default;
    }

#region Backend
    internal const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f, DOUBLE_CLICK_DELAY = 0.5f;
    internal static readonly Stopwatch hold = new(), holdTrigger = new(), doubleClick = new();
    internal static readonly List<Key> pressedKeys = new(), prevPressedKeys = new();
    private static (int width, int height) tilemapSize;
    internal static Block? FocusedPrevious
    {
        get;
        set;
    }

    /// <summary>
    /// Indicates whether user input has been cancelled.
    /// </summary>
    internal static bool IsCanceled
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets a value indicating whether the input was pressed during the previous update.
    /// </summary>
    internal static bool WasPressed
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets a value indicating whether the input is currently pressed.
    /// </summary>
    internal static bool IsPressed
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets a value indicating whether the input has just been pressed.
    /// </summary>
    internal static bool IsJustPressed
    {
        get => WasPressed == false && IsPressed;
    }
    /// <summary>
    /// Gets a value indicating whether the input has just been released.
    /// </summary>
    internal static bool IsJustReleased
    {
        get => WasPressed && IsPressed == false;
    }
    /// <summary>
    /// Gets a value indicating whether the input has just been held.
    /// </summary>
    internal static bool IsJustHeld
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the current position of the input.
    /// </summary>
    internal static (float x, float y) Position
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the previous position of the input.
    /// </summary>
    internal static (float x, float y) PositionPrevious
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the most recent typed text.
    /// </summary>
    internal static string? Typed
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the previous typed text.
    /// </summary>
    internal static string? TypedPrevious
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the scroll delta of the input.
    /// </summary>
    internal static int ScrollDelta
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets an array of currently pressed keys.
    /// </summary>
    internal static Key[]? PressedKeys
    {
        get => pressedKeys.ToArray();
        private set
        {
            pressedKeys.Clear();

            if (value != null && value.Length != 0)
                pressedKeys.AddRange(value);
        }
    }

    /// <param name="key">
    /// The key to check.</param>
    /// <returns>True if the specified key is pressed, false otherwise.</returns>
    internal static bool IsKeyPressed(Key key)
    {
        return pressedKeys.Contains(key);
    }
    /// <param name="key">
    /// The key to check.</param>
    /// <returns>True if the specified key has just been 
    /// pressed, false otherwise.</returns>
    internal static bool IsKeyJustPressed(Key key)
    {
        return IsKeyPressed(key) && prevPressedKeys.Contains(key) == false;
    }
    /// <param name="key">
    /// The key to check.</param>
    /// <returns>True if the specified key has just been 
    /// released, false otherwise.</returns>
    internal static bool IsKeyJustReleased(Key key)
    {
        return IsKeyPressed(key) == false && prevPressedKeys.Contains(key);
    }
#endregion
}