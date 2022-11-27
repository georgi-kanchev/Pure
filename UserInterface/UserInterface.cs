namespace Purity.UserInterface
{
	public abstract class UserInterface
	{
		public (int, int) Position { get; set; }
		public (int, int) Size { get; set; }

		public bool IsFocused
		{
			get => this == focused;
			set => focused = this;
		}
		public bool IsHovered { get; private set; }
		public bool IsPressed { get; private set; }

		public UserInterface((int, int) position, (int, int) size)
		{
			Position = position;
			Size = size;
		}

		public static void UpdateInput((int, int) position, bool isTriggered)
		{
			hadInput = hasInput;
			inputPos = position;
			hasInput = isTriggered;
		}

		#region Backend
		private bool isClicked;

		private static (int, int) inputPos;
		private static bool hasInput, hadInput;
		private static UserInterface? focused;
		private static (int, int) inputBoxCursorPos;

		protected bool TryTrigger()
		{
			IsPressed = false;
			IsHovered = false;

			var hx = inputPos.Item1;
			var hy = inputPos.Item2;
			var x = Position.Item1;
			var y = Position.Item2;
			var w = Size.Item1;
			var h = Size.Item2;
			var isHoveredX = hx >= x && hx < x + w;
			var isHoveredY = hy >= y && hy < y + h;
			if(w < 0)
				isHoveredX = hx > x + w && hx <= x;
			if(h < 0)
				isHoveredY = hy > y + h && hy <= y;

			var isReleased = hasInput == false && hadInput;
			IsHovered = isHoveredX && isHoveredY;
			IsPressed = IsHovered && hasInput && isClicked;

			if(IsHovered && isReleased && isClicked)
			{
				isClicked = false;
				return true;
			}

			if(IsHovered && hasInput && hadInput == false)
				isClicked = true;

			if(isReleased)
				isClicked = false;

			return false;
		}
		#endregion
	}
}
