using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Stepper : Element
{
    public Button Increase { get; private set; }
    public Button Decrease { get; private set; }

    public float Value
    {
        get => value;
        set => this.value = Math.Clamp(value, Range.minimum, Range.maximum);
    }
    public (float minimum, float maximum) Range
    {
        get => range;
        set
        {
            if (value.minimum > value.maximum)
                (value.minimum, value.maximum) = (value.maximum, value.minimum);

            range = value;
            Value = this.value; // reclamp with new range
        }
    }
    public float Step { get; set; } = 1f;

    public Stepper((int x, int y) position, float value = 0) : base(position)
    {
        Size = (10, 2);
        Value = value;

        Init();
    }
    public Stepper(byte[] bytes) : base(bytes)
    {
        Range = (GrabFloat(bytes), GrabFloat(bytes));
        Value = GrabFloat(bytes);
        Step = GrabFloat(bytes);

        Init();
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutFloat(result, Range.minimum);
        PutFloat(result, Range.maximum);
        PutFloat(result, Value);
        PutFloat(result, Step);
        return result.ToArray();
    }

#region Backend
    private float value;
    private (float min, float max) range = (float.NegativeInfinity, float.PositiveInfinity);

    [MemberNotNull(nameof(Increase))]
    [MemberNotNull(nameof(Decrease))]
    private void Init()
    {
        isParent = true;

        Increase = new((0, 0)) { size = (1, 1), hasParent = true };
        Decrease = new((0, 0)) { size = (1, 1), hasParent = true };

        Increase.SubscribeToUserAction(UserAction.Trigger, () => Value += Step);
        Decrease.SubscribeToUserAction(UserAction.Trigger, () => Value -= Step);

        Increase.SubscribeToUserAction(UserAction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Value += Step;
        });
        Decrease.SubscribeToUserAction(UserAction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Value -= Step;
        });
    }

    internal override void OnInput()
    {
        var delta = Input.Current.ScrollDelta;

        if (IsHovered == false || delta == 0)
            return;

        Value += delta > 0 ? Step : -Step;
    }
    internal override void OnUpdate()
    {
        LimitSizeMin((1, 2));
    }
    internal override void OnChildrenUpdate()
    {
        Increase.position = Position;
        Decrease.position = (Position.x, Position.y + Size.height - 1);

        Increase.InheritParent(this);
        Decrease.InheritParent(this);

        Increase.Update();
        Decrease.Update();
    }
#endregion
}