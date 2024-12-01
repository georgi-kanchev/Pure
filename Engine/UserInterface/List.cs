﻿namespace Pure.Engine.UserInterface;

public enum Span { Vertical, Horizontal, Dropdown }

public enum Sort { Alphabetically, Numerically, ByLength }

/// <summary>
/// Stores items, represented as buttons that can be scrolled, clicked, selected and manipulated.
/// </summary>
public class List : Block
{
    [DoNotSave]
    public Action<Button>? OnItemDisplay { get; set; }

    public List<Button> Items { get; } = [];
    [DoNotSave]
    public Scroll Scroll { get; }

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
            if (Span == Span.Dropdown)
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

            foreach (var item in Items)
                item.isTextReadonly = value;
        }
    }
    public bool IsScrollAvailable
    {
        get => (Span == Span.Horizontal ? HasScroll.horizontal : HasScroll.vertical) &&
               IsCollapsed == false;
    }

    [DoNotSave]
    public List<Button> SelectedItems { get; } = [];

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

    public List() : this((0, 0))
    {
    }
    /// <summary>
    /// Initializes a new list instance with the specified position, size and number of items.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the list.</param>
    /// <param name="itemCount">The initial number of buttons in the list.</param>
    /// <param name="span">The type of the list.</param>
    public List((int x, int y) position, int itemCount = 10, Span span = Span.Vertical) : base(position)
    {
        Scroll = new((int.MaxValue, int.MaxValue)) { hasParent = true, wasMaskSet = true };
        OnUpdate += OnRefresh;
        OnInteraction(Interaction.Trigger, () =>
        {
            if (IsCollapsed && IsFocused)
                IsCollapsed = false;
        });
        Size = (6, 8);
        ItemGap = span == Span.Horizontal ? 1 : 0;
        Span = span;

        var items = CreateAmount(itemCount);
        Items.AddRange(items);

        isInitialized = true;

        if (span != Span.Dropdown)
            return;

        IsSingleSelecting = true;

        if (Items.Count > 0)
            Select(Items[0]);
    }

    public void Edit(string[]? items)
    {
        if (items == null || items.Length == 0)
            return;

        for (var i = 0; i < items.Length; i++)
        {
            if (i >= Items.Count)
                return;

            Items[i].text = items[i];
        }

        UpdateSelectedItems();
    }
    public void Sort(Sort sort = UserInterface.Sort.Alphabetically)
    {
        if (sort == UserInterface.Sort.Alphabetically)
            for (var i = 0; i < Items.Count - 1; i++)
                for (var j = 0; j < Items.Count - 1 - i; j++)
                    if (string.Compare(Items[j].Text, Items[j + 1].text, StringComparison.Ordinal) > 0)
                        (Items[j], Items[j + 1]) = (Items[j + 1], Items[j]);

        if (sort == UserInterface.Sort.Numerically)
            for (var i = 0; i < Items.Count - 1; i++)
                for (var j = 0; j < Items.Count - 1 - i; j++)
                {
                    var isNum1 = int.TryParse(Items[j].Text, out var num1);
                    var isNum2 = int.TryParse(Items[j + 1].Text, out var num2);

                    if (isNum1 && isNum2)
                    {
                        // Both are numeric, compare numerically
                        if (num1 > num2)
                            (Items[j], Items[j + 1]) = (Items[j + 1], Items[j]);
                    }
                    else if (isNum1 == false && isNum2)
                        // First item is non-numeric, second is numeric, so swap
                        (Items[j], Items[j + 1]) = (Items[j + 1], Items[j]);

                    // If both are non-numeric or both are numeric, no need to swap.
                }

        if (sort != UserInterface.Sort.ByLength)
            return;

        for (var i = 0; i < Items.Count - 1; i++)
            for (var j = 0; j < Items.Count - 1 - i; j++)
                if (Items[j].Text.Length > Items[j + 1].Text.Length)
                    (Items[j], Items[j + 1]) = (Items[j + 1], Items[j]);
    }
    public void Deselect()
    {
        foreach (var btn in Items)
            Select(btn, false);
    }
    public void Select(Button? item, bool selected = true)
    {
        if (item == null)
            return;

        if (IsSingleSelecting)
            foreach (var btn in Items)
                btn.isSelected = false;

        var prev = item.isSelected;
        item.isSelected = selected;

        if (prev != item.isSelected)
            Interact(Interaction.Select);

        UpdateSelectedItems();
    }

    protected override void OnInput()
    {
        if (Span == Span.Dropdown && IsCollapsed && IsHovered)
            Input.CursorResult = MouseCursor.Hand;
    }

    public void OnItemInteraction(Interaction interaction, Action<Button> method)
    {
        if (itemInteractions.TryAdd(interaction, method) == false)
            itemInteractions[interaction] += method;

        foreach (var item in Items)
            item.OnInteraction(interaction, () => method.Invoke(item));
    }

