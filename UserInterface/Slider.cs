namespace Pure.UserInterface;

public class Slider : UserInterface
{
	public bool IsVertical { get; set; }
	public float Progress
	{
		get => progress;
		set
		{
			var size = IsVertical ? Size.Item2 : Size.Item1;

			progress = value;
			index = (int)MathF.Round(Map(progress, 0, 1, 0, size - 1));
		}
	}

	public Button Handle { get; }

	public Slider((int, int) position, int size = 5, bool isVertical = false)
		: base(position, isVertical ? (1, size) : (size, 1))
	{
		IsVertical = isVertical;
		Handle = new(position, (1, 1));
	}

	public void Move(int delta)
	{
		var size = IsVertical ? Size.Item2 : Size.Item1;
		index += delta * (IsVertical ? -1 : 1);
		index = Math.Clamp(index, 0, size - 1);

		UpdateHandle();
	}
	public void MoveTo((int, int) position)
	{
		var size = IsVertical ? Size.Item2 : Size.Item1;
		var (x, y) = Position;
		var (px, py) = position;
		index = IsVertical ? py - y : px - x;
		index = Math.Clamp(index, 0, size - 1);

		UpdateHandle();
	}

	protected override void OnUpdate()
	{
		UpdateHandle();

		if (IsDisabled)
			return;

		if (IsHovered)
		{
			if (IsDisabled == false)
				SetMouseCursor(MouseCursor.TILE_HAND);

			if (CurrentInput.ScrollDelta != 0)
				Move(CurrentInput.ScrollDelta);
		}

		if (IsClicked)
		{
			var p = CurrentInput.Position;
			MoveTo(((int)p.Item1, (int)p.Item2));
			TriggerUserEvent(UserEvent.DRAG);
		}
	}

	#region Backend
	private float progress;
	private int index;

	private void UpdateHandle()
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var size = IsVertical ? Size.Item2 : Size.Item1;
		index = Math.Clamp(index, 0, size - 1);
		progress = Map(index, 0, size - 1, 0, 1);

		if (IsVertical)
		{
			Handle.Position = (x, y + index);
			Handle.Size = (w, 1);
		}
		else
		{
			Handle.Position = (x + index, y);
			Handle.Size = (1, h);
		}
	}
	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	#endregion
}
