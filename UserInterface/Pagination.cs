namespace Pure.UserInterface;

public class Pagination : Element
{
	public Button First { get; }
	public Button Previous { get; }
	public Button Next { get; }
	public Button Last { get; }

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
			currentPage = Math.Clamp(value, 1, Count);
			var pageCount = GetVisiblePageCount();
			scrollIndex = Math.Clamp(currentPage - pageCount / 2, 1, Math.Max(Count - pageCount + 1, 1));
			RenumberAndSelectPages();
		}
	}

	public Pagination((int x, int y) position, int count = 10) : base(position)
	{
		Size = (13, 1);
		Count = count;

		First = new(default) { hasParent = true };
		Previous = new(default) { hasParent = true };
		Next = new(default) { hasParent = true };
		Last = new(default) { hasParent = true };

		First.SubscribeToUserEvent(UserEvent.Trigger, () => CurrentPage = 0);
		Previous.SubscribeToUserEvent(UserEvent.Trigger, () => CurrentPage--);
		Next.SubscribeToUserEvent(UserEvent.Trigger, () => CurrentPage++);
		Last.SubscribeToUserEvent(UserEvent.Trigger, () => CurrentPage = Count);
	}

	protected override void OnUpdate()
	{
		if (IsDisabled)
			return;

		var (x, y) = Position;
		var (w, h) = Size;
		var visibleWidth = GetVisibleWidth();

		First.position = (x, y);
		Previous.position = (x + 1, y);
		Next.position = (Previous.Position.x + visibleWidth, y);
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

		for (int i = 0; i < visiblePages.Count; i++)
		{
			var page = visiblePages[i];

			page.position = (x + 2 + (PAGE_WIDTH + PAGE_GAP) * i, y);
			page.Update();
			OnPageUpdate(page);
		}

		prevSize = Size;
	}
	protected virtual void OnPageUpdate(Button page) { }

	#region Backend
	private (int, int) prevSize;
	const int PAGE_GAP = 1, PAGE_WIDTH = 2;
	private int count, currentPage = 1, scrollIndex = 1;
	private readonly List<Button> visiblePages = new();

	private int GetVisibleWidth() => Size.width - 4;
	private int GetVisiblePageCount()
	{
		var visibleWidth = GetVisibleWidth();
		var result = 0;
		var width = 0;
		while (width + PAGE_WIDTH < visibleWidth)
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
		for (int i = 0; i < pageCount; i++)
		{
			var page = new Button(default) { size = (2, Size.height), hasParent = true };
			visiblePages.Add(page);
			page.SubscribeToUserEvent(UserEvent.Trigger, () => CurrentPage = int.Parse(page.Text));
		}
	}
	private void RenumberAndSelectPages()
	{
		var pageCount = GetVisiblePageCount();
		for (int i = 0; i < visiblePages.Count; i++)
		{
			var pageNumber = i + scrollIndex;
			visiblePages[i].Text = $"{pageNumber:D2}";
			visiblePages[i].IsSelected = pageNumber == currentPage;
		}
	}

	#endregion
}