#region Backend
    private int originalHeight;
    private bool isSingleSelecting, isCollapsed, veryFirstUpdate = true;
    private readonly bool isInitialized;
    private int itemGap;
    internal (int width, int height) itemSize = (5, 1);
    internal bool isReadOnly;

    [DoNotSave]
    private readonly Dictionary<Interaction, Action<Button>> itemInteractions = new();

    private (bool horizontal, bool vertical) HasScroll
    {
        get
        {
            var (maxW, maxH) = ItemSize;
            var totalW = Items.Count * (maxW + ItemGap) - ItemGap;
            var totalH = Items.Count * (maxH + ItemGap) - ItemGap;

            return (totalW > Size.width, totalH > Size.height);
        }
    }

    private void InitItem(Button item)
    {
        item.size = (Span == Span.Horizontal ? text.Length : Size.width, 1);
        item.hasParent = true;
        item.wasMaskSet = true;
        item.OnInteraction(Interaction.Trigger, () => OnInternalItemTrigger(item));
        item.OnInteraction(Interaction.Scroll, ApplyScroll);

        foreach (var kvp in itemInteractions)
            if (kvp.Key != Interaction.Trigger)
                item.OnInteraction(kvp.Key, () => kvp.Value.Invoke(item));
    }

    private static Button[] CreateAmount(int count)
    {
        var result = new Button[count];
        for (var i = 0; i < count; i++)
            result[i] = new() { Text = $"Item{i}" };

        return result;
    }

    internal void OnRefresh()
    {
        // this is to give time to dropdown to accept size when not collapsed
        if (veryFirstUpdate)
        {
            veryFirstUpdate = false;
            originalHeight = Height;
            IsCollapsed = true;
        }

        UpdateSelectedItems();

        foreach (var btn in Items)
            if (btn.hasParent == false)
                InitItem(btn);

        if (Input.IsButtonJustPressed() && IsHovered == false)
            IsCollapsed = true;

        // for in between items, overwrite mouse cursor (don't give it to the block bellow)
        if (IsDisabled == false && IsHovered)
            Input.CursorResult = MouseCursor.Arrow;

        if (Span != Span.Dropdown)
        {
            // it's possible to have no scroll, single item with width/height of 1
            // that should affect the minimum size, it is possible to be (1, 1)
            var singleItem = Items.Count == 1;
            var (w, h) = ItemSize;
            LimitSizeMin((
                singleItem && w == 1 ? 1 : 2,
                singleItem && h == 1 ? 1 : 2));
        }

        ItemSize = itemSize; // reclamp value
        var totalWidth = Items.Count * (ItemSize.width + ItemGap);
        var totalHeight = Items.Count * (ItemSize.height + ItemGap);
        var totalSize = Span == Span.Horizontal ? totalWidth : totalHeight;
        Scroll.step = 1f / totalSize;
        Scroll.isVertical = Span != Span.Horizontal;

        if (Span != Span.Dropdown)
            return;

        Height = IsCollapsed ? 1 : originalHeight;

        if (Items.Count > 0 && SelectedItems.Count == 0)
            Select(Items[0]);
    }
    private void UpdateSelectedItems()
    {
        SelectedItems.Clear();
        foreach (var btn in Items)
            if (btn.isSelected)
                SelectedItems.Add(btn);
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

        var m = IsScrollAvailable ? Mask : default;
        Scroll.mask = m;

        Scroll.position = (x + w - 1, y);
        Scroll.size = (1, h);

        if (IsCollapsed && isInitialized && SelectedItems.Count > 0)
        {
            var selectedItem = SelectedItems[0];
            selectedItem.position = Position;
            selectedItem.mask = mask;

            TryTrimItem(selectedItem);

            selectedItem.Update();

            Scroll.mask = default;
            if (IsScrollAvailable)
                Scroll.Update();
            return;
        }

        if (Span == Span.Horizontal)
        {
            Scroll.position = (x, y + h - 1);
            Scroll.size = (w, 1);
        }

        if (IsScrollAvailable)
            Scroll.Update();

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];

            item.mask = mask;

            if (Span == Span.Horizontal)
            {
                var totalWidth = Items.Count * (ItemSize.width + ItemGap);
                var p = Map(Scroll.Slider.Progress, 0, 1, 0, totalWidth - w - ItemGap);
                var offsetX = (int)MathF.Round(p);
                offsetX = Math.Max(offsetX, 0);
                item.position = (x - offsetX + i * (ItemSize.width + ItemGap), y);
            }
            else
            {
                var totalHeight = Items.Count * (ItemSize.height + ItemGap);
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
                item.mask = default;

            TryTrimItem(item);
            item.Update();
        }
    }
    internal override void OnChildrenDisplay()
    {
        if (isInitialized == false || Items.Count == 0)
            return;

        if (IsCollapsed && isInitialized && SelectedItems.Count > 0)
        {
            var selectedItem = SelectedItems[0];

            if (selectedItem.IsHidden)
                return;

            OnItemDisplay?.Invoke(selectedItem);
            return;
        }

        foreach (var item in Items)
            if (IsOverlapping(item) && item.IsHidden == false)
                OnItemDisplay?.Invoke(item);
    }

    private void OnInternalItemTrigger(Button item)
    {
        if (IsSingleSelecting)
            Select(item);
        else
        {
            Interact(Interaction.Select);
            UpdateSelectedItems();
        }

        if (Span != Span.Dropdown)
            return;

        IsCollapsed = IsCollapsed == false;
        Select(item);
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

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}