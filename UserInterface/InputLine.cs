namespace Purity.UserInterface
{
	public class InputLine : UserInterface
	{
		public string Text { get; set; } = "";
		public int CursorPosition { get; set; }
		public int SelectionPosition { get; set; }

		public InputLine((int, int) position, (int, int) size, string text = "") : base(position, size)
		{
			Text = text;
		}

		public void Update(Action<InputLine>? result)
		{
			if(Size.Item1 == 0 || Size.Item2 == 0)
				return;

			if(UI_HadInput == false && UI_HasInput)
				SelectionPosition = CursorPosition;

			var isJustPressed = UI_HadInput == false && UI_HasInput;
			var isTriggered = TryTrigger();

			var cursorPos = UI_InputPosition.Item1 - Position.Item1;
			var endOfTextPos = Text.Length;
			var curPos = cursorPos > Text.Length ? endOfTextPos : cursorPos;

			if(IsPressed)
				CursorPosition = curPos;

			if(isJustPressed)
			{
				if(IsFocused)
					UI_FocusedObject = null;

				if(IsHovered)
				{
					UI_FocusedObject = this;
					SelectionPosition = curPos;
				}
			}

			result?.Invoke(this);
		}
	}
}
