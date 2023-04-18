namespace Pure.UserInterface;

public class Button : UserInterface
{
	public Button((int x, int y) position, (int width, int height) size) : base(position, size) { }

	protected override void OnUpdate()
	{
		if (IsDisabled == false && IsHovered)
			SetMouseCursor(MouseCursor.TILE_HAND);
	}
}
