﻿namespace Pure.Engine.UserInterface;

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
        OnInteraction(Interaction.Trigger, () =>
        {
            // not using property since the user click can access it despite of parent
            isSelected = isSelected == false;

            if (IsSelected)
                SimulateInteraction(Interaction.Select);
        });
    }
    protected override void OnInput()
    {
        if (IsHovered)
            Input.CursorResult = MouseCursor.Hand;
    }
#endregion
}