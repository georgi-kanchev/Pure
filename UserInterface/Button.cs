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

	public Button(byte[] bytes) : base(bytes)
	{
		IsSelected = BitConverter.ToBoolean(GetBytes(bytes, 1));
	}
	/// <summary>
	/// Initializes a new button instance with the specified position and default size of (10, 1).
	/// </summary>
	/// <param name="position">The position of the button.</param>
	public Button((int x, int y) position) : base(position)
	{
		Text = "Button";
		Size = (10, 1);
	}

	public override byte[] ToBytes()
	{
		var result = base.ToBytes().ToList();
		result.AddRange(BitConverter.GetBytes(IsSelected));
		return result.ToArray();
	}

	/// <summary>
	/// Called when the button needs to be updated. Subclasses should 
	/// override this method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		if(IsDisabled == false && IsHovered)
			MouseCursorResult = MouseCursor.Hand;
	}
	/// <summary>
	/// Responds to a user event on the button. Subclasses should 
	/// override this method to implement their own behavior.
	/// </summary>
	/// <param name="userEvent">The user event that occurred.</param>
	protected override void OnUserAction(UserAction userEvent)
	{
		if(userEvent == UserAction.Trigger)
			IsSelected = IsSelected == false;
	}
}
