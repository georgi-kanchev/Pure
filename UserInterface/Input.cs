namespace Purity.UserInterface
{
	public struct Input
	{
		public bool IsPressed { get; set; }
		public bool IsReleased => IsPressed == false && WasPressed;

		public (int, int) Position { get; set; }
		public string TypedSymbol { get; set; }

		public bool IsShifted { get; set; }
		public bool IsControled { get; set; }
		public bool IsBackspaced { get; set; }
		public bool IsEscaped { get; set; }
		public bool IsTabed { get; set; }
		public bool IsAlted { get; set; }
		public bool IsEntered { get; set; }

		#region Backend
		internal bool WasPressed { get; set; }
		internal bool WasShifted { get; set; }
		internal bool WasControled { get; set; }
		internal bool WasBackspaced { get; set; }
		internal bool WasEscaped { get; set; }
		internal bool WasTabed { get; set; }
		internal bool WasAlted { get; set; }
		internal bool WasEntered { get; set; }

		internal string PrevTypedSymbol { get; set; }
		#endregion
	}
}
