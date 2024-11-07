namespace Pure.Engine.UserInterface;

using System.Globalization;

public class Stepper : Block
{
    [DoNotSave]
    public Button Increase { get; }
    [DoNotSave]
    public Button Decrease { get; }
    [DoNotSave]
    public Button Minimum { get; }
    [DoNotSave]
    public Button Middle { get; }
    [DoNotSave]
    public Button Maximum { get; }

    public float Value
    {
        get => value;
        set
        {
            value = Math.Clamp(value, Range.minimum, Range.maximum);
            value = MathF.Round(value, Math.Min(Precision(Step), 6));

            var changed = Math.Abs(this.value - value) > 0.001f;
            this.value = value;

            if (changed)
                Interact(Interaction.Select);
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

    public Stepper() : this((0, 0))
    {
    }
    public Stepper((int x, int y) position, float value = 0) : base(position)
    {
        Size = (10, 2);
        Value = value;

        OnUpdate += OnRefresh;

        Increase = new((int.MaxValue, int.MaxValue))
            { size = (1, 1), wasMaskSet = true, hasParent = true };
        Decrease = new((int.MaxValue, int.MaxValue))
            { size = (1, 1), wasMaskSet = true, hasParent = true };
        Minimum = new((int.MaxValue, int.MaxValue))
            { size = (1, 1), wasMaskSet = true, hasParent = true };
        Middle = new((int.MaxValue, int.MaxValue))
            { size = (1, 1), wasMaskSet = true, hasParent = true };
        Maximum = new((int.MaxValue, int.MaxValue))
            { size = (1, 1), wasMaskSet = true, hasParent = true };

        Increase.OnInteraction(Interaction.Scroll, ApplyScroll);
        Increase.OnInteraction(Interaction.Trigger, () => Value += Step);
        Increase.OnInteraction(Interaction.PressAndHold, () =>
        {
            if (Increase.IsHovered)
                Increase.Interact(Interaction.Trigger);
        });
        Decrease.OnInteraction(Interaction.Scroll, ApplyScroll);
        Decrease.OnInteraction(Interaction.Trigger, () => Value -= Step);
        Decrease.OnInteraction(Interaction.PressAndHold, () =>
        {
            if (Decrease.IsHovered)
                Decrease.Interact(Interaction.Trigger);
        });

        Minimum.OnInteraction(Interaction.Scroll, ApplyScroll);
        Minimum.OnInteraction(Interaction.Trigger, () => Value = Range.minimum);

        Middle.OnInteraction(Interaction.Scroll, ApplyScroll);
        Middle.OnInteraction(Interaction.Trigger, () =>
        {
            var isInfinity = float.IsInfinity(Range.maximum) && float.IsInfinity(Range.minimum);
            Value = Snap(isInfinity ? 0 : (Range.maximum + Range.minimum) / 2, Step);
        });

        Maximum.OnInteraction(Interaction.Scroll, ApplyScroll);
        Maximum.OnInteraction(Interaction.Trigger, () => Value = Range.maximum);
    }

#region Backend
    private float value;
    private (float min, float max) range = (float.NegativeInfinity, float.PositiveInfinity);

    internal override void ApplyScroll()
    {
        Value += Input.ScrollDelta * Step;
    }
    internal void OnRefresh()
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

        Increase.mask = mask;
        Decrease.mask = mask;
        Minimum.mask = mask;
        Middle.mask = mask;
        Maximum.mask = mask;

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

        var precision = Math.Min(Precision(interval), 6);
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