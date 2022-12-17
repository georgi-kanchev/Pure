namespace Purity.UserInterface
{
	public abstract class UserInterface
	{
		public (int, int) Position { get; set; }
		public (int, int) Size { get; set; }

		public bool IsFocused => FocusedObject == this;
		public bool IsHovered
		{
			get
			{
				var (ix, iy) = Input.Position;
				var (x, y) = Position;
				var (w, h) = Size;
				var isHoveredX = ix >= x && ix < x + w;
				var isHoveredY = iy >= y && iy < y + h;
				if(w < 0)
					isHoveredX = ix > x + w && ix <= x;
				if(h < 0)
					isHoveredY = iy > y + h && iy <= y;

				return isHoveredX && isHoveredY;
			}
		}
		public bool IsPressed => IsHovered && Input.IsPressed && IsClicked;

		public UserInterface((int, int) position, (int, int) size)
		{
			Position = position;
			Size = size;
		}

		public static void UpdateInput(Input input)
		{
			input.PrevTypedSymbol = Input.TypedSymbol;
			input.WasPressed = Input.IsPressed;
			input.WasPressedAlt = Input.IsPressedAlt;
			input.WasPressedBackspace = Input.IsPressedBackspace;
			input.WasPressedControl = Input.IsPressedControl;
			input.WasPressedEnter = Input.IsPressedEnter;
			input.WasPressedEscape = Input.IsPressedEscape;
			input.WasPressedShift = Input.IsPressedShift;
			input.WasPressedTab = Input.IsPressedTab;

			input.WasPressedLeft = Input.IsPressedLeft;
			input.WasPressedRight = Input.IsPressedRight;
			input.WasPressedUp = Input.IsPressedUp;
			input.WasPressedDown = Input.IsPressedDown;

			Input = input;
		}

		#region Backend
		protected bool IsClicked { get; set; }

		protected static Input Input { get; set; }
		protected static UserInterface? FocusedObject { get; set; }

		protected bool TryTrigger()
		{
			if(IsHovered && Input.IsReleased && IsClicked)
			{
				IsClicked = false;
				return true;
			}

			if(IsHovered && Input.IsPressed && Input.WasPressed == false)
				IsClicked = true;

			if(Input.IsReleased)
				IsClicked = false;

			return false;
		}
		#endregion
	}
}
