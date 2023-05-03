namespace Pure.Examples;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;
using Pure.UserInterface;

public class Program
{
	class MyCustomButton : Button
	{
		private Tilemap tilemap;

		public MyCustomButton(Tilemap tilemap, (int x, int y) position, (int width, int height) size) :
			base(position, size)
		{
			this.tilemap = tilemap;
		}

		protected override void OnUpdate()
		{
			Size = (Text.Length + 2, 3);

			var color = Color.Gray;

			if (IsHovered) color = Color.White;
			if (IsHeld) color = Color.Gray.ToDark();
			if (IsDisabled) color = Color.Red;

			tilemap.SetBorder(Position, Size,
				Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, color.ToDark());
			tilemap.SetTextLine((Position.x + 1, Position.y + 1), Text, color);

		}
		protected override void OnUserEvent(UserEvent userEvent)
		{
			if (userEvent == UserEvent.Trigger)
			{
				Text = Text == "Ouch!" ? "Ow!" : "Ouch!";
				IsDisabled = true;
			}
		}
	}

	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		var button = new MyCustomButton(tilemap, (2, 2), (1, 1)) { Text = "Not Clicked!" };

		Window.Create(Window.Mode.Windowed);

		while (Window.IsOpen)
		{
			Window.Activate(true);

			tilemap.Fill();

			Element.ApplyInput(
				Mouse.IsButtonPressed(Mouse.Button.LEFT),
				tilemap.PointFrom(Mouse.CursorPosition, Window.Size),
				Mouse.ScrollDelta,
				Keyboard.KeysPressed,
				Keyboard.KeyTyped,
				tilemap.Size);
			button.Update();

			Window.DrawTiles(tilemap.ToBundle());
			Window.Activate(false);
		}
	}
}