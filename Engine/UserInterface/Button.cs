namespace Pure.Engine.UserInterface;

/// <summary>
/// Represents a user interface button.
/// </summary>
public class Button : Block
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

    /// <summary>
    /// Initializes a new button instance with the specified position and default size of (10, 1).
    /// </summary>
    /// <param name="position">The position of the button.</param>
    public Button((int x, int y) position = default) : base(position)
    {
        Init();
        Size = (10, 1);
    }
    public Button(byte[] bytes) : base(bytes)
    {
        Init();
        IsSelected = GrabBool(bytes);
    }
    public Button(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsSelected);
        return result.ToArray();
    }

    public static implicit operator string(Button button)
    {
        return button.ToBase64();
    }
    public static implicit operator Button(string base64)
    {
        return new(base64);
    }
    public static implicit operator byte[](Button button)
    {
        return button.ToBytes();
    }
    public static implicit operator Button(byte[] base64)
    {
        return new(base64);
    }

#region Backend
    internal bool isSelected;

    private void Init()
    {
        OnInteraction(Interaction.Trigger, () =>
        {
            // not using property since the user click can access it despite of parent
            isSelected = isSelected == false;

            if (IsSelected)
                Interact(Interaction.Select);
        });
    }
    protected override void OnInput()
    {
        if (IsHovered)
            Input.CursorResult = MouseCursor.Hand;
    }
#endregion
}