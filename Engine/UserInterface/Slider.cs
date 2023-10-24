namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a user interface slider.
/// </summary>
public class Slider : Block
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
        set => progress = Math.Clamp(value, 0, 1);
    }

    /// <summary>
    /// Gets the handle button of the slider.
    /// </summary>
    public Button Handle
    {
        get;
        private set;
    }

    public Slider((int x, int y) position = default, bool isVertical = false) : base(position)
    {
        IsVertical = isVertical;
        Size = isVertical ? (1, 10) : (10, 1);
        Init();
    }
    public Slider(byte[] bytes) : base(bytes)
    {
        IsVertical = GrabBool(bytes);

        Init();
        Progress = GrabFloat(bytes);
        index = GrabInt(bytes);
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
    /// Moves the handle of the slider by the specified amount.
    /// </summary>
    /// <param name="delta">The amount to move the handle.</param>
    public void Move(int delta)
    {
        var sz = IsVertical ? Size.height : Size.width;
        index -= delta;
        index = Math.Clamp(Math.Max(index, 0), 0, Math.Max(sz - 1, 0));
        Progress = Map(index, 0, sz - 1, 0, 1);
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
        Progress = Map(index, 0, sz - 1, 0, 1);
    }

    protected override void OnInput()
    {
        if (IsHovered)
            Input.CursorResult = MouseCursor.Hand;
    }

#region Backend
    internal float progress;
    internal int index;
    internal bool isVertical;

    [MemberNotNull(nameof(Handle))]
    private void Init()
    {
        OnInteraction(Interaction.Trigger, () =>
        {
            var (x, y) = Input.Position;
            MoveTo(((int)x, (int)y));
        });
        OnDrag(_ =>
        {
            var (x, y) = Input.Position;
            MoveTo(((int)x, (int)y));
        });

        Handle = new((int.MaxValue, int.MaxValue)) { Size = (1, 1), hasParent = true };
        Handle.OnDrag(_ =>
        {
            var (x, y) = Input.Position;
            MoveTo(((int)x, (int)y));
        });
        Handle.OnInteraction(Interaction.Scroll, ApplyScroll);
    }

    internal override void ApplyScroll()
    {
        Move(Input.ScrollDelta);
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var curSz = IsVertical ? Size.height : Size.width;
        var sz = Math.Max(0, curSz - 1);
        index = (int)Map(progress, 0, 1, 0, sz);

        if (IsVertical)
        {
            Handle.position = (x, y + index);
            Handle.size = (w, 1);
        }
        else
        {
            {
                Handle.position = (x + index, y);
                Handle.size = (1, h);
            }

            Handle.InheritParent(this);
            Handle.Update();
        }
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}