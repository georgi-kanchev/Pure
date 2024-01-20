﻿namespace Pure.Engine.UserInterface;

using System.Diagnostics;
using System.Text;

[Flags]
public enum SymbolGroup
{
    None = 0,
    Letters = 1,
    Digits = 2,
    Punctuation = 4,
    Math = 8,
    Special = 16,
    Space = 32,
    Other = 64,
    Password = 128,
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
    public bool IsReadOnly { get; set; }
    public bool IsSingleLine { get; set; }
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

        SymbolGroup = SymbolGroup.Letters |
                      SymbolGroup.Digits |
                      SymbolGroup.Punctuation |
                      SymbolGroup.Math |
                      SymbolGroup.Special |
                      SymbolGroup.Space |
                      SymbolGroup.Other;
    }
    public InputBox(byte[] bytes) : base(bytes)
    {
        Init();
        IsReadOnly = GrabBool(bytes);
        IsSingleLine = GrabBool(bytes);
        Placeholder = GrabString(bytes);
        Value = GrabString(bytes);
        SymbolGroup = (SymbolGroup)GrabByte(bytes);
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
        var result = base.ToBytes().ToList();
        PutBool(result, IsReadOnly);
        PutBool(result, IsSingleLine);
        PutString(result, Placeholder);
        PutString(result, Value);
        PutByte(result, (byte)SymbolGroup);
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
        var shift = Pressed(Key.ShiftLeft) || Pressed(Key.ShiftRight);

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

        for (var i = 0; i < Math.Abs(delta.y); i++)
            cy += delta.y < 0 ? -1 : 1;

        CursorIndices = (cx, cy);

        if (isScrolling)
            CursorScroll();

        SelectionIndices = shift && isSelecting ? SelectionIndices : CursorIndices;
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
    public void OnSubmit(Action method)
    {
        submit += method;
    }

    public static implicit operator string(InputBox inputBox)
    {
        return inputBox.ToBase64();
    }
    public static implicit operator InputBox(string base64)
    {
        return new(base64);
    }
    public static implicit operator byte[](InputBox inputBox)
    {
        return inputBox.ToBytes();
    }
    public static implicit operator InputBox(byte[] base64)
    {
        return new(base64);
    }

#region Backend
    private readonly List<string> lines = new() { string.Empty };
    private readonly Dictionary<string, bool> allowedSymbolsCache = new();

    private static readonly Dictionary<SymbolGroup, string> symbolSets = new()
    {
        { SymbolGroup.Letters, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" },
        { SymbolGroup.Digits, "0123456789." },
        { SymbolGroup.Punctuation, ",.;:!?-()[]{}\"'" },
        { SymbolGroup.Math, "+-*/=<>%(),^" },
        { SymbolGroup.Special, "@#&_|\\/^" },
        { SymbolGroup.Space, " " }
    };

    private const char SELECTION = '█', SPACE = ' ', PASSWORD = '*';
    private const float HOLD = 0.06f, HOLD_DELAY = 0.5f, CURSOR_BLINK = 1f;
    private static readonly Stopwatch holdDelay = new(),
        hold = new(),
        clickDelay = new(),
        cursorBlink = new(),
        scrollHold = new();
    private int cx, cy, sx, sy, clicks, scrX, scrY;
    private (int, int) lastClickIndices = (-1, -1), prevSize;
    private string value = string.Empty;

    private Action<string>? type;
    private Action? submit;
    private SymbolGroup symbolGroup;

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
        if (IsReadOnly == false && IsFocused && IsSingleLine && JustPressed(Key.Enter))
            submit?.Invoke();

        var isBellowElement = IsFocused == false || Input.FocusedPrevious != this;
        if (isBellowElement || TrySelectAll() || JustPressed(Key.Tab))
            return;

        var isJustTyped = JustTyped();

        TryResetHoldTimers(out var isHolding, isJustTyped);

        var isAllowedType = isJustTyped || (isHolding && Input.Typed != string.Empty);
        var shouldDelete = isAllowedType ||
                           Allowed(Key.Backspace, isHolding) ||
                           Allowed(Key.Delete, isHolding) ||
                           (Allowed(Key.Enter, isHolding) && IsSingleLine == false);

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
        CursorMove(ctrl ? (-delta, 0) : (0, -delta), true);
    }

    private void UpdateTextAndValue()
    {
        UpdateText();
        UpdateValue();
    }
    private void UpdateText()
    {
        var str = new StringBuilder();
        var maxW = Size.width - 1;
        var maxY = Math.Min(scrY + Size.height, lines.Count);
        for (var i = scrY; i < maxY; i++)
        {
            var newLine = i == scrY ? string.Empty : Environment.NewLine;
            var line = lines[i];
            var secondIndex = Math.Min(line.Length, scrX + maxW + 1);
            var result = scrX >= secondIndex ? string.Empty : line[scrX..secondIndex];

            if (SymbolGroup.HasFlag(SymbolGroup.Password))
                result = new(PASSWORD, result.Length);

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

            if (j <= 0 && step < 0)
                return index;
            if (j >= lines[cy].Length - 1 && step > 0)
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
        var (hx, hy) = Input.Position;
        var ix = (int)Math.Round(scrX + hx - Position.x);
        var iy = (int)Math.Clamp(scrY + hy - Position.y, 0, lines.Count - 1);
        var hasMoved = Input.PositionPrevious != Input.Position;

        if (Input.IsPressed)
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

            if (Input.IsJustPressed == false)
                return;

            // click
            MoveCursor();
            SelectionIndices = CursorIndices;

            return;

            void MoveCursor()
            {
                cy = iy;
                cx = ix;
                CursorIndices = (cx, cy);
                CursorScroll();
            }
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

        if (px > x + w) cx += 1;
        else if (px < x) cx -= 1;
        if (py > y + h) cy += 1;
        else if (py < y) cy -= 1;

        CursorIndices = (cx, cy);
        CursorScroll();
        scrollHold.Restart();
    }
    private void TryCycleSelected()
    {
        var (x, y) = Position;
        var (hx, hy) = Input.Position;
        var selectionIndices = PositionToIndices(((int)hx, (int)hy));

        clicks = clicks == 4 ? 1 : clicks + 1;

        if (clicks == 1)
        {
            var ix = (int)Math.Round(scrX + hx - Position.x);
            var iy = (int)Math.Clamp(scrY + hy - Position.y, 0, lines.Count - 1);
            CursorIndices = (ix, iy);
            CursorScroll();
            SelectionIndices = (ix, iy);
        }
        else if (clicks == 2)
        {
            cx += GetWordEndOffset(1);
            CursorIndices = (cx, cy);
            CursorScroll();
            SelectionIndices = (cx + GetWordEndOffset(-1), selectionIndices.line);
        }
        else if (clicks == 3)
        {
            var p = PositionToIndices((x + lines[cy].Length, y + cy));
            CursorIndices = (p.symbol, cy);
            CursorScroll();
            SelectionIndices = (0, cy);
        }
        else if (clicks == 4)
        {
            CursorIndices = (int.MaxValue, int.MaxValue);
            CursorScroll();
            SelectionIndices = (0, 0);
        }
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
        if (isAllowedType == false || IsReadOnly)
            return;

        var symbols = Input.Typed ?? string.Empty;
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

            var copied = TextCopied.Replace(Environment.NewLine, string.Empty);
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

        if (Allowed(Key.Enter, isHolding) && IsSingleLine == false)
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

        if (ctrl && Input.Typed == "c")
        {
            TextCopied = SelectedText;
            return true;
        }
        else if (ctrl && Input.Typed == "v")
            isPasting = true;
        else if (hasSelection && ctrl && Input.Typed == "x")
        {
            TextCopied = SelectedText;
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
                               JustPressed(Key.Backspace) ||
                               JustPressed(Key.Delete) ||
                               JustPressed(Key.ArrowLeft) ||
                               JustPressed(Key.ArrowRight) ||
                               JustPressed(Key.ArrowUp) ||
                               JustPressed(Key.ArrowDown);

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