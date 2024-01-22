namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;

public class Pages : Block
{
    public Button First { get; private set; }
    public Button Previous { get; private set; }
    public Button Next { get; private set; }
    public Button Last { get; private set; }

    public int Count
    {
        get => count;
        set
        {
            if (count == value)
                return; // no actual change, skip calculations

            count = Math.Max(value, 1);
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

            UpdatePages();

            if (prev != current)
                Interact(Interaction.Select);
        }
    }

    public int ItemWidth
    {
        get => itemWidth;
        set
        {
            itemWidth = Math.Max(value, 1);
            RecreatePages();
            UpdatePages();
        }
    }
    public int ItemGap
    {
        get => itemGap;
        set
        {
            itemGap = Math.Max(value, 0);
            RecreatePages();
            UpdatePages();
        }
    }

    public Pages((int x, int y) position = default, int count = 10) : base(position)
    {
        ItemWidth = 1;
        ItemGap = 1;
        Size = (12, 1);
        Count = count;

        Init();
    }
    public Pages(byte[] bytes) : base(bytes)
    {
        ItemWidth = GrabByte(bytes);
        ItemGap = GrabByte(bytes);
        Count = GrabInt(bytes);
        Current = GrabInt(bytes);

        Init();
    }
    public Pages(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutByte(result, (byte)ItemWidth);
        PutByte(result, (byte)ItemGap);
        PutInt(result, Count);
        PutInt(result, Current);
        return result.ToArray();
    }

    public int IndexOf(Button button)
    {
        var index = visiblePages.IndexOf(button);
        return index == -1 ? -1 : index + scrollIndex - 1;
    }

    public void OnItemDisplay(Action<Button> method)
    {
        itemDisplays += method;
    }
    public void OnItemInteraction(Interaction interaction, Action<Button> method)
    {
        if (itemInteractions.TryAdd(interaction, method) == false)
            itemInteractions[interaction] += method;

        foreach (var item in visiblePages)
            item.OnInteraction(interaction, () => method.Invoke(item));
    }

    public Pages Copy()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Pages pages)
    {
        return pages.ToBytes();
    }
    public static implicit operator Pages(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private (int, int) prevSize;
    private int count, current = 1, scrollIndex = 1;
    private readonly List<Button> visiblePages = new();

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

    private readonly Dictionary<Interaction, Action<Button>> itemInteractions = new();
    private Action<Button>? itemDisplays;
    private int itemWidth;
    private int itemGap;

    [MemberNotNull(nameof(First), nameof(Previous), nameof(Next), nameof(Last))]
    private void Init()
    {
        OnUpdate(OnUpdate);

        First = new((int.MaxValue, int.MaxValue)) { hasParent = true };
        Previous = new((int.MaxValue, int.MaxValue)) { hasParent = true };
        Next = new((int.MaxValue, int.MaxValue)) { hasParent = true };
        Last = new((int.MaxValue, int.MaxValue)) { hasParent = true };

        First.OnInteraction(Interaction.Trigger, () => Current = 0);
        Previous.OnInteraction(Interaction.Trigger, () => Current--);
        Next.OnInteraction(Interaction.Trigger, () => Current++);
        Last.OnInteraction(Interaction.Trigger, () => Current = Count);

        Previous.OnInteraction(Interaction.PressAndHold, () => Current--);
        Next.OnInteraction(Interaction.PressAndHold, () => Current++);

        First.OnInteraction(Interaction.Scroll, ApplyScroll);
        Previous.OnInteraction(Interaction.Scroll, ApplyScroll);
        Next.OnInteraction(Interaction.Scroll, ApplyScroll);
        Last.OnInteraction(Interaction.Scroll, ApplyScroll);
    }

    internal void OnUpdate()
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
            itemDisplays?.Invoke(page);
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var visibleWidth = VisibleWidth;
        var hasNav = HasNavigation;

        First.isHidden = hasNav == false;
        First.isDisabled = hasNav == false;
        Previous.isHidden = hasNav == false;
        Previous.isDisabled = hasNav == false;
        Next.isHidden = hasNav == false;
        Next.isDisabled = hasNav == false;
        Last.isHidden = hasNav == false;
        Last.isDisabled = hasNav == false;

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

            First.InheritParent(this);
            Previous.InheritParent(this);
            Next.InheritParent(this);
            Last.InheritParent(this);

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
            page.InheritParent(this);
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
        {
            var pageNumber = i + scrollIndex;
            visiblePages[i].Text = $"{pageNumber}";
            visiblePages[i].isSelected = pageNumber == current;
        }
    }

    private static float[] Distribute(int amount, (float a, float b) range)
    {
        if (amount <= 0)
            return Array.Empty<float>();

        var result = new float[amount];
        var size = range.b - range.a;
        var spacing = size / (amount + 1);

        for (var i = 1; i <= amount; i++)
            result[i - 1] = range.a + i * spacing;

        return result;
    }
#endregion
}