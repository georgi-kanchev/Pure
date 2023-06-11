namespace Pure.UserInterface;

public class NumericScroll : Element
{
	public Button Up { get; }
	public Button Down { get; }

	public int Value
	{
		get => value;
		set => this.value = Math.Clamp(value, Range.minimum, Range.maximum);
	}
	public (int minimum, int maximum) Range
	{
		get => range;
		set
		{
			if(value.minimum > value.maximum)
				(value.minimum, value.maximum) = (value.maximum, value.minimum);

			range = value;
			Value = this.value; // reclamp with new range
		}
	}

	public NumericScroll((int x, int y) position, int value = 0, int minimum = int.MinValue, int maximum = int.MaxValue) : base(position)
	{
		Size = (1, 3);

		Range = (minimum, maximum);
		Value = value;

		Down = new((Position.x, Position.y + Size.height - 1)) { Size = (1, 1), hasParent = true };
		Up = new(Position) { Size = (1, 1), hasParent = true };

		Down.SubscribeToUserAction(UserAction.Press, () => Value--);
		Up.SubscribeToUserAction(UserAction.Press, () => Value++);

		Down.SubscribeToUserAction(UserAction.PressAndHold, () => Value--);
		Up.SubscribeToUserAction(UserAction.PressAndHold, () => Value++);
	}

	protected override void OnUpdate()
	{
		Down.Update();
		Up.Update();
	}

	#region Backend
	private int value;
	private (int min, int max) range = (int.MinValue, int.MaxValue);

	#endregion
}