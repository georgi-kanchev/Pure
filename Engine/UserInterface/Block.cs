global using Area = (int x, int y, int width, int height);
global using PointI = (int x, int y);
global using PointF = (float x, float y);
global using Size = (int width, int height);
global using Range = (float a, float b);

namespace Pure.Engine.UserInterface;

public enum Pivot { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }

/// <summary>
/// Represents a user interface block that the user can interact with and receive some
/// results back.
/// </summary>
public class Block
{
    public Action? OnDisplay { get; set; }
    public Action? OnUpdate { get; set; }
    public Action<(int deltaX, int deltaY)>? OnDrag { get; set; }

    public Area Area
    {
        get => (X, Y, Width, Height);
        set
        {
            Position = (value.x, value.y);
            Size = (value.width, value.height);
        }
    }
    public Area Mask
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
    public PointI Position
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
    public Size Size
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
    public Size SizeMinimum
    {
        get => sizeMin;
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

            sizeMin = value;
            Size = size;
        }
    }
    /// <summary>
    /// Gets or sets the maximum size that this block can have.
    /// </summary>
    public Size SizeMaximum
    {
        get => sizeMax;
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

            sizeMax = value;
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
    [DoNotSave]
    public bool IsHovered { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the user interface block is currently held by the input,
    /// regardless of being hovered or not.
    /// </summary>
    [DoNotSave]
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
    public Block() : this((0, 0))
    {
    }
    /// <summary>
    /// Initializes a new user interface block instance class with the specified 
    /// position.
    /// </summary>
    /// <param name="position">The position of the user interface block.</param>
    public Block(PointI position)
    {
        Size = (1, 1);
        Position = position;
        Text = GetType().Name;
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

        justInteracted.Clear();
        var (ix, iy) = Input.Position;

        mask = wasMaskSet ? mask : Input.Mask;
        var wasHovered = IsHovered;
        var (mx, my, mw, mh) = mask;
        var isInputInsideMask = ix >= mx && iy >= my && ix < mx + mw && iy < my + mh;
        IsHovered = IsOverlapping(Input.Position) && isInputInsideMask;

        if (IsDisabled)
        {
            if (IsHovered && IsHidden == false)
                Input.CursorResult = MouseCursor.Disable;

            OnUpdate?.Invoke();
            TryDisplaySelfAndProcessChildren();
            return;
        }

        if (IsHovered)
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
            OnDrag?.Invoke(delta);
        }

        OnUpdate?.Invoke();

        if (isInputInsideMask)
            OnInput();

        TryDisplaySelfAndProcessChildren();

        if (Input.ScrollDelta == 0 || IsScrollable == false)
            return; // scroll even when hovering children

        Interact(Interaction.Scroll);
        ApplyScroll();

        void TryDisplaySelfAndProcessChildren()
        {
            if (IsHidden)
                return;

            OnDisplay?.Invoke();

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
    public bool IsOverlapping(PointF point)
    {
        // Check if the point is outside the tilemap boundaries
        if (point.x < 0 ||
            point.x >= Input.Bounds.width ||
            point.y < 0 ||
            point.y >= Input.Bounds.height)
            return false;

        // Check if the point is inside the bounding box
        return point.x >= Position.x &&
               point.x < Position.x + Size.width &&
               point.y >= Position.y &&
               point.y < Position.y + Size.height;
    }
    public bool IsOverlapping(Area area)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (rx, ry, rw, rh) = area;

        return (x + w <= rx || x >= rx + rw || y + h <= ry || y >= ry + rh) == false;
    }
    public bool IsJustInteracted(Interaction interaction)
    {
        return justInteracted.Contains(interaction);
    }

    public void AlignInside(Area? area = null, Pivot pivot = Pivot.Center, PointI offset = default)
    {
        var (ax, ay, aw, ah) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
        var (w, h) = Size;
        var (x, y) = (0, 0);

        if (pivot == Pivot.TopLeft) (x, y) = (ax, ay);
        else if (pivot == Pivot.Top) (x, y) = (ax + aw / 2 - w / 2, ay);
        else if (pivot == Pivot.TopRight) (x, y) = (ax + aw - w, ay);
        else if (pivot == Pivot.Left) (x, y) = (ax, ay + ah / 2 - h / 2);
        else if (pivot == Pivot.Center) (x, y) = (ax + aw / 2 - w / 2, ay + ah / 2 - h / 2);
        else if (pivot == Pivot.Right) (x, y) = (ax + aw - w, ay + ah / 2 - h / 2);
        else if (pivot == Pivot.BottomLeft) (x, y) = (ax, ay + ah - h);
        else if (pivot == Pivot.Bottom) (x, y) = (ax + aw / 2 - w / 2, ay + ah - h);
        else if (pivot == Pivot.BottomRight) (x, y) = (ax + aw - w, ay + ah - h);

        Position = (x + offset.x, y + offset.y);
    }
    public void AlignEdges(Pivot edge, Pivot targetEdge, Area? targetArea = null, float alignment = float.NaN, int offset = 0, bool exceedEdge = false)
    {
        var (rx, ry) = (targetArea?.x ?? 0, targetArea?.y ?? 0);
        var rw = targetArea?.width ?? Input.Bounds.width;
        var rh = targetArea?.height ?? Input.Bounds.height;
        var (x, y) = Position;
        var (w, h) = Size;
        var (rcx, rcy) = (rx + rw / 2, ry + rh / 2);
        var (cx, cy) = (x + w / 2, y + h / 2);
        var notNan = float.IsNaN(alignment) == false;

        offset -= 1;

        if (notNan && edge is Pivot.Top or Pivot.Bottom && targetEdge is Pivot.Top or Pivot.Bottom)
            x = (int)MathF.Round(Map(alignment, (0, 1), exceedEdge ? (rx - w - offset, rx + rw + offset) : (rx, rx + rw - w)));
        else if (notNan && edge is Pivot.Left or Pivot.Right && targetEdge is Pivot.Left or Pivot.Right)
            y = (int)MathF.Round(Map(alignment, (0, 1), exceedEdge ? (ry - h - offset, ry + rh + offset) : (ry, ry + rh - h)));

        if (edge == Pivot.Left && targetEdge is Pivot.Left or Pivot.Right)
            x = targetEdge == Pivot.Left ? rx + offset + 1 : rx + rw + offset;
        else if (edge == Pivot.Left)
            x = notNan ? rcx - cx : (int)MathF.Round(Map(alignment, (0, 1), (rx, rx + rw + offset)));

        if (edge == Pivot.Right && targetEdge is Pivot.Left or Pivot.Right)
            x = targetEdge == Pivot.Left ? rx - w - offset : rx + rw + offset;
        else if (edge == Pivot.Right)
            x = notNan ? rcx - cx : (int)MathF.Round(Map(alignment, (0, 1), (rx - w - offset, rx + rw - w)));

        if (edge == Pivot.Top && targetEdge is Pivot.Left or Pivot.Right)
            y = notNan ? rcy - cy : (int)MathF.Round(Map(alignment, (0, 1), (ry, ry + rh + offset)));
        else if (edge == Pivot.Top)
            y = targetEdge == Pivot.Top ? ry : ry + rh + offset;

        if (edge == Pivot.Bottom && targetEdge is Pivot.Left or Pivot.Right)
            y = notNan ? rcy - cy : (int)MathF.Round(Map(alignment, (0, 1), (ry - h - offset, ry + rh - h)));
        else if (edge == Pivot.Bottom)
            y = targetEdge == Pivot.Top ? ry - h - offset : ry + rh - h + offset + 1;

        Position = (x, y);
    }
    public void AlignInside(PointF alignment, Area? targetArea = null)
    {
        var (rx, ry) = (targetArea?.x ?? 0, targetArea?.y ?? 0);
        var rw = targetArea?.width ?? Input.Bounds.width;
        var rh = targetArea?.height ?? Input.Bounds.height;
        var (x, y) = Position;
        var (w, h) = Size;

        var newX = Map(alignment.x, (0, 1), (rx, rw - w));
        var newY = Map(alignment.y, (0, 1), (ry, rh - h));
        Position = (
            float.IsNaN(alignment.x) ? x : (int)newX,
            float.IsNaN(alignment.y) ? y : (int)newY);
    }
    public void AlignOutside(Pivot targetEdge, Area? targetArea = null, float alignment = float.NaN, int offset = 0, bool exceedEdge = false)
    {
        var opposites = new[] { Pivot.Right, Pivot.Left, Pivot.Bottom, Pivot.Top };
        AlignEdges(opposites[(int)targetEdge], targetEdge, targetArea, alignment, offset + 1,
            exceedEdge);
    }
    public void Fit(Area? targetArea = null)
    {
        var (w, h) = Size;
        var (rx, ry) = (targetArea?.x ?? 0, targetArea?.y ?? 0);
        var rw = targetArea?.width ?? Input.Bounds.width;
        var rh = targetArea?.height ?? Input.Bounds.height;
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
        justInteracted.Add(interaction);

        if (interactions.TryGetValue(interaction, out var act))
            act.Invoke();
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
    /// Called when input is received.
    /// </summary>
    protected virtual void OnInput()
    {
    }

    /// <summary>
    /// Converts the block to a tuple of integers representing the position and size.
    /// </summary>
    /// <param name="block">The block to convert.</param>
    /// <returns>A tuple of integers representing the position and size of the Block.</returns>
    public static implicit operator Area(Block block)
    {
        return (block.Position.x, block.Position.y, block.Size.width, block.Size.height);
    }

#region Backend
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    internal class DoNotSave : Attribute;

    internal PointI position;
    internal Size size, sizeMin = (1, 1), sizeMax = (int.MaxValue, int.MaxValue);
    internal bool hasParent, isTextReadonly;
    internal string text = string.Empty;
    internal Area mask;

    [DoNotSave]
    internal (int, int) listSizeTrimOffset;
    [DoNotSave]
    internal bool wasMaskSet;
    [DoNotSave]
    private bool isReadyForDoubleClick;
    [DoNotSave]
    private readonly List<Interaction> justInteracted = [];
    [DoNotSave]
    private readonly Dictionary<Interaction, Action> interactions = new();

    internal void LimitSizeMin(Size minimumSize)
    {
        if (Size.width < minimumSize.width)
            size = (minimumSize.width, Size.height);
        if (Size.height < minimumSize.height)
            size = (Size.width, minimumSize.height);
    }
    internal void LimitSizeMax(Size maximumSize)
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
        if ((isAllowed == false && isReadyForDoubleClick) || IsHovered == false)
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

            if (isAllowed)
                Interact(Interaction.DoubleTrigger);

            isReadyForDoubleClick = false;
        }

        if (IsHovered && Input.IsButtonJustPressed())
            IsPressedAndHeld = true;

        if (IsHovered == false && Input.IsButtonJustReleased())
            IsPressedAndHeld = false;
    }

    private static float Map(float number, Range range, Range targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
#endregion
}