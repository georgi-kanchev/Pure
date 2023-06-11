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

		Slider = new((0, 0), size, isVertical) { hasParent = true };
		Up = new((0, 0)) { Size = (1, 1), hasParent = true };
		Down = new((0, 0)) { Size = (1, 1), hasParent = true };

		Up.SubscribeToUserAction(UserAction.Press, () => Slider.Move(1));
		Up.SubscribeToUserAction(UserAction.PressAndHold, () => Slider.Move(1));
		Down.SubscribeToUserAction(UserAction.Press, () => Slider.Move(-1));
		Down.SubscribeToUserAction(UserAction.PressAndHold, () => Slider.Move(-1));
	}

	protected override void OnUpdate()
	{
		Size = (IsVertical ? 1 : Size.width, IsVertical ? Size.height : 1);

		var (x, y) = Position;
		var (w, h) = Size;

		if(IsVertical)
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
}