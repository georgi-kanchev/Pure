namespace Pure.Engine.Window;

using SFML.Window;

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
        None = -1,
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
    public static Cursor CursorGraphics
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
            UpdateCursorVisibility();
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
    /// <summary>
    /// Gets or sets whether the mouse cursor graphics are the system ones or custom tiles.
    /// </summary>
    public static bool IsCursorTile
    {
        get
        {
            Window.TryNoWindowException();
            return isCursorTile;
        }
        set
        {
            Window.TryNoWindowException();
            isCursorTile = value;
            UpdateCursorVisibility();
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

    public static (float x, float y) PixelToWorld((int x, int y) pixelPosition)
    {
        Window.TryNoWindowException();

        var (px, py) = (pixelPosition.x * 1f, pixelPosition.y * 1f);
        var (ww, wh) = (Window.Size.width, Window.Size.height);
        var (cw, ch) = (Window.Layer.tilemapCellCount.w, Window.Layer.tilemapCellCount.h);
        var (vw, vh) = Window.renderTextureViewSize;
        var (mw, mh) = (Window.Layer.tilemapSize.w, Window.Layer.tilemapSize.h);
        var (ox, oy) = Window.Layer.offset;

        ox /= mw;
        oy /= mh;

        px -= ww / 2f;
        py -= wh / 2f;

        var x = Map(px, 0, ww, 0, cw);
        var y = Map(py, 0, wh, 0, ch);

        x *= vw / Window.Layer.zoom / mw;
        y *= vh / Window.Layer.zoom / mh;

        x += cw / 2f;
        y += ch / 2f;

        x -= ox * cw / Window.Layer.zoom;
        y -= oy * ch / Window.Layer.zoom;

        return (x, y);
    }

    public static void SetupCursorTile(int tile = 442, uint color = uint.MaxValue)
    {
        Window.TryNoWindowException();
        cursorColor = color;
        cursorTile = tile;
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
    internal static bool isCursorTileVisible;
    private static readonly Dictionary<Button, Action> actionsPress = new(), actionsRelease = new();
    private static Action? actionScroll;
    private static readonly List<Button> pressed = new();
    private static readonly List<(float, float)> cursorOffsets = new()
    {
        (0.0f, 0.0f), (0.0f, 0.0f), (0.4f, 0.4f), (0.4f, 0.4f), (0.3f, 0.0f), (0.4f, 0.4f),
        (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f)
    };

    private static (int x, int y) cursorPos;
    private static int cursorTile = 442, scrollData;
    private static Cursor cursor;
    private static uint cursorColor = uint.MaxValue;
    private static SFML.Window.Cursor sysCursor = new(SFML.Window.Cursor.CursorType.Arrow);
    private static bool isMouseGrabbed, isCursorTile = true;

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

        if (IsCursorTile == false || isCursorTileVisible == false || CursorGraphics == Cursor.None)
            return;

        var (offX, offY) = cursorOffsets[(int)CursorGraphics];

        Window.Layer.graphicsPath = "default";
        Window.Layer.tileSize = (8, 8);

        var cursorTile = Mouse.cursorTile;
        var ang = default(sbyte);

        if (CursorGraphics == Cursor.ResizeVertical)
        {
            cursorTile--;
            ang = 1;
        }
        else if (CursorGraphics == Cursor.ResizeDiagonal1)
        {
            cursorTile--;
            ang = 1;
        }
        else if ((int)CursorGraphics >= (int)Cursor.ResizeDiagonal2)
        {
            cursorTile -= 2;
        }

        (int id, uint tint, sbyte ang, bool h, bool v) tile = default;
        tile.id = cursorTile + (int)CursorGraphics;
        tile.tint = cursorColor;
        tile.ang = ang;

        var (x, y) = PixelToWorld(cursorPos);
        Window.DrawTile((x - offX, y - offY), tile);
    }

    private static void TryUpdateSystemCursor()
    {
        if (IsCursorTile)
            return;

        Window.TryNoWindowException();

        if (sysCursor.CPointer == IntPtr.Zero)
            sysCursor.Dispose();
        else
        {
            var sfmlEnum = (SFML.Window.Cursor.CursorType)(cursor);
            sysCursor.Dispose();
            sysCursor = new(sfmlEnum);
            Window.window.SetMouseCursor(sysCursor);
        }
    }
    internal static void UpdateCursorVisibility()
    {
        Window.TryNoWindowException();
        Window.window.SetMouseCursorVisible(CursorGraphics != Cursor.None);

        if (IsCursorTile)
            Window.window.SetMouseCursorVisible(IsCursorHoveringWindow == false);
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}