namespace Pure.UserInterface
{
	public struct Input
	{
		public bool IsPressed { get; set; }
		public bool IsReleased => IsPressed == false && WasPressed;

		public (float, float) Position { get; set; }
		public string TypedSymbols { get; set; }

		public bool IsPressedShift { get; set; }
		public bool IsPressedControl { get; set; }
		public bool IsPressedBackspace { get; set; }
		public bool IsPressedEscape { get; set; }
		public bool IsPressedTab { get; set; }
		public bool IsPressedAlt { get; set; }
		public bool IsPressedEnter { get; set; }
		public bool IsPressedUp { get; set; }
		public bool IsPressedDown { get; set; }
		public bool IsPressedRight { get; set; }
		public bool IsPressedLeft { get; set; }

		#region Backend
		internal bool WasPressed { get; set; }
		internal bool WasPressedShift { get; set; }
		internal bool WasPressedControl { get; set; }
		internal bool WasPressedBackspace { get; set; }
		internal bool WasPressedEscape { get; set; }
		internal bool WasPressedTab { get; set; }
		internal bool WasPressedAlt { get; set; }
		internal bool WasPressedEnter { get; set; }
		internal bool WasPressedUp { get; set; }
		internal bool WasPressedDown { get; set; }
		internal bool WasPressedRight { get; set; }
		internal bool WasPressedLeft { get; set; }

		internal string PrevTypedSymbols { get; set; }
		#endregion
	}
}
