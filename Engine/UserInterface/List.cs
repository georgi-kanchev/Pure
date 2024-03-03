namespace Pure.Engine.UserInterface;

public enum Span
{
    Vertical,
    Horizontal,
    Dropdown
}

/// <summary>
/// Stores items, represented as buttons that can be
/// scrolled, clicked, selected and manipulated.
/// </summary>
public class List : Block
{
    public Scroll Scroll { get; }

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    public int Count
    {
        get => data.Count;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the list allows single selection or not.
    /// </summary>
    public bool IsSingleSelecting
    {
        get => isSingleSelecting || Span == Span.Dropdown;
        set => isSingleSelecting = value || Span == Span.Dropdown;
    }
    public bool IsCollapsed
    {
        get => isCollapsed && Span == Span.Dropdown;
        set
        {
            if (Span != Span.Dropdown)
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

            foreach (var item in data)
                item.isTextReadonly = value;
        }
    }

    public Button[] ItemsSelected
    {
        get
        {
            var result = new Button[indexesSelected.Count];
            for (var i = 0; i < indexesSelected.Count; i++)
                result[i] = this[indexesSelected[i]];
            return result;
        }
    }
    public Button[] ItemsDisabled
    {
        get
        {
            var result = new Button[indexesDisabled.Count];
            for (var i = 0; i < indexesDisabled.Count; i++)
                result[i] = this[indexesDisabled[i]];
            return result;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the list spans horizontally, vertically or is a dropdown.
    /// </summary>
    public Span Span { get; }

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
        get => data[index];
    }

    /// <summary>
    /// Initializes a new list instance with the specified position, size and number of items.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the list.</param>
    /// <param name="itemCount">The initial number of buttons in the list.</param>
    /// <param name="span">The type of the list.</param>
    public List((int x, int y) position = default, int itemCount = 10, Span span = Span.Vertical)
        : base(position)
    {
        Init();
        Size = (12, 8);
        ItemGap = span == Span.Horizontal ? 1 : 0;
        originalHeight = Size.height;
        Span = span;

        Scroll = new((int.MaxValue, int.MaxValue)) { hasParent = true };

        var items = InternalCreateAmount(itemCount);
        Add(items);

        isInitialized = true;

        if (span != Span.Dropdown)
            return;

        IsSingleSelecting = true;
        IsCollapsed = true;
    }
    public List(byte[] bytes) : base(bytes)
    {
        Init();
        var b = Decompress(bytes);
        Span = (Span)GrabByte(b);
        IsSingleSelecting = GrabBool(b);
        IsReadOnly = GrabBool(b);
        ItemGap = GrabInt(b);
        ItemSize = (GrabInt(b), GrabInt(b));
        var scrollProgress = GrabFloat(b);
        var scrollIndex = GrabInt(b);

        var items = InternalCreateAmount(GrabInt(b));
        InternalAdd(items);

        Scroll = new((int.MaxValue, int.MaxValue)) { hasParent = true };

        for (var i = 0; i < Count; i++)
        {
            Select(i, GrabBool(b));
            Disable(i, GrabBool(b));
            this[i].text = GrabString(b);
        }

        Scroll.Slider.progress = scrollProgress;
        Scroll.Slider.index = scrollIndex;
        isInitialized = true;

        originalHeight = Size.height;

        if (Span == Span.Dropdown)
            IsCollapsed = true;
    }
    public List(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        PutByte(result, (byte)Span);
        PutBool(result, IsSingleSelecting);
        PutBool(result, IsReadOnly);
        PutInt(result, ItemGap);
        PutInt(result, ItemSize.width);
        PutInt(result, ItemSize.height);
        PutFloat(result, Scroll.Slider.Progress);
        PutFloat(result, Scroll.Slider.index);
        PutInt(result, Count);

        for (var i = 0; i < Count; i++)
        {
            PutBool(result, this[i].IsSelected);
            PutBool(result, this[i].IsDisabled);
            PutString(result, this[i].Text);
        }

        return Compress(result.ToArray());
    }
    public Button[] ToArray()
    {
        return data.ToArray();
    }

    public void Shift(int offset, params Button[]? items)
    {
        // this implementation is from Utility.Extensions with some adjustments

        if (items == null || items.Length == 0 || items.Length == data.Count || offset == 0)
            return;

        var results = new List<int>();
        var indexes = new List<int>();
        var itemList = items.ToList();
        var prevTargetIndex = -1;
        var max = data.Count - 1;

        foreach (var item in items)
            indexes.Add(data.IndexOf(item));

        indexes.Sort();

        if (offset > 0)
            indexes.Reverse();

        foreach (var currIndex in indexes)
        {
            var item = data[currIndex];

            if (item == null || data.Contains(item) == false)
                continue;

            var index = data.IndexOf(item);
            var targetIndex = Math.Clamp(index + offset, 0, max);

            // prevent items order change
            if (index > 0 &&
                index < max &&
                itemList.Contains(data[index + (offset > 0 ? 1 : -1)]))
                continue;

            // prevent overshooting of multiple items which would change the order
            var isOvershooting = (targetIndex == 0 && prevTargetIndex == 0) ||
                                 (targetIndex == max && prevTargetIndex == max) ||
                                 results.Contains(targetIndex);
            var i = indexes.IndexOf(data.IndexOf(item));
            var result = isOvershooting ? offset < 0 ? i : max - i : targetIndex;

            data.Remove(item);
            data.Insert(result, item);
            prevTargetIndex = targetIndex;
            results.Add(result);

            ShiftSelectedAndDisabled(index, result);
        }

        void ShiftSelectedAndDisabled(int index, int result)
        {
            var prevSel = indexesSelected.IndexOf(index);
            var prevDis = indexesDisabled.IndexOf(index);
            indexesSelected.Remove(index);
            indexesDisabled.Remove(index);

            // use insert and prev index to keep order
            if (prevSel != -1)
                indexesSelected.Insert(prevSel, result);
            if (prevDis != -1)
                indexesDisabled.Insert(prevDis, result);
        }
    }
    public void Add(params Button[] items)
    {
        Insert(data.Count, items);
    }
    public void Insert(int index, params Button[]? items)
    {
        if (items == null || items.Length == 0 || hasParent)
            return;

        InternalInsert(index, items);
    }
    public void Remove(params Button[]? items)
    {
        if (items == null || items.Length == 0 || hasParent)
            return;

        InternalRemove(items);
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
    public bool IsContaining(Button item)
    {
        return data.Contains(item);
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
        return data.IndexOf(item);
    }

    public void Deselect()
    {
        for (var i = 0; i < data.Count; i++)
            Select(i, false);
    }
    public void Select(int index, bool isSelected = true)
    {
        if (HasIndex(index) == false)
            return;

        if (IsSingleSelecting && singleSelectedIndex != index && isSelected)
        {
            foreach (var item in data)
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

    protected override void OnInput()
    {
        if (Span == Span.Dropdown && IsCollapsed && IsHovered)
            Input.CursorResult = MouseCursor.Hand;
    }

    public void OnItemDisplay(Action<Button> method)
    {
        itemDisplays += method;
    }
    public void OnItemInteraction(Interaction interaction, Action<Button> method)
    {
        if (itemInteractions.TryAdd(interaction, method) == false)
            itemInteractions[interaction] += method;

        foreach (var item in data)
            item.OnInteraction(interaction, () => method.Invoke(item));
    }

    public List Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](List list)
    {
        return list.ToBytes();
    }
    public static implicit operator List(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private int singleSelectedIndex = -1, originalHeight;
    private readonly List<Button> data = new();
    private bool isSingleSelecting, isCollapsed;
    private readonly bool isInitialized;
    private int itemGap;
    internal (int width, int height) itemSize = (6, 1);
    private readonly List<int> indexesDisabled = new(), indexesSelected = new();
    internal bool isReadOnly;

    private readonly Dictionary<Interaction, Action<Button>> itemInteractions = new();
    private Action<Button>? itemDisplays;

    private (bool horizontal, bool vertical) HasScroll
    {
        get
        {
            var (maxW, maxH) = ItemSize;
            var totalW = data.Count * (maxW + ItemGap) - ItemGap;
            var totalH = data.Count * (maxH + ItemGap) - ItemGap;

            return (totalW > Size.width, totalH > Size.height);
        }
    }
    internal bool IsScrollVisible
    {
        get => (Span == Span.Horizontal ? HasScroll.horizontal : HasScroll.vertical) &&
               IsCollapsed == false;
    }

    private void Init()
    {
        OnUpdate(OnUpdate);
        OnInteraction(Interaction.Trigger, () =>
        {
            if (IsCollapsed && IsFocused && this[singleSelectedIndex].IsHovered == false)
                IsCollapsed = false;
        });
    }
    private void InitItem(Button item, int i)
    {
        item.size = (Span == Span.Horizontal ? text.Length : Size.width, 1);
        item.hasParent = true;
        item.OnInteraction(Interaction.Trigger, () => OnInternalItemTrigger(item));
        item.OnInteraction(Interaction.Scroll, ApplyScroll);

        foreach (var kvp in itemInteractions)
            item.OnInteraction(kvp.Key, () => kvp.Value.Invoke(item));
    }

    internal void InternalAdd(params Button[] items)
    {
        InternalInsert(data.Count, items);
    }
    internal void InternalInsert(int index, params Button[] items)
    {
        for (var i = 0; i < items.Length; i++)
        {
            InitItem(items[i], i);
            data.Insert(index, items[i]);
            AdjustIndexesAbove(index, 1);
        }
    }
    internal void InternalRemove(params Button[] items)
    {
        foreach (var item in items)
        {
            var index = IndexOf(item);
            indexesDisabled.Remove(index);
            indexesSelected.Remove(index);

            AdjustIndexesAbove(index, -1);
            data.Remove(item);
        }
    }
    internal void InternalClear()
    {
        indexesDisabled.Clear();
        indexesSelected.Clear();
        data.Clear();
    }
    private static Button[] InternalCreateAmount(int count)
    {
        var result = new Button[count];
        for (var i = 0; i < count; i++)
        {
            var item = new Button();
            result[i] = item;
        }

        return result;
    }

    internal void OnUpdate()
    {
        if (Input.IsButtonJustPressed() && IsHovered == false)
            IsCollapsed = true;

        // for in between items, overwrite mouse cursor (don't give it to the block bellow)
        if (IsDisabled == false && IsHovered)
            Input.CursorResult = MouseCursor.Arrow;

        if (Span != Span.Dropdown)
        {
            // it's possible to have no scroll, single item with width/height of 1
            // that should affect the minimum size, it is possible to be (1, 1)
            var singleItem = data.Count == 1;
            var (w, h) = ItemSize;
            LimitSizeMin((
                singleItem && w == 1 ? 1 : 2,
                singleItem && h == 1 ? 1 : 2));
        }

        ItemSize = itemSize; // reclamp value
        var totalWidth = data.Count * (ItemSize.width + ItemGap);
        var totalHeight = data.Count * (ItemSize.height + ItemGap);
        var totalSize = Span == Span.Horizontal ? totalWidth : totalHeight;
        Scroll.step = 1f / totalSize;
        Scroll.isVertical = Span != Span.Horizontal;

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
        else if (Span == Span.Horizontal)
        {
            Scroll.position = (x, y + h - 1);
            Scroll.size = (w, 1);
        }

        Scroll.Update();

        for (var i = 0; i < data.Count; i++)
        {
            var item = data[i];

            item.isHidden = false;
            item.isDisabled = indexesDisabled.Contains(i);
            item.InheritParent(this);

            if (Span == Span.Horizontal)
            {
                var totalWidth = data.Count * (ItemSize.width + ItemGap);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalWidth - w - ItemGap);
                var offsetX = (int)MathF.Round(p);
                offsetX = Math.Max(offsetX, 0);
                item.position = (x - offsetX + i * (ItemSize.width + ItemGap), y);
            }
            else
            {
                var totalHeight = data.Count * (ItemSize.height + ItemGap);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalHeight - h - ItemGap);
                var offsetY = (int)MathF.Round(p);
                offsetY = Math.Max(offsetY, 0);
                item.position = (x, y - offsetY + i * (ItemSize.height + ItemGap));
            }

            var (ix, iy) = item.position;
            var (offW, offH) = item.listSizeTrimOffset;
            var botEdgeTrim = Span == Span.Horizontal && HasScroll.horizontal ? 1 : 0;
            //var rightEdgeTrim = Type == Types.Horizontal == false && HasScroll().vertical ? 1 : 0;
            var (iw, ih) = (item.Size.width + offW, item.Size.height + offH);
            if (ix + iw <= x ||
                ix >= x + w ||
                iy + ih <= y ||
                iy >= y + h - botEdgeTrim)
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
        if (isInitialized == false || data.Count == 0)
            return;

        if (IsCollapsed && isInitialized && singleSelectedIndex >= 0)
        {
            var selectedItem = this[singleSelectedIndex];

            if (selectedItem.IsHidden)
                return;

            itemDisplays?.Invoke(selectedItem);
            return;
        }

        foreach (var item in data)
            if (IsOverlapping(item))
            {
                if (item.IsHidden)
                    continue;

                itemDisplays?.Invoke(item);
            }
    }

    private void OnInternalItemTrigger(Button item)
    {
        if (IsSingleSelecting)
            Select(item);
        else
        {
            var index = IndexOf(item);
            if (item.IsSelected && indexesSelected.Contains(index) == false)
                indexesSelected.Add(index);
            else if (item.IsSelected == false)
                indexesSelected.Remove(index);
        }

        if (Span != Span.Dropdown)
            return;

        IsCollapsed = IsCollapsed == false;
        Select(item);
    }

    private bool HasIndex(int index)
    {
        return index >= 0 && index < data.Count;
    }
    private void TryTrimItem(Block item)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (ix, iy) = item.position;

        var botEdgeTrim = Span == Span.Horizontal && HasScroll.horizontal ? 1 : 0;
        var rightEdgeTrim = Span == Span.Horizontal == false && HasScroll.vertical ? 1 : 0;
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

    private void AdjustIndexesAbove(int index, int offset)
    {
        for (var i = 0; i < indexesSelected.Count; i++)
            if (index <= indexesSelected[i])
                indexesSelected[i] += offset;

        for (var i = 0; i < indexesDisabled.Count; i++)
            if (index <= indexesDisabled[i])
                indexesDisabled[i] += offset;
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}