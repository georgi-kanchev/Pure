using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Pages : Element
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
                TriggerUserAction(UserAction.Select);
        }
    }

    public Pages((int x, int y) position, int count = 10) : base(position)
    {
        Size = (13, 1);
        Count = count;

        Init();
    }
    public Pages(byte[] bytes) : base(bytes)
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

    protected virtual void OnPageDisplay(Button page) { }

#region Backend
    private (int, int) prevSize;
    private const int PAGE_GAP = 1, PAGE_WIDTH = 2;
    private int count, current = 1, scrollIndex = 1;
    private readonly List<Button> visiblePages = new();

    internal Action<Button>? pageDisplayCallback; // used in the UI class to receive callbacks

    [MemberNotNull(nameof(First))]
    [MemberNotNull(nameof(Previous))]
    [MemberNotNull(nameof(Next))]
    [MemberNotNull(nameof(Last))]
    private void Init()
    {
        First = new((0, 0)) { hasParent = true };
        Previous = new((0, 0)) { hasParent = true };
        Next = new((0, 0)) { hasParent = true };
        Last = new((0, 0)) { hasParent = true };

        First.SubscribeToUserAction(UserAction.Trigger, () => Current = 0);
        Previous.SubscribeToUserAction(UserAction.Trigger, () => Current--);
        Next.SubscribeToUserAction(UserAction.Trigger, () => Current++);
        Last.SubscribeToUserAction(UserAction.Trigger, () => Current = Count);

        Previous.SubscribeToUserAction(UserAction.PressAndHold, () => Current--);
        Next.SubscribeToUserAction(UserAction.PressAndHold, () => Current++);
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
        {
            OnPageDisplay(page);
            pageDisplayCallback?.Invoke(page);
        }
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

    private int GetVisibleWidth() => Size.width - 4;
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
            page.SubscribeToUserAction(UserAction.Trigger, () => Current = int.Parse(page.Text));
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