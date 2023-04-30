namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface button element.
/// </summary>
public class Button : UserInterface
{
	/// <summary>
	/// Initializes a new button instance with the specified position and size.
	/// </summary>
	/// <param name="position">The position of the button.</param>
	/// <param name="size">The size of the button.</param>
	public Button((int x, int y) position, (int width, int height) size) : base(position, size) { }

	/// <summary>
	/// Called when the button needs to be updated. Subclasses should 
	/// override this method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		if (IsDisabled == false && IsHovered)
			SetMouseCursor(MouseCursor.TILE_HAND);
	}
}
