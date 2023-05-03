namespace Pure.UserInterface;

/// <summary>
/// Represents a user input panel element that can be moved and resized by the user (like a window).
/// </summary>
public class Panel : Element
{
	/// <summary>
	/// Gets or sets a value indicating whether this panel can be resized by the user.
	/// </summary>
	public bool IsResizable { get; set; } = true;
	/// <summary>
	/// Gets or sets a value indicating whether this panel can be moved by the user.
	/// </summary>
	public bool IsMovable { get; set; } = true;
	/// <summary>
	/// Gets or sets the minimum additional size that this panel can have beyond its current size.
	/// </summary>
	public (int width, int height) AdditionalMinimumSize { get; set; }

	/// <summary>
	/// Initializes a new panel instance with the specified position and size.
	/// </summary>
	/// <param name="position">The position of the panel.</param>
	/// <param name="size">The size of the panel.</param>
	public Panel((int x, int y) position, (int width, int height) size) : base(position, size) { }

	/// <summary>
	/// Called when the panel needs to be updated. This handles all of the user input
	/// the panel needs for its behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		if (IsDisabled || IsResizable == false && IsMovable == false)
			return;

		var (x, y) = Position;
		var (w, h) = Size;
		var (ix, iy) = Input.Current.Position;
		var (px, py) = Input.Current.PositionPrevious;
		var isClicked = Input.Current.IsPressed && Input.Current.wasPressed == false;
		var wasClicked = Input.Current.IsPressed == false && Input.Current.wasPressed;

		ix = MathF.Floor(ix);
		iy = MathF.Floor(iy);
		px = MathF.Floor(px);
		py = MathF.Floor(py);

		var isHoveringTop = IsBetween(ix, x + 1, x + w - 2) && iy == y;
		var isHoveringTopCorners = (x, y) == (ix, iy) || (x + w - 1, y) == (ix, iy);
		var isHoveringLeft = ix == x && IsBetween(iy, y, y + h - 1);
		var isHoveringRight = ix == x + w - 1 && IsBetween(iy, y, y + h - 1);
		var isHoveringBottom = IsBetween(ix, x, x + w - 1) && iy == y + h - 1;

		if (IsDisabled == false && IsHovered)
			MouseCursorResult = MouseCursor.Arrow;

		if (wasClicked)
		{
			isDragging = false;
			isResizingL = false;
			isResizingR = false;
			isResizingU = false;
			isResizingD = false;
		}

		if (IsMovable && isHoveringTop)
			Process(ref isDragging, MouseCursor.Move);
		else if (IsResizable)
		{
			if (isHoveringLeft)
				Process(ref isResizingL, MouseCursor.ResizeHorizontal);
			if (isHoveringRight)
				Process(ref isResizingR, MouseCursor.ResizeHorizontal);
			if (isHoveringBottom)
				Process(ref isResizingD, MouseCursor.ResizeVertical);
			if (isHoveringTopCorners)
				Process(ref isResizingU, MouseCursor.ResizeVertical);

			var tl = isHoveringLeft && isHoveringTopCorners;
			var tr = isHoveringRight && isHoveringTopCorners;
			var br = isHoveringBottom && isHoveringRight;
			var bl = isHoveringBottom && isHoveringLeft;

			if (IsDisabled == false && (tr || bl))
				MouseCursorResult = MouseCursor.ResizeDiagonal1;
			if (IsDisabled == false && (tl || br))
				MouseCursorResult = MouseCursor.ResizeDiagonal2;
		}

		if (IsFocused && Input.Current.IsPressed &&
			Input.Current.Position != Input.Current.PositionPrevious)
		{
			var (dx, dy) = ((int)ix - (int)px, (int)iy - (int)py);
			var (newX, newY) = (x, y);
			var (newW, newH) = (w, h);
			var (maxX, maxY) = AdditionalMinimumSize;

			if (isDragging && IsBetween(ix, x + 1 + dx, x + w - 2 + dx) && iy == y + dy)
			{
				newX += dx;
				newY += dy;
			}
			if (isResizingL && ix == x + dx)
			{
				newX += dx;
				newW -= dx;
			}
			if (isResizingR && ix == x + w - 1 + dx)
				newW += dx;
			if (isResizingD && iy == y + h - 1 + dy)
				newH += dy;
			if (isResizingU && iy == y + dy)
			{
				newY += dy;
				newH -= dy;
			}

			if (newW < 2 + Math.Abs(maxX) ||
				newH < 2 + Math.Abs(maxY) ||
				newX < 0 ||
				newY < 0 ||
				newX + newW > TilemapSize.Item1 ||
				newY + newH > TilemapSize.Item2)
				return;

			Size = (newW, newH);
			Position = (newX, newY);
		}

		void Process(ref bool condition, MouseCursor cursor)
		{
			if (isClicked)
				condition = true;

			if (IsDisabled == false)
				MouseCursorResult = cursor;
		}
	}

	#region Backend
	private bool isDragging, isResizingL, isResizingR, isResizingU, isResizingD;

	private static bool IsBetween(float number, float rangeA, float rangeB)
	{
		if (rangeA > rangeB)
			(rangeA, rangeB) = (rangeB, rangeA);

		var l = rangeA <= number;
		var u = rangeB >= number;
		return l && u;
	}
	#endregion
}
