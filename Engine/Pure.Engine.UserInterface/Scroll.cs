namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;

public class Scroll : Block
{
    /// <summary>
    /// Gets the slider part of the scroll.
    /// </summary>
    public Slider Slider { get; private set; }

    public Button Increase { get; private set; }
    public Button Decrease { get; private set; }

    public bool IsVertical
    {
        get => isVertical;
        set
        {
            if (hasParent == false)
                isVertical = value;
        }
    }
    public float Step
    {
        get => step;
        set
        {
            if (hasParent == false)
                step = Math.Clamp(value, 0, 1);
        }
    }

    public Scroll((int x, int y) position = default, bool vertical = true) : base(position)
    {
        IsVertical = vertical;
        Size = IsVertical ? (1, 10) : (10, 1);
        Init();
    }
    public Scroll(byte[] bytes) : base(bytes)
    {
        var b = Decompress(bytes);
        IsVertical = GrabBool(b);
        Step = GrabFloat(b);
        Size = IsVertical ? (1, Size.height) : (Size.width, 1);

        Init();
        Slider.progress = GrabFloat(b);
        Slider.index = GrabInt(b);
    }
    public Scroll(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        PutBool(result, IsVertical);
        PutFloat(result, Step);
        PutFloat(result, Slider.Progress);
        PutInt(result, Slider.index);
        return Compress(result.ToArray());
    }

    public Scroll Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Scroll scroll)
    {
        return scroll.ToBytes();
    }
    public static implicit operator Scroll(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    internal bool isVertical;
    internal float step = 0.1f;

    [MemberNotNull(nameof(Slider), nameof(Increase), nameof(Decrease))]
    private void Init()
    {
        OnUpdate(OnUpdate);

        Slider = new((int.MaxValue, int.MaxValue)) { wasMaskSet = true, hasParent = true };
        Increase = new((int.MaxValue, int.MaxValue))
            { Size = (1, 1), wasMaskSet = true, hasParent = true };
        Decrease = new((int.MaxValue, int.MaxValue))
            { Size = (1, 1), wasMaskSet = true, hasParent = true };

        Slider.OnInteraction(Interaction.Scroll, ApplyScroll);

        Increase.OnInteraction(Interaction.Scroll, ApplyScroll);
        Increase.OnInteraction(Interaction.Trigger, () => Slider.Progress += Step);
        Increase.OnInteraction(Interaction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Increase.Interact(Interaction.Trigger);
        });

        Decrease.OnInteraction(Interaction.Scroll, ApplyScroll);
        Decrease.OnInteraction(Interaction.Trigger, () => Slider.Progress -= Step);
        Decrease.OnInteraction(Interaction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Decrease.Interact(Interaction.Trigger);
        });
    }
    internal override void ApplyScroll()
    {
        if (Slider.IsHovered == false)
            Slider.ApplyScroll();
    }

    internal void OnUpdate()
    {
        LimitSizeMin(IsVertical ? (1, 2) : (2, 1));

        Slider.isVertical = IsVertical;
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        if (IsVertical)
        {
            Increase.position = (x, y + h - 1);
            Increase.size = (w, 1);
            Decrease.position = (x, y);
            Decrease.size = (w, 1);
            Slider.position = (x, y + 1);
            Slider.size = (w, h - 2);
        }
        else
        {
            Increase.position = (x + w - 1, y);
            Increase.size = (1, h);
            Decrease.position = (x, y);
            Decrease.size = (1, h);
            Slider.position = (x + 1, y);
            Slider.size = (w - 2, h);
        }

        Increase.mask = mask;
        Decrease.mask = mask;
        Slider.mask = mask;

        if ((IsVertical && Size.height > 2) ||
            (IsVertical == false && Size.width > 2))
            Slider.Update();

        Increase.Update();
        Decrease.Update();
    }
#endregion
}