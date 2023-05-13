namespace Pure.Examples.Systems;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public static class UserInterface
{
	class MyCustomButton : Button
	{
		private readonly Tilemap tilemap;
		private int clickCount = 0;

		public MyCustomButton(Tilemap tilemap, (int x, int y) position) : base(position)
			=> this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			Size = (Text.Length + 2, 3);

			var color = Color.Yellow;

			if (IsHeld) color = color.ToDark();
			else if (IsHovered) color = color.ToBright();

			tilemap.SetBorder(Position, Size,
				Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, color.ToDark());
			tilemap.SetTextLine((Position.x + 1, Position.y + 1), Text, color);
			tilemap.SetTextLine((Position.x + 1 + Size.width, Position.y + 1), $"{clickCount}", Color.White);
		}
		protected override void OnUserEvent(UserEvent userEvent)
		{
			if (userEvent == UserEvent.Trigger)
				clickCount++;
		}
	}
	class MyCustomCheckbox : Checkbox
	{
		private readonly Tilemap tilemap;

		public MyCustomCheckbox(Tilemap tilemap, (int x, int y) position) :
			base(position) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var color = Color.Red;
			Size = (Text.Length + 2, 1);

			if (IsChecked) color = Color.Green;
			if (IsHeld) color = Color.Gray;
			else if (IsHovered) color = Color.White;

			var tile = new Tile(IsChecked ? Tile.ICON_TICK : Tile.UPPERCASE_X, color);
			tilemap.SetTile(Position, tile);
			tilemap.SetTextLine((Position.x + 2, Position.y), Text, color);
		}
	}
	class MyCustomInputBox : InputBox
	{
		private readonly Tilemap back, middle, front;

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
			back.SetTextSquare(Position, Size, Selection, IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
			middle.SetTextSquare(Position, Size, Text, isWordWrapping: false);

			if (string.IsNullOrWhiteSpace(Text) && CursorIndex == 0)
				middle.SetTextSquare(Position, Size, Placeholder, Color.Gray.ToBright(), false);

			if (IsCursorVisible)
				front.SetTile(CursorPosition, new(Tile.SHAPE_LINE, Color.White, 2));
		}
	}
	class MyCustomSlider : Slider
	{
		private readonly Tilemap tilemap;

		public MyCustomSlider(Tilemap tilemap, (int x, int y) position, int size = 5) :
			base(position, size) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var color = Color.Yellow;

			if (IsHeld) color = color.ToDark();
			else if (IsHovered) color = color.ToBright();

			tilemap.SetBar(Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, Size.width);
			tilemap.SetTile(Handle.Position, new(Tile.SHADE_OPAQUE, color));
			tilemap.SetTextLine((Position.x + Size.width + 1, Position.y), $"{Progress:F2}");
		}
	}
	class MyCustomList : List
	{
		private readonly Tilemap tilemap;

		public MyCustomList(Tilemap tilemap, (int x, int y) position, int count = 10, bool isHorizontal = false) :
			base(position, count, isHorizontal) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var scrollUpColor = Color.Gray.ToBright();
			var scrollDownColor = Color.Gray.ToBright();
			var scrollHandleColor = Color.Gray.ToBright();

			if (ScrollUp.IsHovered) scrollUpColor = scrollUpColor.ToBright();
			if (ScrollUp.IsHeld) scrollUpColor = scrollUpColor.ToDark();

			if (Scroll.IsHovered) scrollHandleColor = scrollHandleColor.ToBright();
			if (Scroll.IsHeld) scrollHandleColor = scrollHandleColor.ToDark();

			if (ScrollDown.IsHovered) scrollDownColor = scrollDownColor.ToBright();
			if (ScrollDown.IsHeld) scrollDownColor = scrollDownColor.ToDark();

			var scorllUpAng = (sbyte)(IsHorizontal ? 0 : 3);
			var scorllDownAng = (sbyte)(IsHorizontal ? 2 : 1);
			tilemap.SetTile(ScrollUp.Position, new(Tile.ARROW, scrollUpColor, scorllUpAng));
			tilemap.SetTile(Scroll.Handle.Position, new(Tile.SHAPE_CIRCLE, scrollHandleColor));
			tilemap.SetTile(ScrollDown.Position, new(Tile.ARROW, scrollDownColor, scorllDownAng));
		}
		protected override void OnItemUpdate(Checkbox item)
		{
			var color = Color.Red;

			if (item.IsChecked) color = Color.Green;
			if (item.IsHeld) color = color.ToDark();
			else if (item.IsHovered) color = color.ToBright();

			var text = item.Text;
			if (item.Size.width < text.Length)
			{
				text = text[..Math.Max(item.Size.width - 1, 0)];
				var tile = new Tile(Tile.PUNCTUATION_ELLIPSIS, color);
				tilemap.SetTile((item.Position.x + item.Size.width - 1, item.Position.y), tile);
			}

			tilemap.SetTextLine(item.Position, text, color);
		}
	}
	class MyCustomPanel : Panel
	{
		private readonly Tilemap background, foreground;

		public MyCustomPanel(Tilemap background, Tilemap foreground, (int x, int y) position) : base(position)
		{
			this.background = background;
			this.foreground = foreground;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			background.SetSquare(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
			foreground.SetBorder(Position, Size, Tile.BORDER_GRID_CORNER, Tile.BORDER_GRID_STRAIGHT, Color.Blue);
			background.SetSquare((Position.x + 1, Position.y), (Size.width - 2, 1),
				new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
			foreground.SetTextSquare((Position.x, Position.y), (Size.width, 1), Text,
				alignment: Tilemap.Alignment.Center);
		}
	}

	public static void Run()
	{
		Window.Create(Window.Mode.Windowed);

		var aspectRatio = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));
		var back = new Tilemap(tilemap.Size);
		var front = new Tilemap(tilemap.Size);

		var panelVertical = new MyCustomPanel(back, tilemap, (16, 2)) { Size = (9, 16), MinimumSize = (5, 5) };
		var listVertical = new MyCustomList(tilemap, default, 5) { IsSingleSelecting = true, ItemGap = (0, 0) };

		var panelHorizontal = new MyCustomPanel(back, tilemap, (2, 20))
		{
			Size = (25, 3),
			IsResizable = false,
			IsMovable = false
		};
		var listHorizontal = new MyCustomList(tilemap, (2, 20), 4, true);

		var elements = new List<Element>()
		{
			new MyCustomButton(tilemap, (2, 2)),
			new MyCustomCheckbox(tilemap, (2, 7)),
			new MyCustomInputBox(back, tilemap, front, (2, 10)) { Size = (12, 4) },
			new MyCustomSlider(tilemap, (2, 17), 7),
			panelHorizontal, listHorizontal,
			panelVertical, listVertical,
		};

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

			StickListToPanel(panelVertical, listVertical);
			StickListToPanel(panelHorizontal, listHorizontal);

			for (int i = 0; i < elements.Count; i++)
				elements[i].Update();

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			Window.DrawTiles(back.ToBundle());
			Window.DrawTiles(tilemap.ToBundle());
			Window.DrawTiles(front.ToBundle());
			Window.Activate(false);
		}

		void StickListToPanel(MyCustomPanel panel, MyCustomList list)
		{
			list.Position = (panel.Position.x + 1, panel.Position.y + 1);
			list.Size = (panel.Size.width - 2, panel.Size.height - 2);
		}
	}
}