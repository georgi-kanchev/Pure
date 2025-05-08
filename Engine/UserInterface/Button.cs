namespace Pure.Engine.UserInterface;

using System.Diagnostics;

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
			if (IsToggle == false)
				return;

			var prev = isSelected;

			if (hasParent == false)
				isSelected = value;

			if (prev != isSelected)
				Interact(Interaction.Select);
		}
	}
	public bool IsToggle
	{
		get => isToggle;
		set
		{
			if (hasParent == false)
				isToggle = value;

			if (isToggle == false)
				isSelected = false;
		}
	}
	public (int id, bool holdable) Hotkey { get; set; }

	public Button() : this((0, 0))
	{
	}
	/// <summary>
	/// Initializes a new button instance with the specified position and default size of (10, 1).
	/// </summary>
	/// <param name="position">The position of the button.</param>
	public Button(PointI position) : base(position)
	{
		Size = (10, 1);
		Hotkey = (-1, false);

		hold.Start();
		holdTrigger.Start();

		OnInteraction(Interaction.Trigger, () =>
		{
			if (IsToggle == false)
				return;

			var prev = isSelected;
			// not using property since the user click can access it despite of parent
			isSelected = isSelected == false;

			if (prev != isSelected)
				Interact(Interaction.Select);
		});
	}

#region Backend
	internal bool isSelected, isToggle;
	private static readonly Stopwatch hold = new(), holdTrigger = new();

	protected override void OnInput()
	{
		if (IsHovered)
			Input.CursorResult = MouseCursor.Hand;

		if (Hotkey.id == -1 ||
		    Input.PressedKeys?.Length != 1 ||
		    Input.IsTyping)
			return; // ensure single hotkey press & not typing

		if (Input.IsKeyJustPressed((Key)Hotkey.id))
		{
			Interact(Interaction.Trigger);
			hold.Restart();
		}

		if (Hotkey.holdable == false)
			return;

		var isJustHeld = false;
		if (hold.Elapsed.TotalSeconds > Input.HOLD_DELAY &&
		    holdTrigger.Elapsed.TotalSeconds > Input.HOLD_INTERVAL)
		{
			holdTrigger.Restart();
			isJustHeld = true;
		}

		if (Input.IsKeyPressed((Key)Hotkey.id) && isJustHeld)
			Interact(Interaction.Trigger);
	}
#endregion
}