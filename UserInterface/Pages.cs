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
            count = Math.Clamp(value, 1, 99);
            RecreatePages();
            CurrentPage = currentPage; // reclamp and update pages
        }
    }
    public int CurrentPage
    {
        get => currentPage;
        set
        {
            var prev = currentPage;
            currentPage = Math.Clamp(value, 1, Count);
            var pageCount = GetVisiblePageCount();
            scrollIndex = Math.Clamp(currentPage - pageCount / 2, 1, Math.Max(Count - pageCount + 1, 1));
            RenumberAndSelectPages();

            if (prev != currentPage)
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
        CurrentPage = GrabInt(bytes);

        Init();
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutInt(result, Count);
        PutInt(result, CurrentPage);
        return result.ToArray();
    }

    protected virtual void OnPageDisplay(Button page) { }

#region Backend
    private (int, int) prevSize;
    private const int PAGE_GAP = 1, PAGE_WIDTH = 2;
    private int count, currentPage = 1, scrollIndex = 1;
    private readonly List<Button> visiblePages = new();

    internal Action<Button>? pageDisplayCallback; // used in the UI class to receive callbacks

    [MemberNotNull(nameof(First))]
    [MemberNotNull(nameof(Previous))]
    [MemberNotNull(nameof(Next))]
    [MemberNotNull(nameof(Last))]
    private void Init()
    {
        isParent = true;

        First = new((0, 0)) { hasParent = true };
        Previous = new((0, 0)) { hasParent = true };
        Next = new((0, 0)) { hasParent = true };
        Last = new((0, 0)) { hasParent = true };

        First.SubscribeToUserAction(UserAction.Trigger, () => CurrentPage = 0);
        Previous.SubscribeToUserAction(UserAction.Trigger, () => CurrentPage--);
        Next.SubscribeToUserAction(UserAction.Trigger, () => CurrentPage++);
        Last.SubscribeToUserAction(UserAction.Trigger, () => CurrentPage = Count);

        Previous.SubscribeToUserAction(UserAction.PressAndHold, () => CurrentPage--);
        Next.SubscribeToUserAction(UserAction.PressAndHold, () => CurrentPage++);
    }

    internal override void OnUpdate()
    {
        LimitSizeMin((6, 1));

        if (IsDisabled)
            return;

        // for in between pages, overwrite mouse cursor (don't give it to the element bellow)
        if (IsHovered)
            MouseCursorResult = MouseCursor.Arrow;

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

        First.Update();
        Previous.Update();
        Next.Update();
        Last.Update();

        if (Size != prevSize)
        {
            RecreatePages();
            RenumberAndSelectPages();
        }

        for (var i = 0; i < visiblePages.Count; i++)
        {
            var page = visiblePages[i];

            page.position = (x + 2 + (PAGE_WIDTH + PAGE_GAP) * i, y);
            page.Update();
        }

        prevSize = Size;
    }
    internal override void OnDisplayChildren()
    {
        foreach (var page in visiblePages)
        {
            OnPageDisplay(page);
            pageDisplayCallback?.Invoke(page);
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
            page.SubscribeToUserAction(UserAction.Trigger, () => CurrentPage = int.Parse(page.Text));
        }
    }
    private void RenumberAndSelectPages()
    {
        for (var i = 0; i < visiblePages.Count; i++)
        {
            var pageNumber = i + scrollIndex;
            visiblePages[i].Text = $"{pageNumber:D2}";
            visiblePages[i].isSelected = pageNumber == currentPage;
        }
    }
#endregion
}