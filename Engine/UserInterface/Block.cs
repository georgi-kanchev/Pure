using System.IO.Compression;

namespace Pure.Engine.UserInterface;

using System.Text;

/// <summary>
/// Represents a user interface block that the user can interact with and receive some
/// results back.
/// </summary>
public abstract class Block
{
    public (int x, int y, int width, int height) Area
    {
        get => (X, Y, Width, Height);
        set
        {
            Position = (value.x, value.y);
            Size = (value.x, value.y);
        }
    }
    public (int x, int y, int width, int height) Mask
    {
        get => mask;
        set
        {
            if (hasParent)
                return;

            wasMaskSet = true;
            mask = value;
        }
    }

    public int X
    {
        get => Position.x;
        set => Position = (value, Position.y);
    }
    public int Y
    {
        get => Position.y;
        set => Position = (Position.x, value);
    }
    public int Width
    {
        get => Size.width;
        set => Size = (value, Size.height);
    }
    public int Height
    {
        get => Size.height;
        set => Size = (Size.width, value);
    }

    /// <summary>
    /// Gets or sets the position of the user interface block.
    /// </summary>
    public (int x, int y) Position
    {
        get => position;
        set
        {
            if (hasParent == false)
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

            if (hasParent == false)
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
            if (isTextReadonly == false)
                text = value ?? string.Empty;
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the user interface block is hidden.
    /// </summary>
    public bool IsHidden { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the user interface block is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }
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
    public bool IsHovered { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the user interface block is currently held by the input,
    /// regardless of being hovered or not.
    /// </summary>
    public bool IsPressedAndHeld { get; private set; }
    /// <summary>
    /// Gets a value indicating whether this block belongs to another user interface block.
    /// </summary>
    public bool IsChild
    {
        get => hasParent;
    }
    /// <summary>
    /// Gets a value indicating whether the user interface block is currently scrollable.
    /// </summary>
    internal bool IsScrollable
    {
        get => IsFocused && Input.FocusedPrevious == this && IsHovered;
    }

    /// <summary>
    /// Initializes a new user interface block instance class.
    /// </summary>
    protected Block() : this((0, 0))
    {
        Init();
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
    /// <summary>
    /// Initializes a new user interface block instance class with the specified
    /// bytes.
    /// </summary>
    /// <param name="bytes">The bytes of the user interface block.</param>
    protected Block(byte[] bytes)
    {
        Init(); // should be before

        var b = Decompress(bytes);
        typeName = GrabString(b);
        Position = (GrabInt(b), GrabInt(b));
        SizeMinimum = (GrabInt(b), GrabInt(b));
        SizeMaximum = (GrabInt(b), GrabInt(b));
        Size = (GrabInt(b), GrabInt(b));

        // should set field to not flag wasMaskSet
        mask = (GrabInt(b), GrabInt(b), GrabInt(b), GrabInt(b));

        Text = GrabString(b);
        IsHidden = GrabBool(b);
        IsDisabled = GrabBool(b);
        hasParent = GrabBool(b);
    }
    /// <summary>
    /// Initializes a new user interface block instance class with the specified
    /// </summary>
    /// <param name="base64">The base64 of the user interface block.</param>
    protected Block(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    /// <summary>
    /// Converts the user interface block to a base64 string.
    /// </summary>
    /// <returns>The base64 string representation of the user interface block.</returns>
    public virtual string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    /// <summary>
    /// Converts the user interface block to a byte array.
    /// </summary>
    /// <returns> A byte array representation of the user interface block.</returns>
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
        PutInt(result, Mask.x);
        PutInt(result, Mask.y);
        PutInt(result, Mask.width);
        PutInt(result, Mask.height);
        PutString(result, Text);
        PutBool(result, IsHidden);
        PutBool(result, IsDisabled);
        PutBool(result, hasParent);

        return Compress(result.ToArray());
    }
    /// <summary>
    /// Overrides the ToString method to provide a custom string representation of the object.
    /// </summary>
    /// <returns>A string representation of the object.</returns>
    public override string ToString()
    {
        return $"{GetType().Name} \"{Text}\"";
    }

    /// <summary>
    /// Updates the block's state based on user input and other conditions.
    /// </summary>
    public void Update()
    {
        LimitSizeMin((1, 1));

        var (ix, iy) = Input.Position;

        mask = wasMaskSet ? mask : Input.Mask;
        wasHovered = IsHovered;
        var (mx, my, mw, mh) = mask;
        var isInputInsideMask = ix >= mx && iy >= my && ix < mx + mw && iy < my + mh;
        IsHovered = IsOverlapping(Input.Position) && isInputInsideMask;

        if (IsDisabled)
        {
            if (IsHovered && IsHidden == false)
                Input.CursorResult = MouseCursor.Disable;

            update?.Invoke();
            TryDisplaySelfAndProcessChildren();
            return;
        }
        else if (IsHovered)
            Input.CursorResult = MouseCursor.Arrow;

        var isJustPressed = Input.IsButtonJustPressed() && IsPressed();
        var isJustScrolled = Input.ScrollDelta != 0 && IsHovered;

        if (isJustPressed || isJustScrolled)
            IsFocused = true;

        if (Input.IsKeyJustPressed(Key.Escape))
            IsFocused = false;

        TryTrigger();

        if (IsFocused && Input.FocusedPrevious != this)
            Interact(Interaction.Focus);
        if (IsFocused == false && Input.FocusedPrevious == this)
            Interact(Interaction.Unfocus);
        if (IsHovered && wasHovered == false)
            Interact(Interaction.Hover);
        if (IsHovered == false && wasHovered)
            Interact(Interaction.Unhover);
        if (IsPressed() && Input.IsButtonJustPressed())
            Interact(Interaction.Press);
        if (IsHovered && IsPressed() == false && Input.IsButtonJustReleased())
            Interact(Interaction.Release);
        if (IsPressedAndHeld && Input.IsJustHeld)
            Interact(Interaction.PressAndHold);

        const MouseButton RMB = MouseButton.Right;
        if (IsPressed(RMB) && Input.IsButtonJustPressed(RMB))
            Interact(Interaction.PressRight);
        if (IsHovered && IsPressed(RMB) == false && Input.IsButtonJustReleased(RMB))
            Interact(Interaction.ReleaseRight);

        const MouseButton MMB = MouseButton.Middle;
        if (IsPressed(MMB) && Input.IsButtonJustPressed(MMB))
            Interact(Interaction.PressMiddle);
        if (IsHovered && IsPressed(MMB) == false && Input.IsButtonJustReleased(MMB))
            Interact(Interaction.ReleaseMiddle);

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

        update?.Invoke();

        if (isInputInsideMask)
            OnInput();

        TryDisplaySelfAndProcessChildren();

        if (Input.ScrollDelta == 0 || IsScrollable == false)
            return; // scroll even when hovering children

        Interact(Interaction.Scroll);
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

    /// <summary>
    /// Checks if the mouse button is pressed on the block.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the mouse button is pressed on the block, false otherwise.</returns>
    public bool IsPressed(MouseButton button = default)
    {
        return IsHovered && Input.IsButtonPressed(button);
    }
    /// <summary>
    /// Checks if the block is overlapping with a specified point.
    /// </summary>
    /// <param name="point">The point to check for overlap.</param>
    /// <returns>True if the block is overlapping with the point, false otherwise.</returns>
    public bool IsOverlapping((float x, float y) point)
    {
        // Check if the point is outside the tilemap boundaries
        if (point.x < 0 ||
            point.x >= Input.TilemapSize.width ||
            point.y < 0 ||
            point.y >= Input.TilemapSize.height)
            return false;

        // Check if the point is inside the bounding box
        return point.x >= Position.x &&
               point.x < Position.x + Size.width &&
               point.y >= Position.y &&
               point.y < Position.y + Size.height;
    }
    /// <summary>
    /// Checks if the block is overlapping with another block.
    /// </summary>
    /// <param name="block">The block to check for overlap.</param>
    /// <returns>True if the block is overlapping with the specified block, false otherwise.</returns>
    public bool IsOverlapping(Block block)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (ex, ey) = block.Position;
        var (ew, eh) = block.Size;

        return (x + w <= ex || x >= ex + ew || y + h <= ey || y >= ey + eh) == false;
    }

    public void AlignEdges(Side edge, Side targetEdge, (int x, int y, int width, int height)? rectangle = null, float alignment = float.NaN, int offset = 0, bool exceedEdge = false)
    {
        var (rx, ry) = (rectangle?.x ?? 0, rectangle?.y ?? 0);
        var rw = rectangle?.width ?? Input.TilemapSize.width;
        var rh = rectangle?.height ?? Input.TilemapSize.height;
        var (x, y) = Position;
        var (w, h) = Size;
        var (rxw, ryh) = (rx + rw, ry + rh);
        var (rcx, rcy) = (rx + rw / 2, ry + rh / 2);
        var (cx, cy) = (x + w / 2, y + h / 2);
        var ex = exceedEdge;
        var a = alignment;
        var notNan = float.IsNaN(a) == false;

        offset -= 1;

        if (notNan && edge is Side.Top or Side.Bottom && targetEdge is Side.Top or Side.Bottom)
            x = (int)MathF.Round(Map(a, (0, 1), ex ? (rx - w - offset, rxw + offset) : (rx, rxw - w)));
        else if (notNan && edge is Side.Left or Side.Right && targetEdge is Side.Left or Side.Right)
            y = (int)MathF.Round(Map(a, (0, 1), ex ? (ry - h - offset, ryh + offset) : (ry, ryh - h)));

        if (edge == Side.Left && targetEdge is Side.Left or Side.Right)
            x = targetEdge == Side.Left ? rx : rxw + offset;
        else if (edge == Side.Left)
            x = notNan ? rcx - cx : (int)MathF.Round(Map(a, (0, 1), (rx, rxw + offset)));

        if (edge == Side.Right && targetEdge is Side.Left or Side.Right)
            x = targetEdge == Side.Left ? rxw : rxw - w;
        else if (edge == Side.Right)
            x = notNan ? rcx - cx : (int)MathF.Round(Map(a, (0, 1), (rx - w - offset, rxw - w)));

        if (edge == Side.Top && targetEdge is Side.Left or Side.Right)
            y = notNan ? rcy - cy : (int)MathF.Round(Map(a, (0, 1), (ry, ryh + offset)));
        else if (edge == Side.Top)
            y = targetEdge == Side.Top ? ry : ryh + offset;

        if (edge == Side.Bottom && targetEdge is Side.Left or Side.Right)
            y = notNan ? rcy - cy : (int)MathF.Round(Map(a, (0, 1), (ry - h - offset, ryh - h)));
        else if (edge == Side.Bottom)
            y = targetEdge == Side.Top ? ryh : ryh - h;

        Position = (x, y);
    }
    public void AlignInside((float horizontal, float vertical) alignment, (int x, int y, int width, int height)? rectangle = null)
    {
        var (rx, ry) = (rectangle?.x ?? 0, rectangle?.y ?? 0);
        var rw = rectangle?.width ?? Input.TilemapSize.width;
        var rh = rectangle?.height ?? Input.TilemapSize.height;
        var (x, y) = Position;
        var (w, h) = Size;

        var newX = Map(alignment.horizontal, (0, 1), (rx, rw - w));
        var newY = Map(alignment.vertical, (0, 1), (ry, rh - h));
        Position = (
            float.IsNaN(alignment.horizontal) ? x : (int)newX,
            float.IsNaN(alignment.vertical) ? y : (int)newY);
    }
    public void AlignOutside(Side targetEdge, (int x, int y, int width, int height)? rectangle = null, float alignment = float.NaN, int offset = 0, bool exceedEdge = false)
    {
        var opposites = new[] { Side.Right, Side.Left, Side.Bottom, Side.Top };
        AlignEdges(opposites[(int)targetEdge], targetEdge, rectangle, alignment, offset + 1,
            exceedEdge);
    }
    public void Fit((int x, int y, int width, int height)? rectangle = null)
    {
        var (w, h) = Size;
        var (rx, ry) = (rectangle?.x ?? 0, rectangle?.y ?? 0);
        var rw = rectangle?.width ?? Input.TilemapSize.width;
        var rh = rectangle?.height ?? Input.TilemapSize.height;
        var newX = rw - w < rx ? rx : Math.Clamp(Position.x, rx, rw - w);
        var newY = rh - h < ry ? ry : Math.Clamp(Position.y, ry, rh - h);
        var hasOverflown = false;

        if (rw - w < 0)
        {
            AlignInside((0.5f, float.NaN));
            hasOverflown = true;
        }

        if (rw - h < 0)
        {
            AlignInside((float.NaN, 0.5f));
            hasOverflown = true;
        }

        if (hasOverflown == false)
            Position = (newX, newY);
    }
    /// <summary>
    /// Interacts with the block based on the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction type.</param>
    public void Interact(Interaction interaction)
    {
        if (interactions.TryGetValue(interaction, out var interaction1) == false)
            return;

        interaction1.Invoke();
    }

    /// <summary>
    /// Adds a method to be called when a specific interaction occurs.
    /// </summary>
    /// <param name="interaction">The interaction that triggers the method.</param>
    /// <param name="method">The method to be called.</param>
    public void OnInteraction(Interaction interaction, Action method)
    {
        if (interactions.TryAdd(interaction, method) == false)
            interactions[interaction] += method;
    }
    /// <summary>
    /// Adds a method to be called when the display needs to be updated.
    /// </summary>
    /// <param name="method">The method to be called.</param>
    public void OnDisplay(Action method)
    {
        display += method;
    }
    /// <summary>
    /// Adds a method to be called when an update is needed.
    /// </summary>
    /// <param name="method">The method to be called.</param>
    public void OnUpdate(Action method)
    {
        update += method;
    }

    /// <summary>
    /// Adds a method to be called when a drag occurs.
    /// </summary>
    /// <param name="method">The method to be called, with the delta coordinates of the drag.</param>
    public void OnDrag(Action<(int deltaX, int deltaY)> method)
    {
        drag += method;
    }

    /// <summary>
    /// Called when input is received.
    /// </summary>
    protected virtual void OnInput()
    {
    }

    /// <summary>
    /// Adds a boolean value to the given byte list.
    /// </summary>
    /// <param name="intoBytes">The byte list to add the boolean value to.</param>
    /// <param name="value">The boolean value to add to the byte list.</param>
    protected static void PutBool(List<byte> intoBytes, bool value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>
    /// Adds a byte value to the given byte list.
    /// </summary>
    /// <param name="intoBytes">The byte list to add the byte value to.</param>
    /// <param name="value">The byte value to add to the byte list.</param>
    protected static void PutByte(List<byte> intoBytes, byte value)
    {
        intoBytes.Add(value);
    }
    /// <summary>
    /// Adds an integer value to the given byte list.
    /// </summary>
    /// <param name="intoBytes">The byte list to add the integer value to.</param>
    /// <param name="value">The integer value to add to the byte list.</param>
    protected static void PutInt(List<byte> intoBytes, int value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>
    /// Adds an unsigned integer value to the given byte list.
    /// </summary>
    /// <param name="intoBytes">The byte list to add the unsigned integer value to.</param>
    /// <param name="value">The unsigned integer value to add to the byte list.</param>
    protected static void PutUInt(List<byte> intoBytes, uint value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>
    /// Adds a float value to the given byte list.
    /// </summary>
    /// <param name="intoBytes">The byte list to add the float value to.</param>
    /// <param name="value">The float value to add to the byte list.</param>
    protected static void PutFloat(List<byte> intoBytes, float value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>
    /// Adds a string value to the given byte list.
    /// </summary>
    /// <param name="intoBytes">The byte list to add the string value to.</param>
    /// <param name="value">The string value to add to the byte list.</param>
    protected static void PutString(List<byte> intoBytes, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        PutInt(intoBytes, bytes.Length);
        intoBytes.AddRange(bytes);
    }

    /// <summary>
    /// Retrieves a boolean value from the given byte array.
    /// </summary>
    /// <param name="fromBytes">The byte array to retrieve the boolean value from.</param>
    /// <returns>The retrieved boolean value.</returns>
    protected bool GrabBool(byte[] fromBytes)
    {
        return BitConverter.ToBoolean(GetBytes(fromBytes, 1));
    }
    /// <summary>
    /// Retrieves a byte value from the given byte array.
    /// </summary>
    /// <param name="fromBytes">The byte array to retrieve the byte value from.</param>
    /// <returns>The retrieved byte value.</returns>
    protected byte GrabByte(byte[] fromBytes)
    {
        return GetBytes(fromBytes, 1)[0];
    }
    /// <summary>
    /// Retrieves an integer value from the given byte array.
    /// </summary>
    /// <param name="fromBytes">The byte array to retrieve the integer value from.</param>
    /// <returns>The retrieved integer value.</returns>
    protected int GrabInt(byte[] fromBytes)
    {
        return BitConverter.ToInt32(GetBytes(fromBytes, 4));
    }
    /// <summary>
    /// Retrieves an unsigned integer value from the given byte array.
    /// </summary>
    /// <param name="fromBytes">The byte array to retrieve the unsigned integer value from.</param>
    /// <returns>The retrieved unsigned integer value.</returns>
    protected uint GrabUInt(byte[] fromBytes)
    {
        return BitConverter.ToUInt32(GetBytes(fromBytes, 4));
    }
    /// <summary>
    /// Retrieves a float value from the given byte array.
    /// </summary>
    /// <param name="fromBytes">The byte array to retrieve the float value from.</param>
    /// <returns>The retrieved float value.</returns>
    protected float GrabFloat(byte[] fromBytes)
    {
        return BitConverter.ToSingle(GetBytes(fromBytes, 4));
    }
    /// <summary>
    /// Retrieves a string value from the given byte array.
    /// </summary>
    /// <param name="fromBytes">The byte array to retrieve the string value from.</param>
    /// <returns>The retrieved string value.</returns>
    protected string GrabString(byte[] fromBytes)
    {
        var textBytesLength = GrabInt(fromBytes);
        var bText = GetBytes(fromBytes, textBytesLength);
        return Encoding.UTF8.GetString(bText);
    }

    /// <summary>
    /// Converts the block to a byte array.
    /// </summary>
    /// <param name="block">The block to convert.</param>
    /// <returns>The byte array representation of the block.</returns>
    public static implicit operator byte[](Block block)
    {
        return block.ToBytes();
    }
    /// <summary>
    /// Converts the block to a tuple of integers representing the position and size.
    /// </summary>
    /// <param name="block">The block to convert.</param>
    /// <returns>A tuple of integers representing the position and size of the Block.</returns>
    public static implicit operator (int x, int y, int width, int height)(Block block)
    {
        return (block.Position.x, block.Position.y, block.Size.width, block.Size.height);
    }

#region Backend
    internal (int, int) position,
        size,
        listSizeTrimOffset,
        sizeMinimum = (1, 1),
        sizeMaximum = (int.MaxValue, int.MaxValue);
    internal bool hasParent, isTextReadonly, wasMaskSet;
    internal readonly string typeName;
    private int byteOffset;
    private bool wasHovered, isReadyForDoubleClick;

    internal Action? display, update;
    private readonly Dictionary<Interaction, Action> interactions = new();
    internal Action<(int deltaX, int deltaY)>? drag;

    internal string text = string.Empty;
    internal (int x, int y, int width, int height) mask;

    private void Init()
    {
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

        if (IsHovered && Input.IsButtonJustReleased() && IsPressedAndHeld)
        {
            IsPressedAndHeld = false;
            Interact(Interaction.Trigger);

            if (isReadyForDoubleClick == false)
            {
                Input.doubleClick.Restart();
                isReadyForDoubleClick = true;
                return;
            }

            if (isReadyForDoubleClick && isAllowed)
                Interact(Interaction.DoubleTrigger);

            isReadyForDoubleClick = false;
        }

        if (IsHovered && Input.IsButtonJustPressed())
            IsPressedAndHeld = true;

        if (IsHovered == false && Input.IsButtonJustReleased())
            IsPressedAndHeld = false;
    }

    internal static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            stream.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }
    internal static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
        {
            stream.CopyTo(output);
        }

        return output.ToArray();
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