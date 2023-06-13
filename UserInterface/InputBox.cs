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
	public string Placeholder { get; set; } = "Type...";

	public string Selection
	{
		get
		{
			var totalTextLength = 0;
			for (int i = 0; i < lines.Count; i++)
				totalTextLength += lines[i].Length;

			var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
			var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;
			var sz = b - a;

			var selection = totalTextLength == 0 ? "" :
				new string(SPACE, a) + new string(SELECTION, sz) +
				new string(SPACE, Math.Clamp(totalTextLength - b, 0, totalTextLength));
			return selection;
		}
	}
	/// <summary>
	/// Gets or sets the zero-based index of the first character in the current 
	/// selection of the input box's text.
	/// </summary>
	public int SelectionIndex
	{
		get => selectionIndex;
		set => selectionIndex = Math.Clamp(value, 0, Size.width * Size.height);
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

			var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
			var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;
			return Text[a..b];
		}
	}

	public bool IsCursorVisible => IsFocused && cursorBlink.Elapsed.TotalSeconds <= CURSOR_BLINK / 2f;
	public (int x, int y) CursorPosition => PositionFromIndex(CursorIndex);
	/// <summary>
	/// Gets or sets the zero-based horizontal index of the cursor in the input box's text.
	/// </summary>
	public int CursorIndexSymbol
	{
		get => cx;
		set => cx = Math.Clamp(value, 0, lines[CursorIndexLine].Length);
	}
	/// <summary>
	/// Gets or sets the zero-based vertical index of the line containing the cursor in the input box's text.
	/// </summary>
	public int CursorIndexLine
	{
		get => cy;
		set => cy = Math.Clamp(value, 0, lines.Count - 1);
	}
	/// <summary>
	/// Gets the zero-based index of the character at the cursor's position in the input box's text.
	/// </summary>
	public int CursorIndex => cy * Size.width + cx;

	/// <summary>
	/// Initializes a new input box instance with a specific position and default size of (12, 1).
	/// </summary>
	/// <param name="position">The position of the input box.</param>
	public InputBox((int x, int y) position) : base(position)
	{
		lines.Add("");
		Size = (12, 1);
	}
	public InputBox(byte[] bytes) : base(bytes)
	{
		Placeholder = GrabString(bytes);
	}

	/// <summary>
	/// Sets the text of a specific line in the input box.
	/// </summary>
	/// <param name="lineIndex">The zero-based index of the line to set the text of.</param>
	/// <param name="text">The new text for the line.</param>
	public void SetLine(int lineIndex, string text)
	{
		if (lineIndex < 0 && lineIndex >= lines.Count)
			return;

		text ??= "";
		lines[lineIndex] = text;
	}
	/// <summary>
	/// Gets the text of a specific line in the input box.
	/// </summary>
	/// <param name="lineIndex">The zero-based index of the line to get the text of.</param>
	/// <returns>The text of the specified line.</returns>
	public string TextAt(int lineIndex)
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
	protected int PositionToIndex((float, float) position)
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
	protected (int x, int y) PositionFromIndex(int index)
	{
		var (px, py) = Position;
		var (w, h) = Size;
		index = Math.Clamp(index, 0, w * h - 1);
		var (x, y) = (index % w, index / w);
		var newY = Math.Clamp(y, 0, lines.Count - 1);
		var newX = Math.Clamp(x, 0, lines[newY].Length);

		return (px + newX, py + newY);
	}

	/// <summary>
	/// Moves the cursor by the given offsets, optionally allowing selection.
	/// </summary>
	/// <param name="offsetX">The X offset to move the cursor.</param>
	/// <param name="allowSelection">Whether or not selection is allowed.</param>
	/// <param name="offsetY">The Y offset to move the cursor.</param>
	protected void MoveCursor(int offsetX, bool allowSelection = false, int offsetY = 0)
	{
		var shift = Input.Current.IsKeyPressed(Key.ShiftLeft) ||
			Input.Current.IsKeyPressed(Key.ShiftRight);

		cursorBlink.Restart();

		if ((CursorIndexLine == 0 && offsetY < 0) || (CursorIndexLine == Size.width - 1 && offsetY > 0))
			return;

		if (CursorIndexSymbol == 0 && offsetX < 0)
		{
			offsetX = CursorIndexLine == 0 ? 0 : Size.width + offsetX;
			offsetY = -1;
		}
		else if (CursorIndexSymbol == lines[CursorIndexLine].Length && offsetX > 0)
		{
			offsetX = CursorIndexLine == lines.Count - 1 ? 0 : -Size.width + offsetX;
			offsetY = 1;
		}

		CursorIndexLine += offsetY; // y first cuz X uses it
		CursorIndexSymbol += offsetX;

		SelectionIndex = shift && allowSelection ? SelectionIndex : CursorIndex;
		SelectionIndex = Math.Clamp(SelectionIndex, 0, Text.Length);
	}

	/// <summary>
	/// Called when the input box needs to be updated. This handles all of the user input
	/// the input box needs for its behavior. Subclasses should override this 
	/// method to implement their own behavior.
	/// </summary>
	protected override void OnUpdate()
	{
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
		TryDeleteEnter(isHolding, justDeletedSelected);
		TryMoveCursor(isHolding);
		UpdateText();
	}

	#region Backend
	private readonly List<string> lines = new();

	private const char SELECTION = '█', SPACE = ' ';
	private const float HOLD = 0.06f, HOLD_DELAY = 0.5f, CURSOR_BLINK = 1f;
	private static readonly Stopwatch holdDelay = new(), hold = new(), clickDelay = new(), cursorBlink = new();
	private int selectionIndex, cx, cy, clicks;
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
		for (int i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			line = line.Length > maxW ? line[0..maxW] : line;
			sb.Append(line.PadRight(Size.width, ' '));
		}

		Text = sb.ToString();
	}

	private static bool Allowed(Key key, bool isHolding) => JustPressed(key) || (Pressed(key) && isHolding);
	private static bool JustPressed(Key key) => Input.Current.IsKeyJustPressed(key);
	private static bool Pressed(Key key) => Input.Current.IsKeyPressed(key);
	private static bool JustTyped()
	{
		var typed = Input.Current.Typed ?? "";
		var prev = Input.Current.TypedPrevious ?? "";

		for (int i = 0; i < typed.Length; i++)
		{
			if (prev.Contains(typed[i]) == false)
				return true;
		}
		return false;
	}

	private char GetSymbol(int index)
	{
		return Text.Length == 0 ? default : Text[Math.Clamp(index, 0, Text.Length - 1)];
	}
	private int GetWordEndOffset(int step, bool stopAtLineEnd = true)
	{
		var x = Position.x;
		var index = 0 + (step < 0 ? -1 : 0);
		var startY = PositionFromIndex(CursorIndex).y;
		var symb = GetSymbol(CursorIndex + (step < 0 ? -1 : 0));
		var targetIsWord = char.IsLetterOrDigit(symb) == false;

		for (int i = 0; i < Text.Length; i++)
		{
			var j = CursorIndex + index;
			var (curX, curY) = PositionFromIndex(j);

			if ((j == 0 && step < 0) || (j == Text.Length && step > 0))
				return index;

			if (char.IsLetterOrDigit(GetSymbol(j)) == false ^ targetIsWord)
				return index + (step < 0 ? 1 : 0);

			index += step;

			// stop at start/end of line
			if (stopAtLineEnd == false)
				continue;

			if (startY != curY || curX - x >= lines[CursorIndexLine].Length)
			{
				index -= step - (step < 0 ? 1 : 0);
				return index;
			}
		}

		return Text.Length * step;
	}

	private void TrySelect()
	{
		var isSamePosClick = false;
		var (hx, hy) = Input.Current.Position;
		var ix = (int)Math.Round(hx - Position.x);
		var iy = (int)Math.Clamp(hy - Position.y, 0, lines.Count - 1);

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
				CursorIndexLine = iy;
				CursorIndexSymbol = ix;
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
			CursorIndexLine = indexY;
			CursorIndexSymbol = indexX;
			SelectionIndex = PositionToIndex((hx, hy));
		}
		else if (clicks == 2)
		{
			SelectionIndex = PositionToIndex((hx, hy)) + GetWordEndOffset(1);
			CursorIndexSymbol += GetWordEndOffset(-1);
			CursorIndexSymbol = Math.Clamp(CursorIndexSymbol, 0, lines[indexY].Length);
		}
		else if (clicks == 3)
		{
			var p = PositionToIndex((x + lines[CursorIndexLine].Length, y + CursorIndexLine));
			SelectionIndex = p;
			CursorIndexSymbol = 0;
		}
		else if (clicks == 4)
		{
			SelectionIndex = w * h;
			CursorIndexLine = 0;
			CursorIndexSymbol = 0;
		}
	}
	private bool TrySelectAll()
	{
		var ctrl = Pressed(Key.ControlLeft) || Pressed(Key.ControlRight);

		if (ctrl == false || Input.Current.Typed != "a")
			return false;

		var (w, h) = Size;
		SelectionIndex = w * h;
		CursorIndexSymbol = 0;
		CursorIndexLine = 0;
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

		var hotkeys = new (bool, (int, int))[]
		{
				(hasSel && justL, iy == sy ? (i < s ? 0 : s - i, 0) : i < s ? (0, 0) : (sx - ix, sy - iy)),
				(hasSel && justR, iy == sy ? (i > s ? 0 : s - i, 0) : i > s ? (0, 0) : (sx - ix, sy - iy)),
				(ctrl && JustPressed(Key.ArrowUp), (-CursorIndexSymbol, 0)),
				(ctrl && JustPressed(Key.ArrowDown), (Size.width, 0)),
				(JustPressed(Key.Home), (-CursorIndexSymbol, -CursorIndexLine)),
				(JustPressed(Key.End), Size),
				(Allowed(Key.ArrowLeft, isHolding), (ctrl ? GetWordEndOffset(-1) : -1, 0)),
				(Allowed(Key.ArrowRight, isHolding), (ctrl ? GetWordEndOffset(1) : 1, 0)),
				(Allowed(Key.ArrowUp, isHolding), (0, -1)),
				(Allowed(Key.ArrowDown, isHolding), (0, 1)),
		};

		for (int j = 0; j < hotkeys.Length; j++)
			if (hotkeys[j].Item1)
			{
				MoveCursor(hotkeys[j].Item2.Item1, true, hotkeys[j].Item2.Item2);
				return;
			}
	}

	private void TryType(bool isAllowedType, bool isPasting)
	{
		var (w, h) = Size;

		if (isAllowedType)
		{
			var symbols = Input.Current.Typed ?? "";

			if (isPasting && string.IsNullOrWhiteSpace(TextCopied) == false)
			{
				symbols = TextCopied;

				// crop paste to fit in line
				var pasteLength = Math.Min(w - lines[CursorIndexLine].Length - 1, symbols.Length);
				symbols = symbols[..pasteLength];

				lines[CursorIndexLine] = lines[CursorIndexLine].Insert(CursorIndexSymbol, symbols);
				MoveCursor(symbols.Length);
				return;
			}

			// is at end of current line, not text
			if (CursorIndexSymbol >= w - 1 && CursorIndexLine != h - 1)
			{
				lines.Add("");
				MoveCursor(1);
			}

			if (lines[CursorIndexLine].Length + 1 < w)
			{
				var t = symbols.Length > 1 ? symbols[^1].ToString() : symbols;
				lines[CursorIndexLine] = lines[CursorIndexLine].Insert(CursorIndexSymbol, t);
				MoveCursor(1);
				SelectionIndex = CursorIndex;
			}

			return;
		}
	}
	private void TryDeleteEnter(bool isHolding, bool justDeletedSelection)
	{
		var (_, y) = Position;
		var (w, h) = Size;

		if (Allowed(Key.Enter, isHolding) &&
			CursorIndexLine != y + h - 1) // not last line
		{
			// no space for new line? bail
			if (lines.Count == h)
				return;

			// insert line above?
			if (CursorIndexSymbol == 0)
			{
				lines.Insert(CursorIndexLine, "");
				MoveCursor(0, false, 1);
				return;
			}

			var textForNewLine = lines[CursorIndexLine][CursorIndexSymbol..];

			lines[CursorIndexLine] = lines[CursorIndexLine][..CursorIndexSymbol];
			lines.Insert(CursorIndexLine + 1, textForNewLine);

			CursorIndexLine++;
			CursorIndexSymbol = 0;
			SelectionIndex = CursorIndex;
		}
		else if (Allowed(Key.Backspace, isHolding) && justDeletedSelection == false)
		{
			// cursor is at start of current line
			if (CursorIndexSymbol == 0)
			{
				// first line?
				if (CursorIndexLine == 0)
					return;

				TryMergeBottomLine(CursorIndexLine - 1);
				return;
			}

			var off = GetWordEndOffset(-1, true);
			var ctrl = Pressed(Key.ControlLeft);
			var count = ctrl ? Math.Abs(off) : 1;
			lines[CursorIndexLine] = lines[CursorIndexLine].Remove(
				ctrl ? CursorIndexSymbol + off : CursorIndexSymbol - 1, count);
			MoveCursor(ctrl ? off : -1);
		}
		else if (Allowed(Key.Delete, isHolding) && justDeletedSelection == false)
		{
			// cursor is at end of current line
			if (CursorIndexSymbol == lines[CursorIndexLine].Length)
			{
				// last line?
				if (CursorIndexLine == w - 1)
					return;

				TryMergeBottomLine(CursorIndexLine);
				return;
			}

			var off = GetWordEndOffset(1, true);
			var ctrl = Pressed(Key.ControlLeft);
			var count = ctrl ? Math.Abs(off) : 1;
			lines[CursorIndexLine] = lines[CursorIndexLine].Remove(ctrl ? CursorIndexSymbol : CursorIndexSymbol, count);
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
		var (w, _) = Size;

		for (int i = iya; i <= iyb; i++)
		{
			// single line selected
			if (iya == iyb)
			{
				lines[i] = lines[i][..ixa] + lines[i][ixb..];
				break;
			}

			// multiline selected...
			// first selected line
			if (i == iya)
				lines[i] = lines[i][..ixa];
			// last selected line
			else if (i == iyb)
			{
				lines[i] = lines[i][ixb..];

				// try to fit last line onto first line, leave the rest
				var firstLineFreeSpace = Math.Min(w - lines[iya].Length - 1, lines[i].Length);
				lines[iya] += lines[i][..firstLineFreeSpace];
				lines[i] = lines[i][firstLineFreeSpace..];

				TryRemoveEmptyLinesInRange(iya, iyb);
			}
			// line between first & last
			else
				lines[i] = "";
		}

		// y first cuz x uses it
		CursorIndexLine = iya;
		CursorIndexSymbol = ixa;
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
		// try to fit bottom line onto this line, leave the rest
		var iya = lineIndex;
		var iyb = lineIndex + 1;
		var lineLength = lines[iya].Length;
		var upLineFreeSpace = Math.Min(Size.width - lines[iya].Length - 1, lines[iyb].Length);
		lines[iya] += lines[iyb][..upLineFreeSpace];
		lines[iyb] = lines[iyb][upLineFreeSpace..];

		TryRemoveEmptyLinesInRange(iyb, iyb);

		CursorIndexLine = iya;
		CursorIndexSymbol = lineLength;
		SelectionIndex = CursorIndex;
	}
	private void TryRemoveEmptyLinesInRange(int lineStart, int lineEnd)
	{
		if (lineEnd > lines.Count - 1)
			return;

		for (int j = lineEnd; j >= lineStart; j--)
			if (lines[j] == "" && lines.Count > 1)
				lines.RemoveAt(j);
	}
	private void TrySetMouseCursor()
	{
		if (IsDisabled == false && (IsHovered || IsPressedAndHeld))
			MouseCursorResult = MouseCursor.Text;
	}
	private bool TryCopyPasteCut(ref bool justDeletedSelection, ref bool shouldDelete, out bool isPasting)
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
