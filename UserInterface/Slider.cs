namespace Pure.UserInterface
{
	public class Slider : UserInterface
	{
		public bool IsVertical { get; set; }
		public float Progress { get; set; }

		public Button Handle { get; }

		public Slider((int, int) position, int size = 5, bool isVertical = false)
			: base(position, isVertical ? (1, size) : (size, 1))
		{
			IsVertical = isVertical;
			Handle = new(position, (1, 1));
		}

		public void Scroll(int delta)
		{
			var size = IsVertical ? Size.Item2 : Size.Item1;
			var index = (int)Math.Clamp(MathF.Round(size * Progress), 0, size - 1);
			index += delta * (IsVertical ? -1 : 1);
			index = Math.Clamp(index, 0, size - 1);

			Progress = index / (float)size;
			UpdateHandle();
		}
		public void ScrollTo((int, int) position)
		{
			var size = IsVertical ? Size.Item2 : Size.Item1;
			var (x, y) = Position;
			var (px, py) = position;

			Progress = IsVertical ?
				(Math.Clamp(py, y, y + size - 1) - y) / (float)size :
				(Math.Clamp(px, x, x + size - 1) - x) / (float)size;
			UpdateHandle();
		}

		#region Backend
		protected override void OnUpdate()
		{
			if(IsHovered)
			{
				SetTileAndSystemCursor(TILE_HAND);
				Scroll(CurrentInput.ScrollDelta);
			}

			if(IsClicked)
			{
				var p = CurrentInput.Position;
				ScrollTo(((int)p.Item1, (int)p.Item2));
			}
		}

		private void UpdateHandle()
		{
			var (x, y) = Position;
			var (w, h) = Size;
			var size = IsVertical ? Size.Item2 : Size.Item1;
			var index = (int)Math.Clamp(MathF.Round(size * Progress), 0, size - 1);
			if(IsVertical)
			{
				Handle.Position = (x, y + index);
				Handle.Size = (w, 1);
			}
			else
			{
				Handle.Position = (x + index, y);
				Handle.Size = (1, h);
			}
		}
		#endregion
	}
}
