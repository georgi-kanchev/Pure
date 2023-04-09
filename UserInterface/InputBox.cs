namespace Pure.UserInterface;

using System.Diagnostics;
using System.Text;

public class InputBox : UserInterface
{
	public string Placeholder { get; set; } = "";

	protected int CursorIndexX
	{
		get => cx;
		set => cx = Math.Clamp(value, 0, lines[CursorIndexY].Length);
	}
	protected int CursorIndexY
	{
		get => cy;
		set => cy = Math.Clamp(value, 0, lines.Count - 1);
	}
	protected int CursorIndex => cy * Size.Item1 + cx;
	protected int SelectionIndex
	{
		get => selectionIndex;
		set => selectionIndex = Math.Clamp(value, 0, Size.Item1 * Size.Item2);
	}
	protected string SelectedText
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

	public InputBox((int, int) position, (int, int) size) : base(position, size)
	{
		lines.Add("");
	}

	public void SetLine(int lineIndex, string text)
	{
		if (lineIndex < 0 && lineIndex >= lines.Count)
			throw new IndexOutOfRangeException(nameof(lineIndex));

		text ??= "";
		lines[lineIndex] = text;
	}
	public string GetLine(int lineIndex)
	{
		return lineIndex < 0 && lineIndex >= lines.Count ? "" : lines[lineIndex];
	}
	public void GetGraphics(out string cursor, out string selection, out string placeholder)
	{
		var totalTextLength = 0;
		for (int i = 0; i < lines.Count; i++)
			totalTextLength += lines[i].Length;

		var c = new string(SPACE, CursorIndex) + CURSOR +
			new string(SPACE, Size.Item1 * Size.Item2 - CursorIndex);
		var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
		var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;
		var sz = b - a;

		cursor = IsFocused ? c : "";
		selection = totalTextLength == 0 ? "" :
			new string(SPACE, a) + new string(SELECTION, sz) +
			new string(SPACE, Math.Clamp(totalTextLength - b, 0, totalTextLength));
		placeholder = totalTextLength > 0 ? "" : Placeholder;
	}

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
	protected (int, int) PositionFromIndex(int index)
	{
		var (px, py) = Position;
		var (w, h) = Size;
		index = Math.Clamp(index, 0, w * h - 1);
		var (x, y) = (index % w, index / w);
		var newY = Math.Clamp(y, 0, lines.Count - 1);
		var newX = Math.Clamp(x, 0, lines[newY].Length);

		return (px + newX, py + newY);
	}

	protected void MoveCursor(int offsetX, bool allowSelection = false, int offsetY = 0)
	{
		var shift = CurrentInput.IsKeyPressed(Input.SHIFT_LEFT) ||
			CurrentInput.IsKeyPressed(Input.SHIFT_RIGHT);

		if ((CursorIndexY == 0 && offsetY < 0) || (CursorIndexY == Size.Item2 - 1 && offsetY > 0))
			return;

		if (CursorIndexX == 0 && offsetX < 0)
		{
			offsetX = CursorIndexY == 0 ? 0 : Size.Item1 + offsetX;
			offsetY = -1;
		}
		else if (CursorIndexX == lines[CursorIndexY].Length && offsetX > 0)
		{
			offsetX = CursorIndexY == lines.Count - 1 ? 0 : -Size.Item1 + offsetX;
			offsetY = 1;
		}

		CursorIndexY += offsetY; // y first cuz X uses it
		CursorIndexX += offsetX;

		SelectionIndex = shift && allowSelection ? SelectionIndex : CursorIndex;
		SelectionIndex = Math.Clamp(SelectionIndex, 0, Text.Length);
	}

