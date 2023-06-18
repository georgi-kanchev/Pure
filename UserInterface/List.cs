namespace Pure.UserInterface;

/// <summary>
/// Represents a user input list element storing items, represented as buttons that can be
/// scrolled, clicked, selected and manipulated.
/// </summary>
public class List : Element
{
    public enum Types
    {
        Vertical,
        Horizontal,
        Dropdown
    }

    public Scroll Scroll { get; }

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
        set
        {
            isSingleSelecting = value || Type == Types.Dropdown;
            TrySingleSelectOneItem();
        }
    }
    public bool IsExpanded
    {
        get => isExpanded || Type != Types.Dropdown;
        set => isExpanded = Type == Types.Dropdown && value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the list spans horizontally, vertically or is a dropdown.
    /// </summary>
    public Types Type { get; private set; }
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

    public Button this[int index] => items[index];

    /// <summary>
    /// Initializes a new list instance with the specified position, size and number of items.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the list.</param>
    /// <param name="itemCount">The initial number of buttons in the list.</param>
    /// <param name="type">The type of the list.</param>
    public List((int x, int y) position, int itemCount = 10, Types type = Types.Vertical)
        : base(position)
    {
        Size = (12, 8);
        Type = type;

        var isVertical = type != Types.Horizontal;
        Scroll = new((0, 0), isVertical ? Size.height : Size.width, isVertical) { hasParent = true };

        Add(itemCount);

        if (type != Types.Dropdown)
            UpdateParts();
        UpdateItems();

        isInitialized = true;

        if (type == Types.Dropdown)
            IsSingleSelecting = true;
    }
    public List(byte[] bytes) : base(bytes)
    {
        Type = (Types)GrabByte(bytes);
        IsSingleSelecting = GrabBool(bytes);
        ItemGap = (GrabInt(bytes), GrabInt(bytes));
        ItemMaximumSize = (GrabInt(bytes), GrabInt(bytes));
        var scrollProgress = GrabFloat(bytes);
        Add(GrabInt(bytes));

        var isVertical = Type != Types.Horizontal;
        Scroll = new((0, 0), isVertical ? Size.height : Size.width, isVertical) { hasParent = true };
        if (Type != Types.Dropdown)
            UpdateParts();
        UpdateItems();

        for (var i = 0; i < Count; i++)
            Select(i, GrabBool(bytes));

        Scroll.Slider.Progress = scrollProgress;
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

        for (var i = index; i < count; i++)
        {
            var item = new Button((0, 0))
            {
                Text = $"Item{i}",
                size = (Type == Types.Horizontal ? Text.Length : Size.width, 1),
                hasParent = true
            };
            items.Insert(i, item);
            item.SubscribeToUserAction(UserAction.Trigger, () =>
            {
                OnItemTrigger(item);
                itemTriggerCallback?.Invoke(item);
            });
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
        return items.Contains(item);
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
        return items.IndexOf(item);
    }

    public void Select(int index, bool isSelected = true)
    {
        if (HasIndex(index) == false)
            return;

        singleSelectedIndex = isSelected ? index : singleSelectedIndex;
        this[index].isSelected = isSelected;
    }
    public void Select(Button item, bool isSelected = true) => Select(IndexOf(item), isSelected);

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutByte(result, (byte)Type);
        PutBool(result, IsSingleSelecting);
        PutInt(result, ItemGap.width);
        PutInt(result, ItemGap.height);
        PutInt(result, ItemMaximumSize.width);
        PutInt(result, ItemMaximumSize.height);
        PutFloat(result, Scroll.Slider.Progress);
        PutInt(result, Count);
        for (var i = 0; i < Count; i++)
            PutBool(result, this[i].IsSelected);

        return result.ToArray();
    }

    /// <summary>
    /// Called when the list and its scroll need to be updated. This handles all of the user input
    /// the list and its scroll need for their behavior. Subclasses should override this 
    /// method to implement their own behavior.
    /// </summary>
    protected override void OnUpdate()
    {
        ItemMaximumSize = itemMaxSize; // reclamp value

        if (Type == Types.Dropdown)
        {
            IsSingleSelecting = true;

            if (isExpanded == false)
            {
                Scroll.position = (int.MaxValue, int.MaxValue);
                Scroll.Slider.Handle.position = (int.MaxValue, int.MaxValue);
                Scroll.Down.position = (int.MaxValue, int.MaxValue);
                Scroll.Up.position = (int.MaxValue, int.MaxValue);
            }
        }

        if (IsDisabled)
            return;

        if (IsHovered) // for in between items, overwrite mouse cursor (don't give it to the element bellow)
            MouseCursorResult = MouseCursor.Arrow;

        TrySingleSelect();

        if (Type == Types.Dropdown && isInitialized && isExpanded == false)
        {
            var selectedItem = this[singleSelectedIndex];
            selectedItem.position = Position;
            TryTrimItem(selectedItem);
            // after trim
            selectedItem.size = (selectedItem.Size.width + 1, selectedItem.Size.height);

            selectedItem.Update();
            OnItemUpdate(selectedItem);
            itemUpdateCallback?.Invoke(selectedItem);

            Select(selectedItem);
            return;
        }

        UpdateParts();
        UpdateItems();

        // in case the user hovers in between items - try scroll
        if (IsHovered && Input.Current.ScrollDelta != 0 &&
            (isExpanded || Type != Types.Dropdown) && IsFocused && FocusedPrevious == this)
            Scroll.Slider.Move(Input.Current.ScrollDelta);
        // and in case the user hovers the items themselves - also try scroll
        foreach (var item in items)
            TryScrollWhileHoverButton(item);
    }

    /// <summary>
    /// Called when the items of the list need to be updated. This handles all of the user input
    /// for the items in the list need for their behavior. Subclasses should override this 
    /// method to implement their own behavior.
    /// </summary>
    protected virtual void OnItemUpdate(Button item)
    {
    }
    protected virtual void OnItemTrigger(Button item)
    {
    }

    /// <summary>
    /// Implicitly converts an array of button objects to a list object.
    /// </summary>
    /// <param name="items">The array of button objects to convert.</param>
    /// <returns>A new list object containing the specified button objects.</returns>
    public static implicit operator List(Button[] items)
    {
        var result = new List((0, 0), 0);
        for (var i = 0; i < items.Length; i++)
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
    private bool isSingleSelecting, isExpanded;
    private readonly bool isInitialized;
    private (int width, int height) itemMaxSize = (5, 1), itemGap = (1, 0);

    // used in the UI class to receive callbacks
    internal Action<Button>? itemUpdateCallback;
    internal Action<Button>? itemTriggerCallback;

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
        Select(items[singleSelectedIndex], true);
    }
    internal void TrySingleSelect()
    {
        var isHoveringItems = IsHovered && Scroll.IsHovered == false;

        if (Input.Current.IsJustPressed && IsHovered == false)
        {
            isExpanded = false;
            return;
        }

        if (Input.Current.IsJustReleased == false ||
            IsSingleSelecting == false || isHoveringItems == false)
            return;

        var hoveredIndex = GetHoveredIndex();
        if (HasIndex(hoveredIndex) == false)
            return;

        if (items[hoveredIndex].IsPressedAndHeld == false)
            return;

        singleSelectedIndex = hoveredIndex;
        items[hoveredIndex].Trigger();

        if (Type != Types.Dropdown)
            return;

        isExpanded = isExpanded == false;
    }

    private void UpdateParts()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        Scroll.Update();

        if (Type == Types.Horizontal)
        {
            if (HasScroll().horizontal == false)
            {
                Scroll.position = (int.MaxValue, int.MaxValue);
                return;
            }

            Scroll.position = (x, y + h - 1);
            Scroll.size = (w, 1);
        }
        else
        {
            if (HasScroll().vertical == false)
            {
                Scroll.position = (int.MaxValue, int.MaxValue);
                return;
            }

            Scroll.position = (x + w - 1, y);
            Scroll.size = (1, h);
        }
    }
    private void UpdateItems()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (Type == Types.Horizontal)
            {
                var totalWidth = items.Count * (ItemMaximumSize.width + ItemGap.width);
                var offsetX = (int)Map(Scroll.Slider.Progress, 0, 1, 0, totalWidth - w - ItemGap.width);
                offsetX = Math.Max(offsetX, 0);
                item.position = (x - offsetX + i * (ItemMaximumSize.width + ItemGap.width), y);
            }
            else
            {
                var totalHeight = items.Count * (ItemMaximumSize.height + ItemGap.height);
                var offsetY = (int)Map(Scroll.Slider.Progress, 0, 1, 0,
                    totalHeight - h - ItemGap.height);
                offsetY = Math.Max(offsetY, 0);
                item.position = (x, y - offsetY + i * (ItemMaximumSize.height + ItemGap.height));
            }

            var (ix, iy) = item.position;
            var (offW, offH) = item.listSizeTrimOffset;
            var botEdgeTrim = Type == Types.Horizontal && HasScroll().horizontal ? 1 : 0;
            //var rightEdgeTrim = Type == Types.Horizontal == false && HasScroll().vertical ? 1 : 0;
            var (iw, ih) = (item.Size.width + offW, item.Size.height + offH);
            if (ix + iw <= x || ix >= x + w ||
                iy + ih <= y || iy >= y + h - botEdgeTrim)
                item.position = (int.MaxValue, int.MaxValue);

            TryTrimItem(item);
            item.Update();

            if (isInitialized)
            {
                OnItemUpdate(item);
                itemUpdateCallback?.Invoke(item);
            }

            if (IsSingleSelecting)
                Select(item, false);
        }

        if (IsSingleSelecting && HasIndex(singleSelectedIndex))
            Select(items[singleSelectedIndex]);
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

        var botEdgeTrim = Type == Types.Horizontal && HasScroll().horizontal ? 1 : 0;
        var rightEdgeTrim = Type == Types.Horizontal == false && HasScroll().vertical ? 1 : 0;
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
            item.listSizeTrimOffset =
                (item.listSizeTrimOffset.Item1, ItemMaximumSize.height - newHeight);
        }

        item.size = (newWidth, newHeight);
    }
    private int GetHoveredIndex()
    {
        for (var i = 0; i < items.Count; i++)
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
    private void TryScrollWhileHoverButton(Element btn)
    {
        if (btn.IsHovered && Input.Current.ScrollDelta != 0 && btn.IsFocused &&
            FocusedPrevious == btn)
            Scroll.Slider.Move(Input.Current.ScrollDelta);
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
    #endregion
}