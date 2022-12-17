namespace Purity.UserInterface
{
	public class InputLine : UserInterface
	{
		public string Text { get; set; } = "";
		public string TextSelected
		{
			get
			{
				if(SelectionPosition == CursorPosition)
					return "";

				var a = CursorPosition < SelectionPosition ? CursorPosition : SelectionPosition;
				var b = CursorPosition > SelectionPosition ? CursorPosition : SelectionPosition;
				return Text[a..b];
			}
		}
		public int CursorPosition
		{
			get => curPos;
			set => curPos = Math.Clamp(value, 0, Text.Length);
		}
		public int SelectionPosition
		{
			get => selPos;
			set => selPos = Math.Clamp(value, 0, Text.Length);
		}

		public InputLine((int, int) position, (int, int) size, string text = "") : base(position, size)
		{
			Text = text;
		}

		public void Update(Action<InputLine>? result)
		{
			if(Size.Item1 == 0 || Size.Item2 == 0)
				return;

			if(Input.WasPressed == false && Input.IsPressed)
				SelectionPosition = CursorPosition;

			var isJustPressed = Input.WasPressed == false && Input.IsPressed;
			var isJustTyped = Input.TypedSymbol != "" && Input.TypedSymbol != Input.PrevTypedSymbol;
			var isJustBackspace = Input.WasPressedBackspace == false && Input.IsPressedBackspace;
			var isJustLeft = Input.WasPressedLeft == false && Input.IsPressedLeft;
			var isJustRight = Input.WasPressedRight == false && Input.IsPressedRight;
			var isTriggered = TryTrigger();

			var cursorPos = Input.Position.Item1 - Position.Item1;
			var endOfTextPos = Text.Length;
			var curPos = (int)MathF.Round(cursorPos > Text.Length ? endOfTextPos : cursorPos);

			if(IsPressed)
				CursorPosition = curPos;

			if(isJustPressed)
			{
				if(IsFocused)
					FocusedObject = null;

				if(IsHovered)
				{
					FocusedObject = this;
					SelectionPosition = curPos;
				}
			}

			if(IsFocused)
			{
				var isSelected = SelectionPosition != CursorPosition;
				var justDeleted = false;
				if((isJustTyped || isJustBackspace) && isSelected)
				{
					var a = SelectionPosition < CursorPosition ? SelectionPosition : CursorPosition;
					var b = Math.Abs(SelectionPosition - CursorPosition);

					Text = Text.Remove(a, b);
					CursorPosition = a;
					SelectionPosition = a;
					justDeleted = true;
				}

				if(isJustTyped && Text.Length < Size.Item1 - 1)
				{
					Text = Text.Insert(CursorPosition, Input.TypedSymbol);
					MoveCursorRight();
				}
				else if(isJustBackspace && justDeleted == false && Text.Length > 0)
				{
					Text = Text.Remove(CursorPosition - 1, 1);
					MoveCursorLeft();
				}

				if(isJustLeft && CursorPosition > 0)
					MoveCursorLeft();
				else if(isJustRight && CursorPosition < Text.Length)
					MoveCursorRight();
			}

			result?.Invoke(this);
		}

		#region Backend
		private int curPos, selPos;

		private void MoveCursorLeft()
		{
			CursorPosition--;
			SelectionPosition = CursorPosition;
		}
		private void MoveCursorRight()
		{
			CursorPosition++;
			SelectionPosition = CursorPosition;
		}
		#endregion
	}
}
