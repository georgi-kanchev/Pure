namespace Pure.Engine.UserInterface;

/// <summary>
/// Represents a user interface slider.
/// </summary>
public class Slider : Block
{
	/// <summary>
	/// Gets the handle button of the slider.
	/// </summary>
	[DoNotSave]
	public Button Handle { get; }

	/// <summary>
	/// Gets or sets a value indicating whether this slider is vertical or horizontal.
	/// </summary>
	public bool IsVertical
	{
		get => isVertical;
		set
		{
			if (hasParent == false)
				isVertical = value;
		}
	}
	/// <summary>
	/// Gets or sets the current progress of the slider (ranged 0 to 1).
	/// </summary>
	public float Progress
	{
		get => progress;
		set => progress = Math.Clamp(value, 0, 1);
	}

	public Slider() : this((0, 0))
	{
	}
	public Slider(PointI position, bool vertical = false) : base(position)
	{
		IsVertical = vertical;
		Size = vertical ? (1, 10) : (10, 1);
		OnInteraction(Interaction.Press, () =>
		{
			if (Handle is { IsHovered: true })
				return;

			var (x, y) = Input.Position;
			MoveHandleTo(((int)x, (int)y));
		});
		OnDrag += _ =>
		{
			var (x, y) = Input.Position;
			MoveHandleTo(((int)x, (int)y));
		};

		Handle = new((int.MaxValue, int.MaxValue)) { Size = (1, 1), wasMaskSet = true, hasParent = true };
		Handle.OnDrag += delta => MoveHandle(IsVertical ? -delta.deltaY : -delta.deltaX);
		Handle.OnInteraction(Interaction.Scroll, ApplyScroll);
	}

	/// <summary>
	/// Moves the handle of the slider by the specified amount.
	/// </summary>
	/// <param name="delta">The amount to move the handle.</param>
	public void MoveHandle(int delta)
	{
		var sz = IsVertical ? Height - (Handle.Height - 1) : Width - (Handle.Width - 1);
		index -= delta;
		index = Math.Clamp(Math.Max(index, 0), 0, Math.Max(sz - 1, 0));
		Progress = Map(index, 0, sz - 1, 0, 1);
	}
	/// <summary>
	/// Tries to move the handle of the slider to the specified position. Picks the closest
	/// position on the slider if not successful.
	/// </summary>
	/// <param name="point">The position to try to move the handle to.</param>
	public void MoveHandleTo(PointI point)
	{
		var hSz = IsVertical ? Handle.Height - 1 : Handle.Width - 1;
		var sz = IsVertical ? Height - hSz : Width - hSz;
		var (x, y) = Position;
		var (px, py) = point;
		index = IsVertical ? py - y - hSz / 2 : px - x - hSz / 2;
		index = Math.Clamp(Math.Max(index, 0), 0, Math.Max(sz - 1, 0));
		Progress = Map(index, 0, sz - 1, 0, 1);
	}

#region Backend
	internal float progress;
	internal int index;
	internal bool isVertical;

	internal override void ApplyScroll()
	{
		MoveHandle(Input.ScrollDelta);
	}
	internal override void OnChildrenUpdate()
	{
		var (x, y, w, h) = Area;
		if ((Handle.Width >= w && IsVertical == false) ||
		    (Handle.Height >= h && IsVertical))
		{
			IsHidden = true;
			IsDisabled = true;
			return;
		}

		var curSz = IsVertical ? h - (Handle.Height - 1) : w - (Handle.Width - 1);
		var sz = Math.Max(0, curSz - 1);
		index = (int)Map(progress, 0, 1, 0, sz);

		if (IsDisabled)
			Handle.IsDisabled = true;

		if (IsVertical)
		{
			Handle.size = (w, Handle.Height);
			Handle.position = (x, y + index);
		}
		else
		{
			Handle.size = (Handle.Width, h);
			Handle.position = (x + index, y);
		}

		Handle.mask = mask;
		Handle.Update();
	}

	protected override void OnInput()
	{
		if (IsHovered)
			Input.CursorResult = MouseCursor.Hand;
	}

	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
#endregion
}