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

		#region Backend
		protected override void OnUpdate()
		{
			if(IsHovered)
				SetTileAndSystemCursor(TILE_HAND);

			var size = IsVertical ? Size.Item2 : Size.Item1;
			var index = (int)Math.Clamp(MathF.Round(size * Progress), 0, size - 1);
			var (x, y) = Position;
			var (w, h) = Size;
			var hasJustMoved = Input.Position != Input.prevPosition;
			var (px, py) = ((int)Input.Position.Item1, (int)Input.Position.Item2);

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

			if(IsClicked)
			{
				if(IsVertical)
				{
					var newY = Math.Clamp(py, y, y + size - 1);
					Handle.Position = (Handle.Position.Item1, newY);
					Progress = (newY - y) / (float)size;
				}
				else
				{
					var newX = Math.Clamp(px, x, x + size - 1);
					Handle.Position = (newX, Handle.Position.Item2);
					Progress = (newX - x) / (float)size;
				}
			}
		}
		#endregion
	}
}
