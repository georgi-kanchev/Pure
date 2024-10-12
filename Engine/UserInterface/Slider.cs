namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a user interface slider.
/// </summary>
public class Slider : Block
{
    /// <summary>
    /// Gets the handle button of the slider.
    /// </summary>
    public Button Handle { get; private set; }

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
            if (Math.Abs(progress - value) > 0.001f)
                Interact(Interaction.Select);

            progress = Math.Clamp(value, 0, 1);
        }
    }

    public Slider((int x, int y) position = default, bool vertical = false) : base(position)
    {
        IsVertical = vertical;
        Size = vertical ? (1, 10) : (10, 1);
        Init();
    }
    public Slider(byte[] bytes) : base(bytes)
    {
        var b = Decompress(bytes);
        IsVertical = GrabBool(b);

        Init();
        Progress = GrabFloat(b);
        index = GrabInt(b);
    }
    public Slider(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        Put(result, IsVertical);
        Put(result, Progress);
        Put(result, index);
        return Compress(result.ToArray());
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
    /// <param name="point">The position to try move the handle to.</param>
    public void MoveTo((int x, int y) point)
    {
        var sz = IsVertical ? Size.height : Size.width;
        var (x, y) = Position;
        var (px, py) = point;
        index = IsVertical ? py - y : px - x;
        index = Math.Clamp(Math.Max(index, 0), 0, Math.Max(sz - 1, 0));
        Progress = Map(index, 0, sz - 1, 0, 1);
    }

    protected override void OnInput()
    {
        if (IsHovered)
            Input.CursorResult = MouseCursor.Hand;
    }

    public Slider Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Slider slider)
    {
        return slider.ToBytes();
    }
    public static implicit operator Slider(byte[] bytes)
    {
        return new(bytes);
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

        Handle = new((int.MaxValue, int.MaxValue))
            { Size = (1, 1), wasMaskSet = true, hasParent = true };
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
            Handle.position = (x + index, y);
            Handle.size = (1, h);
        }

        Handle.mask = mask;
        Handle.Update();
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
    #endregion
}