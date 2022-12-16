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
			var wasJustTyped = Input.TypedSymbol != "" && Input.TypedSymbol != Input.PrevTypedSymbol;
			var wasJustBackspaced = Input.WasBackspaced == false && Input.IsBackspaced;
			var isTriggered = TryTrigger();

			var cursorPos = Input.Position.Item1 - Position.Item1;
			var endOfTextPos = Text.Length;
			var curPos = cursorPos > Text.Length ? endOfTextPos : cursorPos;

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
				if((wasJustTyped || wasJustBackspaced) && isSelected)
				{
					var a = SelectionPosition < CursorPosition ? SelectionPosition : CursorPosition;
					var b = Math.Abs(SelectionPosition - CursorPosition);

					Text = Text.Remove(a, b);
					CursorPosition = a;
					SelectionPosition = a;
					justDeleted = true;
				}

				if(wasJustTyped && Text.Length < Size.Item1 - 1)
				{
					Text = Text.Insert(CursorPosition, Input.TypedSymbol);
					CursorPosition++;
					SelectionPosition = CursorPosition;
				}
				else if(wasJustBackspaced && justDeleted == false && Text.Length > 0)
				{
					Text = Text.Remove(CursorPosition - 1, 1);
					CursorPosition--;
					SelectionPosition = CursorPosition;
				}
			}

			result?.Invoke(this);
		}

		#region Backend
		private int curPos, selPos;
		#endregion
	}
}
