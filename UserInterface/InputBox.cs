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

            var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
            var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;

            for (var i = scrY; i < scrY + Size.height; i++)
            {
                for (var j = scrX; j < scrX + Size.width; j++)
                {
                    var index = ToIndex((j, i));
                    var isSelected = index >= a && index < b && j < lines[i].Length;
                    sb.Append(isSelected ? SELECTION : SPACE);
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }
    }
    /// <summary>
    /// Gets or sets the zero-based index of the first character in the current 
    /// selection of the input box's text.
    /// </summary>
    public int SelectionIndex
    {
        get => selectionIndex;
        set
        {
            var (sx, sy) = FromIndex(value);
            sx = Math.Clamp(sx, 0, lines[cy].Length);
            sy = Math.Clamp(sy, 0, lines.Count - 1);
            selectionIndex = ToIndex((sx, sy));
        }
    }
    /// <summary>
    /// Gets the currently selected text in the input box.
    /// </summary>
    public string SelectedText
    {
        get
        {
            if (SelectionIndex == CursorIndex)
                return "";

            var sb = new StringBuilder();

            var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
            var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;

            for (var i = a; i < b; i++)
            {
                var symbol = GetSymbol(i);
                var (ix, iy) = FromIndex(i);

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
        IsOverlapping(PositionFromIndex(CursorIndex));

    /// <summary>
    /// Gets the zero-based index of the character at the cursor's position in the input box's text.
    /// </summary>
    public int CursorIndex
    {
        get => ToIndex((cx, cy));
        set
        {
            var (x, y) = FromIndex(value);
            cx = Math.Clamp(x, 0, lines[cy].Length);
            cy = Math.Clamp(y, 0, lines.Count - 1);
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

    public void Append(int cursorIndex, string? text)
    {
        var prevX = cx;
        var prevY = cy;
        text ??= "";
        var (x, y) = FromIndex(cursorIndex);
        cx = x;
        cy = y;

        foreach (var t in text)
            Type(t.ToString(), false);

        cx = prevX;
        cy = prevY;
        SelectionIndex = CursorIndex;
    }
    /// <summary>
    /// Gets the text of a specific line in the input box.
    /// </summary>
    /// <param name="lineIndex">The zero-based index of the line to get the text of.</param>
    /// <returns>The text of the specified line.</returns>
    public string LineAt(int lineIndex)
    {
        return lineIndex < 0 && lineIndex >= lines.Count ? "" : lines[lineIndex];
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutString(result, Placeholder);
        return result.ToArray();
    }

    /// <summary>
    /// Converts a world position to the closest zero-based index in the input box's text.
    /// </summary>
    /// <param name="position">The position to convert.</param>
    /// <returns>The closest zero-based index to
    /// the given position in the input box's text.</returns>
    public int PositionToIndex((float, float) position)
    {
        var (ppx, ppy) = position;
        var (px, py) = Position;
        var (w, h) = Size;
        var (x, y) = (MathF.Round(ppx) - px, MathF.Floor(ppy) - py);
        y = Math.Clamp(y, 0, lines.Count - 1);
        var index = Math.Clamp(y, 0, h) * w + Math.Clamp(x, 0, lines[(int)y].Length);

        return (int)Math.Clamp(index, 0, w * h);
    }
    /// <summary>
    /// Converts a text index to a world position.
    /// </summary>
    /// <param name="index">The index to convert.</param>
    /// <returns>The position corresponding to the given index.</returns>
    public (int x, int y) PositionFromIndex(int index)
    {
        var (px, py) = Position;
        var (w, h) = (GetMaxLineWidth(), lines.Count - 1);
        var (x, y) = FromIndex(index);
        var newX = Math.Clamp(x, 0, w);
        var newY = Math.Clamp(y, 0, h);

        return (px - scrX + newX, py - scrY + newY);
    }

    public (int indexSymbol, int indexLine) FromIndex(int index)
    {
        var maxLineLength = GetMaxLineWidth();
        var (w, h) = (maxLineLength, lines.Count);
        index = Math.Clamp(index, 0, w * h);
        var result = (index % w, index / w);

        // fixes weird issues when index is == to maxLineLength on non-first lines
        var next = ((index + 1) % w, (index + 1) / w);
        if (index == w * h) return (maxLineLength, lines.Count);
        else if (ToIndex(result) == ToIndex(next)) return (maxLineLength, result.Item2 - 1);

        return result;
    }
    public int ToIndex((int symbol, int line) indices)
    {
        return indices.line * GetMaxLineWidth() + indices.symbol;
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

        SelectionIndex = shift && isSelecting ? SelectionIndex : CursorIndex;
    }
    public void CursorScroll()
    {
        var (cpx, cpy) = PositionFromIndex(CursorIndex);
        while (IsOverlapping((cpx, cpy)) == false)
        {
            var (sx, sy) = (0, 0);

            if (cpx < Position.x) sx = -1;
            else if (cpx >= Position.x + Size.width) sx = 1;

            if (cpy < Position.y) sy = -1;
            else if (cpy >= Position.y + Size.height) sy = 1;

            ScrollIndices = (ScrollIndices.x + sx, ScrollIndices.y + sy);
            (cpx, cpy) = PositionFromIndex(CursorIndex); // update
        }
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

        TrySetMouseCursor();
        UpdateText();

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
        cursorBlink = new();
    private int selectionIndex, cx, cy, clicks, scrX, scrY;
    private (int, int) lastClickIndices = (-1, -1);

    static InputBox()
    {
        holdDelay.Start();
        hold.Start();
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

    private char GetSymbol(int index)
    {
        var (x, y) = FromIndex(index);
        return y < 0 || y >= lines.Count || x < 0 || x >= lines[y].Length ? default : lines[y][x];
    }
    private int GetWordEndOffset(int step)
    {
        var index = step < 0 ? -1 : 0;
        var symbol = GetSymbol(CursorIndex + (step < 0 ? -1 : 0));
        var targetIsWord = char.IsLetterOrDigit(symbol) == false;

        for (var i = 0; i < lines[cy].Length; i++)
        {
            var j = CursorIndex + index;
            var (jx, _) = FromIndex(j);

            if (jx == 0 && step < 0)
                return index;
            if (jx == lines[cy].Length - 1 && step > 0)
                return index + 1;

            if (char.IsLetterOrDigit(GetSymbol(j)) == false ^ targetIsWord)
                return index + (step < 0 ? 1 : 0);

            index += step;
        }

        return step < 0 ? ToIndex((0, cy)) : ToIndex((lines[cy].Length, cy));
    }

    private void TrySelect()
    {
        var isSamePosClick = false;
        var (hx, hy) = Input.Current.Position;
        var ix = (int)Math.Round(scrX + hx - Position.x);
        var iy = (int)Math.Clamp(scrY + hy - Position.y, 0, lines.Count - 1);

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
            if (Input.Current.IsPressed)
            {
                cy = iy;
                cx = ix;
                ClampCursor();
                CursorScroll();
            }

            if (Input.Current.IsJustPressed)
                SelectionIndex = CursorIndex;

            return;
        }

        if (Input.Current.IsJustPressed)
            TryCycleSelected(ix, iy);
    }
    private void TryCycleSelected(int indexX, int indexY)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var (hx, hy) = Input.Current.Position;

        clicks = clicks == 4 ? 1 : clicks + 1;

        if (clicks == 1)
        {
            cy = indexY;
            cx = indexX;
            ClampCursor();
            CursorScroll();
            SelectionIndex = PositionToIndex((hx, hy));
        }
        else if (clicks == 2)
        {
            var i = PositionToIndex((hx, hy));
            cx += GetWordEndOffset(-1);
            ClampCursor();
            CursorScroll();
            SelectionIndex = i + GetWordEndOffset(1);
        }
        else if (clicks == 3)
        {
            var p = PositionToIndex((x + lines[cy].Length, y + cy));
            cx = 0;
            CursorScroll();
            SelectionIndex = p;
        }
        else if (clicks == 4)
        {
            cy = 0;
            cx = 0;
            CursorScroll();
            SelectionIndex = w * h;
        }
    }
    private bool TrySelectAll()
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);

        if (ctrl == false || Input.Current.Typed != "a")
            return false;

        var (w, h) = Size;
        cx = 0;
        cy = 0;
        CursorScroll();
        SelectionIndex = w * h;
        return true;
    }

    private void TryMoveCursor(bool isHolding)
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);
        var shift = Pressed(Key.ShiftLeft) || Pressed(Key.ShiftRight);
        var i = CursorIndex;
        var s = SelectionIndex;
        var hasSel = shift == false && i != s;
        var justL = JustPressed(Key.ArrowLeft);
        var justR = JustPressed(Key.ArrowRight);
        var (ix, iy) = PositionFromIndex(i);
        var (sx, sy) = PositionFromIndex(s);

        var hotkeys = new[]
        {
            (hasSel && justL, iy == sy ? (i < s ? 0 : s - i, 0) : i < s ? (0, 0) : (sx - ix, sy - iy)),
            (hasSel && justR, iy == sy ? (i > s ? 0 : s - i, 0) : i > s ? (0, 0) : (sx - ix, sy - iy)),
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
            return;
        }

        var text = symbols.Length > 1 ? symbols[^1].ToString() : symbols;
        lines[cy] = lines[cy].Insert(cx, text);
        CursorMove((1, 0));
        SelectionIndex = CursorIndex;
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
                return;
            }

            var textForNewLine = lines[cy][cx..];

            lines[cy] = lines[cy][..cx];
            lines.Insert(cy + 1, textForNewLine);

            cy++;
            cx = 0;
            SelectionIndex = CursorIndex;
            CursorScroll();
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
                SelectionIndex = CursorIndex;
                CursorScroll();
                return;
            }

            var off = GetWordEndOffset(-1);
            var ctrl = Pressed(Key.ControlLeft);
            var count = ctrl ? Math.Abs(off) : 1;
            lines[cy] = lines[cy].Remove(ctrl ? cx + off : cx - 1, count);
            CursorMove((ctrl ? off : -1, 0));
            SelectionIndex = CursorIndex;
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
                SelectionIndex = CursorIndex;
                return;
            }

            var off = GetWordEndOffset(1);
            var ctrl = Pressed(Key.ControlLeft);
            var count = ctrl ? Math.Abs(off) : 1;
            lines[cy] = lines[cy].Remove(cx, count);
            SelectionIndex = CursorIndex;
        }
    }
    private void TryDeleteSelected(ref bool justDeletedSelection, bool shouldDelete)
    {
        var (x, y) = Position;
        var isSelected = SelectionIndex != CursorIndex;

        if (shouldDelete == false || isSelected == false)
            return;

        var a = SelectionIndex < CursorIndex ? SelectionIndex : CursorIndex;
        var b = SelectionIndex > CursorIndex ? SelectionIndex : CursorIndex;
        var pa = PositionFromIndex(a);
        var pb = PositionFromIndex(b);
        var (ixa, iya) = (pa.x - x, Math.Clamp(pa.y - y, 0, lines.Count - 1));
        var (ixb, iyb) = (pb.x - x, Math.Clamp(pb.y - y, 0, lines.Count - 1));

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
        SelectionIndex = CursorIndex;
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
        var hasSelection = CursorIndex != SelectionIndex;

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