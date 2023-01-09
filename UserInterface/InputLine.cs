namespace Pure.UserInterface
{
	public class InputLine : UserInterface
	{
		public string TextSelected
		{
			get
			{
				if(IndexSelection == IndexCursor)
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

		public InputLine((int, int) position, (int, int) size, string text = "") : base(position, size)
		{
			Text = text;
		}

		public void Update(Action<InputLine>? custom)
		{
			if(Size.Item1 == 0 || Size.Item2 == 0)
				return;

			if(Input.WasPressed == false && Input.IsPressed)
				IndexSelection = IndexCursor;

			var isJustPressed = Input.WasPressed == false && Input.IsPressed;
			var isJustTyped = Input.TypedSymbols != "" && Input.TypedSymbols != Input.PrevTypedSymbols;
			var isJustBackspace = Input.WasPressedBackspace == false && Input.IsPressedBackspace;
			var isJustLeft = Input.WasPressedLeft == false && Input.IsPressedLeft;
			var isJustRight = Input.WasPressedRight == false && Input.IsPressedRight;
			var isTriggered = TryTrigger();

			var cursorPos = Input.Position.Item1 - Position.Item1;
			var endOfTextPos = Text.Length;
			var curPos = (int)MathF.Round(cursorPos > Text.Length ? endOfTextPos : cursorPos);

			if(IsHovered)
				SetTileAndSystemCursor(CursorResult.TileText);

			if(IsPressed)
				IndexCursor = curPos;

			if(isJustPressed)
			{
				if(IsFocused)
					FocusedObject = null;

				if(IsHovered)
				{
					FocusedObject = this;
					IndexSelection = curPos;
				}
			}

			if(IsFocused)
			{
				var isSelected = IndexSelection != IndexCursor;
				var justDeleted = false;
				if((isJustTyped || isJustBackspace) && isSelected)
				{
					var a = IndexSelection < IndexCursor ? IndexSelection : IndexCursor;
					var b = Math.Abs(IndexSelection - IndexCursor);

					Text = Text.Remove(a, b);
					IndexCursor = a;
					IndexSelection = a;
					justDeleted = true;
				}

				if(isJustTyped && Text.Length < Size.Item1 - 1)
				{
					Text = Text.Insert(IndexCursor, Input.TypedSymbols);
					MoveCursorRight();
				}
				else if(isJustBackspace && justDeleted == false && Text.Length > 0)
				{
					Text = Text.Remove(IndexCursor - 1, 1);
					MoveCursorLeft();
				}

				if(isJustLeft && IndexCursor > 0)
					MoveCursorLeft();
				else if(isJustRight && IndexCursor < Text.Length)
					MoveCursorRight();
			}

			custom?.Invoke(this);
		}

		#region Backend
		private int curPos, selPos;

		private void MoveCursorLeft()
		{
			IndexCursor--;
			IndexSelection = IndexCursor;
		}
		private void MoveCursorRight()
		{
			IndexCursor++;
			IndexSelection = IndexCursor;
		}
		#endregion
	}
}
