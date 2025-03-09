namespace Pure.Engine.UserInterface;

public class Pages : Block
{
    [DoNotSave]
    public Action<Button>? OnItemDisplay { get; set; }

    [DoNotSave]
    public Button First { get; }
    [DoNotSave]
    public Button Previous { get; }
    [DoNotSave]
    public Button Next { get; }
    [DoNotSave]
    public Button Last { get; }

    public int Count
    {
        get => count;
        set
        {
            var prev = count;
            count = Math.Max(value, 1);

            if (prev == count)
                return; // no actual change, skip calculations

            Current = current; // reclamp and update pages
            RecreatePages();
            UpdatePages();
        }
    }
    public int Current
    {
        get => current;
        set
        {
            var prev = current;
            current = Math.Clamp(value, 1, Count);

            if (prev == current)
                return; // no actual change, skip calculations

            UpdatePages();
            Interact(Interaction.Select);
        }
    }

    public int ItemWidth
    {
        get => itemWidth;
        set
        {
            var prev = itemWidth;
            itemWidth = Math.Max(value, 1);

            if (prev == itemWidth)
                return; // no actual change, skip calculations

            RecreatePages();
            UpdatePages();
        }
    }
    public int ItemGap
    {
        get => itemGap;
        set
        {
            var prev = itemGap;
            itemGap = Math.Max(value, 0);

            if (prev == itemGap)
                return; // no actual change, skip calculations

            RecreatePages();
            UpdatePages();
        }
    }

    public Pages() : this((0, 0))
    {
    }
    public Pages(PointI position, int count = 10) : base(position)
    {
        ItemWidth = 1;
        ItemGap = 1;
        Size = (12, 1);
        Count = count;

        OnUpdate += OnRefresh;

        First = new((int.MaxValue, int.MaxValue)) { hasParent = true, wasMaskSet = true };
        Previous = new((int.MaxValue, int.MaxValue)) { hasParent = true, wasMaskSet = true };
        Next = new((int.MaxValue, int.MaxValue)) { hasParent = true, wasMaskSet = true };
        Last = new((int.MaxValue, int.MaxValue)) { hasParent = true, wasMaskSet = true };

        First.OnInteraction(Interaction.Trigger, () => Current = 0);
        Previous.OnInteraction(Interaction.Trigger, () => Current--);
        Next.OnInteraction(Interaction.Trigger, () => Current++);
        Last.OnInteraction(Interaction.Trigger, () => Current = Count);

        Previous.OnInteraction(Interaction.PressAndHold, () => Previous.Interact(Interaction.Trigger));
        Next.OnInteraction(Interaction.PressAndHold, () => Next.Interact(Interaction.Trigger));

        First.OnInteraction(Interaction.Scroll, ApplyScroll);
        Previous.OnInteraction(Interaction.Scroll, ApplyScroll);
        Next.OnInteraction(Interaction.Scroll, ApplyScroll);
        Last.OnInteraction(Interaction.Scroll, ApplyScroll);
    }

    public int IndexOf(Button button)
    {
        var index = visiblePages.IndexOf(button);
        return index == -1 ? -1 : index + scrollIndex - 1;
    }

    public void OnItemInteraction(Interaction interaction, Action<Button> method)
    {
        if (itemInteractions.TryAdd(interaction, method) == false)
            itemInteractions[interaction] += method;

        foreach (var item in visiblePages)
            item.OnInteraction(interaction, () => method.Invoke(item));
    }

#region Backend
    private (int, int) prevSize;
    private int count, current = 1, scrollIndex = 1;
    [DoNotSave]
    private readonly List<Button> visiblePages = [];

