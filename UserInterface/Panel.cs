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
    /// Gets or sets a value indicating whether this panel can be moved or resized by the user 
    /// outside of the tilemap.
    /// </summary>
    public bool IsRestricted { get; set; } = true;

    /// <summary>
    /// Initializes a new panel instance with the specified position.
    /// </summary>
    /// <param name="position">The position of the panel.</param>
    public Panel((int x, int y) position) : base(position)
    {
        Size = (12, 8);
    }
    public Panel(byte[] bytes) : base(bytes)
    {
        SizeMinimum = (GrabInt(bytes), GrabInt(bytes));
        IsResizable = GrabBool(bytes);
        IsMovable = GrabBool(bytes);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutInt(result, SizeMinimum.width);
        PutInt(result, SizeMinimum.height);
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
        LimitSizeMin((2, 2));

        if (IsDisabled || (IsResizable == false && IsMovable == false))
            return;

        var (x, y) = Position;
        var (w, h) = Size;
        var (inputX, inputY) = Input.Current.Position;
        var (prevX, prevY) = Input.Current.PositionPrevious;
        var isClicked = Input.Current.IsPressed && Input.Current.WasPressed == false;
        var wasClicked = Input.Current.IsPressed == false && Input.Current.WasPressed;

        inputX = MathF.Floor(inputX);
        inputY = MathF.Floor(inputY);
        prevX = MathF.Floor(prevX);
        prevY = MathF.Floor(prevY);

        const float e = 0.01f;
        var tlOff = IsResizable ? 1 : 0;
        var trOff = IsResizable ? 2 : 1;
        var isHoveringTop = IsBetween(inputX, x + tlOff, x + w - trOff) && Math.Abs(inputY - y) < e;
        var isHoveringTopCorners = (x, y) == (inputX, inputY) || (x + w - 1, y) == (inputX, inputY);
        var isHoveringLeft = Math.Abs(inputX - x) < e && IsBetween(inputY, y, y + h - 1);
        var isHoveringRight = Math.Abs(inputX - (x + w - 1)) < e && IsBetween(inputY, y, y + h - 1);
        var isHoveringBottom = IsBetween(inputX, x, x + w - 1) && Math.Abs(inputY - (y + h - 1)) < e;
        var isHoveringInside = IsHovered && isHoveringTop == false &&
                               isHoveringLeft == false && isHoveringRight == false &&
                               isHoveringBottom == false;

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

        var isHoveringSides = isHoveringInside || isHoveringTop || ((isHoveringLeft || isHoveringRight ||
                                                                     isHoveringBottom) &&
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
            var (maxX, maxY) = SizeMinimum;

            if (deltaX == 0 && deltaY == 0 || FocusedPrevious != this)
                return;

            if (isDragging)
            {
                newX += deltaX;
                newY += deltaY;
            }

            if (isResizingL && Math.Abs(inputX - (x + deltaX)) < e)
            {
                newX += deltaX;
                newW -= deltaX;
            }

            if (isResizingR && Math.Abs(inputX - (x + w - 1 + deltaX)) < e) newW += deltaX;
            if (isResizingD && Math.Abs(inputY - (y + h - 1 + deltaY)) < e) newH += deltaY;
            if (isResizingU && Math.Abs(inputY - (y + deltaY)) < e)
            {
                newY += deltaY;
                newH -= deltaY;
            }

            var isOutsideScreen =
                newX + newW > TilemapSize.width ||
                newY + newH > TilemapSize.height ||
                newX < 0 ||
                newY < 0;
            var isBelowMinimumSize = newW < Math.Abs(maxX) || newH < Math.Abs(maxY);
            var moveNoResize = isDragging == false &&
                               (isResizingD || isResizingL || isResizingR || isResizingU);

            if (isBelowMinimumSize || (isOutsideScreen && IsRestricted))
                return;

            var prevSz = Size;
            var prevPos = Position;
            Size = (newW, newH);
            Position = (newX, newY);

            // sometimes resizing causes only drag because of the nature of
            // limited size for example - so prevent it
            if (moveNoResize && prevSz == Size && prevPos != Position)
                Position = (x, y);
        }

        void Process(ref bool condition, MouseCursor cursor)
        {
            if (isClicked && IsFocused)
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