namespace Pure.UserInterface
{
	public class Container : UserInterface
	{
		public bool IsResizable { get; set; } = true;
		public bool IsMovable { get; set; } = true;

		public Container((int, int) position, (int, int) size) : base(position, size) { }

		public void Update(Action<Container>? custom)
		{
			TryTrigger();

			if(IsClicked)
				IsFocused = true;

			if(IsResizable == false && IsMovable == false)
				return;

			var (x, y) = Position;
			var (w, h) = Size;
			var (ix, iy) = Input.Position;
			var (px, py) = Input.PrevPosition;
			var isClicked = Input.IsPressed && Input.WasPressed == false;
			var wasClicked = Input.IsPressed == false && Input.WasPressed;

			ix = MathF.Floor(ix);
			iy = MathF.Floor(iy);
			px = MathF.Floor(px);
			py = MathF.Floor(py);

			var isHoveringTop = IsBetween(ix, x + 1, x + w - 2) && iy == y;
			var isHoveringTopCorners = (x, y) == (ix, iy) || (x + w - 1, y) == (ix, iy);
			var isHoveringLeft = ix == x && IsBetween(iy, y, y + h - 1);
			var isHoveringRight = ix == x + w - 1 && IsBetween(iy, y, y + h - 1);
			var isHoveringBottom = IsBetween(ix, x, x + w - 1) && iy == y + h - 1;

			if(wasClicked)
			{
				isDragging = false;
				isResizingL = false;
				isResizingR = false;
				isResizingU = false;
				isResizingD = false;
			}

			if(IsMovable && isHoveringTop)
				Process(ref isDragging, CursorResult.TileMove);
			else if(IsResizable)
			{
				if(isHoveringLeft)
					Process(ref isResizingL, CursorResult.TileResizeHorizontal);
				if(isHoveringRight)
					Process(ref isResizingR, CursorResult.TileResizeHorizontal);
				if(isHoveringBottom)
					Process(ref isResizingD, CursorResult.TileResizeVertical);
				if(isHoveringTopCorners)
					Process(ref isResizingU, CursorResult.TileResizeVertical);

				if((isHoveringRight && isHoveringTopCorners) || (isHoveringBottom && isHoveringLeft))
					SetTileAndSystemCursor(CursorResult.TileResizeDiagonal1);
				if((isHoveringLeft && isHoveringTopCorners) || (isHoveringBottom && isHoveringRight))
					SetTileAndSystemCursor(CursorResult.TileResizeDiagonal2);
			}

			if(Input.IsPressed && Input.Position != Input.PrevPosition)
			{
				var (dx, dy) = ((int)ix - (int)px, (int)iy - (int)py);
				var (newX, newY) = (x, y);
				var (newW, newH) = (w, h);

				if(isDragging)
				{
					newX += dx;
					newY += dy;
				}
				if(isResizingL)
				{
					newX += dx;
					newW -= dx;
				}
				if(isResizingR)
					newW += dx;
				if(isResizingD)
					newH += dy;
				if(isResizingU)
				{
					newY += dy;
					newH -= dy;
				}

				Position = (newX, newY);
				Size = (newW, newH);
			}

			custom?.Invoke(this);

			void Process(ref bool condition, CursorResult cursor)
			{
				if(isClicked)
					condition = true;

				SetTileAndSystemCursor(cursor);
			}
		}

		#region Backend
		private static bool isDragging, isResizingL, isResizingR, isResizingU, isResizingD;

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
