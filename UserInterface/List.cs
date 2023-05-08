namespace Pure.UserInterface;

/// <summary>
/// Represents a user input list element storing items, represented as checkboxes that can be
/// scrolled, clicked, selected and manipulated.
/// </summary>
public class List : Element
{
	/// <summary>
	/// Gets the slider used to scroll the list.
	/// </summary>
	public Slider Scroll { get; }
	/// <summary>
	/// Gets the button used to scroll up.
	/// </summary>
	public Button ScrollUp { get; }
	/// <summary>
	/// Gets the button used to scroll down.
	/// </summary>
	public Button ScrollDown { get; }

	/// <summary>
	/// Gets the number of items in the list.
	/// </summary>
	public int Count => items.Count;
	/// <summary>
	/// Gets or sets a value indicating whether the list allows single selection or not.
	/// </summary>
	public bool IsSingleSelecting
	{
		get => isSingleSelecting;
		set { isSingleSelecting = value; TrySingleSelectOneItem(); }
	}

	/// <summary>
	/// Gets the checkbox at the specified index, or null if the index is out of range.
	/// </summary>
	/// <param name="index">The index of the checkbox to get.</param>
	public Checkbox? this[int index] => HasIndex(index) ? items[index] : default;

	/// <summary>
	/// Initializes a new list instance with the specified position, size and number of items.
	/// </summary>
	/// <param name="position">The position of the top-left corner of the list.</param>
	/// <param name="size">The size of the list.</param>
	/// <param name="count">The initial number of checkboxes in the list.</param>
	public List((int x, int y) position, int count)
		: base(position)
	{
		Size = (12, 8);
		var (x, y) = Position;
		var (w, h) = Size;

		Scroll = new((x + w - 1, y + 1), h - 2, true);
		ScrollUp = new((x + w, y)) { Size = (1, 1) };
		ScrollDown = new((x + w, y + w)) { Size = (1, 1) };

		Add(count);

		ScrollUp.SubscribeToUserEvent(UserEvent.Press, () => Scroll.Move(1));
		ScrollUp.SubscribeToUserEvent(UserEvent.Hold, () => Scroll.Move(1));
		ScrollDown.SubscribeToUserEvent(UserEvent.Press, () => Scroll.Move(-1));
		ScrollDown.SubscribeToUserEvent(UserEvent.Hold, () => Scroll.Move(-1));

		UpdateParts();
		UpdateItems();
		isInitialized = true;
	}

	/// <summary>
	/// Adds the specified number of items to the list.
	/// </summary>
	/// <param name="count">The number of items to add.</param>
	public void Add(int count = 1)
	{
		for(int i = 0; i < count; i++)
			items.Add(new(Position) { Size = (Size.Item1, 1) });

		TrySingleSelectOneItem();
	}
	/// <summary>
	/// Removes the item at the specified index and adjusts the selection accordingly.
	/// </summary>
	/// <param name="index">The index of the item to remove.</param>
	public void Remove(int index)
	{
		if(HasIndex(index) == false)
			return;

		items.RemoveAt(index);

		if(index == singleSelectedIndex && index != items.Count)
			return;
		else if(index <= singleSelectedIndex)
			singleSelectedIndex--;

		TrySingleSelectOneItem();
	}
	/// <summary>
	/// Clears all items from the list and resets the selection.
	/// </summary>
	public void Clear()
	{
		items.Clear();
		singleSelectedIndex = -1;
		TrySingleSelectOneItem();
	}

	/// <summary>
	/// Determines whether the list contains the specified checkbox item.
	/// </summary>
	/// <param name="item">The checkbox item to locate in the list.</param>
	/// <returns>True if the checkbox item is found in the list; otherwise, false.</returns>
	public bool Contains(Checkbox item)
	{
		return item != null && items.Contains(item);
	}
	/// <summary>
	/// Searches for the specified checkbox item and returns the zero-based index of the first occurrence
	/// within the entire list.
	/// </summary>
	/// <param name="item">The checkbox item to locate in the list.</param>
	/// <returns>The zero-based index of the first occurrence of the checkbox item within the entire list,
	/// if found; otherwise, -1.</returns>
	public int IndexOf(Checkbox item)
	{
		return item == null ? -1 : items.IndexOf(item);
	}

