namespace Pure.UserInterface;

using System.Diagnostics;
using System.Text;

/// <summary>
/// A user interface element for accepting text input from the user.
/// </summary>
public class InputBox : Element
{
    /// <summary>
    /// The text displayed in the input box when it is empty.
    /// </summary>
    public string Placeholder { get; set; } = "Type…";

    public string Selection
    {
        get
        {
            var sb = new StringBuilder();
            var cursorIndex = IndicesToIndex(CursorIndices);
            var selectionIndex = IndicesToIndex(SelectionIndices);
            var a = cursorIndex < selectionIndex ? CursorIndices : SelectionIndices;
            var b = cursorIndex > selectionIndex ? CursorIndices : SelectionIndices;

            var targetY = Math.Min(scrY + Size.height, lines.Count);

            for (var i = scrY; i < targetY; i++)
            {
                for (var j = scrX; j < scrX + Size.width; j++)
                {
                    var isBeforeEndOfLine = j < lines[i].Length;
                    var isBetweenVert = i > a.line && i < b.line;
                    var isBetweenHor = j >= a.symbol && j < b.symbol;
                    var isSelected = isBetweenVert;

                    if (i == a.line && a.line == b.line)
                        isSelected |= isBetweenHor;
                    else if (i == a.line)
                        isSelected |= j >= a.symbol;
                    else if (i == b.line)
                        isSelected |= j < b.symbol;

                    isSelected &= isBeforeEndOfLine;

                    sb.Append(isSelected ? SELECTION : SPACE);
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }
    }
    public (int symbol, int line) SelectionIndices
    {
        get => (sx, sy);
        set
        {
            var (x, y) = value;
            sy = Math.Clamp(y, 0, lines.Count - 1);
            sx = Math.Clamp(x, 0, lines[sy].Length);
        }
    }
    /// <summary>
    /// Gets the currently selected text in the input box.
    /// </summary>
    public string SelectedText
    {
        get
        {
            if (SelectionIndices == CursorIndices)
                return "";

            var sb = new StringBuilder();
            var cursorIndex = IndicesToIndex(CursorIndices);
            var selectionIndex = IndicesToIndex(SelectionIndices);
            var a = cursorIndex < selectionIndex ? cursorIndex : selectionIndex;
            var b = cursorIndex > selectionIndex ? cursorIndex : selectionIndex;

            for (var i = a; i < b; i++)
            {
                var (ix, iy) = IndicesFromIndex(i);
                var symbol = GetSymbol((ix, iy));

                if (symbol == default && ix == lines[iy].Length)
                {
                    sb.Append(Environment.NewLine);
                    continue;
                }

                if (symbol != default)
                    sb.Append(symbol);
            }

            return sb.ToString();
        }
    }

    public bool IsCursorVisible =>
        IsFocused &&
        cursorBlink.Elapsed.TotalSeconds <= CURSOR_BLINK / 2f &&
        IsOverlapping(PositionFromIndices(CursorIndices));

    public (int symbol, int line) CursorIndices
    {
        get => (cx, cy);
        set
        {
            var (x, y) = value;
            cy = Math.Clamp(y, 0, lines.Count - 1);
            cx = Math.Clamp(x, 0, lines[cy].Length);
        }
    }

    public (int x, int y) ScrollIndices
    {
        get => (scrX, scrY);
        set
        {
            scrX = Math.Clamp(value.x, 0, Math.Max(0, lines[cy].Length - Size.width + 1));
            scrY = Math.Clamp(value.y, 0, Math.Max(0, lines.Count - Size.height));
        }
    }

    public string? this[int lineIndex]
    {
        get => lineIndex < 0 || lineIndex >= lines.Count ? default : lines[lineIndex];
        set
        {
            if (lineIndex < 0 || lineIndex >= lines.Count)
                return;

            if (value == null)
                lines.RemoveAt(lineIndex);
            else
                lines[lineIndex] = value;

            UpdateText();
        }
    }

    /// <summary>
    /// Initializes a new input box instance with a specific position and default size of (12, 1).
    /// </summary>
    /// <param name="position">The position of the input box.</param>
    public InputBox((int x, int y) position) : base(position)
    {
        Size = (12, 1);
        lines[0] = Text;
        UpdateText();
    }
    public InputBox(byte[] bytes) : base(bytes)
    {
        Placeholder = GrabString(bytes);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutString(result, Placeholder);
        return result.ToArray();
    }

    public (int symbol, int line) PositionToIndices((int x, int y) position)
    {
        var (px, py) = Position;
        var (w, h) = (GetMaxLineWidth(), lines.Count - 1);
        var (x, y) = position;
        var newX = Math.Clamp(px - scrX + x, 0, w);
        var newY = Math.Clamp(py - scrY + y, 0, h);

        return (newX, newY);
    }
    public (int x, int y) PositionFromIndices((int symbol, int line) indices)
    {
        var (px, py) = Position;
        var (w, h) = (GetMaxLineWidth(), lines.Count - 1);
        var (x, y) = indices;
        var newX = Math.Clamp(x, 0, w);
        var newY = Math.Clamp(y, 0, h);

        return (px - scrX + newX, py - scrY + newY);
    }

    public void CursorMove((int x, int y) delta, bool isSelecting = false, bool isScrolling = true)
    {
        var shift = Input.Current.IsKeyPressed(Key.ShiftLeft) ||
                    Input.Current.IsKeyPressed(Key.ShiftRight);

        cursorBlink.Restart();

        var cxPrev = cx;
        var cyPrev = cy;

        cy += delta.y;
        cx += delta.x;
        ClampCursor();

        if (delta.x > 0 && // trying to move right
            cxPrev + delta.x > lines[cy].Length) // but line ended
        {
            var x = cx;
            cx = 0;
            CursorMove((cxPrev + delta.x - x, 1)); // keep going with the remaining delta
        }

        if (delta.x < 0 && // trying to move left
            cxPrev + delta.x < 0) // but line ended
        {
            var x = cx;
            cx = lines[cy].Length;
            CursorMove((cxPrev + delta.x - x, -1)); // keep going with the remaining delta
        }

        if (delta.x < 0 && // trying to move left
            cx == 0 && cxPrev == 0 && // but line ended
            cy != 0) // not first line
        {
            cy -= 1;
            cx = lines[cy].Length;
            CursorMove((0, 0), isSelecting, isScrolling);
        }
        else if (delta.x > 0 && // trying to move right
                 cx == lines[cy].Length && cxPrev == lines[cy].Length && // but line ended
                 cy != lines.Count - 1) // not last line
        {
            cx = 0;
            cy += 1;
            CursorMove((0, 0), isSelecting, isScrolling);
        }

        if (delta.y > 0 && // trying to move down
            cy == lines.Count - 1 && cyPrev == lines.Count - 1) // last line
            cx = lines[cy].Length;
        else if (delta.y < 0 && // trying to move up
                 cy == 0 && cyPrev == 0) // first line
            cx = 0;

        if (isScrolling)
            CursorScroll();

        SelectionIndices = shift && isSelecting ? SelectionIndices : CursorIndices;
    }
    public void CursorScroll()
    {
        var (cpx, cpy) = PositionFromIndices(CursorIndices);
        while (IsOverlapping((cpx, cpy)) == false)
        {
            var (sx, sy) = (0, 0);

            if (cpx < Position.x) sx = -1;
            else if (cpx >= Position.x + Size.width) sx = 1;

            if (cpy < Position.y) sy = -1;
            else if (cpy >= Position.y + Size.height) sy = 1;

            ScrollIndices = (ScrollIndices.x + sx, ScrollIndices.y + sy);
            (cpx, cpy) = PositionFromIndices(CursorIndices); // update
        }

        UpdateText();
    }

    /// <summary>
    /// Called when the input box needs to be updated. This handles all of the user input
    /// the input box needs for its behavior. Subclasses should override this 
    /// method to implement their own behavior.
    /// </summary>
    protected override void OnUpdate()
    {
        // reclamp despite scrolling or not cuz maybe the text changed
        ScrollIndices = (scrX, scrY);

        if (prevSize != Size)
            UpdateText();
        prevSize = Size;

        TrySetMouseCursor();

        if (IsDisabled || IsFocused == false || TrySelectAll() || JustPressed(Key.Tab))
            return;

        var isJustTyped = JustTyped();

        TryResetCursorBlinkTimer();
        TryResetHoldTimers(out var isHolding, isJustTyped);

        var isAllowedType = isJustTyped || (isHolding && Input.Current.Typed != "");
        var shouldDelete = isAllowedType || Allowed(Key.Backspace, isHolding) ||
                           Allowed(Key.Delete, isHolding) || Allowed(Key.Enter, isHolding);

        var justDeletedSelected = false;
        if (TryCopyPasteCut(ref justDeletedSelected, ref shouldDelete, out var isPasting))
            return;

        TrySelect();
        TryDeleteSelected(ref justDeletedSelected, shouldDelete);
        TryType(isAllowedType, isPasting);
        TryBackspaceDeleteEnter(isHolding, justDeletedSelected);
        TryMoveCursor(isHolding);
    }

#region Backend
    private readonly List<string> lines = new() { "" };

    private const char SELECTION = '█', SPACE = ' ';
    private const float HOLD = 0.06f, HOLD_DELAY = 0.5f, CURSOR_BLINK = 1f;
    private static readonly Stopwatch holdDelay = new(),
        hold = new(),
        clickDelay = new(),
        cursorBlink = new(),
        scrollHold = new();
    private int cx, cy, sx, sy, clicks, scrX, scrY;
    private (int, int) lastClickIndices = (-1, -1), prevSize;

    static InputBox()
    {
        holdDelay.Start();
        hold.Start();
        scrollHold.Start();
    }

    protected override void OnUserAction(UserAction userEvent)
    {
        if (userEvent == UserAction.Press)
            cursorBlink.Restart();
    }

    private void UpdateText()
    {
        var sb = new StringBuilder();
        var maxW = Size.width - 1;
        var maxY = Math.Min(scrY + Size.height, lines.Count);
        for (var i = scrY; i < maxY; i++)
        {
            var line = lines[i];
            var secondIndex = Math.Min(line.Length, scrX + maxW + 1);
            sb.Append((scrX >= secondIndex ? "" : line[scrX..secondIndex]) + Environment.NewLine);
        }

        Text = sb.ToString();
    }

    private static bool Allowed(Key key, bool isHolding) =>
        JustPressed(key) || (Pressed(key) && isHolding);
    private static bool JustPressed(Key key) => Input.Current.IsKeyJustPressed(key);
    private static bool Pressed(Key key) => Input.Current.IsKeyPressed(key);
    private static bool JustTyped()
    {
        var typed = Input.Current.Typed ?? "";
        var prev = Input.Current.TypedPrevious ?? "";

        foreach (var s in typed)
            if (prev.Contains(s) == false)
                return true;

        return false;
    }

    private char GetSymbol((int symbol, int line) indices)
    {
        var (x, y) = indices;
        return y < 0 || y >= lines.Count || x < 0 || x >= lines[y].Length ? default : lines[y][x];
    }
    private int GetWordEndOffset(int step)
    {
        var index = step < 0 ? -1 : 0;
        var symbol = GetSymbol((cx + (step < 0 ? -1 : 0), cy));
        var targetIsWord = char.IsLetterOrDigit(symbol) == false;

        for (var i = 0; i < lines[cy].Length; i++)
        {
            var j = cx + index;

            if (j == 0 && step < 0)
                return index;
            if (j == lines[cy].Length - 1 && step > 0)
                return index + 1;

            if (char.IsLetterOrDigit(GetSymbol((j, cy))) == false ^ targetIsWord)
                return index + (step < 0 ? 1 : 0);

            index += step;
        }

        return step < 0 ? IndicesToIndex((0, cy)) : IndicesToIndex((lines[cy].Length, cy));
    }

    private void TrySelect()
    {
        var isSamePosClick = false;
        var (hx, hy) = Input.Current.Position;
        var ix = (int)Math.Round(scrX + hx - Position.x);
        var iy = (int)Math.Clamp(scrY + hy - Position.y, 0, lines.Count - 1);
        var hasMoved = Input.Current.PositionPrevious != Input.Current.Position;

        if (Input.Current.IsPressed)
        {
            cursorBlink.Restart();
            clickDelay.Restart();
            isSamePosClick = lastClickIndices == (ix, iy);
            lastClickIndices = (ix, iy);
        }

        if (clickDelay.Elapsed.TotalSeconds > 1f)
        {
            clickDelay.Stop();
            clicks = 0;
            lastClickIndices = (-1, -1);
            return;
        }

        if (isSamePosClick == false)
        {
            // hold & drag inside
            if (IsPressedAndHeld && hasMoved)
                MoveCursor();

            if (Input.Current.IsJustPressed == false)
                return;

            // click
            MoveCursor();
            SelectionIndices = CursorIndices;

            return;

            void MoveCursor()
            {
                cy = iy;
                cx = ix;
                ClampCursor();
                CursorScroll();
            }
        }

        // hold & drag outside
        if (hasMoved == false && IsPressedAndHeld && IsHovered == false &&
            scrollHold.Elapsed.TotalSeconds > 0.12f)
        {
            var (px, py) = Input.Current.Position;
            var (x, y) = Position;
            var (w, h) = Size;

            if (px > x + w) cx += 1;
            else if (px < x) cx -= 1;
            if (py > y + h) cy += 1;
            else if (py < y) cy -= 1;

            ClampCursor();
            CursorScroll();
            scrollHold.Restart();
            return;
        }

        if (Input.Current.IsJustPressed)
            TryCycleSelected(ix, iy);
    }
    private void TryCycleSelected(int indexX, int indexY)
    {
        var (x, y) = Position;
        var (hx, hy) = Input.Current.Position;
        var selectionIndices = PositionToIndices(((int)hx, (int)hy));

        clicks = clicks == 4 ? 1 : clicks + 1;

        if (clicks == 1)
        {
            cy = indexY;
            cx = indexX;
            ClampCursor();
            CursorScroll();
            SelectionIndices = selectionIndices;
        }
        else if (clicks == 2)
        {
            cx += GetWordEndOffset(-1);
            ClampCursor();
            CursorScroll();
            SelectionIndices = (selectionIndices.symbol + GetWordEndOffset(1), selectionIndices.line);
        }
        else if (clicks == 3)
        {
            var p = PositionToIndices((x + lines[cy].Length, y + cy));
            cx = 0;
            CursorScroll();
            SelectionIndices = p;
        }
        else if (clicks == 4)
        {
            cy = 0;
            cx = 0;
            CursorScroll();
            SelectionIndices = (int.MaxValue, int.MaxValue);
        }
    }
    private bool TrySelectAll()
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);

        if (ctrl == false || Input.Current.Typed != "a")
            return false;

        cx = 0;
        cy = 0;
        CursorScroll();
        SelectionIndices = (int.MaxValue, int.MaxValue);
        return true;
    }

