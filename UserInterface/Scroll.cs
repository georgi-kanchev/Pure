using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Scroll : Element
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

    public Scroll((int x, int y) position, int size = 5, bool isVertical = true) : base(position)
    {
        IsVertical = isVertical;
        Size = IsVertical ? (1, size) : (size, 1);

        Init();
    }
    public Scroll(byte[] bytes) : base(bytes)
    {
        IsVertical = GrabBool(bytes);
        Size = IsVertical ? (1, Size.height) : (Size.width, 1);

        Init();
        Slider.progress = GrabFloat(bytes);
        Slider.index = GrabInt(bytes);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsVertical);
        PutFloat(result, Slider.Progress);
        PutInt(result, Slider.index);
        return result.ToArray();
    }

#region Backend
    internal bool isVertical;
    internal float step = 0.1f;

    [MemberNotNull(nameof(Slider), nameof(Increase), nameof(Decrease))]
    private void Init()
    {
        Slider = new((0, 0), IsVertical ? Size.height : Size.width, IsVertical) { hasParent = true };
        Increase = new((0, 0)) { Size = (1, 1), hasParent = true };
        Decrease = new((0, 0)) { Size = (1, 1), hasParent = true };
        var dir = IsVertical ? -1 : 1;

        Slider.OnUserAction(UserAction.Scroll, ApplyScroll);

        Increase.OnUserAction(UserAction.Scroll, ApplyScroll);
        Increase.OnUserAction(UserAction.Trigger, () => Slider.Progress += dir * Step);
        Increase.OnUserAction(UserAction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Slider.Progress += dir * Step;
        });

        Decrease.OnUserAction(UserAction.Scroll, ApplyScroll);
        Decrease.OnUserAction(UserAction.Trigger, () => Slider.Progress += dir * -Step);
        Decrease.OnUserAction(UserAction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Slider.Progress += dir * -Step;
        });
    }
    internal override void ApplyScroll()
    {
        if (Slider.IsHovered == false)
            Slider.ApplyScroll();
    }

    internal override void OnUpdate()
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
            Increase.position = (x, y);
            Increase.size = (w, 1);
            Decrease.position = (x, y + h - 1);
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