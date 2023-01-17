namespace Pure.UserInterface
{
	public class Panel : UserInterface
	{
		public bool IsResizable { get; set; } = true;
		public bool IsMovable { get; set; } = true;
		public (int, int) AdditionalMinSize { get; set; }

		public Panel((int, int) position, (int, int) size) : base(position, size) { }

		protected override void OnUpdate()
		{
			if(IsDisabled || IsResizable == false && IsMovable == false)
				return;

			var (x, y) = Position;
			var (w, h) = Size;
			var (ix, iy) = CurrentInput.Position;
			var (px, py) = CurrentInput.PositionPrevious;
			var isClicked = CurrentInput.IsPressed && CurrentInput.wasPressed == false;
			var wasClicked = CurrentInput.IsPressed == false && CurrentInput.wasPressed;

			ix = MathF.Floor(ix);
			iy = MathF.Floor(iy);
			px = MathF.Floor(px);
			py = MathF.Floor(py);

			var isHoveringTop = IsBetween(ix, x + 1, x + w - 2) && iy == y;
			var isHoveringTopCorners = (x, y) == (ix, iy) || (x + w - 1, y) == (ix, iy);
			var isHoveringLeft = ix == x && IsBetween(iy, y, y + h - 1);
			var isHoveringRight = ix == x + w - 1 && IsBetween(iy, y, y + h - 1);
			var isHoveringBottom = IsBetween(ix, x, x + w - 1) && iy == y + h - 1;

			if(IsHovered)
				TrySetTileAndSystemCursor(TILE_ARROW);

			if(wasClicked)
			{
				isDragging = false;
				isResizingL = false;
				isResizingR = false;
				isResizingU = false;
				isResizingD = false;
			}

			if(IsMovable && isHoveringTop)
				Process(ref isDragging, TILE_MOVE);
			else if(IsResizable)
			{
				if(isHoveringLeft)
					Process(ref isResizingL, TILE_RESIZE_HORIZONTAL);
				if(isHoveringRight)
					Process(ref isResizingR, TILE_RESIZE_HORIZONTAL);
				if(isHoveringBottom)
					Process(ref isResizingD, TILE_RESIZE_VERTICAL);
				if(isHoveringTopCorners)
					Process(ref isResizingU, TILE_RESIZE_VERTICAL);

				if((isHoveringRight && isHoveringTopCorners) || (isHoveringBottom && isHoveringLeft))
					TrySetTileAndSystemCursor(TILE_RESIZE_DIAGONAL_1);
				if((isHoveringLeft && isHoveringTopCorners) || (isHoveringBottom && isHoveringRight))
					TrySetTileAndSystemCursor(TILE_RESIZE_DIAGONAL_2);
			}

			if(IsFocused && CurrentInput.IsPressed &&
				CurrentInput.Position != CurrentInput.PositionPrevious)
			{
				var (dx, dy) = ((int)ix - (int)px, (int)iy - (int)py);
				var (newX, newY) = (x, y);
				var (newW, newH) = (w, h);
				var (maxX, maxY) = AdditionalMinSize;

				if(isDragging && IsBetween(ix, x + 1 + dx, x + w - 2 + dx) && iy == y + dy)
				{
					newX += dx;
					newY += dy;
				}
				if(isResizingL && ix == x + dx)
				{
					newX += dx;
					newW -= dx;
				}
				if(isResizingR && ix == x + w - 1 + dx)
					newW += dx;
				if(isResizingD && iy == y + h - 1 + dy)
					newH += dy;
				if(isResizingU && iy == y + dy)
				{
					newY += dy;
					newH -= dy;
				}

				if(newW < 2 + Math.Abs(maxX) ||
					newH < 2 + Math.Abs(maxY) ||
					newX < 0 ||
					newY < 0 ||
					newX + newW > TilemapSize.Item1 ||
					newY + newH > TilemapSize.Item2)
					return;

				Size = (newW, newH);
				Position = (newX, newY);
			}

			void Process(ref bool condition, int cursor)
			{
				if(isClicked)
					condition = true;

				TrySetTileAndSystemCursor(cursor);
			}
		}

		#region Backend
		private bool isDragging, isResizingL, isResizingR, isResizingU, isResizingD;

		private static bool IsBetween(float number, float rangeA, float rangeB)
		{
			if(rangeA > rangeB)
				(rangeA, rangeB) = (rangeB, rangeA);

			var l = rangeA <= number;
			var u = rangeB >= number;
			return l && u;
		}
		#endregion
	}
}
