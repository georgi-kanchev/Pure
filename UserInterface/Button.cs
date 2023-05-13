namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface button element.
/// </summary>
public class Button : Element
{
	/// <summary>
	/// Gets or sets a value indicating whether the button is selected.
	/// </summary>
	public bool IsSelected { get; set; }

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

	/// <summary>
	/// Responds to a user event on the button. Subclasses should 
	/// override this method to implement their own behavior.
	/// </summary>
	/// <param name="userEvent">The user event that occurred.</param>
	protected override void OnUserEvent(UserEvent userEvent)
	{
		if (userEvent == UserEvent.Trigger)
			IsSelected = IsSelected == false;
	}
}
