using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class NumericScroll : Element
{
	public Button Up { get; private set; }
	public Button Down { get; private set; }

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

	public NumericScroll((int x, int y) position, float value = 0) : base(position)
	{
		Size = (1, 3);

		Value = value;

		Init();
	}
	public NumericScroll(byte[] bytes) : base(bytes)
	{
		Range = (GrabFloat(bytes), GrabFloat(bytes));
		Value = GrabFloat(bytes);
		Step = GrabFloat(bytes);

		Init();
	}

	protected override void OnUpdate()
	{
		Down.Update();
		Up.Update();
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
	private (float min, float max) range = (float.MinValue, float.MaxValue);

	[MemberNotNull(nameof(Up))]
	[MemberNotNull(nameof(Down))]
	private void Init()
	{
		Down = new((Position.x, Position.y + Size.height - 1)) { Size = (1, 1), hasParent = true };
		Up = new(Position) { Size = (1, 1), hasParent = true };

		Down.SubscribeToUserAction(UserAction.Press, () => Value--);
		Up.SubscribeToUserAction(UserAction.Press, () => Value++);

		Down.SubscribeToUserAction(UserAction.PressAndHold, () => Value--);
		Up.SubscribeToUserAction(UserAction.PressAndHold, () => Value++);
	}
	#endregion
}