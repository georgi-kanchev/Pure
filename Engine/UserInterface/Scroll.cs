namespace Pure.Engine.UserInterface;

public class Scroll : Block
{
    /// <summary>
    /// Gets the slider part of the scroll.
    /// </summary>
    [DoNotSave]
    public Slider Slider { get; }

    [DoNotSave]
    public Button Increase { get; }
    [DoNotSave]
    public Button Decrease { get; }

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

    public Scroll() : this((0, 0))
    {
    }
    public Scroll((int x, int y) position, bool vertical = true) : base(position)
    {
        IsVertical = vertical;
        Size = IsVertical ? (1, 10) : (10, 1);
        OnUpdate += OnRefresh;

        Slider = new((int.MaxValue, int.MaxValue)) { wasMaskSet = true, hasParent = true };
        Increase = new((int.MaxValue, int.MaxValue))
            { Size = (1, 1), wasMaskSet = true, hasParent = true };
        Decrease = new((int.MaxValue, int.MaxValue))
            { Size = (1, 1), wasMaskSet = true, hasParent = true };

        Slider.OnInteraction(Interaction.Scroll, ApplyScroll);
        Slider.OnInteraction(Interaction.Select, () => Interact(Interaction.Select));

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

#region Backend
    internal bool isVertical;
    internal float step = 0.1f;

    internal override void ApplyScroll()
    {
        if (Slider.IsHovered == false)
            Slider.ApplyScroll();
    }

    internal void OnRefresh()
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