	protected override void OnUpdate()
	{
		var (x, y) = Position;
		var (w, h) = Size;

		TrySetMouseCursor();

		if (IsDisabled || IsFocused == false || TrySelectAll() || JustPressed(Input.TAB))
			return;

		var isJustTyped = JustTyped();

		TryResetHoldTimers(out var isHolding, isJustTyped);

		var isAllowedType = isJustTyped || (isHolding && CurrentInput.Typed != "");
		var shouldDelete = isAllowedType || Allowed(Input.BACKSPACE, isHolding) ||
			Allowed(Input.DELETE, isHolding) || Allowed(Input.ENTER, isHolding);

		var justDeletedSelected = false;
		if (TryCopyPasteCut(ref justDeletedSelected, ref shouldDelete, out var isPasting))
			return;

		TrySelect();
		TryDeleteSelected(ref justDeletedSelected, shouldDelete);
		TryType(isHolding, justDeletedSelected, isAllowedType, isPasting);
		TryDeleteEnter(isHolding, justDeletedSelected);
		TryMoveCursor(isHolding);
		UpdateText();
	}

	#region Backend
	private readonly List<string> lines = new();

	private const char SELECTION = '█', CURSOR = '▏', SPACE = ' ';
	private const float HOLD = 0.06f, HOLD_DELAY = 0.5f;
	private static readonly Stopwatch holdDelay = new(), hold = new(), clickDelay = new();
	private int selectionIndex, cx, cy, clicks;
	private (int, int) lastClickIndices = (-1, -1);

	static InputBox()
	{
		holdDelay.Start();
		hold.Start();
	}

	private void UpdateText()
	{
		var sb = new StringBuilder();

		var maxW = Size.Item1 - 1;
		for (int i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			line = line.Length > maxW ? line[0..maxW] : line;
			sb.Append(line.PadRight(Size.Item1, ' '));
		}

		Text = sb.ToString();
	}

	private static bool Allowed(int key, bool isHolding) => JustPressed(key) || (Pressed(key) && isHolding);
	private static bool JustPressed(int key) => CurrentInput.IsKeyJustPressed(key);
	private static bool Pressed(int key) => CurrentInput.IsKeyPressed(key);
	private static bool JustTyped()
	{
		var typed = CurrentInput.Typed ?? "";
		var prev = CurrentInput.TypedPrevious ?? "";

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
		var x = Position.Item1;
		var index = 0 + (step < 0 ? -1 : 0);
		var startY = PositionFromIndex(CursorIndex).Item2;
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

			if (startY != curY || curX - x >= lines[CursorIndexY].Length)
			{
				index -= step - (step < 0 ? 1 : 0);
				return index;
			}
		}

