namespace Pure.UserInterface
{
	public class Button : UserInterface
	{
		public Button((int, int) position, (int, int) size) : base(position, size) { }

		protected override void OnUpdate()
		{
			if(IsHovered)
				TrySetTileAndSystemCursor(TILE_HAND);
		}
	}
}
