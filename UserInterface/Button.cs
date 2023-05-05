namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface button element.
/// </summary>
public class Button : Element
{
	/// <summary>
	/// Initializes a new button instance with the specified position and size.
	/// </summary>
	/// <param name="position">The position of the button.</param>
	/// <param name="size">The size of the button.</param>
	public Button((int x, int y) position) : base(position)
	{
		Text = "Button";
		Size = (10, 1);
	}

	/// <summary>
	/// Called when the button needs to be updated. Subclasses should 
	/// override this method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		if (IsDisabled == false && IsHovered)
			MouseCursorResult = MouseCursor.Hand;
	}
}
