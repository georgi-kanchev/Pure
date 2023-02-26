namespace Pure.UserInterface;

using System.Diagnostics;

public class InputBox : UserInterface
{
	public string Placeholder { get; set; } = "";
	public int CursorIndex
	{
		get => curPos;
		set => curPos = Math.Clamp(value, 0, Text.Length);
	}

	public InputBox((int, int) position, (int, int) size, string text = "") : base(position, size)
	{
		Text = text;
	}
	static InputBox()
	{
		holdDelay.Start();
		hold.Start();
	}

	public void GetGraphics(out string cursor, out string selection, out string placeholder)
	{
		var c = new string(SPACE, CursorIndex) + CURSOR + new string(SPACE, Text.Length - CursorIndex);
		var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
		var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;
		var sz = b - a;

		cursor = IsFocused ? c : "";
		selection = new string(SPACE, a) + new string(SELECTION, sz) + new string(SPACE, Text.Length - b);
		placeholder = Text.Length > 0 ? "" : Placeholder;
	}

	public int PositionToIndex((float, float) position)
	{
		var (ppx, ppy) = position;
		var (px, py) = Position;
		var (w, h) = Size;
		var (x, y) = (MathF.Round(ppx) - px, MathF.Floor(ppy) - py);
		var index = Math.Clamp(y, 0, h) * w + Math.Clamp(x, 0, w);

		return (int)Math.Clamp(index, 0, Text.Length);
	}
	public (int, int) PositionFromIndex(int index)
	{
		var (px, py) = Position;
		var (w, h) = Size;
		var (x, y) = (index % w, index / w);

		return (Math.Clamp(px + x, px, px + w), Math.Clamp(py + y, py, py + h - 1));
	}

