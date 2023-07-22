namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface button element.
/// </summary>
public class Button : Element
{
    /// <summary>
    /// Gets or sets a value indicating whether the button is selected.
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (hasParent == false) isSelected = value;
        }
    }

    public Button(byte[] bytes) : base(bytes)
    {
        IsSelected = GrabBool(bytes);
    }
    /// <summary>
    /// Initializes a new button instance with the specified position and default size of (10, 1).
    /// </summary>
    /// <param name="position">The position of the button.</param>
    public Button((int x, int y) position) : base(position)
    {
        Size = (10, 1);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsSelected);
        return result.ToArray();
    }

    /// <summary>
    /// Responds to a user event on the button. Subclasses should 
    /// override this method to implement their own behavior.
    /// </summary>
    /// <param name="userEvent">The user event that occurred.</param>
    protected override void OnUserAction(UserAction userEvent)
    {
        if (userEvent != UserAction.Trigger)
            return;

        isSelected = isSelected == false; // the user click can access it despite of parent

        if (IsSelected)
            TriggerUserAction(UserAction.Select);
    }

#region Backend
    internal bool isSelected;

    internal override void OnUpdate()
    {
        if (IsDisabled == false && IsHovered)
            MouseCursorResult = MouseCursor.Hand;
    }
#endregion
}