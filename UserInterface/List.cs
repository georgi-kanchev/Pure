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

		public Checkbox? this[int index] => HasIndex(index) ? items[index] : default;

		public List((int, int) position, (int, int) size, int count)
			: base(position, size)
		{
			var (x, y) = Position;
			var (w, h) = Size;

			Scroll = new((x + w - 1, y + 1), h - 2, true);
			ScrollUp = new((x + w, y), (1, 1));
			ScrollDown = new((x + w, y + w), (1, 1));

			Add(count);

			ScrollUp.On(UserAction.Press, () => Scroll.Move(1));
			ScrollUp.On(UserAction.Hold, () => Scroll.Move(1));
			ScrollDown.On(UserAction.Press, () => Scroll.Move(-1));
			ScrollDown.On(UserAction.Hold, () => Scroll.Move(-1));

			UpdateParts();
			UpdateItems();
		}

		public void Add(int count = 1)
		{
			for(int i = 0; i < count; i++)
				items.Add(new(Position) { Size = (Size.Item1, 1) });

			TrySingleSelectOneItem();
		}
		public void Remove(int index)
		{
			if(HasIndex(index) == false)
				return;

			items.RemoveAt(index);

			if(index == singleSelectedIndex && index != items.Count)
				return;
			else if(index <= singleSelectedIndex)
				singleSelectedIndex--;

			TrySingleSelectOneItem();
		}
		public void Clear()
		{
			items.Clear();
			singleSelectedIndex = -1;
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

			TrySingleSelect();

			UpdateParts();
			UpdateItems();
		}

		public static implicit operator List(Checkbox[] items)
		{
			var result = new List((0, 0), (10, Math.Max(items.Length, 5)), 0);
			for(int i = 0; i < items?.Length; i++)
				result.items[i] = items[i];

			return result;
		}
		public static implicit operator Checkbox[](List list) => list.items.ToArray();

		#region Backend
		private int singleSelectedIndex = -1;
		private readonly List<Checkbox> items = new();
		private bool isSingleSelecting;

		private void TrySingleSelectOneItem()
		{
			if(IsSingleSelecting == false || items.Count == 0)
			{
				singleSelectedIndex = -1;
				return;
			}

			var isOneSelected = HasIndex(singleSelectedIndex);

			if(isOneSelected)
				return;

			singleSelectedIndex = 0;
			items[singleSelectedIndex].IsChecked = true;
		}
		private void TrySingleSelect()
		{
			var isHoveringItems = IsHovered && Scroll.IsHovered == false &&
				ScrollUp.IsHovered == false && ScrollDown.IsHovered == false;

			if(CurrentInput.IsJustReleased == false ||
				IsSingleSelecting == false || isHoveringItems == false)
				return;

			var hoveredIndex = (int)CurrentInput.Position.Item2 - Position.Item2 + GetScrollIndex();

			if(hoveredIndex == singleSelectedIndex || HasIndex(hoveredIndex) == false)
				return;

			if(items[hoveredIndex].IsClicked)
				singleSelectedIndex = hoveredIndex;
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
			var top = GetScrollIndex();
			var bottom = Math.Min(items.Count, top + Size.Item2);
			for(int i = 0; i < items.Count; i++)
			{
				var item = items[i];

				if(i < top || i >= bottom)
				{
					item.Position = (int.MaxValue, int.MaxValue);
					continue;
				}

				item.Position = (x, y + (i - top));
				item.Size = (Size.Item1 - 1, 1);
				item.Update();

				if(IsSingleSelecting)
					item.IsChecked = false;
			}

			if(IsSingleSelecting && HasIndex(singleSelectedIndex))
				items[singleSelectedIndex].IsChecked = true;
		}

		private int GetScrollIndex()
		{
			var end = Math.Max(0, items.Count - Scroll.Size.Item2 - 2);
			var index = (int)MathF.Round(Map(Scroll.Progress, 0, 1, 0, end));
			return index;
		}
		private bool HasIndex(int index)
		{
			return index >= 0 && index < items.Count;
		}

		private static float Map(float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		#endregion
	}
}
