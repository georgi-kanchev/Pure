using System.Text.RegularExpressions;

namespace Pure.Engine.UserInterface;

using System.Diagnostics;
using System.Text;

[Flags]
public enum SymbolGroup
{
    None = 1 << 0,
    /// <summary>
    /// abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ
    /// </summary>
    Letters = 1 << 1,
    /// <summary>
    /// 0123456789.,
    /// </summary>
    Decimals = 1 << 2,
    /// <summary>
    /// 0123456789
    /// </summary>
    Integers = 1 << 3,
    /// <summary>
    /// ,.;:!?-()[]{}\"'
    /// </summary>
    Punctuation = 1 << 4,
    /// <summary>
    /// +-*/=ᐸᐳ%(),^
    /// </summary>
    Math = 1 << 5,
    /// <summary>
    /// @#＆_|\\/^
    /// </summary>
    Special = 1 << 6,
    Space = 1 << 7,
    Other = 1 << 8,
    All = Letters | Decimals | Integers | Punctuation | Math | Special | Space | Other
}

/// <summary>
/// Accepts text input from the user.
/// </summary>
public class InputBox : Block
{
    /// <summary>
    /// The text that should be displayed in the input box when it is empty.
    /// Defaults to "Type…".
    /// </summary>
    public string Placeholder { get; set; }
    public string Value
    {
        get => value;
        set
        {
            value = value[..Math.Min(value.Length, MaximumSymbolCount)];
            this.value = value;
            var split = value.Split(Environment.NewLine);

            lines.Clear();
            foreach (var line in split)
                lines.Add(line);

            UpdateText();
            CursorScroll();

            // reclamp
            SelectionIndices = (sx, sy);
            CursorIndices = (cx, cy);
        }
    }
    public string? SymbolMask
    {
        get => symbolMask;
        set
        {
            symbolMask = value;
            UpdateText();
        }
    }
    public bool IsReadOnly { get; set; }
    public SymbolGroup SymbolGroup
    {
        get => symbolGroup;
        set
        {
            if (symbolGroup != value)
                allowedSymbolsCache.Clear();

            symbolGroup = value;
        }
    }
    public int MaximumSymbolCount { get; set; }

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

                sb.Append(Environment.NewLine);
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
                return string.Empty;

            var sb = new StringBuilder();
            var cursorIndex = IndicesToIndex(CursorIndices);
            var selectionIndex = IndicesToIndex(SelectionIndices);
            var start = Math.Min(cursorIndex, selectionIndex);
            var end = Math.Max(cursorIndex, selectionIndex);

            for (var i = start; i < end; i++)
            {
                var (ix, iy) = IndicesFromIndex(i);

                if (ix == lines[iy].Length)
                {
                    sb.AppendLine();
                    continue;
                }

                var symbol = GetSymbol((ix, iy));
                if (symbol != default)
                    sb.Append(symbol);
            }

