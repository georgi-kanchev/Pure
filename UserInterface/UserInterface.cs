namespace Purity.UserInterface
{
	public abstract class UserInterface
	{
		public enum SpecialKey
		{
			None, Shift, Control, Backspace, Escape, Tab, Alt, Return,
#pragma warning disable CA1069
			Enter = 7
#pragma warning restore CA1069
		}

		public (int, int) Position { get; set; }
		public (int, int) Size { get; set; }

		public bool IsFocused
		{
			get => this == UI_FocusedObject;
			set => UI_FocusedObject = value ? this : default;
		}
		public bool IsHovered
		{
			get
			{
				var ix = UI_InputPosition.Item1;
				var iy = UI_InputPosition.Item2;
				var x = Position.Item1;
				var y = Position.Item2;
				var w = Size.Item1;
				var h = Size.Item2;
				var isHoveredX = ix >= x && ix < x + w;
				var isHoveredY = iy >= y && iy < y + h;
				if(w < 0)
					isHoveredX = ix > x + w && ix <= x;
				if(h < 0)
					isHoveredY = iy > y + h && iy <= y;

				return isHoveredX && isHoveredY;
			}
		}
		public bool IsPressed => IsHovered && UI_HasInput && IsClicked;

		public UserInterface((int, int) position, (int, int) size)
		{
			Position = position;
			Size = size;
		}

		public static void Input((int, int) position, bool isPressed,
			char typedSymbol = default, SpecialKey specialKey = default)
		{
			UI_HadInput = UI_HasInput;
			UI_InputPosition = position;
			UI_HasInput = isPressed;
		}

		#region Backend
		protected bool IsClicked { get; set; }

		protected static bool UI_IsReleased => UI_HasInput == false && UI_HadInput;
		protected static (int, int) UI_InputPosition { get; set; }
		protected static bool UI_HasInput { get; set; }
		protected static bool UI_HadInput { get; set; }
		protected static UserInterface? UI_FocusedObject { get; set; }

		protected bool TryTrigger()
		{
			if(IsHovered && UI_IsReleased && IsClicked)
			{
				IsClicked = false;
				return true;
			}

			if(IsHovered && UI_HasInput && UI_HadInput == false)
				IsClicked = true;

			if(UI_IsReleased)
				IsClicked = false;

			return false;
		}
		#endregion
	}
}
