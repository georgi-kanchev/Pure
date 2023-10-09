using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Pages : Element
{
    public Button First
    {
        get;
        private set;
    }
    public Button Previous
    {
        get;
        private set;
    }
    public Button Next
    {
        get;
        private set;
    }
    public Button Last
    {
        get;
        private set;
    }

    public int Count
    {
        get => count;
        set
        {
            if (count == value)
                return; // no actual change, skip calculations

            count = Math.Clamp(value, 1, 99);
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
                SimulateInteraction(Interaction.Select);
        }
    }

    public Pages((int x, int y) position = default, int count = 10)
        : base(position)
    {
        Size = (12, 1);
        Count = count;

        Init();
    }
    public Pages(byte[] bytes)
        : base(bytes)
    {
        Count = GrabInt(bytes);
        Current = GrabInt(bytes);

        Init();
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutInt(result, Count);
        PutInt(result, Current);
        return result.ToArray();
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

#region Backend
    private (int, int) prevSize;
    private const int PAGE_GAP = 1, PAGE_WIDTH = 2;
    private int count, current = 1, scrollIndex = 1;
    private readonly List<Button> visiblePages = new();

    private readonly Dictionary<Interaction, Action<Button>> itemInteractions = new();
    private Action<Button>? itemDisplays;

    [MemberNotNull(nameof(First), nameof(Previous), nameof(Next), nameof(Last))]
    private void Init()
    {
        First = new((0, 0)) { hasParent = true };
        Previous = new((0, 0)) { hasParent = true };
        Next = new((0, 0)) { hasParent = true };
        Last = new((0, 0)) { hasParent = true };

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

    internal override void OnUpdate()
    {
        // for in between pages, overwrite mouse cursor (don't give it to the element bellow)
        if (IsDisabled == false && IsHovered)
            MouseCursorResult = MouseCursor.Arrow;

        LimitSizeMin((6, 1));

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
        var visibleWidth = GetVisibleWidth();

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

        for (var i = 0; i < visiblePages.Count; i++)
        {
            var page = visiblePages[i];
            page.position = (x + 2 + (PAGE_WIDTH + PAGE_GAP) * i, y);
            page.InheritParent(this);
            page.Update();
        }
    }
    internal override void ApplyScroll()
    {
        Current += Input.Current.ScrollDelta;
    }

    private int GetVisibleWidth()
    {
        return Size.width - 4;
    }
    private int GetVisiblePageCount()
    {
        var visibleWidth = GetVisibleWidth();
        var result = 0;
        var width = 0;
        while (width + PAGE_WIDTH <= visibleWidth)
        {
            width += PAGE_WIDTH + PAGE_GAP;
            result++;

            if (result > Count)
                return Count;
        }

        return result;
    }

    private void RecreatePages()
    {
        var pageCount = GetVisiblePageCount();

        visiblePages.Clear();
        for (var i = 0; i < pageCount; i++)
        {
            var page = new Button((0, 0)) { size = (2, Size.height), hasParent = true };
            visiblePages.Add(page);
            page.OnInteraction(Interaction.Trigger, () => Current = int.Parse(page.Text));
            page.OnInteraction(Interaction.Scroll, ApplyScroll);
        }
    }
    private void UpdatePages()
    {
        var pageCount = GetVisiblePageCount();
        scrollIndex = Math.Clamp(current - pageCount / 2, 1, Math.Max(Count - pageCount + 1, 1));

        for (var i = 0; i < visiblePages.Count; i++)
        {
            var pageNumber = i + scrollIndex;
            visiblePages[i].Text = $"{pageNumber:D2}";
            visiblePages[i].isSelected = pageNumber == current;
        }
    }
#endregion
}