	protected override void OnUpdate()
	{
		var isControlPressed = Pressed(CONTROL_LEFT) || Pressed(CONTROL_RIGHT);
		var maxTotalLength = Size.Item1 * Size.Item2;
		var end = Math.Min(Text.Length, maxTotalLength);

		Text = Text[0..end];
		CursorIndex = Math.Clamp(CursorIndex, 0, Text.Length);
		SelectionIndex = Math.Clamp(SelectionIndex, 0, Text.Length);

		TryRemoveSelection();
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
		void TryRemoveSelection()
		{
			if (JustPressed(ESCAPE) || (CurrentInput.wasPressed == false && CurrentInput.IsPressed))
				SelectionIndex = CursorIndex;
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
				var b = Math.Abs(SelectionIndex - CursorIndex);

				Text = Text.Remove(a, b);
				CursorIndex = a;
				SelectionIndex = a;
				justDeletedSelection = true;
			}
		}
		void TryTypeEnterDelete()
		{
			var i = CursorIndex;
			var (cx, cy) = PositionFromIndex(i);
			var (x, y) = Position;
			var (w, h) = Size;

			if (isAllowedType && Text.Length < maxTotalLength)
			{
				var symbols = CurrentInput.Typed ?? "";

				if (isPasting && string.IsNullOrWhiteSpace(TextCopied) == false)
				{
					symbols = TextCopied;
					if (i + symbols.Length > maxTotalLength)
					{
						var newSize = maxTotalLength - i;
						symbols = symbols[0..(newSize)];
					}

					Text = Text.Insert(i, symbols);
					MoveCursor(symbols.Length);
					return;
				}

				Text = Text.Insert(i, symbols.Length > 1 ? symbols[^1].ToString() : symbols);
				MoveCursor(1);
			}
			else if (Allowed(ENTER) && justDeletedSelection == false &&
				cy != y + h - 1) // not last line
			{
				var spacesUntilEndOfLine = x + w - cx;
				Text = Text.Insert(i, new string(SPACE, spacesUntilEndOfLine));
				MoveCursor(spacesUntilEndOfLine);
			}
			else if (Allowed(BACKSPACE) && justDeletedSelection == false &&
				Text.Length > 0 && i > 0)
			{
				var off = GetWordEndOffset(-1, true);

				// cursor is at local x = 0 & y != 0 and there are empty spaces
				// so remove "new line" spaces from prev line (if any)
				if (cx == x && cy != y && Text[i - 1] == ' ')
				{
					off = GetWordEndOffset(-1, false);
					Text = Text.Remove(i + off, Math.Abs(off));
					MoveCursor(off);
					return;
				}

				var ctrl = Pressed(CONTROL_LEFT);
				var count = ctrl ? Math.Abs(off) : 1;
				Text = Text.Remove(ctrl ? i + off : i - 1, count);
				MoveCursor(ctrl ? off : -1);
			}
			else if (Allowed(DELETE) && justDeletedSelection == false &&
				Text.Length > 0 && i < Text.Length)
			{
				var off = GetWordEndOffset(1, true);
				var off2 = GetWordEndOffset(1, false);

				// cursor is at y != h and there are empty spaces
				// so remove "new line" spaces from current line (if any)
				if (cy != y + h - 1 && i != Text.Length - 1 && Text[i + 1] == ' ' &&
					cy != PositionFromIndex(i + off2).Item2)
				{
					Text = Text.Remove(i, Math.Abs(off2));
					return;
				}

				var ctrl = Pressed(CONTROL_LEFT);
				var count = ctrl ? Math.Abs(off) : 1;
				Text = Text.Remove(ctrl ? i : i, count);
			}
		}
		void TryMoveCursor()
		{
			var ctrl = Pressed(CONTROL_LEFT) || Pressed(CONTROL_RIGHT);
			var i = CursorIndex;
			var s = SelectionIndex;
			var hasSelection = i != s;
			var w = Size.Item1;
			var (cx, cy) = PositionFromIndex(i);
			var (x, y) = Position;
			var selectionCursorDiff = i > s ? i - s : s - i;
			var hotkeys = new (bool, int)[]
			{
				( hasSelection && JustPressed(ARROW_UP), i < s ? -w : s - i - w ),
				( hasSelection && JustPressed(ARROW_DOWN), i < s ? s - i + w : w ),
				( hasSelection && JustPressed(ARROW_LEFT), i < s ? 0 : s - i ),
				( hasSelection && JustPressed(ARROW_RIGHT), i < s ? s - i : 0 ),
				( ctrl && JustPressed(ARROW_UP), cx - x - w ),
				( ctrl && JustPressed(ARROW_DOWN), w - (cx - x) + w ),
				( JustPressed(HOME), x - cx ),
				( JustPressed(END), w - (cx - x) ),
				( Allowed(ARROW_LEFT), ctrl ? GetWordEndOffset(-1) : -1 ),
				( Allowed(ARROW_RIGHT), ctrl ? GetWordEndOffset(1) : 1 ),
				( Allowed(ARROW_UP), -w ),
				( Allowed(ARROW_DOWN), w ),
			};

			for (int j = 0; j < hotkeys.Length; j++)
				if (hotkeys[j].Item1)
				{
					MoveCursor(hotkeys[j].Item2, true);
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
				SelectionIndex = 0;
				CursorIndex = Text.Length;
				return true;
			}
			return false;
		}
		void TrySelect()
		{
			var h = PositionToIndex(CurrentInput.Position);
			var isSamePosClick = false;

			if (CurrentInput.IsPressed)
			{
				clickDelay.Restart();
				isSamePosClick = lastClickIndex == h;
				lastClickIndex = h;
			}
			if (clickDelay.Elapsed.TotalSeconds > 1f)
			{
				clickDelay.Stop();
				clicks = 0;
				lastClickIndex = -1;
				return;
			}

			if (isSamePosClick == false)
			{
				if (CurrentInput.IsPressed)
					CursorIndex = h;

				if (CurrentInput.IsJustPressed)
					SelectionIndex = h;

				return;
			}


			if (CurrentInput.IsJustPressed == false)
				return;

			clicks = clicks == 4 ? 1 : clicks + 1;

			System.Console.WriteLine(clicks);

			if (clicks == 1)
			{
				SelectionIndex = h;
				CursorIndex = h;
			}
			else if (clicks == 2)
			{
				SelectionIndex = h + GetWordEndOffset(1);
				CursorIndex += GetWordEndOffset(-1);
			}
			else if (clicks == 3)
			{
				var x = Position.Item1;
				var cy = PositionFromIndex(h).Item2;
				SelectionIndex = PositionToIndex((x + Size.Item1, cy));
				CursorIndex = PositionToIndex((x, cy));
			}
			else if (clicks == 4)
			{
				SelectionIndex = 0;
				CursorIndex = Text.Length;
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
				if (stopAtLineEnd && startY != curY && CursorIndex + index != Text.Length)
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
	private const char SELECTION = '█', CURSOR = '▏', SPACE = ' ';
	private const float HOLD = 0.06f, HOLD_DELAY = 0.5f;
	private static readonly Stopwatch holdDelay = new(), hold = new(), clickDelay = new();
	private int curPos, selPos, clicks, lastClickIndex = -1;

	protected int SelectionIndex
	{
		get => selPos;
		set => selPos = Math.Clamp(value, 0, Text.Length);
	}
	protected string GetSelectedText()
	{
		if (SelectionIndex == CursorIndex)
			return "";

		var a = CursorIndex < SelectionIndex ? CursorIndex : SelectionIndex;
		var b = CursorIndex > SelectionIndex ? CursorIndex : SelectionIndex;
		return Text[a..b];
	}
	protected void MoveCursor(int offset, bool allowSelection = false)
	{
		var shift = CurrentInput.IsKeyPressed(SHIFT_LEFT) || CurrentInput.IsKeyPressed(SHIFT_RIGHT);

		CursorIndex += offset;
		CursorIndex = Math.Clamp(CursorIndex, 0, Text.Length);
		SelectionIndex = shift && allowSelection ? SelectionIndex : CursorIndex;
		SelectionIndex = Math.Clamp(SelectionIndex, 0, Text.Length);
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
