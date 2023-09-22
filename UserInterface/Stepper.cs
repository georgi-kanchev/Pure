namespace Pure.UserInterface;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

public class Stepper : Element
{
    public Button Increase { get; private set; }
    public Button Decrease { get; private set; }
    public Button Minimum { get; private set; }
    public Button Middle { get; private set; }
    public Button Maximum { get; private set; }

    public float Value
    {
        get => value;
        set
        {
            value = Math.Clamp(value, Range.minimum, Range.maximum);
            value = MathF.Round(value, Precision(Step));
            this.value = value;
        }
    }
    public (float minimum, float maximum) Range
    {
        get => range;
        set
        {
            if (value.minimum > value.maximum)
                (value.minimum, value.maximum) = (value.maximum, value.minimum);

            var prev = range;
            range = value;

            if (prev != range)
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

    protected override void OnUserAction(UserAction userAction)
    {
        if (userAction != UserAction.Scroll)
            return;

        ApplyScroll();
    }

#region Backend
    private float value;
    private (float min, float max) range = (float.NegativeInfinity, float.PositiveInfinity);

    [MemberNotNull(nameof(Increase), nameof(Decrease), nameof(Minimum), nameof(Middle), nameof(Maximum))]
    private void Init()
    {
        Increase = new((0, 0)) { size = (1, 1), hasParent = true };
        Decrease = new((0, 0)) { size = (1, 1), hasParent = true };
        Minimum = new((0, 0)) { size = (1, 1), hasParent = true };
        Middle = new((0, 0)) { size = (1, 1), hasParent = true };
        Maximum = new((0, 0)) { size = (1, 1), hasParent = true };

        Increase.SubscribeToUserAction(UserAction.Scroll, ApplyScroll);
        Increase.SubscribeToUserAction(UserAction.Trigger, () => Value += Step);
        Increase.SubscribeToUserAction(UserAction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Value += Step;
        });
        Decrease.SubscribeToUserAction(UserAction.Scroll, ApplyScroll);
        Decrease.SubscribeToUserAction(UserAction.Trigger, () => Value -= Step);
        Decrease.SubscribeToUserAction(UserAction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Value -= Step;
        });

        Minimum.SubscribeToUserAction(UserAction.Scroll, ApplyScroll);
        Minimum.SubscribeToUserAction(UserAction.Trigger, () => Value = Range.minimum);

        Middle.SubscribeToUserAction(UserAction.Scroll, ApplyScroll);
        Middle.SubscribeToUserAction(UserAction.Trigger,
            () =>
            {
                Value = Snap(float.IsPositiveInfinity(Range.maximum) ? 0 : Range.maximum / 2, Step);
            });

        Maximum.SubscribeToUserAction(UserAction.Scroll, ApplyScroll);
        Maximum.SubscribeToUserAction(UserAction.Trigger, () => Value = Range.maximum);
    }

    internal override void ApplyScroll()
    {
        Value += Input.Current.ScrollDelta * Step;
    }
    internal override void OnUpdate()
    {
        LimitSizeMin((4, 2));
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;
        Increase.position = Position;
        Decrease.position = (x, y + h - 1);
        Minimum.position = (x + w - 3, y + h - 1);
        Middle.position = (x + w - 2, y + h - 1);
        Maximum.position = (x + w - 1, y + h - 1);

        Increase.InheritParent(this);
        Decrease.InheritParent(this);
        Minimum.InheritParent(this);
        Middle.InheritParent(this);
        Maximum.InheritParent(this);

        Increase.Update();
        Decrease.Update();
        Minimum.Update();
        Middle.Update();
        Maximum.Update();
    }

    private static float Snap(float number, float interval)
    {
        if (float.IsInfinity(number) || Math.Abs(interval) < 0.001f)
            return number;

        var precision = Precision(interval);
        var round = MathF.Round(number % interval, precision);
        if (Precision(round) == precision)
            return number;

        // this prevents -0
        //var value = number - (number < 0 ? interval : 0);
        number -= number % interval;
        return number;
    }
    private static int Precision(float number)
    {
        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var split = number.ToString(CultureInfo.CurrentCulture).Split(cultDecPoint);
        return split.Length > 1 ? split[1].Length : 0;
    }
#endregion
}