namespace Pure.UserInterface;

using System.Diagnostics;

public class InputLine : UserInterface
{
	public string TextSelected
	{
		get
		{
			if (IndexSelection == IndexCursor)
				return "";

			var a = IndexCursor < IndexSelection ? IndexCursor : IndexSelection;
			var b = IndexCursor > IndexSelection ? IndexCursor : IndexSelection;
			return Text[a..b];
		}
	}
	public int IndexCursor
	{
		get => curPos;
		set => curPos = Math.Clamp(value, 0, Text.Length);
	}
	public int IndexSelection
	{
		get => selPos;
		set => selPos = Math.Clamp(value, 0, Text.Length);
	}

	public InputLine((int, int) position, int width, string text = "") : base(position, (width, 1))
	{
		Text = text;
	}

	static InputLine()
	{
		holdDelay.Start();
		hold.Start();
	}

	protected override void OnUpdate()
	{
		var isControlPressed = Pressed(CONTROL_LEFT) || Pressed(CONTROL_RIGHT);
		var end = Math.Min(Text.Length, Size.Item1);

		Text = Text[0..end];
		Size = (Size.Item1, 1);
		IndexCursor = Math.Clamp(IndexCursor, 0, Text.Length);
		IndexSelection = Math.Clamp(IndexSelection, 0, Text.Length);

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
		var shouldDelete = isAllowedType || Allowed(BACKSPACE) || Allowed(DELETE);

		if (TryCopyPasteOrCut())
			return;

		TrySelect();
		TryDeleteSelected();
		TryTypeOrDelete();
		TryMoveCursor();

		bool TryCopyPasteOrCut()
		{
			var hasSelection = IndexCursor != IndexSelection;
			if (isControlPressed && CurrentInput.Typed == "c")
			{
				CopiedText = TextSelected;
				return true;
			}
			else if (isControlPressed && CurrentInput.Typed == "v")
				isPasting = true;
			else if (hasSelection && isControlPressed && CurrentInput.Typed == "x")
			{
				CopiedText = TextSelected;
				shouldDelete = true;
				TryDeleteSelected();
				return true;
			}
			return false;
		}
		void TryRemoveSelection()
		{
			if (JustPressed(ESCAPE) || (CurrentInput.wasPressed == false && CurrentInput.IsPressed))
				IndexSelection = IndexCursor;
		}
		void TrySetMouseCursor()
		{
			if (IsHovered)
				TrySetTileAndSystemCursor(TILE_TEXT);

		}
		void TryDeleteSelected()
		{
			var isSelected = IndexSelection != IndexCursor;
			if (shouldDelete && isSelected)
			{
				var a = IndexSelection < IndexCursor ? IndexSelection : IndexCursor;
				var b = Math.Abs(IndexSelection - IndexCursor);

				Text = Text.Remove(a, b);
				IndexCursor = a;
				IndexSelection = a;
				justDeletedSelection = true;
			}
		}
		void TryTypeOrDelete()
		{
			if (isAllowedType && Text.Length < Size.Item1)
			{
				var symbols = CurrentInput.Typed ?? "";

				if (isPasting && string.IsNullOrWhiteSpace(CopiedText) == false)
				{
					symbols = CopiedText;
					if (IndexCursor + symbols.Length > Size.Item1)
					{
						var newSize = Size.Item1 - IndexCursor;
						symbols = symbols[0..(newSize)];
					}

					Text = Text.Insert(IndexCursor, symbols);
					MoveCursor(symbols.Length);
					return;
				}

				Text = Text.Insert(IndexCursor, symbols.Length > 1 ? symbols[^1].ToString() : symbols);
				MoveCursor(1);
			}
			else if (Allowed(BACKSPACE) && justDeletedSelection == false &&
				Text.Length > 0 && IndexCursor > 0)
			{
				Text = Text.Remove(IndexCursor - 1, 1);
				MoveCursor(-1);
			}
			else if (Allowed(DELETE) && justDeletedSelection == false &&
				Text.Length > 0 && IndexCursor < Text.Length)
				Text = Text.Remove(IndexCursor, 1);
		}
		void TryMoveCursor()
		{
			var ctrl = Pressed(CONTROL_LEFT) || Pressed(CONTROL_RIGHT);

			if (Allowed(ARROW_LEFT) && IndexCursor > 0)
				MoveCursor(ctrl ? GetWordEndOffset(-1) : -1, true);
			else if (Allowed(ARROW_RIGHT) && IndexCursor < Text.Length)
				MoveCursor(ctrl ? GetWordEndOffset(1) : 1, true);

			if (JustPressed(ARROW_UP) || JustPressed(END))
				MoveCursor(Text.Length - IndexCursor, true);
			else if (JustPressed(ARROW_DOWN) || JustPressed(HOME))
				MoveCursor(-IndexCursor, true);
		}
		void TryResetHoldTimers()
		{
			var isAnyJustPressed = JustPressed(BACKSPACE) || JustPressed(DELETE) ||
				JustPressed(ARROW_LEFT) || JustPressed(ARROW_RIGHT) || isJustTyped;

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
				IndexSelection = 0;
				IndexCursor = Text.Length;
				return true;
			}
			return false;
		}
		void TrySelect()
		{
			var cursorPos = CurrentInput.Position.Item1 - Position.Item1;
			var newCurPos = (int)MathF.Round(cursorPos > Text.Length ? Text.Length : cursorPos);

			if (CurrentInput.IsPressed)
				IndexCursor = newCurPos;

			if (CurrentInput.IsJustPressed)
				IndexSelection = newCurPos;
		}

		int GetWordEndOffset(int step)
		{
			var index = 0 + (step < 0 ? -1 : 0);
			var targetIsWord = GetSymbol(IndexCursor + (step < 0 ? -1 : 0)) == ' ';
			for (int i = 0; i < Text.Length; i++)
			{
				var j = IndexCursor + index;
				if ((j == 0 && step < 0) || (j == Text.Length && step > 0))
					return index;

				if (GetSymbol(j) == ' ' ^ targetIsWord)
					return index + (step < 0 ? 1 : 0);

				index += step;
			}

			return Text.Length * step;
		}
		char GetSymbol(int index) => Text[Math.Min(index, Text.Length - 1)];

		bool Pressed(int key) => CurrentInput.IsKeyPressed(key);
		bool JustPressed(int key) => CurrentInput.IsKeyJustPressed(key);
		bool Allowed(int key) => JustPressed(key) || (Pressed(key) && isHolding);
	}

	#region Backend
	private const float HOLD = 0.06f, HOLD_DELAY = 0.5f;
	private static readonly Stopwatch holdDelay = new(), hold = new();

	private int curPos, selPos;

	private void MoveCursor(int offset, bool allowSelection = false)
	{
		var shift = CurrentInput.IsKeyPressed(SHIFT_LEFT) || CurrentInput.IsKeyPressed(SHIFT_RIGHT);

		IndexCursor += offset;
		IndexSelection = shift && allowSelection ? IndexSelection : IndexCursor;
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
