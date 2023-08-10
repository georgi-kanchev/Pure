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
        set
        {
            if (Type != Types.Dropdown)
                return;

            if (value)
                Size = (Size.width, originalHeight);
            else if (isExpanded)
                originalHeight = Size.height;

            isExpanded = Type == Types.Dropdown && value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the list spans horizontally, vertically or is a dropdown.
    /// </summary>
    public Types Type { get; }

    public (int width, int height) ItemSize
    {
        get => itemSize;
        set
        {
            if (hasParent == false)
                itemSize = (Math.Max(value.width, 1), Math.Max(value.height, 1));
        }
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
        isParent = true;
        originalHeight = Size.height;
        Type = type;

        var isVertical = type != Types.Horizontal;
        Scroll = new((0, 0), isVertical ? Size.height : Size.width, isVertical) { hasParent = true };

        Add(itemCount);

        isInitialized = true;

        if (type == Types.Dropdown)
            IsSingleSelecting = true;
    }
    public List(byte[] bytes) : base(bytes)
    {
        Type = (Types)GrabByte(bytes);
        isParent = true;
        IsSingleSelecting = GrabBool(bytes);
        ItemGap = (GrabInt(bytes), GrabInt(bytes));
        ItemSize = (GrabInt(bytes), GrabInt(bytes));
        var scrollProgress = GrabFloat(bytes);
        Add(GrabInt(bytes));

        var isVertical = Type != Types.Horizontal;
        Scroll = new((0, 0), isVertical ? Size.height : Size.width, isVertical) { hasParent = true };

        for (var i = 0; i < Count; i++)
            Select(i, GrabBool(bytes));

        Scroll.Slider.Progress = scrollProgress;
        isInitialized = true;

        originalHeight = Size.height;
    }

    /// <summary>
    /// Adds the specified number of items to the end of the list.
    /// </summary>
    /// <param name="count">The number of items to add.</param>
    public void Add(int count = 1) => Insert(items.Count, count);
    public void Insert(int index = 0, int count = 1)
    {
        if (hasParent == false)
            InternalInsert(index, count);
    }
    /// <summary>
    /// Removes the item at the specified index and adjusts the selection accordingly.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    public void Remove(int index = 0)
    {
        if (hasParent == false)
            InternalRemove(index);
    }
    /// <summary>
    /// Clears all items from the list and resets the selection.
    /// </summary>
    public void Clear()
    {
        if (hasParent == false)
            InternalClear();
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

        if (IsSingleSelecting)
        {
            var wasSelected = singleSelectedIndex != -1 && this[index].isSelected;

            foreach (var item in items)
                item.isSelected = false;

            this[index].isSelected = isSelected;

            if (singleSelectedIndex == index || wasSelected || isSelected == false)
                return;

            singleSelectedIndex = isSelected ? index : singleSelectedIndex;
            OnItemSelect(this[index]);
            itemSelectCallback?.Invoke(this[index]);

            return;
        }

        var wasSel = this[index].isSelected;
        this[index].isSelected = isSelected;

        if (wasSel || isSelected == false)
            return;

        OnItemSelect(this[index]);
        itemSelectCallback?.Invoke(this[index]);
    }
    public void Select(Button item, bool isSelected = true) => Select(IndexOf(item), isSelected);

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutByte(result, (byte)Type);
        PutBool(result, IsSingleSelecting);
        PutInt(result, ItemGap.width);
        PutInt(result, ItemGap.height);
        PutInt(result, ItemSize.width);
        PutInt(result, ItemSize.height);
        PutFloat(result, Scroll.Slider.Progress);
        PutInt(result, Count);
        for (var i = 0; i < Count; i++)
            PutBool(result, this[i].IsSelected);

        return result.ToArray();
    }

    protected virtual void OnItemDisplay(Button item) { }
    protected virtual void OnItemTrigger(Button item) { }
    protected virtual void OnItemSelect(Button item) { }

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
    private int singleSelectedIndex = -1, originalHeight;
    private readonly List<Button> items = new();
    private bool isSingleSelecting, isExpanded;
    private readonly bool isInitialized;
    private (int width, int height) itemGap = (1, 0);
    internal (int width, int height) itemSize = (5, 1);

    // used in the UI class to receive callbacks
    internal Action<Button>? itemDisplayCallback;
    internal Action<Button>? itemTriggerCallback;
    internal Action<Button>? itemSelectCallback;

    protected override void OnUserAction(UserAction userAction)
    {
        if (userAction != UserAction.Trigger || Type != Types.Dropdown || IsExpanded ||
            IsFocused == false || this[singleSelectedIndex].IsHovered)
            return;

        IsExpanded = true;
    }

    internal void InternalAdd(int count = 1) => InternalInsert(items.Count, count);
    internal void InternalInsert(int index = 0, int count = 1)
    {
        if (index < 0 || index > items.Count)
            return;

        for (var i = index; i < index + count; i++)
        {
            var item = new Button((0, 0))
            {
                Text = $"Item{i}",
                size = (Type == Types.Horizontal ? Text.Length : Size.width, 1),
                hasParent = true
            };
            items.Insert(i, item);
            item.SubscribeToUserAction(UserAction.Trigger, () => OnInternalItemTrigger(item));
            item.SubscribeToUserAction(UserAction.Select, () => OnInternalItemSelect(item));
        }

        TrySingleSelectOneItem();
    }
    internal void InternalRemove(int index = 0)
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
    internal void InternalClear()
    {
        items.Clear();
        singleSelectedIndex = -1;
        TrySingleSelectOneItem();
    }

    internal override void OnInput()
    {
        if (Type == Types.Dropdown && IsExpanded == false && IsHovered)
            MouseCursorResult = MouseCursor.Hand;

        // in case the user hovers in between items - try scroll
        if (IsHovered && Input.Current.ScrollDelta != 0 &&
            (isExpanded || Type != Types.Dropdown) && IsFocused && FocusedPrevious == this)
            Scroll.Slider.Move(Input.Current.ScrollDelta);
        // and in case the user hovers the items themselves - also try scroll
        foreach (var item in items)
            TryScrollWhileHover(item);
    }
    internal override void OnUpdate()
    {
        if (IsSingleSelecting && singleSelectedIndex == -1)
            TrySingleSelectOneItem();

        if (Input.Current.IsJustPressed && IsHovered == false)
            IsExpanded = false;

        // for in between items, overwrite mouse cursor (don't give it to the element bellow)
        if (IsDisabled == false && IsHovered)
            MouseCursorResult = MouseCursor.Arrow;

        if (Type != Types.Dropdown)
            LimitSizeMin((2, 2));

        ItemSize = itemSize; // reclamp value
        var totalWidth = items.Count * (ItemSize.width + ItemGap.width);
        var totalHeight = items.Count * (ItemSize.height + ItemGap.height);
        var totalSize = Type == Types.Horizontal ? totalWidth : totalHeight;
        Scroll.step = 1f / totalSize;

        if (Type == Types.Dropdown && isExpanded == false)
            Size = (Size.width, 1);
    }

    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        Scroll.position = (x + w - 1, y);
        Scroll.size = (1, h);

        if (HasScroll().vertical == false)
            Scroll.position = (int.MaxValue, int.MaxValue);

        if (Type == Types.Dropdown && isExpanded == false)
        {
            Scroll.position = (int.MaxValue, int.MaxValue);
            Scroll.Slider.Handle.position = (int.MaxValue, int.MaxValue);
            Scroll.Decrease.position = (int.MaxValue, int.MaxValue);
            Scroll.Increase.position = (int.MaxValue, int.MaxValue);

            Scroll.InheritParent(this);
            Scroll.Update();

            if (isInitialized == false || singleSelectedIndex == -1)
                return;

            var selectedItem = this[singleSelectedIndex];
            selectedItem.position = Position;
            TryTrimItem(selectedItem);

            selectedItem.InheritParent(this);
            selectedItem.Update();

            return;
        }
        else if (Type == Types.Horizontal)
        {
            Scroll.position = (x, y + h - 1);
            Scroll.size = (w, 1);

            if (HasScroll().horizontal == false)
                Scroll.position = (int.MaxValue, int.MaxValue);
        }

        Scroll.InheritParent(this);
        Scroll.Update();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (Type == Types.Horizontal)
            {
                var totalWidth = items.Count * (ItemSize.width + ItemGap.width);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalWidth - w - ItemGap.width);
                var offsetX = (int)MathF.Round(p);
                offsetX = Math.Max(offsetX, 0);
                item.position = (x - offsetX + i * (ItemSize.width + ItemGap.width), y);
            }
            else
            {
                var totalHeight = items.Count * (ItemSize.height + ItemGap.height);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalHeight - h - ItemGap.height);
                var offsetY = (int)MathF.Round(p);
                offsetY = Math.Max(offsetY, 0);
                item.position = (x, y - offsetY + i * (ItemSize.height + ItemGap.height));
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
            item.InheritParent(this);
            item.Update();
        }
    }
    internal override void OnChildrenDisplay()
    {
        if (isInitialized == false || items.Count == 0)
            return;

        if (Type == Types.Dropdown && isInitialized && isExpanded == false && singleSelectedIndex != -1)
        {
            var selectedItem = this[singleSelectedIndex];

            OnItemDisplay(selectedItem);
            itemDisplayCallback?.Invoke(selectedItem);
            return;
        }

        foreach (var item in items)
            if (IsOverlapping(item))
            {
                OnItemDisplay(item);
                itemDisplayCallback?.Invoke(item);
            }
    }

    internal void TrySingleSelectOneItem()
    {
        if (IsSingleSelecting == false || items.Count == 0)
        {
            singleSelectedIndex = -1;
            return;
        }

        Select(items[0]);
    }

    private void OnInternalItemTrigger(Button item)
    {
        OnItemTrigger(item);
        itemTriggerCallback?.Invoke(item);

        if (Type != Types.Dropdown)
            return;

        IsExpanded = isExpanded == false;
        Select(item);
    }
    private void OnInternalItemSelect(Button item)
    {
        OnItemSelect(item);
        itemSelectCallback?.Invoke(item);

        TrySingleSelectOneItem();

        if (IsSingleSelecting)
            Select(item);
    }

    private bool HasIndex(int index)
    {
        return index >= 0 && index < items.Count;
    }
    private void TryTrimItem(Element item)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (ix, iy) = item.position;

        var botEdgeTrim = Type == Types.Horizontal && HasScroll().horizontal ? 1 : 0;
        var rightEdgeTrim = Type == Types.Horizontal == false && HasScroll().vertical ? 1 : 0;
        var newWidth = ItemSize.width;
        var newHeight = ItemSize.height;
        var iw = newWidth;
        var ih = newHeight;

        item.listSizeTrimOffset = (0, 0);

        if (ix <= x + w && ix + iw >= x + w) // on right edge
            newWidth = Math.Min(ItemSize.width, x + w - rightEdgeTrim - ix);
        if (ix < x && ix + iw > x) // on left edge
        {
            newWidth = ix + iw - x;
            item.position = (x, item.position.Item2);
            item.listSizeTrimOffset = (ItemSize.width - newWidth, item.listSizeTrimOffset.Item2);
        }

        if (iy <= y + h && iy + ih >= y + h - botEdgeTrim) // on bottom edge
            newHeight = Math.Min(ItemSize.height, y + h - botEdgeTrim - iy);
        if (iy < y && iy + ih > y) // on top edge
        {
            newHeight = iy + ih - y;
            item.position = (item.position.Item1, y);
            item.listSizeTrimOffset =
                (item.listSizeTrimOffset.Item1, ItemSize.height - newHeight);
        }

        item.size = (newWidth, newHeight);
    }
    private (bool horizontal, bool vertical) HasScroll()
    {
        var (maxW, maxH) = ItemSize;
        var totalW = items.Count * (maxW + ItemGap.width) - ItemGap.width;
        var totalH = items.Count * (maxH + ItemGap.height) - ItemGap.height;

        return (totalW > Size.width, totalH > Size.height);
    }
    private void TryScrollWhileHover(Element element)
    {
        if (element.IsHovered && Input.Current.ScrollDelta != 0 && element.IsFocused &&
            FocusedPrevious == element)
            Scroll.Slider.Progress -= Input.Current.ScrollDelta * Scroll.Step;
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}