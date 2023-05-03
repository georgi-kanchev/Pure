namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface checkbox element.
/// A checkbox is a type of button that can be toggled between a checked and unchecked state.
/// </summary>
public class Checkbox : Button
{
	/// <summary>
	/// Gets or sets a value indicating whether the checkbox is checked.
	/// </summary>
	public bool IsChecked { get; set; }

	/// <summary>
	/// Initializes a new checkbox instance with the specified position.
	/// </summary>
	/// <param name="position">The position of the checkbox.</param>
	public Checkbox((int x, int y) position) : base(position, (1, 1)) { }

	/// <summary>
	/// Responds to a user event on the checkbox. Subclasses should 
	/// override this method to implement their own behavior.
	/// </summary>
	/// <param name="userEvent">The user event that occurred.</param>
	protected override void OnUserEvent(UserEvent userEvent)
	{
		if (userEvent == UserEvent.Trigger)
			IsChecked = IsChecked == false;
	}
}
