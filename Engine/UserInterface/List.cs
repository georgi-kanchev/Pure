namespace Pure.Engine.UserInterface;

/// <summary>
/// Stores items, represented as buttons that can be
/// scrolled, clicked, selected and manipulated.
/// </summary>
public class List : Block
{
    public enum Spans
    {
        Vertical,
        Horizontal,
        Dropdown
    }

    public Scroll Scroll
    {
        get;
    }

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    public int Count
    {
        get => items.Count;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the list allows single selection or not.
    /// </summary>
    public bool IsSingleSelecting
    {
        get => isSingleSelecting || Span == Spans.Dropdown;
        set
        {
            var was = isSingleSelecting;
            isSingleSelecting = value || Span == Spans.Dropdown;

            if (was != isSingleSelecting)
                TrySingleSelectOneItem();
        }
    }
    public bool IsCollapsed
    {
        get => isCollapsed && Span == Spans.Dropdown;
        set
        {
            if (Span != Spans.Dropdown)
                return;

            if (value == false)
                Size = (Size.width, originalHeight);
            else if (isCollapsed == false)
                originalHeight = Size.height;

            isCollapsed = value;
        }
    }
    public bool IsReadOnly
    {
        get => isReadOnly;
        set
        {
            if (hasParent)
                return;

            isReadOnly = value;

            foreach (var item in items)
                item.isTextReadonly = value;
        }
    }

    public int[] IndexesSelected
    {
        get => indexesSelected.ToArray();
    }
    public int[] IndexesDisabled
    {
        get => indexesDisabled.ToArray();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the list spans horizontally, vertically or is a dropdown.
    /// </summary>
    public Spans Span
    {
        get;
    }

    public (int width, int height) ItemSize
    {
        get => itemSize;
        set
        {
            if (hasParent == false)
                itemSize = (Math.Max(value.width, 1), Math.Max(value.height, 1));
        }
    }
    public int ItemGap
    {
        get => itemGap;
        set => itemGap = Math.Max(value, 0);
    }

    public Button this[int index]
    {
        get => items[index];
    }

    /// <summary>
    /// Initializes a new list instance with the specified position, size and number of items.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the list.</param>
    /// <param name="itemCount">The initial number of buttons in the list.</param>
    /// <param name="span">The type of the list.</param>
    public List((int x, int y) position = default, int itemCount = 10, Spans span = Spans.Vertical)
        : base(position)
    {
        Init();
        Size = (12, 8);
        ItemGap = span == Spans.Horizontal ? 1 : 0;
        originalHeight = Size.height;
        Span = span;

        Scroll = new((int.MaxValue, int.MaxValue)) { hasParent = true };

        Add(itemCount);

        isInitialized = true;

        if (span != Spans.Dropdown)
            return;

        IsSingleSelecting = true;
        IsCollapsed = true;
    }
    public List(byte[] bytes) : base(bytes)
    {
        Init();
        Span = (Spans)GrabByte(bytes);
        IsSingleSelecting = GrabBool(bytes);
        ItemGap = GrabInt(bytes);
        ItemSize = (GrabInt(bytes), GrabInt(bytes));
        var scrollProgress = GrabFloat(bytes);
        Add(GrabInt(bytes));

        Scroll = new((int.MaxValue, int.MaxValue)) { hasParent = true };

        for (var i = 0; i < Count; i++)
            Select(i, GrabBool(bytes));

        Scroll.Slider.Progress = scrollProgress;
        isInitialized = true;

        originalHeight = Size.height;

        if (Span == Spans.Dropdown)
            IsCollapsed = true;
    }

    /// <summary>
    /// Adds the specified number of items to the end of the list.
    /// </summary>
    /// <param name="count">The number of items to add.</param>
    public void Add(int count = 1)
    {
        Insert(items.Count, count);
    }
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

        if (IsSingleSelecting && singleSelectedIndex != index && isSelected)
        {
            foreach (var item in items)
                item.isSelected = false;

            this[index].isSelected = isSelected;
            singleSelectedIndex = isSelected ? index : singleSelectedIndex;

            indexesSelected.Clear();
            if (isSelected)
                indexesSelected.Add(index);

            return;
        }

        var wasSel = this[index].isSelected;
        this[index].isSelected = isSelected;

        if (isSelected && wasSel == false && indexesSelected.Contains(index) == false)
            indexesSelected.Add(index);
        else if (isSelected == false && wasSel && indexesSelected.Contains(index))
            indexesSelected.Remove(index);
    }
    public void Select(Button item, bool isSelected = true)
    {
        Select(IndexOf(item), isSelected);
    }
    public void Disable(int index, bool isDisabled = true)
    {
        if (HasIndex(index) == false)
            return;

        var hasIndex = indexesDisabled.Contains(index);
        this[index].isDisabled = isDisabled;

        if (isDisabled && hasIndex == false)
            indexesDisabled.Add(index);
        else if (isDisabled == false && hasIndex)
            indexesDisabled.Remove(index);
    }
    public void Disable(Button item, bool isDisabled = true)
    {
        Disable(IndexOf(item), isDisabled);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutByte(result, (byte)Span);
        PutBool(result, IsSingleSelecting);
        PutInt(result, ItemGap);
        PutInt(result, ItemSize.width);
        PutInt(result, ItemSize.height);
        PutFloat(result, Scroll.Slider.Progress);
        PutInt(result, Count);
        for (var i = 0; i < Count; i++)
            PutBool(result, this[i].IsSelected);

        return result.ToArray();
    }

    protected override void OnInput()
    {
        if (Span == Spans.Dropdown && IsCollapsed && IsHovered)
            Input.MouseCursorResult = MouseCursor.Hand;
    }

    public void OnItemDisplay(Action<Button> method)
    {
        itemDisplays += method;
    }
    public void OnItemInteraction(Interaction interaction, Action<Button> method)
    {
        if (itemInteractions.TryAdd(interaction, method) == false)
            itemInteractions[interaction] += method;

        foreach (var item in items)
            item.OnInteraction(interaction, () => method.Invoke(item));
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
    public static implicit operator Button[](List list)
    {
        return list.items.ToArray();
    }

#region Backend
    private int singleSelectedIndex = -1, originalHeight;
    private readonly List<Button> items = new();
    private bool isSingleSelecting, isCollapsed;
    private readonly bool isInitialized;
    private int itemGap;
    internal (int width, int height) itemSize = (5, 1);
    private readonly List<int> indexesDisabled = new(), indexesSelected = new();
    internal bool isReadOnly;

    private readonly Dictionary<Interaction, Action<Button>> itemInteractions = new();
    private Action<Button>? itemDisplays;

    private (bool horizontal, bool vertical) HasScroll
    {
        get
        {
            var (maxW, maxH) = ItemSize;
            var totalW = items.Count * (maxW + ItemGap) - ItemGap;
            var totalH = items.Count * (maxH + ItemGap) - ItemGap;

            return (totalW > Size.width, totalH > Size.height);
        }
    }
    internal bool IsScrollVisible
    {
        get => (Span == Spans.Horizontal ? HasScroll.horizontal : HasScroll.vertical) &&
               IsCollapsed == false;
    }

    private void Init()
    {
        OnInteraction(Interaction.Trigger, () =>
        {
            if (IsCollapsed && IsFocused && this[singleSelectedIndex].IsHovered == false)
                IsCollapsed = false;
        });
    }

    internal void InternalAdd(int count = 1)
    {
        InternalInsert(items.Count, count);
    }
    internal void InternalInsert(int index = 0, int count = 1)
    {
        if (index < 0 || index > items.Count)
            return;

        for (var i = index; i < index + count; i++)
        {
            var item = new Button((0, 0))
            {
                text = $"Item{i}",
                size = (Span == Spans.Horizontal ? text.Length : Size.width, 1),
                hasParent = true
            };
            items.Insert(i, item);
            item.OnInteraction(Interaction.Trigger, () => OnInternalItemTrigger(item));
            item.OnInteraction(Interaction.Select, TrySingleSelectOneItem);
            item.OnInteraction(Interaction.Scroll, ApplyScroll);

            foreach (var kvp in itemInteractions)
                item.OnInteraction(kvp.Key, () => kvp.Value.Invoke(item));
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
        TrySingleSelectOneItem();
    }

    internal override void OnUpdate()
    {
        if (IsSingleSelecting && singleSelectedIndex == -1)
            TrySingleSelectOneItem();

        if (Input.IsJustPressed && IsHovered == false)
            IsCollapsed = true;

        // for in between items, overwrite mouse cursor (don't give it to the block bellow)
        if (IsDisabled == false && IsHovered)
            Input.MouseCursorResult = MouseCursor.Arrow;

        if (Span != Spans.Dropdown)
            LimitSizeMin((2, 2));

        ItemSize = itemSize; // reclamp value
        var totalWidth = items.Count * (ItemSize.width + ItemGap);
        var totalHeight = items.Count * (ItemSize.height + ItemGap);
        var totalSize = Span == Spans.Horizontal ? totalWidth : totalHeight;
        Scroll.step = 1f / totalSize;
        Scroll.isVertical = Span != Spans.Horizontal;

        if (IsCollapsed)
            Size = (Size.width, 1);
    }
    internal override void ApplyScroll()
    {
        if (Scroll.IsHovered == false)
            Scroll.Slider.Progress -= Input.ScrollDelta * Scroll.Step;
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        var hidesScr = IsScrollVisible == false;
        Scroll.Increase.isHidden = hidesScr;
        Scroll.Increase.isDisabled = hidesScr;
        Scroll.Decrease.isHidden = hidesScr;
        Scroll.Decrease.isDisabled = hidesScr;
        Scroll.Slider.isHidden = hidesScr;
        Scroll.Slider.isDisabled = hidesScr;
        Scroll.isHidden = hidesScr;
        Scroll.isDisabled = hidesScr;
        Scroll.InheritParent(this);

        Scroll.position = (x + w - 1, y);
        Scroll.size = (1, h);

        if (IsCollapsed && isInitialized && singleSelectedIndex >= 0)
        {
            var selectedItem = this[singleSelectedIndex];
            selectedItem.position = Position;
            selectedItem.isHidden = false;
            selectedItem.isDisabled = false;

            TryTrimItem(selectedItem);

            selectedItem.InheritParent(this);
            selectedItem.Update();

            Scroll.Update();
            return;
        }
        else if (Span == Spans.Horizontal)
        {
            Scroll.position = (x, y + h - 1);
            Scroll.size = (w, 1);
        }

        Scroll.Update();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            item.isHidden = false;
            item.isDisabled = indexesDisabled.Contains(i);
            item.InheritParent(this);

            if (Span == Spans.Horizontal)
            {
                var totalWidth = items.Count * (ItemSize.width + ItemGap);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalWidth - w - ItemGap);
                var offsetX = (int)MathF.Round(p);
                offsetX = Math.Max(offsetX, 0);
                item.position = (x - offsetX + i * (ItemSize.width + ItemGap), y);
            }
            else
            {
                var totalHeight = items.Count * (ItemSize.height + ItemGap);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalHeight - h - ItemGap);
                var offsetY = (int)MathF.Round(p);
                offsetY = Math.Max(offsetY, 0);
                item.position = (x, y - offsetY + i * (ItemSize.height + ItemGap));
            }

            var (ix, iy) = item.position;
            var (offW, offH) = item.listSizeTrimOffset;
            var botEdgeTrim = Span == Spans.Horizontal && HasScroll.horizontal ? 1 : 0;
            //var rightEdgeTrim = Type == Types.Horizontal == false && HasScroll().vertical ? 1 : 0;
            var (iw, ih) = (item.Size.width + offW, item.Size.height + offH);
            if (ix + iw <= x || ix >= x + w ||
                iy + ih <= y || iy >= y + h - botEdgeTrim)
            {
                item.isHidden = true;
                item.isDisabled = true;
            }

            TryTrimItem(item);
            item.Update();
        }
    }
    internal override void OnChildrenDisplay()
    {
        if (isInitialized == false || items.Count == 0)
            return;

        if (IsCollapsed && isInitialized && singleSelectedIndex >= 0)
        {
            var selectedItem = this[singleSelectedIndex];

            if (selectedItem.IsHidden)
                return;

            itemDisplays?.Invoke(selectedItem);
            return;
        }

        foreach (var item in items)
            if (IsOverlapping(item))
            {
                if (item.IsHidden)
                    continue;

                itemDisplays?.Invoke(item);
            }
    }

    internal void TrySingleSelectOneItem()
    {
        if (IsSingleSelecting == false || items.Count == 0)
        {
            singleSelectedIndex = -1;
            return;
        }

        if (indexesSelected.Count == 1)
            return;

        Select(0);
    }

    private void OnInternalItemTrigger(Button item)
    {
        if (IsSingleSelecting)
            Select(item);
        else
        {
            var index = IndexOf(item);
            if (item.IsSelected)
                indexesSelected.Add(index);
            else
                indexesSelected.Remove(index);
        }

        if (Span != Spans.Dropdown)
            return;

        IsCollapsed = IsCollapsed == false;
        Select(item);
    }

    private bool HasIndex(int index)
    {
        return index >= 0 && index < items.Count;
    }
    private void TryTrimItem(Block item)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (ix, iy) = item.position;

        var botEdgeTrim = Span == Spans.Horizontal && HasScroll.horizontal ? 1 : 0;
        var rightEdgeTrim = Span == Spans.Horizontal == false && HasScroll.vertical ? 1 : 0;
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

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}