		return Text.Length * step;
	}

	private void TrySelect()
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var isSamePosClick = false;
		var (hx, hy) = CurrentInput.Position;
		var ix = (int)Math.Round(hx - Position.Item1);
		var iy = (int)Math.Clamp(hy - Position.Item2, 0, lines.Count - 1);

		if (CurrentInput.IsPressed)
		{
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
			if (CurrentInput.IsPressed)
			{
				CursorIndexY = iy;
				iy = CursorIndexY;
				CursorIndexX = ix;
				ix = CursorIndexX;
			}

			if (CurrentInput.IsJustPressed)
				SelectionIndex = CursorIndex;

			return;
		}

		if (CurrentInput.IsJustPressed)
			TryCycleSelected(ix, iy);
	}
	private void TryCycleSelected(int indexX, int indexY)
	{
		var (x, y) = Position;
		var (w, h) = Size;
		var (hx, hy) = CurrentInput.Position;

		clicks = clicks == 4 ? 1 : clicks + 1;

		if (clicks == 1)
		{
			CursorIndexY = indexY;
			indexY = CursorIndexY;
			CursorIndexX = indexX;
			SelectionIndex = PositionToIndex((hx, hy));
		}
		else if (clicks == 2)
		{
			SelectionIndex = PositionToIndex((hx, hy)) + GetWordEndOffset(1);
			CursorIndexX += GetWordEndOffset(-1);
			CursorIndexX = Math.Clamp(CursorIndexX, 0, lines[indexY].Length);
		}
		else if (clicks == 3)
		{
			var p = PositionToIndex((x + lines[CursorIndexY].Length, y + CursorIndexY));
			SelectionIndex = p;
			CursorIndexX = 0;
		}
		else if (clicks == 4)
		{
			SelectionIndex = w * h;
			CursorIndexY = 0;
			CursorIndexX = 0;
		}
	}
	private bool TrySelectAll()
	{
		var ctrl = Pressed(Input.CONTROL_LEFT) || Pressed(Input.CONTROL_RIGHT);

		if (ctrl == false || CurrentInput.Typed != "a")
			return false;

		var (w, h) = Size;
		SelectionIndex = w * h;
		CursorIndexX = 0;
		CursorIndexY = 0;
		return true;
	}

	private void TryMoveCursor(bool isHolding)
	{
		var ctrl = Pressed(Input.CONTROL_LEFT) || Pressed(Input.CONTROL_RIGHT);
		var shift = Pressed(Input.SHIFT_LEFT) || Pressed(Input.SHIFT_RIGHT);
		var i = CursorIndex;
		var s = SelectionIndex;
		var hasSel = shift == false && i != s;
		var (cx, cy) = PositionFromIndex(i);
		var (x, y) = Position;
		var selectionCursorDiff = i > s ? i - s : s - i;
		var justL = JustPressed(Input.ARROW_LEFT);
		var justR = JustPressed(Input.ARROW_RIGHT);
		var (ix, iy) = PositionFromIndex(i);
		var (sx, sy) = PositionFromIndex(s);

		var hotkeys = new (bool, (int, int))[]
		{
				(hasSel && justL, iy == sy ? (i < s ? 0 : s - i, 0) : i < s ? (0, 0) : (sx - ix, sy - iy)),
				(hasSel && justR, iy == sy ? (i > s ? 0 : s - i, 0) : i > s ? (0, 0) : (sx - ix, sy - iy)),
				(ctrl && JustPressed(Input.ARROW_UP), (-CursorIndexX, 0)),
				(ctrl && JustPressed(Input.ARROW_DOWN), (Size.Item1, 0)),
				(JustPressed(Input.HOME), (-CursorIndexX, -CursorIndexY)),
				(JustPressed(Input.END), Size),
				(Allowed(Input.ARROW_LEFT, isHolding), (ctrl ? GetWordEndOffset(-1) : -1, 0)),
				(Allowed(Input.ARROW_RIGHT, isHolding), (ctrl ? GetWordEndOffset(1) : 1, 0)),
				(Allowed(Input.ARROW_UP, isHolding), (0, -1)),
				(Allowed(Input.ARROW_DOWN, isHolding), (0, 1)),
		};

		for (int j = 0; j < hotkeys.Length; j++)
			if (hotkeys[j].Item1)
			{
				MoveCursor(hotkeys[j].Item2.Item1, true, hotkeys[j].Item2.Item2);
				return;
			}
	}

	private void TryType(bool isHolding, bool justDeletedSelection, bool isAllowedType, bool isPasting)
	{
		var (x, y) = Position;
		var (w, h) = Size;

		if (isAllowedType)
		{
			var symbols = CurrentInput.Typed ?? "";

			if (isPasting && string.IsNullOrWhiteSpace(TextCopied) == false)
			{
				symbols = TextCopied;

				// crop paste to fit in line
				var pasteLength = Math.Min(w - lines[CursorIndexY].Length - 1, symbols.Length - 1);
				symbols = symbols[..pasteLength];

				lines[CursorIndexY] = lines[CursorIndexY].Insert(CursorIndexX, symbols);
				MoveCursor(symbols.Length);
				return;
			}

			// is at end of current line, not text
			if (CursorIndexX >= w - 1 && CursorIndexY != h - 1)
			{
				lines.Add("");
				MoveCursor(1);
			}

			if (lines[CursorIndexY].Length + 1 < w)
			{
				var t = symbols.Length > 1 ? symbols[^1].ToString() : symbols;
				lines[CursorIndexY] = lines[CursorIndexY].Insert(CursorIndexX, t);
				MoveCursor(1);
				SelectionIndex = CursorIndex;
			}

			return;
		}
	}
	private void TryDeleteEnter(bool isHolding, bool justDeletedSelection)
	{
		var (x, y) = Position;
		var (w, h) = Size;

		if (Allowed(Input.ENTER, isHolding) &&
			CursorIndexY != y + h - 1) // not last line
		{
			// no space for new line? bail
			if (lines.Count == h)
				return;

			// insert line above?
			if (CursorIndexX == 0)
			{
				lines.Insert(CursorIndexY, "");
				MoveCursor(0, false, 1);
				return;
			}

			var textForNewLine = lines[CursorIndexY][CursorIndexX..];

			lines[CursorIndexY] = lines[CursorIndexY][..CursorIndexX];
			lines.Insert(CursorIndexY + 1, textForNewLine);

			CursorIndexY++;
			CursorIndexX = 0;
			SelectionIndex = CursorIndex;
		}
		else if (Allowed(Input.BACKSPACE, isHolding) && justDeletedSelection == false)
		{
			// cursor is at start of current line
			if (CursorIndexX == 0)
			{
				// first line?
				if (CursorIndexY == 0)
					return;

				TryMergeBottomLine(CursorIndexY - 1);
				return;
			}

			var off = GetWordEndOffset(-1, true);
			var ctrl = Pressed(Input.CONTROL_LEFT);
			var count = ctrl ? Math.Abs(off) : 1;
			lines[CursorIndexY] = lines[CursorIndexY].Remove(
				ctrl ? CursorIndexX + off : CursorIndexX - 1, count);
			MoveCursor(ctrl ? off : -1);
		}
		else if (Allowed(Input.DELETE, isHolding) && justDeletedSelection == false)
		{
			// cursor is at end of current line
			if (CursorIndexX == lines[CursorIndexY].Length)
			{
				// last line?
				if (CursorIndexY == w - 1)
					return;

				TryMergeBottomLine(CursorIndexY);
				return;
			}

			var off = GetWordEndOffset(1, true);
			var off2 = GetWordEndOffset(1, false);
			var ctrl = Pressed(Input.CONTROL_LEFT);
			var count = ctrl ? Math.Abs(off) : 1;
			lines[CursorIndexY] = lines[CursorIndexY].Remove(ctrl ? CursorIndexX : CursorIndexX, count);
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
		var (ixa, iya) = (pa.Item1 - x, Math.Clamp(pa.Item2 - y, 0, lines.Count - 1));
		var (ixb, iyb) = (pb.Item1 - x, Math.Clamp(pb.Item2 - y, 0, lines.Count - 1));
		var (w, h) = Size;

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
		CursorIndexY = iya;
		CursorIndexX = ixa;
		SelectionIndex = CursorIndex;
		justDeletedSelection = true;
	}

	private void TryMergeBottomLine(int lineIndex)
	{
		// try to fit bottom line onto this line, leave the rest
		var iya = lineIndex;
		var iyb = lineIndex + 1;
		var lineLength = lines[iya].Length;
		var upLineFreeSpace = Math.Min(Size.Item1 - lines[iya].Length - 1, lines[iyb].Length);
		lines[iya] += lines[iyb][..upLineFreeSpace];
		lines[iyb] = lines[iyb][upLineFreeSpace..];

		TryRemoveEmptyLinesInRange(iyb, iyb);

		CursorIndexY = iya;
		CursorIndexX = lineLength;
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
		if (IsDisabled == false && (IsHovered || IsClicked))
			SetMouseCursor(MouseCursor.TILE_TEXT);
	}
	private bool TryCopyPasteCut(ref bool justDeletedSelection, ref bool shouldDelete, out bool isPasting)
	{
		var ctrl = Pressed(Input.CONTROL_LEFT) || Pressed(Input.CONTROL_RIGHT);
		var (x, y) = Position;
		var hasSelection = CursorIndex != SelectionIndex;

		isPasting = false;

		if (ctrl && CurrentInput.Typed == "c")
		{
			TextCopied = SelectedText;
			return true;
		}
		else if (ctrl && CurrentInput.Typed == "v")
			isPasting = true;
		else if (hasSelection && ctrl && CurrentInput.Typed == "x")
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
			JustPressed(Input.ENTER) ||
			JustPressed(Input.BACKSPACE) || JustPressed(Input.DELETE) ||
			JustPressed(Input.ARROW_LEFT) || JustPressed(Input.ARROW_RIGHT) ||
			JustPressed(Input.ARROW_UP) || JustPressed(Input.ARROW_DOWN);

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
