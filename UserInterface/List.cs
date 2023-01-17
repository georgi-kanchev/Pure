namespace Pure.UserInterface
{
	public class List : UserInterface
	{
		public Slider Scroll { get; }
		public Button ScrollUp { get; }
		public Button ScrollDown { get; }

		public int Count => items.Count;
		public bool IsSingleSelecting
		{
			get => isSingleSelecting;
			set { isSingleSelecting = value; TrySingleSelectOneItem(); }
		}

		public Checkbox? this[int index] => index < 0 || index >= items.Count ? default : items[index];

		public List((int, int) position, (int, int) size, int count)
			: base(position, size)
		{
			var (x, y) = Position;
			var (w, h) = Size;

			Scroll = new((x + w - 1, y + 1), h - 2, true);
			ScrollUp = new((x + w, y), (1, 1));
			ScrollDown = new((x + w, y + w), (1, 1));

			for(int i = 0; i < count; i++)
				items.Add(new(Position) { Size = (w - 1, 1) });

			ScrollUp.Subscribe(Event.Press, () => Scroll.Move(1));
			ScrollUp.Subscribe(Event.Hold, () => Scroll.Move(1));
			ScrollDown.Subscribe(Event.Press, () => Scroll.Move(-1));
			ScrollDown.Subscribe(Event.Hold, () => Scroll.Move(-1));

			UpdateParts();
			UpdateItems();
		}

		public void Add()
		{
			items.Add(new(Position) { Size = (Size.Item1, 1) });
			TrySingleSelectOneItem();
		}
		public void Remove(int index)
		{
			if(index < 0 && index >= items.Count)
				return;

			items.RemoveAt(index);
			TrySingleSelectOneItem();
		}
		public bool Contains(Checkbox item)
		{
			return item != null && items.Contains(item);
		}
		public int IndexOf(Checkbox item)
		{
			return item == null ? -1 : items.IndexOf(item);
		}

		protected override void OnUpdate()
		{
			if(IsDisabled)
				return;

			if(IsHovered && CurrentInput.ScrollDelta != 0)
				Scroll.Move(CurrentInput.ScrollDelta);

			TryClickOnItem();

			UpdateParts();
			UpdateItems();
		}

		public static implicit operator List(Checkbox[] items)
		{
			var result = new List((0, 0), (5, 5), 0);
			for(int i = 0; i < items?.Length; i++)
				result.items[i] = items[i];

			return result;
		}
		public static implicit operator Checkbox[](List list) => list.items.ToArray();

		#region Backend
		private readonly List<Checkbox> items = new();
		private bool isSingleSelecting;

		private void TrySingleSelectOneItem()
		{
			if(IsSingleSelecting == false || items.Count == 0)
				return;

			var isOneSelected = false;
			for(int i = 0; i < items.Count; i++)
				isOneSelected |= items[i].IsChecked;

			items[0].IsChecked = isOneSelected == false;
		}
		private void TryClickOnItem()
		{
			var isHoveringItems = IsHovered && Scroll.IsHovered == false &&
				ScrollUp.IsHovered == false && ScrollDown.IsHovered == false;
			var hoveredIndex = (int)CurrentInput.Position.Item2 - Position.Item2 + GetScrollIndex();

			if(CurrentInput.IsJustReleased && isHoveringItems && IsSingleSelecting)
				for(int i = 0; i < items.Count; i++)
					if(i != hoveredIndex)
						items[i].IsChecked = false;
		}

		private void UpdateParts()
		{
			var (x, y) = Position;
			var (w, h) = Size;

			Scroll.Position = (x + w - 1, y + 1);
			Scroll.Size = (1, h - 2);
			Scroll.IsVertical = true;

			ScrollUp.Position = (x + w - 1, y);
			ScrollUp.Size = (1, 1);

			ScrollDown.Position = (x + w - 1, y + h - 1);
			ScrollDown.Size = (1, 1);

			Scroll.Update();
			ScrollUp.Update();
			ScrollDown.Update();
		}
		private void UpdateItems()
		{
			var (x, y) = Position;
			var index = GetScrollIndex();
			var max = Math.Min(items.Count, index + Size.Item2);
			for(int i = 0; i < items.Count; i++)
			{
				var item = items[i];

				if(i < index || i >= max)
				{
					item.Position = (int.MaxValue, int.MaxValue);
					continue;
				}

				item.Position = (x, y + (i - index));
				item.Size = (Size.Item1 - 1, 1);

				var wasChecked = item.IsChecked;
				item.Update();

				if(wasChecked && item.IsChecked == false && IsSingleSelecting)
					item.IsChecked = true;
			}
		}

		private int GetScrollIndex()
		{
			var end = Math.Max(0, items.Count - Scroll.Size.Item2 - 2);
			var index = (int)MathF.Round(Map(Scroll.Progress, 0, 1, 0, end));
			return index;
		}

		private static float Map(float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		#endregion
	}
}
