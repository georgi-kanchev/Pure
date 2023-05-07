namespace Pure.Examples;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	class MyCustomButton : Button
	{
		private Tilemap tilemap;
		private Color currentColor = Color.Green;

		public MyCustomButton(Tilemap tilemap, (int x, int y) position) : base(position)
			=> this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Size = (Text.Length + 2, 3);

			var color = currentColor;

			if(IsHovered) color = color.ToBright();
			if(IsHeld) color = color.ToDark();

			tilemap.SetBorder(Position, Size,
				Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, color.ToDark());
			tilemap.SetTextLine((Position.x + 1, Position.y + 1), Text, color);
		}
		protected override void OnUserEvent(UserEvent userEvent)
		{
			if(userEvent == UserEvent.Trigger)
				currentColor = currentColor == Color.Green ? Color.Gray : Color.Green;
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

			if(IsChecked) color = Color.Green;
			if(IsHovered) color = Color.White;
			if(IsHeld) color = Color.Gray;

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
			back.SetTextSquare(Position, Size, Selection, IsFocused ? Color.Blue : Color.Cyan, isWordWrapping: false);
			middle.SetTextSquare(Position, Size, Text, isWordWrapping: false);

			if(string.IsNullOrWhiteSpace(Text) && CursorIndex == 0)
				middle.SetTextSquare(Position, Size, Placeholder, Color.Gray.ToBright(), false);

			if(IsCursorVisible)
				front.SetTile(CursorPosition, new(Tile.SHAPE_LINE, Color.White, 2));
		}
	}
	class MyCustomSlider : Slider
	{
		private Tilemap tilemap;

		public MyCustomSlider(Tilemap tilemap, (int x, int y) position, int size = 5) :
			base(position, size) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var color = Color.Yellow;

			if(IsHovered) color = color.ToBright();
			if(IsHeld) color = color.ToDark();

			tilemap.SetBar(Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, Size.width);
			tilemap.SetTile(Handle.Position, new(Tile.SHADE_OPAQUE, color));
			tilemap.SetTextLine((Position.x + Size.width + 1, Position.y), $"{Progress:F2}");
		}
	}
	class MyCustomList : List
	{
		private Tilemap tilemap;

		public MyCustomList(Tilemap tilemap, (int x, int y) position, int count = 10) :
			base(position, count) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			tilemap.SetTile(ScrollUp.Position, new(Tile.ARROW, Color.White, 3));
			tilemap.SetTile(Scroll.Handle.Position, new(Tile.SHAPE_CIRCLE, Color.White));
			tilemap.SetTile(ScrollDown.Position, new(Tile.ARROW, Color.White, 1));

			for(int i = 0; i < Count; i++)
			{
				var item = this[i];
				if(item == null)
					continue;

				var itemColor = Color.Red;
				if(item.IsChecked)
					itemColor = Color.Green;

				tilemap.SetTextLine(item.Position, $"{item.Text} {i}", itemColor);
			}
		}
	}
	class MyCustomPanel : Panel
	{
		private Tilemap background, foreground;

		public MyCustomPanel(Tilemap background, Tilemap foreground, (int x, int y) position) : base(position)
		{
			this.background = background;
			this.foreground = foreground;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			background.SetSquare(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray));
			foreground.SetBorder(Position, Size, Tile.BORDER_SOLID_CORNER, Tile.BORDER_SOLID_STRAIGHT, Color.Blue);
			foreground.SetTextLine((Position.x + 1, Position.y), Text);
		}
	}

	static void Main()
	{
		// panel less than minimum size can't move or resize

		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		var back = new Tilemap(tilemap.Size);
		var front = new Tilemap(tilemap.Size);

		var panel = new MyCustomPanel(back, tilemap, (20, 12)) { Size = (14, 7), AdditionalMinimumSize = (11, 3) };
		var list = new MyCustomList(tilemap, (20, 2));
		var elements = new List<Element>()
		{
			new MyCustomButton(tilemap, (2, 2)),
			new MyCustomCheckbox(tilemap, (2, 6)),
			new MyCustomInputBox(back, tilemap, front, (2, 8)),
			new MyCustomSlider(tilemap, (2, 12), 7),
			panel,
			list
		};

		Window.Create(Window.Mode.Windowed);

		while(Window.IsOpen)
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

			list.Position = (panel.Position.x + 1, panel.Position.y + 1);
			list.Size = (panel.Size.width - 2, panel.Size.height - 2);

			for(int i = 0; i < elements.Count; i++)
				elements[i].Update();

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			Window.DrawTiles(back.ToBundle());
			Window.DrawTiles(tilemap.ToBundle());
			Window.DrawTiles(front.ToBundle());
			Window.Activate(false);
		}
	}
}