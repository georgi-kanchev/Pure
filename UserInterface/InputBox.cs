namespace Pure.UserInterface;

using System.Diagnostics;

public class InputBox : UserInterface
{
	public string Placeholder { get; set; } = "";

	public InputBox((int, int) position, (int, int) size) : base(position, size) { }
	static InputBox()
	{
		holdDelay.Start();
		hold.Start();
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
		var index = Math.Clamp(y, 0, h) * w + Math.Clamp(x, 0, lines[(int)y].Length);

		return (int)Math.Clamp(index, 0, Text.Length);
	}
	protected (int, int) PositionFromIndex(int index)
	{
		var (px, py) = Position;
		var (w, h) = Size;
		var (x, y) = (index % w, index / w);

		return (Math.Clamp(px + x, px, px + w), Math.Clamp(py + y, py, py + h - 1));
	}

	protected override void OnUpdate()
	{
		var isControlPressed = Pressed(CONTROL_LEFT) || Pressed(CONTROL_RIGHT);
		var (x, y) = Position;
		var (w, h) = Size;

		while (lines.Count != h)
			lines.Add("");

		TrySetMouseCursor();

		if (IsDisabled || IsFocused == false || TrySelectAll() || JustPressed(TAB))
			return;

		var isPasting = false;
		var isHolding = false;
		var justDeletedSelection = false;
		var isJustTyped = IsJustTyped();

		TryResetHoldTimers();
		var isAllowedType = isJustTyped || (isHolding && CurrentInput.Typed != "");
		var shouldDelete = isAllowedType || Allowed(BACKSPACE) || Allowed(DELETE) || Allowed(ENTER);

		if (TryCopyPasteOrCut())
			return;

		TrySelect();
		TryDeleteSelected();
		TryTypeEnterDelete();
		TryMoveCursor();

		UpdateText();

		bool TryCopyPasteOrCut()
		{
			var hasSelection = CursorIndex != SelectionIndex;
			if (isControlPressed && CurrentInput.Typed == "c")
			{
				TextCopied = GetSelectedText();
				return true;
			}
			else if (isControlPressed && CurrentInput.Typed == "v")
				isPasting = true;
			else if (hasSelection && isControlPressed && CurrentInput.Typed == "x")
			{
				TextCopied = GetSelectedText();
				shouldDelete = true;
				TryDeleteSelected();
				return true;
			}
			return false;
		}
		void TrySetMouseCursor()
		{
			if (IsHovered)
				TrySetTileAndSystemCursor(TILE_TEXT);
		}
		void TryDeleteSelected()
		{
			var isSelected = SelectionIndex != CursorIndex;
			if (shouldDelete && isSelected)
			{
				var a = SelectionIndex < CursorIndex ? SelectionIndex : CursorIndex;
				var b = SelectionIndex > CursorIndex ? SelectionIndex : CursorIndex;
				var pa = PositionFromIndex(a);
				var pb = PositionFromIndex(b);
				var (ixa, iya) = (pa.Item1 - x, pa.Item2 - y);
				var (ixb, iyb) = (pb.Item1 - x, pb.Item2 - y);

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
						lines[i] = lines[i][ixb..];
					// line between first & last
					else
						lines[i] = "";
				}

				cursorIndexX = ixa;
				cursorIndexY = iya;
				SelectionIndex = CursorIndex;
				justDeletedSelection = true;
			}
		}
		void TryTypeEnterDelete()
		{
			var (cx, cy) = (cursorIndexX, cursorIndexY);

			if (isAllowedType)
			{
				var symbols = CurrentInput.Typed ?? "";

				//if (isPasting && string.IsNullOrWhiteSpace(TextCopied) == false)
				//{
				//	symbols = TextCopied;
				//	lines[cy] = lines[cy].Insert(cx, symbols);
				//	MoveLineCursor(symbols.Length);
				//	return;
				//}

				// is at end of current line
				if (cx >= w - 1)
				{
					CursorIndexX = 0;
					CursorIndexY++;
					cx = CursorIndexX;
					cy = CursorIndexY;
				}

				if (lines[cy].Length + 1 < w)
				{
					var t = symbols.Length > 1 ? symbols[^1].ToString() : symbols;
					lines[cy] = lines[cy].Insert(cx, t);
					MoveLineCursor(1);
				}

				return;
			}

			if (Allowed(ENTER) && justDeletedSelection == false &&
				cy != y + h - 1) // not last line
			{
				var spacesUntilEndOfLine = x + w - cx;
				Text = Text.Insert(cx, new string(SPACE, spacesUntilEndOfLine));
				MoveLineCursor(spacesUntilEndOfLine);
			}
			else if (Allowed(BACKSPACE) && justDeletedSelection == false &&
				lines[cy].Length > 0 && cx > 0)
			{
				var off = GetWordEndOffset(-1, true);
				var ctrl = Pressed(CONTROL_LEFT);
				var count = ctrl ? Math.Abs(off) : 1;
				lines[cy] = lines[cy].Remove(ctrl ? cx + off : cx - 1, count);
				MoveLineCursor(ctrl ? off : -1);
			}
			else if (Allowed(DELETE) && justDeletedSelection == false &&
				lines[cy].Length > 0 && cx < lines[cy].Length)
			{
				var off = GetWordEndOffset(1, true);
				var off2 = GetWordEndOffset(1, false);
				var ctrl = Pressed(CONTROL_LEFT);
				var count = ctrl ? Math.Abs(off) : 1;
				lines[cy] = lines[cy].Remove(ctrl ? cx : cx, count);
			}
		}
		void TryMoveCursor()
		{
			var ctrl = Pressed(CONTROL_LEFT) || Pressed(CONTROL_RIGHT);
			var shift = Pressed(SHIFT_LEFT) || Pressed(SHIFT_RIGHT);
			var i = CursorIndex;
			var s = SelectionIndex;
			var hasSelection = i != s;
			var (cx, cy) = PositionFromIndex(i);
			var (x, y) = Position;
			var selectionCursorDiff = i > s ? i - s : s - i;
			var hotkeys = new (bool, (int, int))[]
			{
				( shift == false && hasSelection && JustPressed(ARROW_LEFT), (i < s ? 0 : s - i, 0) ),
				( shift == false && hasSelection && JustPressed(ARROW_RIGHT), (i < s ? s - i : 0, 0) ),
				( ctrl && JustPressed(ARROW_UP), (cx - x - w, 0) ),
				( ctrl && JustPressed(ARROW_DOWN), (w - (cx - x) + w, 0) ),
				( JustPressed(HOME), (x - cx, 0) ),
				( JustPressed(END), (w - (cx - x), 0) ),
				( Allowed(ARROW_LEFT), (ctrl ? GetWordEndOffset(-1) : -1, 0) ),
				( Allowed(ARROW_RIGHT), (ctrl ? GetWordEndOffset(1) : 1, 0) ),
				( Allowed(ARROW_UP), (0, -1) ),
				( Allowed(ARROW_DOWN), (0, 1) ),
			};

			for (int j = 0; j < hotkeys.Length; j++)
				if (hotkeys[j].Item1)
				{
					MoveLineCursor(hotkeys[j].Item2.Item1, true, hotkeys[j].Item2.Item2);
					return;
				}
		}
		void TryResetHoldTimers()
		{
			var isAnyJustPressed = isJustTyped ||
				JustPressed(ENTER) ||
				JustPressed(BACKSPACE) || JustPressed(DELETE) ||
				JustPressed(ARROW_LEFT) || JustPressed(ARROW_RIGHT) ||
				JustPressed(ARROW_UP) || JustPressed(ARROW_DOWN);

			if (isAnyJustPressed)
				holdDelay.Restart();

			if (holdDelay.Elapsed.TotalSeconds > HOLD_DELAY &&
				hold.Elapsed.TotalSeconds > HOLD)
			{
				hold.Restart();
				isHolding = true;
			}
		}
		bool TrySelectAll()
		{
			if (isControlPressed && CurrentInput.Typed == "a")
			{
				SelectionIndex = w * h;
				cursorIndexX = 0;
				cursorIndexY = 0;
				return true;
			}
			return false;
		}
		void TrySelect()
		{
			var isSamePosClick = false;
			var (hx, hy) = CurrentInput.Position;
			var ix = (int)Math.Round(hx - Position.Item1);
			var iy = (int)(hy - Position.Item2);
			var (w, h) = Size;

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
					cursorIndexY = Math.Clamp(iy, 0, h - 1);
					iy = cursorIndexY;
					cursorIndexX = Math.Clamp(ix, 0, lines[iy].Length);
				}

