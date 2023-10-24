namespace Pure.Engine.UserInterface;

using System.Text;

/// <summary>
/// The various user interface actions that can be triggered by user input.
/// </summary>
public enum Interaction
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
/// Represents a user interface block that the user can interact with and receive some
/// results back.
/// </summary>
public abstract class Block
{
    /// <summary>
    /// Gets or sets the position of the user interface block.
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
    /// Gets or sets the size of the user interface block.
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
    /// Gets or sets the minimum size that this block can have.
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
    /// Gets or sets the maximum size that this block can have.
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
    /// Gets or sets the text displayed (if any) by the user interface block.
    /// </summary>
    public string Text
    {
        get => text ?? string.Empty;
        set
        {
            if (isTextReadonly)
                return;

            text = value ?? string.Empty;
        }
    }
    /// <summary>
    /// Gets or sets the text that has been copied to the user interface clipboard.
    /// </summary>
    public static string? TextCopied
    {
        get;
        set;
    } = "";
    /// <summary>
    /// Gets or sets a value indicating whether the user interface block is hidden.
    /// </summary>
    public bool IsHidden
    {
        get => isHidden;
        set
        {
            if (hasParent == false)
                isHidden = value;
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the user interface block is disabled.
    /// </summary>
    public bool IsDisabled
    {
        get => isDisabled;
        set
        {
            if (hasParent == false)
                isDisabled = value;
        }
    }
    /// <summary>
    /// Gets a value indicating whether the user interface block is currently focused by
    /// the user input.
    /// </summary>
    public bool IsFocused
    {
        get => Input.Focused == this;
        set => Input.Focused = value ? this : default;
    }
    /// <summary>
    /// Gets a value indicating whether the input position is currently hovering 
    /// over the user interface block.
    /// </summary>
    public bool IsHovered
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets a value indicating whether the user interface block is currently 
    /// being pressed and hovered by the input.
    /// </summary>
    public bool IsPressed
    {
        get => IsHovered && Input.IsPressed;
    }
    /// <summary>
    /// Gets a value indicating whether the user interface block is currently held by the input,
    /// regardless of being hovered or not.
    /// </summary>
    public bool IsPressedAndHeld
    {
        get;
        private set;
    }

    internal bool IsScrollable
    {
        get => IsFocused && Input.FocusedPrevious == this && IsHovered;
    }

    protected Block() : this((0, 0))
    {
    }
    /// <summary>
    /// Initializes a new user interface block instance class with the specified 
    /// position.
    /// </summary>
    /// <param name="position">The position of the user interface block.</param>
    protected Block((int x, int y) position = default)
    {
        Size = (1, 1);
        Position = position;

        typeName = GetType().Name;
        Init();
    }
    protected Block(byte[] bytes)
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

    public override string ToString()
    {
        return $"{GetType().Name} \"{Text}\"";
    }

    public virtual void Update()
    {
        LimitSizeMin((1, 1));

        var (mw, mh) = Input.TilemapSize;
        var (ix, iy) = Input.Position;
        var isInputInsideMap = ix >= 0 && iy >= 0 && ix < mw && iy < mh;

        wasHovered = IsHovered;
        IsHovered = IsOverlapping(Input.Position) && isInputInsideMap;

        if (IsDisabled)
        {
            if (IsHovered && IsHidden == false)
                Input.CursorResult = MouseCursor.Disable;

            OnUpdate();
            TryDisplaySelfAndProcessChildren();
            return;
        }
        else if (IsHovered)
            Input.CursorResult = MouseCursor.Arrow;

        var isJustPressed = Input.WasPressed == false && IsPressed;
        var isJustScrolled = Input.ScrollDelta != 0 && IsHovered;

        if (isJustPressed || isJustScrolled)
            IsFocused = true;

        if (Input.IsKeyJustPressed(Key.Escape))
            IsFocused = false;

        TryTrigger();

        if (IsFocused && Input.FocusedPrevious != this)
            SimulateInteraction(Interaction.Focus);
        if (IsFocused == false && Input.FocusedPrevious == this)
            SimulateInteraction(Interaction.Unfocus);
        if (IsHovered && wasHovered == false)
            SimulateInteraction(Interaction.Hover);
        if (IsHovered == false && wasHovered)
            SimulateInteraction(Interaction.Unhover);
        if (IsPressed && Input.WasPressed == false)
            SimulateInteraction(Interaction.Press);
        if (IsHovered && IsPressed == false && Input.WasPressed)
            SimulateInteraction(Interaction.Release);
        if (IsPressedAndHeld && Input.IsJustHeld)
            SimulateInteraction(Interaction.PressAndHold);

        var p = Input.Position;
        var pp = Input.PositionPrevious;
        var px = (int)Math.Floor(p.x);
        var py = (int)Math.Floor(p.y);
        var ppx = (int)Math.Floor(pp.x);
        var ppy = (int)Math.Floor(pp.y);

        if ((px != ppx || py != ppy) && IsPressedAndHeld && IsFocused && Input.FocusedPrevious == this)
        {
            var delta = (px - ppx, py - ppy);
            drag?.Invoke(delta);
        }

        OnUpdate();

        if (isInputInsideMap)
            OnInput();

        TryDisplaySelfAndProcessChildren();

        if (Input.ScrollDelta == 0 || IsScrollable == false)
            return; // scroll even when hovering children

        SimulateInteraction(Interaction.Scroll);
        ApplyScroll();

        return;

        void TryDisplaySelfAndProcessChildren()
        {
            if (IsHidden)
                return;

            display?.Invoke();

            OnChildrenUpdate();
            // parents call OnDisplay on children and themselves to ensure order if needed
            OnChildrenDisplay();
        }
    }

    public bool IsOverlapping((float x, float y) point)
    {
        if (point.x < 0 || point.x >= Input.TilemapSize.width ||
            point.y < 0 || point.y >= Input.TilemapSize.height)
            return false;

        return point.x >= Position.x && point.x < Position.x + Size.width &&
               point.y >= Position.y && point.y < Position.y + Size.height;
    }
    public bool IsOverlapping(Block block)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (ex, ey) = block.Position;
        var (ew, eh) = block.Size;

        return (x + w <= ex || x >= ex + ew || y + h <= ey || y >= ey + eh) == false;
    }

    public void Align((float horizontal, float vertical) alignment)
    {
        var newX = Map(alignment.horizontal, (0, 1), (0, Input.TilemapSize.width - Size.width));
        var newY = Map(alignment.vertical, (0, 1), (0, Input.TilemapSize.height - Size.height));
        Position = (
            float.IsNaN(alignment.horizontal) ? Position.x : (int)newX,
            float.IsNaN(alignment.vertical) ? Position.y : (int)newY);
    }

    public void SimulateInteraction(Interaction interaction)
    {
        if (interactions.ContainsKey(interaction) == false)
            return;

        interactions[interaction].Invoke();
    }

    public void OnInteraction(Interaction interaction, Action method)
    {
        if (interactions.TryAdd(interaction, method) == false)
            interactions[interaction] += method;
    }
    public void OnDisplay(Action method)
    {
        display += method;
    }
    public void OnDrag(Action<(int deltaX, int deltaY)> method)
    {
        drag += method;
    }

    protected virtual void OnInput()
    {
    }
    protected internal void InheritParent(Block parent)
    {
        isHidden |= parent.IsHidden;
        isDisabled |= parent.IsDisabled;
    }

    protected static void PutBool(List<byte> intoBytes, bool value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    protected static void PutByte(List<byte> intoBytes, byte value)
    {
        intoBytes.Add(value);
    }
    protected static void PutInt(List<byte> intoBytes, int value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    protected static void PutUInt(List<byte> intoBytes, uint value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    protected static void PutFloat(List<byte> intoBytes, float value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    protected static void PutString(List<byte> intoBytes, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        PutInt(intoBytes, bytes.Length);
        intoBytes.AddRange(bytes);
    }

    protected bool GrabBool(byte[] fromBytes)
    {
        return BitConverter.ToBoolean(GetBytes(fromBytes, 1));
    }
    protected byte GrabByte(byte[] fromBytes)
    {
        return GetBytes(fromBytes, 1)[0];
    }
    protected int GrabInt(byte[] fromBytes)
    {
        return BitConverter.ToInt32(GetBytes(fromBytes, 4));
    }
    protected uint GrabUInt(byte[] fromBytes)
    {
        return BitConverter.ToUInt32(GetBytes(fromBytes, 4));
    }
    protected float GrabFloat(byte[] fromBytes)
    {
        return BitConverter.ToSingle(GetBytes(fromBytes, 4));
    }
    protected string GrabString(byte[] fromBytes)
    {
        var textBytesLength = GrabInt(fromBytes);
        var bText = GetBytes(fromBytes, textBytesLength);
        return Encoding.UTF8.GetString(bText);
    }

#region Backend
    internal (int, int) position,
        size,
        listSizeTrimOffset,
        sizeMinimum = (1, 1),
        sizeMaximum = (int.MaxValue, int.MaxValue);
    internal bool hasParent, isTextReadonly, isHidden, isDisabled;
    internal readonly string typeName;
    private int byteOffset;
    private bool wasHovered, isReadyForDoubleClick;

    internal Action? display;
    private readonly Dictionary<Interaction, Action> interactions = new();
    internal Action<(int deltaX, int deltaY)>? drag;

    internal string text = "";

    private void Init()
    {
        Input.hold.Start();
        Input.holdTrigger.Start();
        Input.doubleClick.Start();

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

    internal virtual void OnUpdate()
    {
    }
    internal virtual void OnChildrenUpdate()
    {
    }
    internal virtual void OnChildrenDisplay()
    {
    }
    internal virtual void ApplyScroll()
    {
    }

    private void TryTrigger()
    {
        var isAllowed = Input.DOUBLE_CLICK_DELAY > Input.doubleClick.Elapsed.TotalSeconds;
        if (isAllowed == false && isReadyForDoubleClick)
            isReadyForDoubleClick = false;

        if (IsFocused == false || IsDisabled)
        {
            IsPressedAndHeld = false;
            return;
        }

        if (IsHovered && Input.IsJustReleased && IsPressedAndHeld)
        {
            IsPressedAndHeld = false;
            SimulateInteraction(Interaction.Trigger);

            if (isReadyForDoubleClick == false)
            {
                Input.doubleClick.Restart();
                isReadyForDoubleClick = true;
                return;
            }

            if (isReadyForDoubleClick && isAllowed)
                SimulateInteraction(Interaction.DoubleTrigger);

            isReadyForDoubleClick = false;
        }

        if (IsHovered && Input.IsJustPressed)
            IsPressedAndHeld = true;

        if (IsHovered == false && Input.IsJustReleased)
            IsPressedAndHeld = false;
    }

    private byte[] GetBytes(byte[] fromBytes, int amount)
    {
        var result = fromBytes[byteOffset..(byteOffset + amount)];
        byteOffset += amount;
        return result;
    }
    private static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
#endregion
}