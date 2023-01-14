namespace Pure.UserInterface
{
	public class Checkbox : UserInterface
	{
		public bool IsChecked { get; set; }

		public Checkbox((int, int) position) : base(position, (1, 1)) { }

		#region Backend
		protected override void OnUpdate()
		{
			if(IsHovered)
				SetTileAndSystemCursor(TILE_HAND);
		}

		protected override void OnEvent(When when)
		{
			if(when == When.Trigger)
				IsChecked = IsChecked == false;
		}
		#endregion
	}
}
