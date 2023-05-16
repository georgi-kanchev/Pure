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

			tilemap.SetBorder(Position, Size,
				Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, GetColor(this, Color.Yellow).ToDark());
			tilemap.SetTextLine((Position.x + 1, Position.y + 1), Text, GetColor(this, Color.Yellow));
			tilemap.SetTextLine((Position.x + 1 + Size.width, Position.y + 1), $"{clickCount}", Color.White);
		}
		protected override void OnUserEvent(UserEvent userEvent)
		{
			if (userEvent == UserEvent.Trigger)
				clickCount++;
		}
	}
	class MyCustomCheckbox : Button
	{
		private readonly Tilemap tilemap;

		public MyCustomCheckbox(Tilemap tilemap, (int x, int y) position) :
			base(position)
		{
			this.tilemap = tilemap;
			Text = "Checkbox";
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			Size = (Text.Length + 2, 1);

			var tile = new Tile(IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X, GetColor(this, Color.Red));
			tilemap.SetTile(Position, tile);
			tilemap.SetTextLine((Position.x + 2, Position.y), Text, GetColor(this, Color.Red));
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

			back.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray));
			back.SetTextRectangle(Position, Size, Selection, IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
			middle.SetTextRectangle(Position, Size, Text, isWordWrapping: false);

			if (string.IsNullOrWhiteSpace(Text) && CursorIndex == 0)
				middle.SetTextRectangle(Position, Size, Placeholder, Color.Gray.ToBright(), false);

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

			tilemap.SetBar(Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, Size.width);
			tilemap.SetTile(Handle.Position, new(Tile.SHADE_OPAQUE, GetColor(this, Color.Yellow)));
			tilemap.SetTextLine((Position.x + Size.width + 1, Position.y), $"{Progress:F2}");
		}
	}
	class MyCustomList : List
	{
		private readonly Tilemap tilemap;

		public MyCustomList(Tilemap tilemap, (int x, int y) position, Types type, int itemCount = 10) :
			base(position, itemCount, type) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var scorllUpAng = (sbyte)(Type == Types.Horizontal ? 0 : 3);
			var scorllDownAng = (sbyte)(Type == Types.Horizontal ? 2 : 1);
			var scrollColor = Color.Gray.ToBright();
			tilemap.SetTile(ScrollUp.Position, new(Tile.ARROW, GetColor(ScrollUp, scrollColor), scorllUpAng));
			tilemap.SetTile(Scroll.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(Scroll, scrollColor)));
			tilemap.SetTile(ScrollDown.Position, new(Tile.ARROW, GetColor(ScrollDown, scrollColor), scorllDownAng));
		}
		protected override void OnItemUpdate(Button item)
		{
			var color = item.IsSelected ? Color.Green : Color.Red;

			if (item.IsPressedAndHeld) color = color.ToDark();
			else if (item.IsHovered) color = color.ToBright();

			var text = item.Text;
			if (item.Size.width < text.Length)
			{
				text = text[..Math.Max(item.Size.width - 1, 0)];
				var tile = new Tile(Tile.PUNCTUATION_ELLIPSIS, color);
				tilemap.SetTile((item.Position.x + item.Size.width - 1, item.Position.y), tile);
			}

			tilemap.SetTextLine(item.Position, text, color);

			var (itemX, itemY) = item.Position;
			if (IsExpanded == false)
				tilemap.SetTile((itemX + item.Size.width - 1, itemY), new(Tile.MATH_GREATER, color, 1));
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

			background.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
			foreground.SetBorder(Position, Size, Tile.BORDER_GRID_CORNER, Tile.BORDER_GRID_STRAIGHT, Color.Blue);
			background.SetRectangle((Position.x + 1, Position.y), (Size.width - 2, 1),
				new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
			foreground.SetTextRectangle((Position.x, Position.y), (Size.width, 1), Text,
				alignment: Tilemap.Alignment.Center);
		}
	}
	class MyCustomPagination : Pagination
	{
		private readonly Tilemap tilemap;

		public MyCustomPagination(Tilemap tilemap, (int x, int y) position, int count = 10) :
			base(position, count) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			tilemap.SetTile(First.Position, new(Tile.MATH_MUCH_LESS, GetColor(First, Color.Gray)));
			tilemap.SetTile(Previous.Position, new(Tile.MATH_LESS, GetColor(Previous, Color.Gray)));
			tilemap.SetTile(Next.Position, new(Tile.MATH_GREATER, GetColor(Next, Color.Gray)));
			tilemap.SetTile(Last.Position, new(Tile.MATH_MUCH_GREATER, GetColor(Last, Color.Gray)));
		}
		protected override void OnPageUpdate(Button page)
		{
			var color = page.IsSelected ? Color.Green : Color.Gray;
			tilemap.SetTextLine(page.Position, page.Text, GetColor(page, color));
		}
	}
	class MyCustomPalette : Palette
	{
		private readonly Tilemap tilemap;

		public MyCustomPalette(Tilemap tilemap, (int x, int y) position, int brightnessLevels) :
			base(position, brightnessLevels) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var (x, y) = Position;
			var tile = new Tile(Tile.SHADE_OPAQUE, GetColor(Opacity, Color.Gray.ToBright()));
			var alpha = Opacity;
			tilemap.SetBar(alpha.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, alpha.Size.width);
			tilemap.SetTile(alpha.Handle.Position, tile);

			var first = Brightness.First;
			var previous = Brightness.Previous;
			var next = Brightness.Next;
			var last = Brightness.Last;
			tilemap.SetTile(first.Position, new(Tile.MATH_MUCH_LESS, GetColor(first, Color.Gray)));
			tilemap.SetTile(previous.Position, new(Tile.MATH_LESS, GetColor(previous, Color.Gray)));
			tilemap.SetTile(next.Position, new(Tile.MATH_GREATER, GetColor(next, Color.Gray)));
			tilemap.SetTile(last.Position, new(Tile.MATH_MUCH_GREATER, GetColor(last, Color.Gray)));

			tilemap.SetTile(Pick.Position, new(Tile.MATH_PLUS, GetColor(Pick, Color.Gray)));

			tilemap.SetRectangle((x, y - 3), (Size.width, 3), new(Tile.SHADE_OPAQUE, SelectedColor));
		}

		protected override void OnSampleUpdate(Button sample, uint color)
		{
			tilemap.SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
		}
		protected override void OnPageUpdate(Button page)
		{
			var color = page.IsSelected ? Color.Green : Color.Gray;
			tilemap.SetTextLine(page.Position, page.Text, GetColor(page, color));
		}
		protected override uint OnPick((float x, float y) position)
		{
			return tilemap.TileAt(((int)position.x, (int)position.y)).Tint;
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
		var listVertical = new MyCustomList(tilemap, default, List.Types.Vertical, itemCount: 20)
		{
			IsSingleSelecting = true,
			ItemGap = (0, 0)
		};

		var panelHorizontal = new MyCustomPanel(back, tilemap, (2, 20))
		{
			Size = (25, 3),
			IsResizable = false,
			IsMovable = false
		};
		var listHorizontal = new MyCustomList(tilemap, (2, 20), List.Types.Horizontal, itemCount: 4);

		var elements = new List<Element>()
		{
			new MyCustomButton(tilemap, (2, 2)),
			new MyCustomCheckbox(tilemap, (2, 7)),
			new MyCustomInputBox(back, tilemap, front, (2, 10)) { Size = (12, 4) },
			new MyCustomSlider(tilemap, (2, 17), size: 7),
			new MyCustomPagination(tilemap, (27, 2)) { Size = (18, 2) },
			new MyCustomList(tilemap, (27, 5), List.Types.Dropdown) { Size = (6, 9) },
			panelHorizontal, listHorizontal,
			panelVertical, listVertical,
			new MyCustomPalette(tilemap, (34, 16), brightnessLevels: 30),
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

			back.SetRectangle((34, 17), (13, 1), new(Tile.SHADE_5, Color.Gray.ToDark()));
			back.SetRectangle((34, 13), (13, 3), new(Tile.SHADE_5, Color.Gray.ToDark()));

			for (int i = 0; i < elements.Count; i++)
				elements[i].Update();

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			if (Keyboard.IsKeyPressed(Keyboard.Key.A).Once("hoink"))
			{
				var pagination = (MyCustomPagination)elements[4];
				pagination.Count = 3;
			}
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

	private static Color GetColor(Element element, Color baseColor)
	{
		if (element.IsPressedAndHeld) return baseColor.ToDark();
		else if (element.IsHovered) return baseColor.ToBright();

		return baseColor;
	}
}