    private void TryMoveCursor(bool isHolding)
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);
        var shift = Pressed(Key.ShiftLeft) || Pressed(Key.ShiftRight);
        var i = IndicesToIndex(CursorIndices);
        var s = IndicesToIndex(SelectionIndices);
        var hasSel = shift == false && i != s;
        var justL = JustPressed(Key.ArrowLeft);
        var justR = JustPressed(Key.ArrowRight);

        var hotkeys = new[]
        {
            (hasSel && justL, cy == sy ? (i < s ? 0 : s - i, 0) : i < s ? (0, 0) : (sx - cx, sy - cy)),
            (hasSel && justR, cy == sy ? (i > s ? 0 : s - i, 0) : i > s ? (0, 0) : (sx - cx, sy - cy)),
            (ctrl && JustPressed(Key.ArrowUp), (-cx, 0)),
            (ctrl && JustPressed(Key.ArrowDown), (Size.width, 0)),
            (JustPressed(Key.Home), (-cx, -cy)),
            (JustPressed(Key.End), Size),
            (Allowed(Key.ArrowLeft, isHolding), (ctrl ? GetWordEndOffset(-1) : -1, 0)),
            (Allowed(Key.ArrowRight, isHolding), (ctrl ? GetWordEndOffset(1) : 1, 0)),
            (Allowed(Key.ArrowUp, isHolding), (0, -1)),
            (Allowed(Key.ArrowDown, isHolding), (0, 1)),
        };

        for (var j = 0; j < hotkeys.Length; j++)
            if (hotkeys[j].Item1)
            {
                CursorMove(hotkeys[j].Item2, true);
                return;
            }
    }

    private void TryType(bool isAllowedType, bool isPasting)
    {
        if (isAllowedType == false)
            return;

        var symbols = Input.Current.Typed ?? "";
        Type(symbols, isPasting);
    }
    private void Type(string symbols, bool isPasting)
    {
        if (isPasting && string.IsNullOrWhiteSpace(TextCopied) == false)
        {
            var pastedLines = TextCopied.Split(Environment.NewLine);
            var carry = string.Empty;

            for (var i = 0; i < pastedLines.Length; i++)
            {
                var line = pastedLines[i];
                if (i == 0) // first line
                {
                    if (pastedLines.Length > 1)
                        carry = lines[cy + i][cx..];

                    lines[cy + i] = lines[cy + i][..cx];
                    lines[cy + i] = lines[cy + i].Insert(cx, line);

                    continue;
                }

                lines.Insert(cy + i, line);

                if (i == pastedLines.Length - 1 && // last line
                    string.IsNullOrEmpty(carry) == false) // has carry 
                    lines[cy + i] += carry;
            }

            var copied = TextCopied.Replace(Environment.NewLine, string.Empty);
            CursorMove((copied.Length, 0));
            UpdateText();
            return;
        }

        var text = symbols.Length > 1 ? symbols[^1].ToString() : symbols;
        lines[cy] = lines[cy].Insert(cx, text);
        UpdateText();
        CursorMove((1, 0));
        SelectionIndices = CursorIndices;
    }
    private void TryBackspaceDeleteEnter(bool isHolding, bool justDeletedSelection)
    {
        var (w, _) = Size;

        if (Allowed(Key.Enter, isHolding))
        {
            // insert line above?
            if (cx == 0)
            {
                lines.Insert(cy, "");
                CursorMove((0, 1));
                UpdateText();
                return;
            }

            var textForNewLine = lines[cy][cx..];

            lines[cy] = lines[cy][..cx];
            lines.Insert(cy + 1, textForNewLine);
            cy++;
            cx = 0;
            cursorBlink.Restart();
            ClampCursor();
            SelectionIndices = CursorIndices;
            CursorScroll();
            UpdateText();
        }
        else if (Allowed(Key.Backspace, isHolding) && justDeletedSelection == false)
        {
            // cursor is at start of current line
            if (cx == 0)
            {
                // first line?
                if (cy == 0)
                    return;

                cy -= 1;
                cx = lines[cy].Length;
                ClampCursor();
                TryMergeBottomLine(cy);
                SelectionIndices = CursorIndices;
                CursorScroll();
                return;
            }

            var off = GetWordEndOffset(-1);
            var ctrl = Pressed(Key.ControlLeft);
            var count = ctrl ? Math.Abs(off) : 1;
            lines[cy] = lines[cy].Remove(ctrl ? cx + off : cx - 1, count);
            UpdateText();
            CursorMove((ctrl ? off : -1, 0));
            SelectionIndices = CursorIndices;
        }
        else if (Allowed(Key.Delete, isHolding) && justDeletedSelection == false)
        {
            // cursor is at end of current line
            if (cx == lines[cy].Length)
            {
                // last line?
                if (cy == w - 1)
                    return;

                TryMergeBottomLine(cy);
                SelectionIndices = CursorIndices;
                return;
            }

            var off = GetWordEndOffset(1);
            var ctrl = Pressed(Key.ControlLeft);
            var count = ctrl ? Math.Abs(off) : 1;
            lines[cy] = lines[cy].Remove(cx, count);
            SelectionIndices = CursorIndices;
            UpdateText();
        }
    }
    private void TryDeleteSelected(ref bool justDeletedSelection, bool shouldDelete)
    {
        var isSelected = SelectionIndices != CursorIndices;

        if (shouldDelete == false || isSelected == false)
            return;

        var cursorIndex = IndicesToIndex(CursorIndices);
        var selectionIndex = IndicesToIndex(SelectionIndices);
        var a = cursorIndex < selectionIndex ? CursorIndices : SelectionIndices;
        var b = cursorIndex > selectionIndex ? CursorIndices : SelectionIndices;
        var (ixa, iya) = a;
        var (ixb, iyb) = b;

        for (var i = iya; i <= iyb; i++)
        {
            if (iya == iyb) // single line selected
            {
                lines[i] = lines[i][..ixa] + lines[i][ixb..];
                break;
            }

            // multiline selected...
            if (i == iya) // first selected line
                lines[i] = lines[i][..ixa];
            else if (i == iyb) // last selected line
            {
                var min = Math.Min(ixb, lines[i].Length);
                lines[i] = lines[i][min..];
                lines[iya] += lines[i];
                lines.RemoveAt(i);
            }
            else if (i < iyb) // line between first & last
            {
                lines.RemoveAt(i);
                i--;
                iyb--;
            }
        }

        cx = ixa;
        cy = iya;
        CursorScroll(); // update scrolling
        UpdateText();
        SelectionIndices = CursorIndices;
        justDeletedSelection = true;
    }

    private static void TryResetCursorBlinkTimer()
    {
        if (cursorBlink.Elapsed.TotalSeconds >= CURSOR_BLINK)
            cursorBlink.Restart();
    }
    private void TryMergeBottomLine(int lineIndex)
    {
        var nextLineIndex = lineIndex + 1;

        if (nextLineIndex >= lines.Count)
            return;

        lines[lineIndex] += lines[nextLineIndex];
        lines.RemoveAt(nextLineIndex);
        UpdateText();
    }
    private void TrySetMouseCursor()
    {
        if (IsDisabled == false && (IsHovered || IsPressedAndHeld))
            MouseCursorResult = MouseCursor.Text;
    }
    private bool TryCopyPasteCut(ref bool justDeletedSelection, ref bool shouldDelete,
        out bool isPasting)
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);
        var hasSelection = CursorIndices != SelectionIndices;

        isPasting = false;

        if (ctrl && Input.Current.Typed == "c")
        {
            TextCopied = SelectedText;
            return true;
        }
        else if (ctrl && Input.Current.Typed == "v")
            isPasting = true;
        else if (hasSelection && ctrl && Input.Current.Typed == "x")
        {
            TextCopied = SelectedText;
            shouldDelete = true;
            TryDeleteSelected(ref justDeletedSelection, shouldDelete);
            return true;
        }

        return false;
    }
    private void ClampCursor()
    {
        cy = Math.Clamp(cy, 0, lines.Count - 1);
        cx = Math.Clamp(cx, 0, lines[cy].Length);
    }

    private int GetMaxLineWidth()
    {
        var max = 1;
        foreach (var t in lines)
        {
            if (t.Length <= max)
                continue;

            // +1 since each line has valid cursor index on the very right which
            // has the "same" index as the cursor index on the next line to the very left
            // so that simulates a non-existing symbol that can be "indexed"
            max = t.Length + 1;
        }

        return max;
    }

    private int IndicesToIndex((int symbol, int line) indices)
    {
        indices.line = Math.Clamp(indices.line, 0, lines.Count - 1);

        var i = 0;
        var result = 0;
        for (; i < indices.line; i++)
            result += lines[i].Length;

        result += Math.Clamp(indices.symbol, 0, lines[i].Length);

        return result;
    }
    private (int symbol, int line) IndicesFromIndex(int index)
    {
        var curIndex = 0;
        var symbol = 0;
        var line = 0;
        foreach (var l in lines)
        {
            if (index >= curIndex && index <= curIndex + l.Length)
            {
                symbol += l.Length - index;
                return (symbol, line);
            }

            curIndex += l.Length;
            line++;
        }

        return (symbol, line);
    }

    private static void TryResetHoldTimers(out bool isHolding, bool isJustTyped)
    {
        var isAnyJustPressed = isJustTyped ||
                               JustPressed(Key.Enter) ||
                               JustPressed(Key.Backspace) || JustPressed(Key.Delete) ||
                               JustPressed(Key.ArrowLeft) || JustPressed(Key.ArrowRight) ||
                               JustPressed(Key.ArrowUp) || JustPressed(Key.ArrowDown);

        if (isAnyJustPressed)
            holdDelay.Restart();

        if (holdDelay.Elapsed.TotalSeconds > HOLD_DELAY &&
            hold.Elapsed.TotalSeconds > HOLD)
        {
            hold.Restart();
            isHolding = true;
        }
        else
            isHolding = false;
    }
#endregion
}