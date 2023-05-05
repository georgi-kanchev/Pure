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
		private Color currentColor = Color.Red;

		public MyCustomButton(Tilemap tilemap, (int x, int y) position) : base(position)
			=> this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Size = (Text.Length + 2, 3);

			var color = currentColor;

			if (IsHovered) color = color.ToBright();
			if (IsHeld) color = color.ToDark();

			tilemap.SetBorder(Position, Size,
				Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, color.ToDark());
			tilemap.SetTextLine((Position.x + 1, Position.y + 1), Text, color);

		}
		protected override void OnUserEvent(UserEvent userEvent)
		{
			if (userEvent == UserEvent.Trigger)
				currentColor = currentColor == Color.Red ? Color.Gray : Color.Red;
		}
	}
	class MyCustomCheckbox : Checkbox
	{
		private Tilemap tilemap;

		public MyCustomCheckbox(Tilemap tilemap, (int x, int y) position) :
			base(position) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var color = Color.Red;
			Size = (Text.Length + 2, 1);

			if (IsChecked) color = Color.Green;
			if (IsHovered) color = Color.White;
			if (IsHeld) color = Color.Gray;

			var tile = new Tile(IsChecked ? Tile.ICON_TICK : Tile.UPPERCASE_X, color);
			tilemap.SetTile(Position, tile);
			tilemap.SetTextLine((Position.x + 2, Position.y), Text, color);
		}
	}
	class MyCustomInputBox : InputBox
	{
		private Tilemap back, middle, front;

		public MyCustomInputBox(Tilemap back, Tilemap middle, Tilemap front, (int x, int y) position) :
			base(position)
		{
			this.back = back;
			this.middle = middle;
			this.front = front;
			Size = (12, 3);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			back.SetSquare(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray));
			back.SetTextSquare(Position, Size, Selection, Color.Blue, isWordWrapping: false);
			middle.SetTextSquare(Position, Size, Text, isWordWrapping: false);

			if (string.IsNullOrWhiteSpace(Text) && CursorIndex == 0)
				middle.SetTextSquare(Position, Size, Placeholder, Color.Gray.ToBright(), false);

			if (IsFocused)
				front.SetTile(CursorPosition, new(Tile.SHAPE_LINE, Color.White, 2));
		}
	}

	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		var back = new Tilemap(tilemap.Size);
		var front = new Tilemap(tilemap.Size);

		var elements = new List<Element>();
		var button = new MyCustomButton(tilemap, (2, 2));
		var checkbox = new MyCustomCheckbox(tilemap, (2, 6));
		var inputbox = new MyCustomInputBox(back, tilemap, front, (2, 8));

		elements.Add(button);
		elements.Add(checkbox);
		elements.Add(inputbox);

		Window.Create(Window.Mode.Windowed);
		while (Window.IsOpen)
		{
			Window.Activate(true);

			back.Fill();
			tilemap.Fill();
			front.Fill();

			Element.ApplyInput(
				isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
				position: tilemap.PointFrom(Mouse.CursorPosition, Window.Size),
				scrollDelta: Mouse.ScrollDelta,
				keysPressed: Keyboard.KeyIDsPressed,
				keysTyped: Keyboard.KeyTyped,
				tilemapSize: tilemap.Size);

			for (int i = 0; i < elements.Count; i++)
				elements[i].Update();

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			Window.DrawTiles(back.ToBundle());
			Window.DrawTiles(tilemap.ToBundle());
			Window.DrawTiles(front.ToBundle());
			Window.Activate(false);
		}
	}
}