namespace Pure.UserInterface;

public class Scroll : Element
{
	/// <summary>
	/// Gets the slider part of the scroll.
	/// </summary>
	public Slider Slider { get; }
	/// <summary>
	/// Gets the button used to scroll up.
	/// </summary>
	public Button Up { get; }
	/// <summary>
	/// Gets the button used to scroll down.
	/// </summary>
	public Button Down { get; }

	public bool IsVertical { get; private set; }

	public Scroll((int x, int y) position, int size = 5, bool isVertical = true) : base(position)
	{
		IsVertical = isVertical;
		Size = (IsVertical ? 1 : size, IsVertical ? size : 1);

		Slider = new(default, size, isVertical) { hasParent = true };
		Up = new(default) { Size = (1, 1), hasParent = true };
		Down = new(default) { Size = (1, 1), hasParent = true };

		Up.SubscribeToUserEvent(UserEvent.Press, () => Slider.Move(1));
		Up.SubscribeToUserEvent(UserEvent.PressAndHold, () => Slider.Move(1));
		Down.SubscribeToUserEvent(UserEvent.Press, () => Slider.Move(-1));
		Down.SubscribeToUserEvent(UserEvent.PressAndHold, () => Slider.Move(-1));
	}

	protected override void OnUpdate()
	{
		Size = (IsVertical ? 1 : Size.width, IsVertical ? Size.height : 1);

		var (x, y) = Position;
		var (w, h) = Size;

		if (IsVertical)
		{
			Up.position = (x, y);
			Down.position = (x, y + h - 1);
			Slider.position = (x, y + 1);
			Slider.size = (w, h - 2);
		}
		else
		{
			Up.position = (x + w - 1, y);
			Down.position = (x, y);
			Slider.position = (x + 1, y);
			Slider.size = (w - 2, h);
		}

		Slider.Update();
		Up.Update();
		Down.Update();
	}

	#region Backend

	#endregion
}