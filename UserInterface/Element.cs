namespace Pure.UserInterface;

using System.Diagnostics;
using System.Text;

/// <summary>
/// The various user interface actions that can be triggered by user input.
/// </summary>
public enum UserAction
{
    Focus,
    Unfocus,
    Hover,
    Unhover,
    Press,
    Release,
    Trigger,
    DoubleTrigger,
    PressAndHold,
    Scroll,
    Select
}

/// <summary>
/// The type of mouse cursor result from a user interaction with the user interface.
/// </summary>
public enum MouseCursor
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
/// Represents the keyboard keys used for input by the user interface.
/// </summary>
public enum Key
{
    Escape = 36,
    ControlLeft = 37,
    ShiftLeft = 38,
    AltLeft = 39,
    ControlRight = 41,
    ShiftRight = 42,
    AltRight = 43,
    Enter = 58,
    Backspace = 59,
    Tab = 60,
    PageUp = 61,
    PageDown = 62,
    End = 63,
    Home = 64,
    Insert = 65,
    Delete = 66,
    ArrowLeft = 71,
    ArrowRight = 72,
    ArrowUp = 73,
    ArrowDown = 74
}

/// <summary>
/// Represents a user interface element that the user can interact with and receive some
/// results back.
/// </summary>
public abstract partial class Element
{
    /// <summary>
    /// Gets or sets the mouse cursor graphics result. Usually set by each user interface element
    /// when the user interacts with that specific element.
    /// </summary>
    public static MouseCursor MouseCursorResult { get; protected set; }

