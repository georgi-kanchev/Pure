using System.Diagnostics;

namespace Pure.Engine.UserInterface;

/// <summary>
/// The various user interface actions that can be triggered by user input.
/// </summary>
public enum Interaction
{
    Focus, Unfocus,
    Hover, Unhover,
    Press, Release,
    Trigger, DoubleTrigger,
    PressAndHold, PressRight, ReleaseRight, PressMiddle, ReleaseMiddle,
    Scroll,
    Select
}

public enum MouseCursor
{
    None = -1, Arrow, ArrowWait, Wait, Text, Hand,
    ResizeHorizontal, ResizeVertical, ResizeDiagonal1, ResizeDiagonal2,
    Move, Crosshair, Help, Disable
}

public enum MouseButton { Left, Right, Middle }

/// <summary>
/// Represents the keyboard keys used for input by the user interface.
/// </summary>
internal enum Key
{
    Escape = 36, ControlLeft = 37, ShiftLeft = 38, AltLeft = 39, ControlRight = 41, ShiftRight = 42,
    AltRight = 43, Enter = 58, Backspace = 59, Tab = 60, PageUp = 61, PageDown = 62, End = 63, Home = 64,
    Insert = 65, Delete = 66, ArrowLeft = 71, ArrowRight = 72, ArrowUp = 73, ArrowDown = 74
}

/// <summary>
/// Represents user input for received by the user interface.
/// </summary>
public static class Input
{
    /// <summary>
    /// Gets or sets the cursor graphics result. Usually set by each user interface block
    /// when the user interacts with that specific block.
    /// </summary>
    public static MouseCursor CursorResult { get; set; }
    /// <summary>
    /// The currently focused user interface block.
    /// </summary>
    public static Block? Focused { get; set; }
    /// <summary>
    /// The size of the tilemap being used by the user interface.
    /// </summary>
    public static (int width, int height) TilemapSize
    {
        get => tilemapSize;
        set => tilemapSize = (Math.Abs(value.width), Math.Abs(value.height));
    }
    /// <summary>
    /// Gets the current position of the input.
    /// </summary>
    public static (float x, float y) Position { get; set; }
    /// <summary>
    /// Gets the previous position of the input.
    /// </summary>
    public static (float x, float y) PositionPrevious { get; set; }
    public static string? Clipboard { get; set; }
    public static bool IsTyping { get; internal set; }

    public static void Update(int[]? buttonsPressed = default, int scrollDelta = default, int[]? keysPressed = default, string? keysTyped = default, string? clipboard = default)
    {
        Clipboard = clipboard;

        CursorResult = MouseCursor.Arrow;
        TypedPrevious = Typed;
        prevPressedKeys.Clear();
        prevPressedKeys.AddRange(pressedKeys);
        prevPressedBtns.Clear();
        prevPressedBtns.AddRange(pressedBtns);

        if (keysPressed != null)
        {
            var keys = new Key[keysPressed.Length];
            for (var i = 0; i < keysPressed.Length; i++)
                keys[i] = (Key)keysPressed[i];

            PressedKeys = keys;
        }

        if (buttonsPressed != null)
        {
            var buttons = new MouseButton[buttonsPressed.Length];
            for (var i = 0; i < buttonsPressed.Length; i++)
                buttons[i] = (MouseButton)buttonsPressed[i];

            PressedButtons = buttons;
        }

        var mt = string.Empty;
        Typed = keysTyped?.Replace("\n", mt).Replace("\t", mt).Replace("\r", mt);

        ScrollDelta = scrollDelta;

        if (IsButtonJustPressed())
            hold.Restart();

        IsJustHeld = false;
        if (hold.Elapsed.TotalSeconds > HOLD_DELAY &&
            holdTrigger.Elapsed.TotalSeconds > HOLD_INTERVAL)
        {
            holdTrigger.Restart();
            IsJustHeld = true;
        }

        FocusedPrevious = Focused;
        if (IsButtonJustPressed())
            Focused = default;
    }
    public static void OnTextCopy(Action method)
    {
        onTextCopy += method;
    }

#region Backend
    internal const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f;
    internal const float DOUBLE_CLICK_DELAY = 0.5f;
    internal static readonly Stopwatch hold = new(), holdTrigger = new(), doubleClick = new();
    private static readonly List<Key> pressedKeys = [], prevPressedKeys = [];
    internal static readonly List<MouseButton> pressedBtns = [], prevPressedBtns = [];
    private static (int width, int height) tilemapSize;
    internal static Action? onTextCopy;
    internal static Block? FocusedPrevious { get; set; }

    internal static bool IsJustHeld { get; private set; }

    internal static string? Typed { get; private set; }
    internal static string? TypedPrevious { get; private set; }
    internal static int ScrollDelta { get; private set; }
    internal static (int x, int y, int width, int height) Mask
    {
        get => (0, 0, TilemapSize.width, TilemapSize.height);
    }

    static Input()
    {
        hold.Start();
        holdTrigger.Start();
        doubleClick.Start();
    }

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
    internal static MouseButton[]? PressedButtons
    {
        get => pressedBtns.ToArray();
        private set
        {
            pressedBtns.Clear();

            if (value != null && value.Length != 0)
                pressedBtns.AddRange(value);
        }
    }

    internal static bool IsAnyKeyPressed()
    {
        return pressedKeys.Count > 0;
    }
    internal static bool IsAnyButtonPressed()
    {
        return pressedBtns.Count > 0;
    }
    internal static bool IsKeyPressed(Key key)
    {
        return pressedKeys.Contains(key);
    }
    internal static bool IsKeyJustPressed(Key key)
    {
        return IsKeyPressed(key) && prevPressedKeys.Contains(key) == false;
    }
    internal static bool IsKeyJustReleased(Key key)
    {
        return IsKeyPressed(key) == false && prevPressedKeys.Contains(key);
    }
    internal static bool IsButtonPressed(MouseButton button = default)
    {
        return pressedBtns.Contains(button);
    }
    internal static bool IsButtonJustPressed(MouseButton button = default)
    {
        return IsButtonPressed(button) && prevPressedBtns.Contains(button) == false;
    }
    internal static bool IsButtonJustReleased(MouseButton button = default)
    {
        return IsButtonPressed(button) == false && prevPressedBtns.Contains(button);
    }
#endregion
}