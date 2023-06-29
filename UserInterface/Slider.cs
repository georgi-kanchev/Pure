using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface slider element.
/// </summary>
public class Slider : Element
{
    /// <summary>
    /// Gets or sets a value indicating whether this slider is vertical or horizontal.
    /// </summary>
    public bool IsVertical
    {
        get => isVertical;
        set
        {
            if (hasParent == false) isVertical = value;
        }
    }
    /// <summary>
    /// Gets or sets the current progress of the slider (ranged 0 to 1).
    /// </summary>
    public float Progress
    {
        get => progress;
        set
        {
            var sz = IsVertical ? Size.height : Size.width;

            progress = value;
            index = (int)Map(progress, 0, 1, 0, sz);
            UpdateHandle();
        }
    }

    /// <summary>
    /// Gets the handle button of the slider.
    /// </summary>
    public Button Handle { get; private set; }

    /// <summary>
    /// Initializes a slider new instance with the specified position, size and orientation.
    /// </summary>
    /// <param name="position">The position of the slider.</param>
    /// <param name="size">The size of the slider.</param>
    /// <param name="isVertical">Whether the slider is vertical or horizontal.</param>
    public Slider((int x, int y) position, int size = 5, bool isVertical = false) : base(position)
    {
        IsVertical = isVertical;
        Size = isVertical ? (1, size) : (size, 1);

        Init();
    }
    public Slider(byte[] bytes) : base(bytes)
    {
        IsVertical = GrabBool(bytes);

        Init();
        progress = GrabFloat(bytes);
        index = GrabInt(bytes);

        UpdateHandle();
    }

    /// <summary>
    /// Moves the handle of the slider by the specified amount.
    /// </summary>
    /// <param name="delta">The amount to move the handle.</param>
    public void Move(int delta)
    {
        var sz = IsVertical ? Size.height : Size.width;
        index -= delta;
        index = Math.Clamp(Math.Max(index, 0), 0, Math.Max(sz - 1, 0));

        UpdateHandle();
    }
    /// <summary>
    /// Tries to move the handle of the slider to the specified position. Picks the closest
    /// position on the slider if not successful.
    /// </summary>
    /// <param name="position">The position to try move the handle to.</param>
    public void MoveTo((int x, int y) position)
    {
        var sz = IsVertical ? Size.height : Size.width;
        var (x, y) = Position;
        var (px, py) = position;
        index = IsVertical ? py - y : px - x;
        index = Math.Clamp(Math.Max(index, 0), 0, Math.Max(sz - 1, 0));

        UpdateHandle();
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsVertical);
        PutFloat(result, Progress);
        PutInt(result, index);
        return result.ToArray();
    }

    /// <summary>
    /// Called when the slider needs to be updated. This handles all of the user input
    /// the slider needs for its behavior. Subclasses should override this 
    /// method to implement their own behavior.
    /// </summary>
    protected override void OnUpdate()
    {
        LimitSizeMin(IsVertical ? (1, 2) : (2, 1));

        UpdateHandle();

        if (IsDisabled)
            return;

        if (IsHovered)
            MouseCursorResult = MouseCursor.Hand;

        if (IsHovered && Input.Current.ScrollDelta != 0 && IsFocused && FocusedPrevious == this)
            Move(Input.Current.ScrollDelta);
    }
    protected override void OnUserAction(UserAction userAction)
    {
        if (IsDisabled || userAction != UserAction.Trigger)
            return;

        var (x, y) = Input.Current.Position;
        MoveTo(((int)x, (int)y));
    }
    protected override void OnDrag((int x, int y) delta)
    {
        if (IsDisabled || FocusedPrevious != this)
            return;

        var (x, y) = Input.Current.Position;
        MoveTo(((int)x, (int)y));
    }

    #region Backend
    internal float progress;
    internal int index;
    internal bool isVertical;

    [MemberNotNull(nameof(Handle))]
    private void Init()
    {
        Handle = new(position) { Size = (1, 1), hasParent = true };
    }

    private void UpdateHandle()
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var curSz = IsVertical ? Size.height : Size.width;
        var sz = Math.Max(0, curSz - 1);
        index = Math.Clamp(index, 0, sz);
        progress = Map(index, 0, curSz - 1, 0, 1);

        if (IsVertical)
        {
            Handle.position = (x, y + index);
            Handle.size = (w, 1);
        }
        else
        {
            Handle.position = (x + index, y);
            Handle.size = (1, h);
        }
    }
    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
    #endregion
}