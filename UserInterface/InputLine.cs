using System.Diagnostics;

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
			Size = (Size.Item1, 1);

			if(Input.WasPressed == false && Input.IsPressed)
				IndexSelection = IndexCursor;

			if(IsHovered)
				SetTileAndSystemCursor(CursorResult.TileText);

			if(IsFocused == false)
				return;

			var isJustPressed = Input.WasPressed == false && Input.IsPressed;
			var isJustTyped = IsJustTyped();
			var isJustBackspace = Input.WasPressedBackspace == false && Input.IsPressedBackspace;
			var isJustLeft = Input.WasPressedLeft == false && Input.IsPressedLeft;
			var isJustRight = Input.WasPressedRight == false && Input.IsPressedRight;

			if(Input.IsPressedControl && Input.TypedSymbols == "a")
			{
				IndexSelection = 0;
				IndexCursor = Text.Length;
				return;
			}

			var isHolding = false;

			if(isJustTyped || isJustBackspace || isJustLeft || isJustRight)
				holdDelay.Restart();

			if(holdDelay.Elapsed.TotalSeconds > HOLD_DELAY &&
				hold.Elapsed.TotalSeconds > HOLD)
			{
				hold.Restart();
				isHolding = true;
			}

			var isHoldingType = isHolding && Input.TypedSymbols != "";
			var isHoldingBackspace = isHolding && Input.IsPressedBackspace;
			var isHoldingLeft = isHolding && Input.IsPressedLeft;
			var isHoldingRight = isHolding && Input.IsPressedRight;

			var isAllowedBackspace = isJustBackspace || isHoldingBackspace;
			var isAllowedType = isJustTyped || isHoldingType;
			var isAllowedLeft = isJustLeft || isHoldingLeft;
			var isAllowedRight = isJustRight || isHoldingRight;

			var cursorPos = Input.Position.Item1 - Position.Item1;
			var endOfTextPos = Text.Length;
			var curPos = (int)MathF.Round(cursorPos > Text.Length ? endOfTextPos : cursorPos);

			if(IsPressed)
				IndexCursor = curPos;

			if(isJustPressed && IsHovered)
				IndexSelection = curPos;

			var isSelected = IndexSelection != IndexCursor;
			var justDeleted = false;
			if((isAllowedType || isAllowedBackspace) && isSelected)
			{
				var a = IndexSelection < IndexCursor ? IndexSelection : IndexCursor;
				var b = Math.Abs(IndexSelection - IndexCursor);

				Text = Text.Remove(a, b);
				IndexCursor = a;
				IndexSelection = a;
				justDeleted = true;
			}

			if(isAllowedType && Text.Length < Size.Item1)
			{
				var symbols = Input.TypedSymbols;
				Console.WriteLine(symbols);
				Text = Text.Insert(IndexCursor, symbols.Length > 1 ? symbols[^1].ToString() : symbols);
				MoveCursor(1);
			}
			else if(isAllowedBackspace && justDeleted == false && Text.Length > 0 && IndexCursor > 0)
			{
				Text = Text.Remove(IndexCursor - 1, 1);
				MoveCursor(-1);
			}

			if(isAllowedLeft && IndexCursor > 0)
				MoveCursor(-1);
			else if(isAllowedRight && IndexCursor < Text.Length)
				MoveCursor(1);
		}

		#region Backend
		private const float HOLD = 0.06f, HOLD_DELAY = 0.5f;
		private static readonly Stopwatch holdDelay = new(), hold = new();

		private int curPos, selPos;

		private void MoveCursor(int offset)
		{
			IndexCursor += offset;
			IndexSelection = IndexCursor;
		}
		private bool IsJustTyped()
		{
			for(int i = 0; i < Input.TypedSymbols.Length; i++)
			{
				if(Input.PrevTypedSymbols.Contains(Input.TypedSymbols[i]) == false)
					return true;
			}
			return false;
		}
		#endregion
	}
}
