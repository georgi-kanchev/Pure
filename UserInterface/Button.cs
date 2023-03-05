namespace Pure.UserInterface;

public class Button : UserInterface
{
	public Button((int, int) position, (int, int) size) : base(position, size) { }

	protected override void OnUpdate()
	{
		if (IsDisabled == false && IsHovered)
			SetMouseCursor(MouseCursor.TILE_HAND);
	}
}