    /// <summary>
    /// The currently focused user interface element.
    /// </summary>
    public static Element? Focused { get; set; }
    /// <summary>
    /// The user interface element that was focused during the previous update.
    /// </summary>
    protected static Element? FocusedPrevious { get; private set; }
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
        set
        {
            if (hasParent)
                return;

            position = value;
        }
    }
    /// <summary>
    /// Gets or sets the size of the user interface element.
    /// </summary>
    public (int width, int height) Size
    {
        get => size;
        set
        {
            value.width = Math.Clamp(value.width, SizeMinimum.width, SizeMaximum.width);
            value.height = Math.Clamp(value.height, SizeMinimum.height, SizeMaximum.height);

            if (hasParent)
                return;

            size = value;
        }
    }
    /// <summary>
    /// Gets or sets the minimum size that this element can have.
    /// </summary>
    public (int width, int height) SizeMinimum
    {
        get => sizeMinimum;
        set
        {
            if (hasParent)
                return;

            value.width = Math.Max(value.width, 1);
            value.height = Math.Max(value.height, 1);

            if (value.width > SizeMaximum.width)
                value.width = SizeMaximum.width;
            if (value.height > SizeMaximum.height)
                value.height = SizeMaximum.height;

            sizeMinimum = value;
            Size = size;
        }
    }
    /// <summary>
    /// Gets or sets the maximum size that this element can have.
    /// </summary>
    public (int width, int height) SizeMaximum
    {
        get => sizeMaximum;
        set
        {
            if (hasParent)
                return;

            value.width = Math.Max(value.width, 1);
            value.height = Math.Max(value.height, 1);

            if (value.width < SizeMinimum.width)
                value.width = SizeMinimum.width;
            if (value.height < SizeMinimum.height)
                value.height = SizeMinimum.height;

            sizeMaximum = value;
            Size = size;
        }
    }
    /// <summary>
    /// Gets or sets the text displayed (if any) by the user interface element.
    /// </summary>
    public string Text
    {
        get => text;
        set
        {
            if (isTextReadonly)
                return;

            text = value;
        }
    }
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
        set => Focused = value ? this : default;
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
    /// Initializes a new user interface element instance class with the specified 
    /// position.
    /// </summary>
    /// <param name="position">The position of the user interface element.</param>
    protected Element((int x, int y) position)
    {
        Size = (1, 1);
        Position = position;

        typeName = GetType().Name;
        Init();
    }
    protected Element(byte[] bytes)
    {
        Init(); // should be before

        typeName = GrabString(bytes);
        Position = (GrabInt(bytes), GrabInt(bytes));
        SizeMinimum = (GrabInt(bytes), GrabInt(bytes));
        SizeMaximum = (GrabInt(bytes), GrabInt(bytes));
        Size = (GrabInt(bytes), GrabInt(bytes));
        Text = GrabString(bytes);
        IsHidden = GrabBool(bytes);
        IsDisabled = GrabBool(bytes);
        hasParent = GrabBool(bytes);
    }

    public virtual void Update()
    {
        LimitSizeMin((1, 1));

        wasFocused = IsFocused;
        wasHovered = IsHovered;

        UpdateHovered();

        if (IsDisabled)
        {
            if (IsHovered && IsHidden == false)
                MouseCursorResult = MouseCursor.Disable;

            OnUpdate();
            OnChildrenUpdate();
            CallDisplay();
            return;
        }

        var isJustPressed = Input.Current.WasPressed == false && IsPressed;
        var isJustScrolled = Input.Current.ScrollDelta != 0 && IsHovered;
        if (isJustPressed || isJustScrolled)
            IsFocused = true;

        if (Input.Current.IsKeyJustPressed(Key.Escape))
            IsFocused = false;

        TryTrigger();

        if (IsFocused && wasFocused == false)
            TriggerUserAction(UserAction.Focus);
        if (IsFocused == false && wasFocused)
            TriggerUserAction(UserAction.Unfocus);
        if (IsHovered && wasHovered == false)
            TriggerUserAction(UserAction.Hover);
        if (IsHovered == false && wasHovered)
            TriggerUserAction(UserAction.Unhover);
        if (IsPressed && Input.Current.WasPressed == false)
            TriggerUserAction(UserAction.Press);
        if (IsHovered && IsPressed == false && Input.Current.WasPressed)
            TriggerUserAction(UserAction.Release);
        if (IsPressedAndHeld && Input.Current.IsJustHeld)
            TriggerUserAction(UserAction.PressAndHold);
        if (Input.Current.ScrollDelta != 0)
            TriggerUserAction(UserAction.Scroll);

        var p = Input.Current.Position;
        var pp = Input.Current.PositionPrevious;
        var px = (int)Math.Floor(p.x);
        var py = (int)Math.Floor(p.y);
        var ppx = (int)Math.Floor(pp.x);
        var ppy = (int)Math.Floor(pp.y);

        if ((px != ppx || py != ppy) && IsPressedAndHeld)
        {
            var delta = (px - ppx, py - ppy);
            OnDrag(delta);
            dragCallback?.Invoke(delta);
        }

        OnUpdate();
        OnChildrenUpdate();
        OnInput();
        CallDisplay();

        void CallDisplay()
        {
            OnDisplay();
            displayCallback?.Invoke();

            // parents call OnDisplay on children and themselves to ensure order if needed
            OnChildrenDisplay();
        }
    }
    /// <summary>
    /// Simulates a user click over this user interface element.
    /// </summary>
    public void Trigger()
    {
        IsPressedAndHeld = false;
        TriggerUserAction(UserAction.Trigger);
    }

    public bool IsOverlapping((float x, float y) point)
    {
        return point.x >= Position.x && point.x < Position.x + Size.width &&
               point.y >= Position.y && point.y < Position.y + Size.height;
    }
    public bool IsOverlapping(Element element)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (ex, ey) = element.Position;
        var (ew, eh) = element.Size;

        return (x + w <= ex || x >= ex + ew || y + h <= ey || y >= ey + eh) == false;
    }

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
        TilemapSize = (Math.Abs(tilemapSize.width), Math.Abs(tilemapSize.height));

        Input.Current.WasPressed = Input.Current.IsPressed;
        Input.Current.TypedPrevious = Input.Current.Typed;
        Input.Current.PositionPrevious = Input.Current.Position;
        Input.Current.prevPressedKeys.Clear();
        Input.Current.prevPressedKeys.AddRange(Input.Current.pressedKeys);

        Input.Current.IsPressed = isPressed;
        Input.Current.Position = position;

        var keys = new Key[keysPressed.Length];
        for (var i = 0; i < keysPressed.Length; i++)
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

        FocusedPrevious = Focused;
        if (Input.Current.WasPressed == false && Input.Current.IsPressed)
            Focused = default;
    }

    public virtual byte[] ToBytes()
    {
        var result = new List<byte>();

        PutString(result, typeName);
        PutInt(result, Position.x);
        PutInt(result, Position.y);
        PutInt(result, SizeMinimum.width);
        PutInt(result, SizeMinimum.height);
        PutInt(result, SizeMaximum.width);
        PutInt(result, SizeMaximum.height);
        PutInt(result, Size.width);
        PutInt(result, Size.height);
        PutString(result, Text);
        PutBool(result, IsHidden);
        PutBool(result, IsDisabled);
        PutBool(result, hasParent);

        return result.ToArray();
    }

    public override string ToString() => $"{GetType().Name} \"{Text}\"";

    /// <summary>
    /// Invokes all the registered methods associated with the specified user action.
    /// Used internally by the user interface elements to notify subscribers of
    /// user interactions and state changes.
    /// </summary>
    /// <param name="userAction">The identifier of the user action to trigger.</param>
    protected internal void TriggerUserAction(UserAction userAction)
    {
        OnUserAction(userAction);

        if (userActions.ContainsKey(userAction) == false)
            return;

        for (var i = 0; i < userActions[userAction].Count; i++)
            userActions[userAction][i].Invoke();
    }
    /// <summary>
    /// Subscribes the specified method to the specified user action, 
    /// so that it will be invoked every time the action is triggered. Multiple methods can be 
    /// associated with the same action.
    /// </summary>
    /// <param name="userAction">The identifier of the user action to subscribe to.</param>
    /// <param name="method">The method to subscribe.</param>
    protected internal void SubscribeToUserAction(UserAction userAction, Action method)
    {
        if (userActions.ContainsKey(userAction) == false)
            userActions[userAction] = new();

        userActions[userAction].Add(method);
    }
    protected internal void UnsubscribeAll() => userActions.Clear();

    /// <summary>
    /// Called by <see cref="Update"/> to update the state and appearance of the user interface element. 
    /// Subclasses should override this method to implement their own behavior.
    /// </summary>
    protected virtual void OnUserAction(UserAction userAction) { }
    protected internal virtual void OnDisplay() { }
    protected virtual void OnDrag((int x, int y) delta) { }

    protected internal void InheritParent(Element parent)
    {
        IsHidden |= parent.IsHidden;
        IsDisabled |= parent.IsDisabled;
    }

    protected static void PutBool(List<byte> intoBytes, bool value) =>
        intoBytes.AddRange(BitConverter.GetBytes(value));
    protected static void PutByte(List<byte> intoBytes, byte value) => intoBytes.Add(value);
    protected static void PutInt(List<byte> intoBytes, int value) =>
        intoBytes.AddRange(BitConverter.GetBytes(value));
    protected static void PutUInt(List<byte> intoBytes, uint value) =>
        intoBytes.AddRange(BitConverter.GetBytes(value));
    protected static void PutFloat(List<byte> intoBytes, float value) =>
        intoBytes.AddRange(BitConverter.GetBytes(value));
    protected static void PutString(List<byte> intoBytes, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        PutInt(intoBytes, bytes.Length);
        intoBytes.AddRange(bytes);
    }

    protected bool GrabBool(byte[] fromBytes) => BitConverter.ToBoolean(GetBytes(fromBytes, 1));
    protected byte GrabByte(byte[] fromBytes) => GetBytes(fromBytes, 1)[0];
    protected int GrabInt(byte[] fromBytes) => BitConverter.ToInt32(GetBytes(fromBytes, 4));
    protected uint GrabUInt(byte[] fromBytes) => BitConverter.ToUInt32(GetBytes(fromBytes, 4));
    protected float GrabFloat(byte[] fromBytes) => BitConverter.ToSingle(GetBytes(fromBytes, 4));
    protected string GrabString(byte[] fromBytes)
    {
        var textBytesLength = GrabInt(fromBytes);
        var bText = GetBytes(fromBytes, textBytesLength);
        return Encoding.UTF8.GetString(bText);
    }

