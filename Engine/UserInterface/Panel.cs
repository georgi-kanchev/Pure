namespace Pure.Engine.UserInterface;

/// <summary>
/// Can be moved and resized by the user (like a window).
/// </summary>
public class Panel : Block
{
    [DoNotSave]
    public Action<(int deltaWidth, int deltaHeight)>? OnResize { get; set; }

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
    /// outside the tilemap.
    /// </summary>
    public bool IsRestricted { get; set; } = true;

    public Panel() : this((0, 0))
    {
    }
    /// <summary>
    /// Initializes a new panel instance with the specified position.
    /// </summary>
    /// <param name="position">The position of the panel.</param>
    public Panel(PointI position) : base(position)
    {
        Size = (12, 8);
        OnUpdate += OnRefresh;
    }

#region Backend
    private bool isMoving, isResizingL, isResizingR, isResizingU, isResizingD;
    private PointI startBotR;

    private void OnRefresh()
    {
        LimitSizeMin(IsResizable ? (2, 2) : (1, 1));
    }

    protected override void OnInput()
    {
        if (IsResizable == false && IsMovable == false)
            return;

        var (x, y) = Position;
        var (w, h) = Size;
        var (inputX, inputY) = Input.Position;
        var (prevX, prevY) = Input.PositionPrevious;

        inputX = MathF.Floor(inputX);
        inputY = MathF.Floor(inputY);
        prevX = MathF.Floor(prevX);
        prevY = MathF.Floor(prevY);

        var tlOff = IsResizable ? 1 : 0;
        var trOff = IsResizable ? 2 : 1;
        const float E = 0.01f;
        var isHovCornersT = (x, y) == (inputX, inputY) || (x + w - 1, y) == (inputX, inputY);
        var isHovT = IsBetween(inputX, x + tlOff, x + w - trOff) && Math.Abs(inputY - y) < E;
        var isHovL = Math.Abs(inputX - x) < E && IsBetween(inputY, y, y + h - 1);
        var isHovR = Math.Abs(inputX - (x + w - 1)) < E && IsBetween(inputY, y, y + h - 1);
        var isHovB = IsBetween(inputX, x, x + w - 1) && Math.Abs(inputY - (y + h - 1)) < E;
        var isHovInside = IsHovered &&
                          isHovT == false &&
                          isHovL == false &&
                          isHovR == false &&
                          isHovB == false;
        var isHoveringSides = isHovInside ||
                              isHovT ||
                              ((isHovL || isHovR || isHovB) &&
                               IsResizable == false);

        TrySetCursorAndCache(isHoveringSides, isHovL, isHovR, isHovB, isHovCornersT, isHovT);

        if (IsFocused == false ||
            Input.IsButtonPressed() == false ||
            Input.Position == Input.PositionPrevious)
            return;

        TryMoveAndResize(inputX, inputY, prevX, prevY);
    }

    private void TrySetCursorAndCache(
        bool isHoveringSides,
        bool isHoveringLeft,
        bool isHoveringRight,
        bool isHoveringBottom,
        bool isHoveringTopCorners,
        bool isHoveringTop)
    {
        if (IsDisabled == false && IsHovered)
            Input.CursorResult = MouseCursor.Arrow;

        if (Input.IsButtonJustReleased())
        {
            isMoving = false;
            isResizingL = false;
            isResizingR = false;
            isResizingU = false;
            isResizingD = false;
        }

        if (IsMovable && isHoveringSides)
            Process(ref isMoving, MouseCursor.Move);
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
                Input.CursorResult = MouseCursor.ResizeDiagonal1;
            if (IsDisabled == false && (tr || bl))
                Input.CursorResult = MouseCursor.ResizeDiagonal2;
        }

        void Process(ref bool condition, MouseCursor cursor)
        {
            if (Input.IsButtonJustPressed() && IsFocused)
            {
                condition = true;
                startBotR = (Position.x + Size.width, Position.y + Size.height);
            }

            if (IsDisabled == false)
                Input.CursorResult = cursor;
        }
    }
    private void TryMoveAndResize(float inputX, float inputY, float prevX, float prevY)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (deltaX, deltaY) = ((int)inputX - (int)prevX, (int)inputY - (int)prevY);
        var (newX, newY) = (x, y);
        var (newW, newH) = (w, h);
        var (minX, minY) = SizeMinimum;

        if (Input.FocusedPrevious != this)
            return;

        if (isMoving)
        {
            newX += deltaX;
            newY += deltaY;
        }

        if (isResizingR)
        {
            newW = (int)inputX - x + 1;

            if (inputX <= x)
                return;
        }

        if (isResizingD)
        {
            newH = (int)inputY - y + 1;

            if (inputY <= y)
                return;
        }

        if (isResizingL)
        {
            newW = startBotR.x - (int)inputX;
            newX = (int)inputX;

            if (inputX >= startBotR.x - 1)
                return;
        }

        if (isResizingU)
        {
            newH = startBotR.y - (int)inputY;
            newY = (int)inputY;

            if (inputY >= startBotR.y - 1)
                return;
        }

        var isOutsideScreen =
            newX + newW > Input.TilemapSize.width ||
            newY + newH > Input.TilemapSize.height ||
            newX < 0 ||
            newY < 0;
        var isBelowMinimumSize = newW < Math.Abs(minX) || newH < Math.Abs(minY);

        if (isBelowMinimumSize || (isOutsideScreen && IsRestricted))
            return;

        var prevSz = Size;
        Size = (newW, newH);
        Position = (newX, newY);

        if (prevSz == Size)
            return;

        var delta = (Size.width - prevSz.width, Size.height - prevSz.height);
        OnResize?.Invoke(delta);
    }

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