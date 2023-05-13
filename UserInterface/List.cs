namespace Pure.UserInterface;

/// <summary>
/// Represents a user input list element storing items, represented as buttons that can be
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
	/// Gets or sets a value indicating whether the list spans horizontally or vertically.
	/// </summary>
	public bool IsHorizontal { get; private set; }
	/// <summary>
	/// Gets or sets maximum size of each item.
	/// </summary>
	public (int width, int height) ItemMaximumSize
	{
		get => itemMaxSize;
		set => itemMaxSize = (Math.Max(value.width, 1), Math.Max(value.height, 1));
	}
	public (int width, int height) ItemGap
	{
		get => itemGap;
		set => itemGap = (Math.Max(value.width, 0), Math.Max(value.height, 0));
	}

	/// <summary>
	/// Gets the button item at the specified index, or null if the index is out of range.
	/// </summary>
	/// <param name="index">The index of the button to get.</param>
	public Button? this[int index] => HasIndex(index) ? items[index] : default;

	/// <summary>
	/// Initializes a new list instance with the specified position, size and number of items.
	/// </summary>
	/// <param name="position">The position of the top-left corner of the list.</param>
	/// <param name="size">The size of the list.</param>
	/// <param name="count">The initial number of buttons in the list.</param>
	public List((int x, int y) position, int count = 10, bool isHorizontal = false)
		: base(position)
	{
		Size = (12, 8);
		IsHorizontal = isHorizontal;
		var (x, y) = Position;
		var (w, h) = Size;

		Scroll = new(default, 0, true) { hasParent = true };
		ScrollUp = new(default) { hasParent = true };
		ScrollDown = new(default) { hasParent = true };

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
	/// Adds the specified number of items to the end of the list.
	/// </summary>
	/// <param name="count">The number of items to add.</param>
	public void Add(int count = 1) => Insert(items.Count, count);
	public void Insert(int index = 0, int count = 1)
	{
		if (index < 0 || index > items.Count)
			return;

		for (int i = index; i < count; i++)
		{
			var item = new Button(default) { Text = $"Item{i}" };
			item.size = (IsHorizontal ? item.Text.Length : Size.width, 1);
			item.hasParent = true;
			items.Insert(i, item);
		}
		TrySingleSelectOneItem();
	}
	/// <summary>
	/// Removes the item at the specified index and adjusts the selection accordingly.
	/// </summary>
	/// <param name="index">The index of the item to remove.</param>
	public void Remove(int index = 0)
	{
		if (HasIndex(index) == false)
			return;

		items.RemoveAt(index);

		if (index == singleSelectedIndex && index != items.Count)
			return;
		else if (index <= singleSelectedIndex)
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
	/// Determines whether the list contains the specified item.
	/// </summary>
	/// <param name="item">The item to locate in the list.</param>
	/// <returns>True if the item is found in the list; otherwise, false.</returns>
	public bool Contains(Button item)
	{
		return item != null && items.Contains(item);
	}
	/// <summary>
	/// Searches for the specified item and returns the zero-based index of the first occurrence
	/// within the entire list.
	/// </summary>
	/// <param name="item">The item to locate in the list.</param>
	/// <returns>The zero-based index of the first occurrence of the item within the entire list,
	/// if found; otherwise, -1.</returns>
	public int IndexOf(Button item)
	{
		return item == null ? -1 : items.IndexOf(item);
	}

	public void Select(int index, bool isSelected = true)
	{
		if (HasIndex(index) == false)
			return;

		singleSelectedIndex = index;

		var item = this[index];
		if (item != null)
			item.IsSelected = isSelected;
	}
	public void Select(Button item, bool isSelected = true) => Select(IndexOf(item), isSelected);

	/// <summary>
	/// Called when the list and its scroll need to be updated. This handles all of the user input
	/// the list and its scroll need for thier behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		ItemMaximumSize = itemMaxSize; // reclamp value

		if (IsDisabled)
			return;

		if (IsHovered && Input.Current.ScrollDelta != 0)
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
	protected virtual void OnItemUpdate(Button item) { }

	/// <summary>
	/// Implicitly converts an array of button objects to a list object.
	/// </summary>
	/// <param name="items">The array of button objects to convert.</param>
	/// <returns>A new list object containing the specified button objects.</returns>
	public static implicit operator List(Button[] items)
	{
		var result = new List((0, 0), 0);
		for (int i = 0; i < items?.Length; i++)
			result.items[i] = items[i];

		return result;
	}
	/// <summary>
	/// Implicitly converts a list object to an array of its button item objects.
	/// </summary>
	/// <param name="list">The list object to convert.</param>
	/// <returns>An array of button objects contained in the list object.</returns>
	public static implicit operator Button[](List list) => list.items.ToArray();

	#region Backend
	private int singleSelectedIndex = -1;
	private readonly List<Button> items = new();
	private bool isSingleSelecting;
	private readonly bool isInitialized;
	private (int width, int height) itemMaxSize = (5, 1), itemGap = (1, 1);

	internal void TrySingleSelectOneItem()
	{
		if (IsSingleSelecting == false || items.Count == 0)
		{
			singleSelectedIndex = -1;
			return;
		}

		var isOneSelected = HasIndex(singleSelectedIndex);

		if (isOneSelected)
			return;

		singleSelectedIndex = 0;
		items[singleSelectedIndex].IsSelected = true;
	}
	internal void TrySingleSelect()
	{
		var isHoveringItems = IsHovered && Scroll.IsHovered == false &&
			ScrollUp.IsHovered == false && ScrollDown.IsHovered == false;

		if (Input.Current.IsJustReleased == false ||
			IsSingleSelecting == false || isHoveringItems == false)
			return;

		var hoveredIndex = GetHoveredIndex();
		if (hoveredIndex == singleSelectedIndex || HasIndex(hoveredIndex) == false)
			return;

		if (items[hoveredIndex].IsHeld)
			singleSelectedIndex = hoveredIndex;
	}

	private void UpdateParts()
	{
		var (x, y) = Position;
		var (w, h) = Size;

		if (IsHorizontal)
		{
			if (HasScroll().horizontal == false)
			{
				Disable();
				return;
			}

			Scroll.position = (x + 1, y + h - 1);
			Scroll.size = (w - 2, 1);
			Scroll.IsVertical = false;

			ScrollUp.position = (x + w - 1, y + h - 1);
			ScrollUp.size = (1, 1);

			ScrollDown.position = (x, y + h - 1);
			ScrollDown.size = (1, 1);
		}
		else
		{
			if (HasScroll().vertical == false)
			{
				Disable();
				return;
			}

			Scroll.position = (x + w - 1, y + 1);
			Scroll.size = (1, h - 2);
			Scroll.IsVertical = true;

			ScrollUp.position = (x + w - 1, y);
			ScrollUp.size = (1, 1);

			ScrollDown.position = (x + w - 1, y + h - 1);
			ScrollDown.size = (1, 1);
		}

		Scroll.Update();
		ScrollUp.Update();
		ScrollDown.Update();

		void Disable()
		{
			Scroll.position = (int.MaxValue, int.MaxValue);
			Scroll.Handle.position = (int.MaxValue, int.MaxValue);
			ScrollUp.position = (int.MaxValue, int.MaxValue);
			ScrollDown.position = (int.MaxValue, int.MaxValue);
		}
	}
	private void UpdateItems()
	{
		var (x, y) = Position;
		var (w, h) = Size;

		for (int i = 0; i < items.Count; i++)
		{
			var item = items[i];

			if (item == null)
				continue;

			if (IsHorizontal)
			{
				var totalWidth = items.Count * (ItemMaximumSize.width + ItemGap.width);
				var offsetX = (int)Map(Scroll.Progress, 0, 1, 0, totalWidth - w - ItemGap.width);
				offsetX = Math.Max(offsetX, 0);
				item.position = (x - offsetX + i * (ItemMaximumSize.width + ItemGap.width), y);
			}
			else
			{
				var totalHeight = items.Count * (ItemMaximumSize.height + ItemGap.height);
				var offsetY = (int)Map(Scroll.Progress, 0, 1, 0, totalHeight - h - ItemGap.height);
				offsetY = Math.Max(offsetY, 0);
				item.position = (x, y - offsetY + i * (ItemMaximumSize.height + ItemGap.height));
			}

			var (ix, iy) = item.position;
			var (offW, offH) = item.listSizeTrimOffset;
			var botEdgeTrim = IsHorizontal && HasScroll().horizontal ? 1 : 0;
			var rightEdgeTrim = IsHorizontal == false && HasScroll().vertical ? 1 : 0;
			var (iw, ih) = (item.Size.width + offW, item.Size.height + offH);
			if (ix + iw <= x || ix >= x + w ||
				iy + ih <= y || iy >= y + h - botEdgeTrim)
				item.position = (int.MaxValue, int.MaxValue);

			TryTrimItem(item);
			item.Update();

			if (isInitialized)
				OnItemUpdate(item);

			if (IsSingleSelecting)
				item.IsSelected = false;
		}

		if (IsSingleSelecting && HasIndex(singleSelectedIndex))
			items[singleSelectedIndex].IsSelected = true;
	}

	private int GetScrollIndex()
	{
		var end = Math.Max(0, items.Count - Scroll.Size.height - 2);

		if (IsHorizontal)
			end = items.Count;

		return (int)MathF.Round(Map(Scroll.Progress, 0, 1, 0, end));
	}
	private bool HasIndex(int index)
	{
		return index >= 0 && index < items.Count;
	}
	private void TryTrimItem(Button item)
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var (ix, iy) = item.position;

		var (maxW, maxH) = ItemMaximumSize;
		var botEdgeTrim = IsHorizontal && HasScroll().horizontal ? 1 : 0;
		var rightEdgeTrim = IsHorizontal == false && HasScroll().vertical ? 1 : 0;
		var newWidth = ItemMaximumSize.width;
		var newHeight = ItemMaximumSize.height;
		var iw = newWidth;
		var ih = newHeight;

		item.listSizeTrimOffset = (0, 0);

		if (ix <= x + w && ix + iw >= x + w) // on right edge
			newWidth = Math.Min(ItemMaximumSize.width, x + w - rightEdgeTrim - ix);
		if (ix < x && ix + iw > x) // on left edge
		{
			newWidth = ix + iw - x;
			item.position = (x, item.position.Item2);
			item.listSizeTrimOffset = (ItemMaximumSize.width - newWidth, item.listSizeTrimOffset.Item2);
		}

		if (iy <= y + h && iy + ih >= y + h - botEdgeTrim) // on bottom edge
			newHeight = Math.Min(ItemMaximumSize.height, y + h - botEdgeTrim - iy);
		if (iy < y && iy + ih > y) // on top edge
		{
			newHeight = iy + ih - y;
			item.position = (item.position.Item1, y);
			item.listSizeTrimOffset = (item.listSizeTrimOffset.Item1, ItemMaximumSize.height - newHeight);
		}

		item.size = (newWidth, newHeight);
	}
	private int GetHoveredIndex()
	{
		var (x, y) = Input.Current.Position;
		for (int i = 0; i < items.Count; i++)
			if (items[i].IsHovered)
				return i;

		return -1;
	}
	private (bool horizontal, bool vertical) HasScroll()
	{
		var (maxW, maxH) = ItemMaximumSize;
		var totalW = items.Count * (maxW + ItemGap.width) - ItemGap.width;
		var totalH = items.Count * (maxH + ItemGap.height) - ItemGap.height;

		return (totalW > Size.width, totalH > Size.height);
	}

	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	#endregion
}