            return sb.ToString();
        }
    }

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
    public bool IsCursorVisible
    {
        get =>
            IsFocused &&
            IsReadOnly == false &&
            cursorBlink.Elapsed.TotalSeconds <= CURSOR_BLINK / 2f &&
            IsOverlapping(PositionFromIndices(CursorIndices));
    }

    public int LineCount
    {
        get => lines.Count;
    }

    public string? this[int lineIndex]
    {
        get => lineIndex < 0 || lineIndex >= lines.Count ? default : lines[lineIndex];
        set
        {
            if (lineIndex < 0)
                return;

            if (lineIndex >= lines.Count)
                for (var i = lines.Count - 1; i <= lineIndex; i++)
                    lines.Insert(i, string.Empty);

            if (value == null)
                lines.RemoveAt(lineIndex);
            else
                lines[lineIndex] = value;

            UpdateTextAndValue();
        }
    }

    /// <summary>
    /// Initializes a new input box instance with a specific position and default size of (12, 1).
    /// </summary>
    /// <param name="position">The position of the input box.</param>
    public InputBox((int x, int y) position = default) : base(position)
    {
        Init();
        Placeholder = "Type…";
        Size = (12, 1);
        lines[0] = Text;
        Value = Text;
        MaximumSymbolCount = int.MaxValue;

        SymbolGroup = SymbolGroup.All;
    }
    public InputBox(byte[] bytes) : base(bytes)
    {
        Init();
        var b = Decompress(bytes);
        IsReadOnly = GrabBool(b);
        Placeholder = GrabString(b);
        Value = GrabString(b);
        SymbolGroup = (SymbolGroup)GrabByte(b);
        MaximumSymbolCount = GrabInt(b);
    }
    public InputBox(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        PutBool(result, IsReadOnly);
        PutString(result, Placeholder);
        PutString(result, Value);
        PutByte(result, (byte)SymbolGroup);
        PutInt(result, MaximumSymbolCount);
        return Compress(result.ToArray());
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

    public void CursorMove((int x, int y) delta, bool select = false, bool scroll = true)
    {
        clicks = 0;
        cursorBlink.Restart();

        for (var i = 0; i < Math.Abs(delta.x); i++)
        {
            var isGoingLeft = delta.x < 0;
            var isGoingRight = delta.x > 0;
            var isAtStart = cx == 0 && cy == 0;
            var isAtEnd = cy == lines.Count - 1 && cx == lines[cy].Length;

            if ((isAtStart && isGoingLeft) || (isAtEnd && isGoingRight))
                continue;

            cx += delta.x < 0 ? -1 : 1;

            if (cx < 0)
            {
                cy--;
                cx = lines[cy].Length;
            }

            if (cx <= lines[cy].Length)
                continue;

            cx = 0;
            cy++;
        }

        cy += delta.y;

        // trying to keep the cx the when moving up & down
        if (delta.x == 0 && delta.y != 0)
            cx = cxDesired;

        // last line, trying to go down, should move to the end of line on cx
        if (cy >= lines.Count)
            cx = lines[^1].Length;
        // same thing for first line going up, should move to start of line on cx
        if (cy < 0)
            cx = 0;

        CursorIndices = (cx, cy); // clamp

        if (scroll)
            CursorScroll();

        SelectionIndices = select ? SelectionIndices : CursorIndices;

        if (delta.x != 0)
            cxDesired = cx;
    }
    public void CursorScroll()
    {
        var (cpx, cpy) = PositionFromIndices(CursorIndices);
        for (var i = 0; i < Value.Length; i++)
        {
            if (IsOverlapping((cpx, cpy)))
                break;

            var (offX, offY) = (0, 0);

            if (cpx < Position.x) offX = -1;
            else if (cpx >= Position.x + Size.width) offX = 1;

            if (cpy < Position.y) offY = -1;
            else if (cpy >= Position.y + Size.height) offY = 1;

            ScrollIndices = (ScrollIndices.x + offX, ScrollIndices.y + offY);
            (cpx, cpy) = PositionFromIndices(CursorIndices); // update
        }

        UpdateText();
    }
    public void SelectAll()
    {
        sx = 0;
        sy = 0;
        CursorIndices = (int.MaxValue, int.MaxValue);
        CursorScroll();
    }

    public bool IsAllowed(string symbol)
    {
        if (allowedSymbolsCache.TryGetValue(symbol, out var allowed))
            return allowed;

        var set = string.Empty;
        var values = symbolSets.Values;
        var keys = symbolSets.Keys;
        var isNotInSets = true;

        foreach (var flag in keys)
            if (SymbolGroup.HasFlag(flag))
                set += symbolSets[flag];

        foreach (var charSet in values)
            if (charSet.Contains(symbol))
                isNotInSets = false;

        var isOther = SymbolGroup.HasFlag(SymbolGroup.Other) && isNotInSets;
        var result = set.Contains(symbol) || isOther;
        allowedSymbolsCache[symbol] = result;
        return result;
    }

    public void OnType(Action<string> method)
    {
        type += method;
    }

    public InputBox Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](InputBox inputBox)
    {
        return inputBox.ToBytes();
    }
    public static implicit operator InputBox(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private readonly List<string> lines = [string.Empty];
    private readonly Dictionary<string, bool> allowedSymbolsCache = new();

    private static readonly Dictionary<SymbolGroup, string> symbolSets = new()
    {
        { SymbolGroup.Letters, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" },
        { SymbolGroup.Decimals, "0123456789." },
        { SymbolGroup.Integers, "0123456789" },
        { SymbolGroup.Punctuation, ",.;:!?-()[]{}\"'" },
        { SymbolGroup.Math, "+-*/=<>%(),^" },
        { SymbolGroup.Special, "@#&_|\\/^" },
        { SymbolGroup.Space, " " }
    };

    private const char SELECTION = '█', SPACE = ' ';
    private const float HOLD = 0.06f, HOLD_DELAY = 0.5f, CURSOR_BLINK = 1f;
    private static readonly Stopwatch holdDelay = new(),
        hold = new(),
        clickDelay = new(),
        cursorBlink = new(),
        scrollHold = new();
    private int cx, cy, sx, sy, clicks, scrX, scrY, cxDesired;
    private (int, int) lastClickIndices = (-1, -1), prevSize;
    private string value = string.Empty;

    private Action<string>? type;
    private SymbolGroup symbolGroup;
    private string? symbolMask;

    static InputBox()
    {
        holdDelay.Start();
        hold.Start();
        scrollHold.Start();
    }

    private void Init()
    {
        isTextReadonly = true;

        OnUpdate(OnUpdate);
        OnInteraction(Interaction.Press, () =>
        {
            cursorBlink.Restart();
            TryCycleSelected();
        });
        OnInteraction(Interaction.Focus, () =>
        {
            clicks = 0;
            clickDelay.Restart();
            TrySelect();
        });
    }

    protected override void OnInput()
    {
        var isMultiLine = Height > 1;
        if (IsReadOnly == false && IsFocused && isMultiLine == false && JustPressed(Key.Enter))
            Interact(Interaction.Select);

        var isBellowElement = IsFocused == false || Input.FocusedPrevious != this;
        if (isBellowElement || TrySelectAll() || JustPressed(Key.Tab))
            return;

        var isJustTyped = JustTyped();

        TryResetHoldTimers(out var isHolding, isJustTyped);

        var isAllowedType = isJustTyped || (isHolding && Input.Typed != string.Empty);
        var shouldDelete = isAllowedType ||
                           Allowed(Key.Backspace, isHolding) ||
                           Allowed(Key.Delete, isHolding) ||
                           (Allowed(Key.Enter, isHolding) && isMultiLine);

        var justDeletedSelected = false;
        if (TryCopyPasteCut(ref justDeletedSelected, ref shouldDelete, out var isPasting))
            return;

        TrySelect();
        TryDeleteSelected(ref justDeletedSelected, shouldDelete);
        TryType(isAllowedType, isPasting);
        TryBackspaceDeleteEnter(isHolding, justDeletedSelected);
        TryMoveCursor(isHolding);
    }
    internal void OnUpdate()
    {
        // reclamp despite scrolling or not cuz maybe the text changed
        CursorIndices = (cx, cy);
        ScrollIndices = (scrX, scrY);

        if (prevSize != Size)
            UpdateText();
        prevSize = Size;

        TrySetMouseCursor();
        TryResetCursorBlinkTimer();
    }
    internal override void ApplyScroll()
    {
        var delta = Input.ScrollDelta;
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);
        var shift = Pressed(Key.ShiftLeft) || Pressed(Key.ShiftRight);
        CursorMove(ctrl ? (-delta, 0) : (0, -delta), shift);
    }

    private void UpdateTextAndValue()
    {
        UpdateText();
        UpdateValue();
    }
    private void UpdateText()
    {
        var str = new StringBuilder();
        var maxW = Width - 1;
        var maxY = Math.Min(scrY + Height, lines.Count);
        for (var i = scrY; i < maxY; i++)
        {
            var newLine = i == scrY ? string.Empty : Environment.NewLine;
            var line = lines[i];
            var secondIndex = Math.Min(line.Length, scrX + maxW + 1);
            var result = scrX >= secondIndex ? string.Empty : line[scrX..secondIndex];

            if (SymbolMask != null)
                result = Regex.Replace(result, ".", SymbolMask);

            if (Width > 3 && scrX > 0 && line.Length > 0)
            {
                var chars = (string.IsNullOrEmpty(result) ? " " : result).ToCharArray();
                chars[0] = '…';
                result = new(chars);
            }

            if (scrX < line.Length - maxW - 1 && line.Length > maxW - 1)
            {
                var chars = (string.IsNullOrEmpty(result) ? " " : result).ToCharArray();
                chars[^1] = '…';
                result = new(chars);
            }

            str.Append(newLine + result);
        }

        text = str.ToString();

        // reclamp
        CursorIndices = (cx, cy);
        ScrollIndices = (scrX, scrY);
    }
    private void UpdateValue()
    {
        var str = new StringBuilder();
        for (var i = 0; i < lines.Count; i++)
            str.Append((i > 0 ? Environment.NewLine : string.Empty) + lines[i]);

        var result = str.ToString();
        Value = result;
    }

    private static bool Allowed(Key key, bool isHolding)
    {
        return JustPressed(key) || (Pressed(key) && isHolding);
    }
    private static bool JustPressed(Key key)
    {
        return Input.IsKeyJustPressed(key);
    }
    private static bool Pressed(Key key)
    {
        return Input.IsKeyPressed(key);
    }
    private static bool JustTyped()
    {
        var typed = Input.Typed ?? string.Empty;
        var prev = Input.TypedPrevious ?? string.Empty;

        foreach (var s in typed)
            if (prev.Contains(s) == false)
                return true;

        return false;
    }

    private void TrySelect()
    {
        var (hx, hy) = Input.Position;
        var ix = (int)Math.Round(scrX + hx - Position.x);
        var iy = (int)Math.Clamp(scrY + hy - Position.y, 0, lines.Count - 1);
        var hasMoved = Input.PositionPrevious != Input.Position;

        if (Input.IsButtonPressed())
        {
            cursorBlink.Restart();
            clickDelay.Restart();
            lastClickIndices = (ix, iy);
        }

        if (clickDelay.Elapsed.TotalSeconds > 1f)
        {
            clickDelay.Stop();
            clicks = 0;
            lastClickIndices = (-1, -1);
            return;
        }

        if (IsPressedAndHeld && hasMoved)
        {
            // hold & drag inside
            cy = iy;
            cx = ix;
            CursorIndices = (cx, cy);
            cxDesired = cx;
            CursorScroll();
        }

        // hold & drag outside
        if (hasMoved ||
            !IsPressedAndHeld ||
            IsHovered ||
            scrollHold.Elapsed.TotalSeconds > 0.15f == false)
            return;

        var (px, py) = Input.Position;
        var (x, y) = Position;
        var (w, h) = Size;

        if (px > x + w)
            cx += 1;
        else if (px < x)
            cx -= 1;
        if (py > y + h)
            cy += 1;
        else if (py < y)
            cy -= 1;

        CursorIndices = (cx, cy);
        cxDesired = cx;
        CursorScroll();
        scrollHold.Restart();
    }
    private void TryCycleSelected()
    {
        var (hx, hy) = Input.Position;
        var isWholeLineSelected = sx == 0 && cx == lines[cy].Length;
        var ix = (int)Math.Round(scrX + hx - Position.x);
        var iy = (int)Math.Clamp(scrY + hy - Position.y, 0, lines.Count - 1);

        if (lastClickIndices != (ix, iy))
        {
            CursorIndices = (ix, iy);
            SelectionIndices = (ix, iy);
            return; // not clicking on the same spot?
        }

        clicks = clicks == 4 ? 1 : clicks + 1;

        var skipWord = clicks == 2 && SymbolMask != null; // there are only whole lines
        clicks += skipWord ? 1 : 0;

        var skipLine = clicks == 3 && (Height == 1 || isWholeLineSelected);
        clicks += skipLine ? 1 : 0;

        if (clicks == 1) // cursor to mouse
        {
            CursorIndices = (ix, iy);
            SelectionIndices = (ix, iy);
        }
        else if (clicks == 2) // select word
        {
            cx += GetWordEndOffset(1);
            CursorIndices = (cx, cy);
            CursorScroll();
            SelectionIndices = (cx + GetWordEndOffset(-1), cy);
        }
        else if (clicks == 3) // select line
        {
            CursorIndices = (lines[cy].Length, cy);
            CursorScroll();
            SelectionIndices = (0, cy);
        }
        else if (clicks == 4)
            SelectAll();
    }
    private bool TrySelectAll()
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);

        if (ctrl == false || Input.Typed != "a")
            return false;

        SelectAll();
        return true;
    }

    private void TryMoveCursor(bool isHolding)
    {
        if (IsReadOnly)
            return;

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
            (Allowed(Key.ArrowDown, isHolding), (0, 1))
        };

        for (var j = 0; j < hotkeys.Length; j++)
            if (hotkeys[j].Item1)
            {
                CursorMove(hotkeys[j].Item2, shift);
                return;
            }
    }

    private void TryType(bool isAllowedType, bool isPasting)
    {
        if (isAllowedType == false || IsReadOnly)
            return;

        var symbols = Input.Typed ?? string.Empty;
        Type(symbols, isPasting);
    }
    private void Type(string symbols, bool isPasting)
    {
        if (isPasting)
        {
            var paste = Input.Clipboard ?? string.Empty;
            var pastedLines = paste.Split(Environment.NewLine);
            var carry = string.Empty;

            for (var i = 0; i < pastedLines.Length; i++)
            {
                var line = RemoveForbiddenSymbols(pastedLines[i]);
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
                    lines[cy + i] += RemoveForbiddenSymbols(carry);
            }

            var copied = paste.Replace(Environment.NewLine, string.Empty);
            CursorMove((copied.Length, 0));
            UpdateTextAndValue();
            return;
        }

        var str = symbols.Length > 1 ? symbols[^1].ToString() : symbols;

        if (IsAllowed(str) == false)
            return;

        lines[cy] = lines[cy].Insert(cx, str);
        UpdateTextAndValue();
        CursorMove((1, 0));
        SelectionIndices = CursorIndices;

        type?.Invoke(str);
    }
    private void TryBackspaceDeleteEnter(bool isHolding, bool justDeletedSelection)
    {
        if (IsReadOnly)
            return;

        var (w, _) = Size;

        if (Allowed(Key.Enter, isHolding) && Height > 1)
        {
            // insert line above?
            if (cx == 0)
            {
                lines.Insert(cy, string.Empty);
                CursorMove((0, 1));
                UpdateTextAndValue();
                return;
            }

            var textForNewLine = lines[cy][cx..];

            lines[cy] = lines[cy][..cx];
            lines.Insert(cy + 1, textForNewLine);
            cy++;
            cx = 0;
            CursorIndices = (cx, cy);

            SelectionIndices = CursorIndices;
            CursorScroll();
            UpdateTextAndValue();

            cursorBlink.Restart();
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
                CursorIndices = (cx, cy);
                SelectionIndices = CursorIndices;

                TryMergeBottomLine(cy);
                CursorScroll();
                UpdateTextAndValue();
                return;
            }

            var ctrl = Pressed(Key.ControlLeft);
            var off = ctrl ? GetWordEndOffset(-1) : 0;
            var count = ctrl ? Math.Abs(off) : 1;

            lines[cy] = lines[cy].Remove(ctrl ? cx + off : cx - 1, count);

            cx -= ctrl ? off : 1;
            CursorIndices = (cx, cy);
            SelectionIndices = CursorIndices;

            scrX -= Math.Abs(ctrl ? off : 1);
            ScrollIndices = (scrX, scrY);
            CursorScroll();

            UpdateTextAndValue();
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

            scrX -= Math.Abs(ctrl ? off : -1);
            ScrollIndices = (scrX, scrY);
            CursorScroll();

            SelectionIndices = CursorIndices;
            UpdateTextAndValue();
        }
    }
    private void TryDeleteSelected(ref bool justDeletedSelection, bool shouldDelete)
    {
        var isSelected = SelectionIndices != CursorIndices;

        if (shouldDelete == false || isSelected == false || IsReadOnly)
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
        UpdateTextAndValue();
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
        UpdateTextAndValue();
    }
    private void TrySetMouseCursor()
    {
        if (IsDisabled == false && (IsHovered || IsPressedAndHeld))
            Input.CursorResult = MouseCursor.Text;

        if (IsHovered && IsReadOnly && Value.Length == 0)
            Input.CursorResult = MouseCursor.Arrow;
    }
    private bool TryCopyPasteCut(
        ref bool justDeletedSelection,
        ref bool shouldDelete,
        out bool isPasting)
    {
        var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);
        var hasSelection = CursorIndices != SelectionIndices;

        isPasting = false;

        if (IsReadOnly)
            return false;

        if (ctrl && Input.Typed == "c" && Input.TypedPrevious != "c")
        {
            Input.Clipboard = SelectedText;
            Input.onTextCopy?.Invoke();
            return true;
        }

        if (ctrl && Input.Typed == "v")
            isPasting = true;
        else if (hasSelection && ctrl && Input.Typed == "x")
        {
            Input.Clipboard = SelectedText;
            Input.onTextCopy?.Invoke();
            shouldDelete = true;
            TryDeleteSelected(ref justDeletedSelection, shouldDelete);
            return true;
        }

        return false;
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
    private char GetSymbol((int symbol, int line) indices)
    {
        var (x, y) = indices;
        return y < 0 || y >= lines.Count || x < 0 || x >= lines[y].Length ? default : lines[y][x];
    }
    private int GetWordEndOffset(int step)
    {
        if (SymbolMask != null)
            return step < 0 ? -cx : lines[cy].Length - cx;

        var index = step < 0 ? -1 : 0;
        var symbol = GetSymbol((cx + (step < 0 ? -1 : 0), cy));
        var targetIsWord = char.IsLetterOrDigit(symbol) == false;

        for (var i = 0; i < lines[cy].Length; i++)
        {
            var j = cx + index;

            if (j <= 0 && step < 0)
                return index;
            if (j >= lines[cy].Length - 1 && step > 0)
                return index + 1;

            if ((char.IsLetterOrDigit(GetSymbol((j, cy))) == false) ^ targetIsWord)
                return index + (step < 0 ? 1 : 0);

            index += step;
        }

        return step < 0 ? IndicesToIndex((0, cy)) : IndicesToIndex((lines[cy].Length, cy));
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
        var line = 0;

        foreach (var l in lines)
        {
            if (index < curIndex + l.Length)
            {
                var symbol = l.Length - (curIndex + l.Length - index);
                return (symbol, line);
            }

            curIndex += l.Length;
            line++;
        }

        return (0, 0);
    }

    private static void TryResetHoldTimers(out bool isHolding, bool isJustTyped)
    {
        var isAnyJustPressed = isJustTyped ||
                               JustPressed(Key.Enter) ||
                               JustPressed(Key.Backspace) ||
                               JustPressed(Key.Delete) ||
                               JustPressed(Key.ArrowLeft) ||
                               JustPressed(Key.ArrowRight) ||
                               JustPressed(Key.ArrowUp) ||
                               JustPressed(Key.ArrowDown);

        if (isAnyJustPressed)
            holdDelay.Restart();

        if (Input.IsAnyKeyPressed() &&
            holdDelay.Elapsed.TotalSeconds > HOLD_DELAY &&
            hold.Elapsed.TotalSeconds > HOLD)
        {
            hold.Restart();
            isHolding = true;
        }
        else
            isHolding = false;
    }

    private string RemoveForbiddenSymbols(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var result = new StringBuilder(str.Length);

        foreach (var symbol in str)
            if (IsAllowed(symbol.ToString()))
                result.Append(symbol);

        return result.ToString();
    }
#endregion
}