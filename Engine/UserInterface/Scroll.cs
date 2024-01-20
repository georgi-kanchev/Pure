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

    public Scroll((int x, int y) position = default, bool isVertical = true) : base(position)
    {
        IsVertical = isVertical;
        Size = IsVertical ? (1, 10) : (10, 1);
        Init();
    }
    public Scroll(byte[] bytes) : base(bytes)
    {
        IsVertical = GrabBool(bytes);
        Step = GrabFloat(bytes);
        Size = IsVertical ? (1, Size.height) : (Size.width, 1);

        Init();
        Slider.progress = GrabFloat(bytes);
        Slider.index = GrabInt(bytes);
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
        var result = base.ToBytes().ToList();
        PutBool(result, IsVertical);
        PutFloat(result, Step);
        PutFloat(result, Slider.Progress);
        PutInt(result, Slider.index);
        return result.ToArray();
    }

    public static implicit operator string(Scroll scroll)
    {
        return scroll.ToBase64();
    }
    public static implicit operator Scroll(string base64)
    {
        return new(base64);
    }
    public static implicit operator byte[](Scroll scroll)
    {
        return scroll.ToBytes();
    }
    public static implicit operator Scroll(byte[] base64)
    {
        return new(base64);
    }

#region Backend
    internal bool isVertical;
    internal float step = 0.1f;

    [MemberNotNull(nameof(Slider), nameof(Increase), nameof(Decrease))]
    private void Init()
    {
        OnUpdate(OnUpdate);

        Slider = new((int.MaxValue, int.MaxValue)) { hasParent = true };
        Increase = new((int.MaxValue, int.MaxValue)) { Size = (1, 1), hasParent = true };
        Decrease = new((int.MaxValue, int.MaxValue)) { Size = (1, 1), hasParent = true };

        Slider.OnInteraction(Interaction.Scroll, ApplyScroll);

        Increase.OnInteraction(Interaction.Scroll, ApplyScroll);
        Increase.OnInteraction(Interaction.Trigger, () => Slider.Progress += Step);
        Increase.OnInteraction(Interaction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Slider.Progress += Step;
        });

        Decrease.OnInteraction(Interaction.Scroll, ApplyScroll);
        Decrease.OnInteraction(Interaction.Trigger, () => Slider.Progress -= Step);
        Decrease.OnInteraction(Interaction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Slider.Progress -= Step;
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

        Slider.InheritParent(this);
        Increase.InheritParent(this);
        Decrease.InheritParent(this);

        Slider.Update();
        Increase.Update();
        Decrease.Update();
    }
#endregion
}