#region Backend
    // save format
    // [amount of bytes]	- data
    // --------------------------------
    // [4]					- x
    // [4]					- y
    // [4]					- width
    // [4]					- height
    // [4]					- text length
    // [text length]		- text (base 64-ed)
    // [1]					- is hidden
    // [1]					- is disabled
    // [4]					- type name length
    // [type name length]	- type name (Button, InputBox, Slider etc...) - used in the UI class
    private const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f, DOUBLE_CLICK_DELAY = 0.5f;
    internal (int, int) position,
        size,
        listSizeTrimOffset,
        sizeMinimum = (1, 1),
        sizeMaximum = (int.MaxValue, int.MaxValue);
    internal bool hasParent, isTextReadonly;
    internal readonly string typeName;
    private static readonly Stopwatch hold = new(), holdTrigger = new(), doubleClick = new();
    private int byteOffset;
    private bool wasFocused, wasHovered, isReadyForDoubleClick;
    private readonly Dictionary<UserAction, List<Action>> userActions = new();

    // used in the UI class to receive callbacks
    internal Action<(int width, int height)>? dragCallback;
    internal Action? displayCallback;
    internal string text = "";

    private void Init()
    {
        hold.Start();
        holdTrigger.Start();
        doubleClick.Start();

        Text = GetType().Name;
    }
    internal void LimitSizeMin((int width, int height) minimumSize)
    {
        if (Size.width < minimumSize.width)
            size = (minimumSize.width, Size.height);
        if (Size.height < minimumSize.height)
            size = (Size.width, minimumSize.height);
    }
    internal void LimitSizeMax((int width, int height) maximumSize)
    {
        if (Size.width > maximumSize.width)
            size = (maximumSize.width, Size.height);
        if (Size.height > maximumSize.height)
            size = (Size.width, maximumSize.height);
    }

    internal virtual void OnUpdate() { }
    internal virtual void OnChildrenUpdate() { }
    internal virtual void OnChildrenDisplay() { }
    internal virtual void OnInput() { }

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
        var isAllowed = DOUBLE_CLICK_DELAY > doubleClick.Elapsed.TotalSeconds;
        if (isAllowed == false && isReadyForDoubleClick)
            isReadyForDoubleClick = false;

        if (IsFocused == false || IsDisabled)
        {
            IsPressedAndHeld = false;
            return;
        }

        if (IsHovered && Input.Current.IsJustReleased && IsPressedAndHeld)
        {
            Trigger();

            if (isReadyForDoubleClick == false)
            {
                doubleClick.Restart();
                isReadyForDoubleClick = true;
                return;
            }

            if (isReadyForDoubleClick && isAllowed)
                TriggerUserAction(UserAction.DoubleTrigger);

            isReadyForDoubleClick = false;
        }

        if (IsHovered && Input.Current.IsJustPressed)
            IsPressedAndHeld = true;

        if (IsHovered == false && Input.Current.IsJustReleased)
            IsPressedAndHeld = false;
    }

    private byte[] GetBytes(byte[] fromBytes, int amount)
    {
        var result = fromBytes[byteOffset..(byteOffset + amount)];
        byteOffset += amount;
        return result;
    }
#endregion
}