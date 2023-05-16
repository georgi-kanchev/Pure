namespace Pure.UserInterface;

/// <summary>
/// Represents a user interface slider element.
/// </summary>
public class Slider : Element
{
	/// <summary>
	/// Gets or sets a value indicating whether this slider is vertical or horizontal.
	/// </summary>
	public bool IsVertical { get; set; }
	/// <summary>
	/// Gets or sets the current progress of the slider (ranged 0 to 1).
	/// </summary>
	public float Progress
	{
		get => progress;
		set
		{
			var size = IsVertical ? Size.Item2 : Size.Item1;

			progress = value;
			index = (int)MathF.Round(Map(progress, 0, 1, 0, size - 1));
		}
	}

	/// <summary>
	/// Gets the handle button of the slider.
	/// </summary>
	public Button Handle { get; }

	/// <summary>
	/// Initializes a slider new instance with the specified position, size and orientation.
	/// </summary>
	/// <param name="position">The position of the slider.</param>
	/// <param name="size">The size of the slider.</param>
	/// <param name="isVertical">Whether the slider is vertical or horizontal.</param>
	public Slider((int x, int y) position, int size = 5, bool isVertical = false) : base(position)
	{
		IsVertical = isVertical;
		Handle = new(position) { Size = (1, 1), hasParent = true };
		Size = isVertical ? (1, size) : (size, 1);
	}

	/// <summary>
	/// Moves the handle of the slider by the specified amount.
	/// </summary>
	/// <param name="delta">The amount to move the handle.</param>
	public void Move(int delta)
	{
		var size = IsVertical ? Size.height : Size.width;
		index += delta * (IsVertical ? -1 : 1);
		index = Math.Clamp(index, 0, size - 1);

		UpdateHandle();
	}
	/// <summary>
	/// Tries to move the handle of the slider to the specified position. Picks the closest
	/// position on the slider if not succesful.
	/// </summary>
	/// <param name="position">The position to try move the handle to.</param>
	public void MoveTo((int x, int y) position)
	{
		var size = IsVertical ? Size.Item2 : Size.Item1;
		var (x, y) = Position;
		var (px, py) = position;
		index = IsVertical ? py - y : px - x;
		index = Math.Clamp(index, 0, size - 1);

		UpdateHandle();
	}

	/// <summary>
	/// Called when the slider needs to be updated. This handles all of the user input
	/// the slider needs for its behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		UpdateHandle();

		if (IsDisabled)
			return;

		if (IsHovered)
		{
			if (IsDisabled == false)
				MouseCursorResult = MouseCursor.Hand;

			if (Input.Current.ScrollDelta != 0)
				Move(Input.Current.ScrollDelta);
		}

		if (IsPressedAndHeld)
		{
			var p = Input.Current.Position;
			MoveTo(((int)p.Item1, (int)p.Item2));
			TriggerUserEvent(UserEvent.Drag);
		}
	}

	#region Backend
	private float progress;
	private int index;

	private void UpdateHandle()
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var size = IsVertical ? Size.height : Size.width;
		var sz = Math.Max(0, size - 1);
		index = Math.Clamp(index, 0, sz);
		progress = Map(index, 0, size - 1, 0, 1);

		if (IsVertical)
		{
			Handle.position = (x, y + index);
			Handle.size = (w, 1);
		}
		else
		{
			Handle.position = (x + index, y);
			Handle.size = (1, h);
		}
	}
	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	#endregion
}