				if (CurrentInput.IsJustPressed)
					SelectionIndex = CursorIndex;

				return;
			}

			if (CurrentInput.IsJustPressed == false)
				return;

			clicks = clicks == 4 ? 1 : clicks + 1;

			if (clicks == 1)
			{
				cursorIndexY = Math.Clamp(iy, 0, h - 1);
				iy = cursorIndexY;
				SelectionIndex = PositionToIndex((hx, hy));
				cursorIndexX = Math.Clamp(ix, 0, lines[iy].Length);
			}
			else if (clicks == 2)
			{
				SelectionIndex = PositionToIndex((hx, hy)) + GetWordEndOffset(1);
				cursorIndexX += GetWordEndOffset(-1);
				cursorIndexX = Math.Clamp(cursorIndexX, 0, lines[iy].Length);
			}
			else if (clicks == 3)
			{
				var p = PositionToIndex((x + lines[cursorIndexY].Length, y + cursorIndexY));
				SelectionIndex = p;
				cursorIndexX = 0;
			}
			else if (clicks == 4)
			{
				SelectionIndex = w * h;
				cursorIndexY = 0;
				cursorIndexX = 0;
			}
		}

		int GetWordEndOffset(int step, bool stopAtLineEnd = true)
		{
			var index = 0 + (step < 0 ? -1 : 0);
			var startY = PositionFromIndex(CursorIndex).Item2;
			var symb = GetSymbol(CursorIndex + (step < 0 ? -1 : 0));
			var targetIsWord = char.IsLetterOrDigit(symb) == false;

			for (int i = 0; i < Text.Length; i++)
			{
				var j = CursorIndex + index;
				var curY = PositionFromIndex(j).Item2;

				if ((j == 0 && step < 0) || (j == Text.Length && step > 0))
					return index;

				if (char.IsLetterOrDigit(GetSymbol(j)) == false ^ targetIsWord)
					return index + (step < 0 ? 1 : 0);

				index += step;

				// stop at start/end of line
				if (stopAtLineEnd == false)
					continue;

				if (startY != curY || index > lines[cursorIndexY].Length)
				{
					index -= step - (step < 0 ? 1 : 0);
					return index;
				}
			}

			return Text.Length * step;
		}
		char GetSymbol(int index) => Text.Length == 0 ? default : Text[Math.Clamp(index, 0, Text.Length - 1)];

		bool Pressed(int key) => CurrentInput.IsKeyPressed(key);
		bool JustPressed(int key) => CurrentInput.IsKeyJustPressed(key);
		bool Allowed(int key) => JustPressed(key) || (Pressed(key) && isHolding);
	}

	#region Backend
	private readonly List<string> lines = new();

	private const char SELECTION = '█', CURSOR = '▏', SPACE = ' ';
	private const float HOLD = 0.06f, HOLD_DELAY = 0.5f;
	private static readonly Stopwatch holdDelay = new(), hold = new(), clickDelay = new();
	private int selectionIndex, cursorIndexX, cursorIndexY, clicks;
	private (int, int) lastClickIndices = (-1, -1);

	protected int CursorIndexX
	{
		get => cursorIndexX;
		set => cursorIndexX = Math.Clamp(value, 0, lines[CursorIndexY].Length);
	}
	protected int CursorIndexY
	{
		get => cursorIndexY;
		set => cursorIndexY = Math.Clamp(value, 0, Size.Item2 - 1);
	}
	protected int CursorIndex => cursorIndexY * Size.Item1 + cursorIndexX;
	protected int SelectionIndex
	{
		get => selectionIndex;
		set => selectionIndex = Math.Clamp(value, 0, Size.Item1 * Size.Item2);
	}
	protected string GetSelectedText()
	{
		if (SelectionIndex == CursorIndex)
			return "";

		var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
		var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;
		return Text[a..b];
	}
	protected void MoveLineCursor(int offsetX, bool allowSelection = false, int offsetY = 0)
	{
		var shift = CurrentInput.IsKeyPressed(SHIFT_LEFT) || CurrentInput.IsKeyPressed(SHIFT_RIGHT);

		if ((cursorIndexY == 0 && offsetY < 0) || (cursorIndexY == Size.Item2 - 1 && offsetY > 0))
			return;

		if (CursorIndexX == 0 && offsetX < 0)
		{
			offsetX = Size.Item1 + offsetX;
			offsetY = -1;
		}
		else if (CursorIndexX == Size.Item1 - 1 && offsetX > 0)
		{
			offsetX = -Size.Item1 + offsetX;
			offsetY = 1;
		}

		CursorIndexY += offsetY; // y first cuz X uses it
		CursorIndexX += offsetX;

		SelectionIndex = shift && allowSelection ? SelectionIndex : CursorIndex;
		SelectionIndex = Math.Clamp(SelectionIndex, 0, Text.Length);
	}
	private void UpdateText()
	{
		Text = "";

		var maxW = Size.Item1 - 1;
		for (int i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			line = line.Length > maxW ? line[0..maxW] : line;
			Text += line.PadRight(Size.Item1, ' ');
		}
	}

	private static bool IsJustTyped()
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
	#endregion
}
