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
        Init();
        IsSelected = GrabBool(bytes);
    }
    /// <summary>
    /// Initializes a new button instance with the specified position and default size of (10, 1).
    /// </summary>
    /// <param name="position">The position of the button.</param>
    public Button((int x, int y) position) : base(position)
    {
        Init();
        Size = (10, 1);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsSelected);
        return result.ToArray();
    }

#region Backend
    internal bool isSelected;

    private void Init()
    {
        OnUserAction(UserAction.Trigger, () =>
        {
            // not using property since the user click can access it despite of parent
            isSelected = isSelected == false;

            if (IsSelected)
                SimulateUserAction(UserAction.Select);
        });
    }
    protected override void OnInput()
    {
        if (IsHovered)
            MouseCursorResult = MouseCursor.Hand;
    }
#endregion
}