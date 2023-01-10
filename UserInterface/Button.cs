namespace Pure.UserInterface
{
	public class Button : UserInterface
	{
		public Button((int, int) position, (int, int) size) : base(position, size) { }

		#region Backend
		protected override void OnUpdate()
		{
			if(IsHovered)
				SetTileAndSystemCursor(CursorResult.TileHand);
		}
		#endregion
	}
}