	/// <summary>
	/// Called when the list and its scroll need to be updated. This handles all of the user input
	/// the list and its scroll need for thier behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		if(IsDisabled)
			return;

		if(IsHovered && Input.Current.ScrollDelta != 0)
			Scroll.Move(Input.Current.ScrollDelta);

		TrySingleSelect();

		UpdateParts();
		UpdateItems();
	}

	/// <summary>
	/// Called when the items of the list need to be updated. This handles all of the user input
	/// for the items in the list need for thier behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected virtual void OnItemUpdate(Checkbox item) { }

	/// <summary>
	/// Implicitly converts an array of checkbox objects to a list object.
	/// </summary>
	/// <param name="items">The array of checkbox objects to convert.</param>
	/// <returns>A new list object containing the specified checkbox objects.</returns>
	public static implicit operator List(Checkbox[] items)
	{
		var result = new List((0, 0), 0);
		for(int i = 0; i < items?.Length; i++)
			result.items[i] = items[i];

		return result;
	}
	/// <summary>
	/// Implicitly converts a list object to an array of its checkbox item objects.
	/// </summary>
	/// <param name="list">The list object to convert.</param>
	/// <returns>An array of checkbox objects contained in the list object.</returns>
	public static implicit operator Checkbox[](List list) => list.items.ToArray();

	#region Backend
	private int singleSelectedIndex = -1;
	private readonly List<Checkbox> items = new();
	private bool isSingleSelecting, isInitialized;

	private void TrySingleSelectOneItem()
	{
		if(IsSingleSelecting == false || items.Count == 0)
		{
			singleSelectedIndex = -1;
			return;
		}

		var isOneSelected = HasIndex(singleSelectedIndex);

		if(isOneSelected)
			return;

		singleSelectedIndex = 0;
		items[singleSelectedIndex].IsChecked = true;
	}
	private void TrySingleSelect()
	{
		var isHoveringItems = IsHovered && Scroll.IsHovered == false &&
			ScrollUp.IsHovered == false && ScrollDown.IsHovered == false;

		if(Input.Current.IsJustReleased == false ||
			IsSingleSelecting == false || isHoveringItems == false)
			return;

		var hoveredIndex = (int)Input.Current.Position.Item2 - Position.Item2 + GetScrollIndex();

		if(hoveredIndex == singleSelectedIndex || HasIndex(hoveredIndex) == false)
			return;

		if(items[hoveredIndex].IsHeld)
			singleSelectedIndex = hoveredIndex;
	}

	private void UpdateParts()
	{
		var (x, y) = Position;
		var (w, h) = Size;

		Scroll.Position = (x + w - 1, y + 1);
		Scroll.Size = (1, h - 2);
		Scroll.IsVertical = true;

		ScrollUp.Position = (x + w - 1, y);
		ScrollUp.Size = (1, 1);

		ScrollDown.Position = (x + w - 1, y + h - 1);
		ScrollDown.Size = (1, 1);

		Scroll.Update();
		ScrollUp.Update();
		ScrollDown.Update();
	}
	private void UpdateItems()
	{
		var (x, y) = Position;
		var top = GetScrollIndex();
		var bottom = Math.Min(items.Count, top + Size.Item2);
		for(int i = 0; i < items.Count; i++)
		{
			var item = items[i];

			if(item == null)
				continue;

			if(i < top || i >= bottom)
			{
				item.Position = (int.MaxValue, int.MaxValue);
				continue;
			}

			item.Position = (x, y + (i - top));
			item.Size = (Size.width - 1, 1);
			item.Update();

			if(isInitialized)
				OnItemUpdate(item);

			if(IsSingleSelecting)
				item.IsChecked = false;
		}

		if(IsSingleSelecting && HasIndex(singleSelectedIndex))
			items[singleSelectedIndex].IsChecked = true;
	}

	private int GetScrollIndex()
	{
		var end = Math.Max(0, items.Count - Scroll.Size.Item2 - 2);
		var index = (int)MathF.Round(Map(Scroll.Progress, 0, 1, 0, end));
		return index;
	}
	private bool HasIndex(int index)
	{
		return index >= 0 && index < items.Count;
	}

	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	#endregion
}
