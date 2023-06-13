using System.Xml.Serialization;

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
	public (int width, int height) MinimumSize
	{
		get => minimumSize;
		set
		{
			value.width = Math.Max(value.width, 3);
			value.height = Math.Max(value.height, 3);

			minimumSize = value;
		}
	}

	/// <summary>
	/// Initializes a new panel instance with the specified position and size.
	/// </summary>
	/// <param name="position">The position of the panel.</param>
	/// <param name="size">The size of the panel.</param>
	public Panel((int x, int y) position) : base(position)
	{
		Text = "Panel";
		Size = (12, 8);
	}
	public Panel(byte[] bytes) : base(bytes)
	{
		MinimumSize = (GrabInt(bytes), GrabInt(bytes));
		IsResizable = GrabBool(bytes);
		IsMovable = GrabBool(bytes);
	}

	public override byte[] ToBytes()
	{
		var result = base.ToBytes().ToList();
		PutInt(result, MinimumSize.width);
		PutInt(result, MinimumSize.height);
		PutBool(result, IsResizable);
		PutBool(result, IsMovable);
		return result.ToArray();
	}

	/// <summary>
	/// Called when the panel needs to be updated. This handles all of the user input
	/// the panel needs for its behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
		if (MinimumSize.width > Size.width)
			Size = (MinimumSize.width, Size.height);
		if (MinimumSize.height > Size.height)
			Size = (Size.width, MinimumSize.height);

		if (IsDisabled || IsResizable == false && IsMovable == false)
			return;

		var (x, y) = Position;
		var (w, h) = Size;
		var (inputX, inputY) = Input.Current.Position;
		var (prevX, prevY) = Input.Current.PositionPrevious;
		var isClicked = Input.Current.IsPressed && Input.Current.wasPressed == false;
		var wasClicked = Input.Current.IsPressed == false && Input.Current.wasPressed;

		inputX = MathF.Floor(inputX);
		inputY = MathF.Floor(inputY);
		prevX = MathF.Floor(prevX);
		prevY = MathF.Floor(prevY);

		var tlOff = IsResizable ? 1 : 0;
		var trOff = IsResizable ? 2 : 1;
		var isHoveringTop = IsBetween(inputX, x + tlOff, x + w - trOff) && inputY == y;
		var isHoveringTopCorners = (x, y) == (inputX, inputY) || (x + w - 1, y) == (inputX, inputY);
		var isHoveringLeft = inputX == x && IsBetween(inputY, y, y + h - 1);
		var isHoveringRight = inputX == x + w - 1 && IsBetween(inputY, y, y + h - 1);
		var isHoveringBottom = IsBetween(inputX, x, x + w - 1) && inputY == y + h - 1;

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

		var isHoveringSides = isHoveringTop || ((isHoveringLeft || isHoveringRight || isHoveringBottom) &&
			IsResizable == false);
		if (IsMovable && isHoveringSides)
			Process(ref isDragging, MouseCursor.Move);
		else if (IsResizable)
		{
			if (isHoveringLeft)
				Process(ref isResizingL, MouseCursor.ResizeHorizontal);
			if (isHoveringRight)
				Process(ref isResizingR, MouseCursor.ResizeHorizontal);
			if (isHoveringBottom)
				Process(ref isResizingD, MouseCursor.ResizeVertical);
			if (isHoveringTopCorners || (IsMovable == false && isHoveringTop))
				Process(ref isResizingU, MouseCursor.ResizeVertical);

			var tl = isHoveringLeft && isHoveringTopCorners;
			var tr = isHoveringRight && isHoveringTopCorners;
			var br = isHoveringBottom && isHoveringRight;
			var bl = isHoveringBottom && isHoveringLeft;

			if (IsDisabled == false && (tl || br))
				MouseCursorResult = MouseCursor.ResizeDiagonal1;
			if (IsDisabled == false && (tr || bl))
				MouseCursorResult = MouseCursor.ResizeDiagonal2;
		}

		if (IsFocused && Input.Current.IsPressed &&
			Input.Current.Position != Input.Current.PositionPrevious)
		{
			var (deltaX, deltaY) = ((int)inputX - (int)prevX, (int)inputY - (int)prevY);
			var (newX, newY) = (x, y);
			var (newW, newH) = (w, h);
			var (maxX, maxY) = MinimumSize;

			if (isDragging)
			{
				newX += deltaX;
				newY += deltaY;
			}
			if (isResizingL && inputX == x + deltaX) { newX += deltaX; newW -= deltaX; }
			if (isResizingR && inputX == x + w - 1 + deltaX) newW += deltaX;
			if (isResizingD && inputY == y + h - 1 + deltaY) newH += deltaY;
			if (isResizingU && inputY == y + deltaY) { newY += deltaY; newH -= deltaY; }

			var isOutsideScreen =
				newX + newW > TilemapSize.width ||
				newY + newH > TilemapSize.height ||
				newX < 0 ||
				newY < 0;
			var isBelowMinimumSize = newW < Math.Abs(maxX) || newH < Math.Abs(maxY);

			if (isOutsideScreen || isBelowMinimumSize)
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
	private (int, int) minimumSize = (2, 2);

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
