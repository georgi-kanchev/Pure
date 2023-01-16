namespace Pure.UserInterface
{
	public class List : UserInterface
	{
		public Slider Scroll { get; }
		public Button ScrollUp { get; }
		public Button ScrollDown { get; }

		public int Count => items.Count;

		public Button? this[int index]
		{
			get => index > 0 && index < items.Count ? items[index] : default;
		}

		public List((int, int) position, (int, int) size, params Button[] items) : base(position, size)
		{
			var (x, y) = Position;
			var (w, h) = Size;

			Scroll = new((x + w - 1, y + 1), h - 2, true);
			ScrollUp = new((x + w, y), (1, 1));
			ScrollDown = new((x + w, y + w), (1, 1));

			if(items != null && items.Length != 0)
				this.items = new(items);

			UpdateParts();
		}

		public void Add(Button item)
		{
			if(items.Contains(item))
				return;

			items.Add(item);
		}
		public void Remove(Button item)
		{
			items.Remove(item);
		}
		public bool Contains(Button item)
		{
			return item != null && items.Contains(item);
		}
		public int IndexOf(Button item)
		{
			return items.IndexOf(item);
		}

		protected override void OnUpdate()
		{
			UpdateParts();
		}

		public static implicit operator List(Button[] items) => new((0, 0), (5, 5), items);
		public static implicit operator Button[](List list) => list.items.ToArray();

		#region Backend
		private readonly List<Button> items = new();

		private void UpdateParts()
		{
			var (x, y) = Position;
			var (w, h) = Size;

			Scroll.Position = (x + w - 1, y + 1);
			Scroll.Size = (1, h - 2);
			Scroll.IsVertical = true;

			ScrollUp.Position = (x + w, y);
			ScrollUp.Size = (1, 1);

			ScrollDown.Position = (x + w, y + w);
			ScrollDown.Size = (1, 1);
		}
		#endregion
	}
}