    private int VisibleWidth
    {
        get => Count * (ItemWidth + ItemGap) <= Size.width ? Size.width : Size.width - 4;
    }
    private int VisiblePageCount
    {
        get
        {
            var result = Math.Min(VisibleWidth / (ItemWidth + ItemGap), Count);

            // check if there's a gap at the end and if so, increment the result by 1
            if (result < Count && VisibleWidth % (ItemWidth + ItemGap) >= ItemWidth)
                result++;

            return result;
        }
    }
    private bool HasFreeSpace
    {
        get
        {
            var totalItemSpace = Count * ItemWidth + (Count - 1) * ItemGap;
            var remainingSpace = VisibleWidth - totalItemSpace;
            return remainingSpace >= ItemWidth;
        }
    }
    private bool HasNavigation
    {
        get => Count * (ItemWidth + ItemGap) > Size.width;
    }

    [DoNotSave]
    private readonly Dictionary<Interaction, Action<Button>> itemInteractions = new();
    private int itemWidth = 1;
    private int itemGap = 1;

    internal void OnRefresh()
    {
        // for in between pages, overwrite mouse cursor (don't give it to the block bellow)
        if (IsDisabled == false && IsHovered)
            Input.CursorResult = MouseCursor.Arrow;

        if (Count > Size.width)
            LimitSizeMin((5, 1));

        if (Size != prevSize)
        {
            RecreatePages();
            UpdatePages();
        }

        prevSize = Size;
    }
    internal override void OnChildrenDisplay()
    {
        foreach (var page in visiblePages)
            OnItemDisplay?.Invoke(page);
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var visibleWidth = VisibleWidth;
        var hasNav = HasNavigation;

        var m = HasNavigation ? mask : Input.Mask;
        First.mask = m;
        Previous.mask = m;
        Next.mask = m;
        Last.mask = m;

        if (hasNav)
        {
            First.position = (x, y);
            Previous.position = (x + 1, y);
            Next.position = (Previous.Position.x + visibleWidth + 1, y);
            Last.position = (Next.Position.x + 1, y);

            First.size = (1, Size.height);
            Previous.size = (1, Size.height);
            Next.size = (1, Size.height);
            Last.size = (1, Size.height);

            First.Update();
            Previous.Update();
            Next.Update();
            Last.Update();
        }

        var hasFreeSpace = HasFreeSpace;
        var range = hasFreeSpace ? (Position.x + 2, Position.x + Size.width - 2) : (0, 0);
        var xs = Distribute(hasFreeSpace ? visiblePages.Count : 0, range);
        var navOff = hasNav ? 2 : 0;

        for (var i = 0; i < visiblePages.Count; i++)
        {
            var page = visiblePages[i];
            var pos = (x + navOff + (ItemWidth + ItemGap) * i, y);

            if (hasFreeSpace && hasNav)
                pos = ((int)xs[i], y);

            page.position = pos;
            page.mask = mask;
            page.isSelected = scrollIndex + i == Current;
            page.Update();
        }
    }
    internal override void ApplyScroll()
    {
        Current -= Input.ScrollDelta;
    }

    private void RecreatePages()
    {
        var pageCount = VisiblePageCount;

        visiblePages.Clear();
        for (var i = 0; i < pageCount; i++)
        {
            var page = new Button((0, 0))
            {
                size = (ItemWidth, Size.height),
                hasParent = true
            };
            visiblePages.Add(page);
            page.OnInteraction(Interaction.Trigger, () => Current = int.Parse(page.Text));
            page.OnInteraction(Interaction.Scroll, ApplyScroll);

            foreach (var kvp in itemInteractions)
                page.OnInteraction(kvp.Key, () => kvp.Value.Invoke(page));
        }
    }
    private void UpdatePages()
    {
        var pageCount = VisiblePageCount;
        scrollIndex = Math.Clamp(current - pageCount / 2, 1, Math.Max(Count - pageCount + 1, 1));

        for (var i = 0; i < visiblePages.Count; i++)
            visiblePages[i].Text = $"{i + scrollIndex}";
    }

    private static float[] Distribute(int amount, Range range)
    {
        if (amount <= 0)
            return [];

        var result = new float[amount];
        var size = range.b - range.a;
        var spacing = size / (amount + 1);

        for (var i = 1; i <= amount; i++)
            result[i - 1] = range.a + i * spacing;

        return result;
    }
#endregion
}