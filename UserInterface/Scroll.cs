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
            if (hasParent == false) isVertical = value;
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

    [MemberNotNull(nameof(Slider))]
    [MemberNotNull(nameof(Increase))]
    [MemberNotNull(nameof(Decrease))]
    private void Init()
    {
        isParent = true;

        Slider = new((0, 0), IsVertical ? Size.height : Size.width, IsVertical) { hasParent = true };
        Increase = new((0, 0)) { Size = (1, 1), hasParent = true };
        Decrease = new((0, 0)) { Size = (1, 1), hasParent = true };
        var dir = IsVertical ? -1 : 1;

        Increase.SubscribeToUserAction(UserAction.Trigger, () => Slider.Progress += dir * Step);
        Increase.SubscribeToUserAction(UserAction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Slider.Progress += dir * Step;
        });
        Decrease.SubscribeToUserAction(UserAction.Trigger, () => Slider.Progress += dir * -Step);
        Decrease.SubscribeToUserAction(UserAction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Slider.Progress += dir * -Step;
        });
    }

    internal override void OnUpdate()
    {
        LimitSizeMin(IsVertical ? (1, 2) : (2, 1));

        Slider.isVertical = IsVertical;
    }
    internal override void OnInput()
    {
        // buttons gain focus priority over the slider so
        // retrigger the scrolling behavior when scrolling over them
        TryScrollWhileHoverButton(Increase);
        TryScrollWhileHoverButton(Decrease);
        TryScrollWhileHoverButton(Slider.Handle);
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

    private void TryScrollWhileHoverButton(Element btn)
    {
        if (btn.IsHovered && Input.Current.ScrollDelta != 0 && btn.IsFocused &&
            FocusedPrevious == btn)
            Slider.Progress -= Input.Current.ScrollDelta * Step;
    }
#endregion
}