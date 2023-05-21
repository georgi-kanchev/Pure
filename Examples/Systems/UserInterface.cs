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

			var color = IsSelected ? Color.Green : Color.Red;
			var tile = new Tile(IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X, GetColor(this, color));
			tilemap.SetTile(Position, tile);
			tilemap.SetTextLine((Position.x + 2, Position.y), Text, GetColor(this, color));
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
			tilemap.SetTile(Scroll.Up.Position, new(Tile.ARROW, GetColor(Scroll.Up, scrollColor), scorllUpAng));
			tilemap.SetTile(Scroll.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(Scroll.Slider, scrollColor)));
			tilemap.SetTile(Scroll.Down.Position, new(Tile.ARROW, GetColor(Scroll.Down, scrollColor), scorllDownAng));
		}
		protected override void OnItemUpdate(Button item)
		{
			var color = item.IsSelected ? Color.Green : Color.Red;

			var text = item.Text;
			if (item.Size.width < text.Length)
			{
				text = text[..Math.Max(item.Size.width - 1, 0)];
				var tile = new Tile(Tile.PUNCTUATION_ELLIPSIS, color);
				tilemap.SetTile((item.Position.x + item.Size.width - 1, item.Position.y), tile);
			}

			tilemap.SetTextLine(item.Position, text, GetColor(item, color));

			var (itemX, itemY) = item.Position;
			var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(item, color), 1);
			if (IsExpanded == false)
				tilemap.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
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
	class MyCustomPages : Pages
	{
		private readonly Tilemap tilemap;

		public MyCustomPages(Tilemap tilemap, (int x, int y) position, int count = 10) :
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
	class MyCustomNumericScroll : NumericScroll
	{
		private readonly Tilemap tilemap;

		public MyCustomNumericScroll(Tilemap tilemap, (int x, int y) position) :
			base(position) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			tilemap.SetTile(Down.Position, new(Tile.ARROW, GetColor(Down, Color.Gray), 1));
			tilemap.SetTextLine((Position.x, Position.y + 1), $"{Value}");
			tilemap.SetTile(Up.Position, new(Tile.ARROW, GetColor(Up, Color.Gray), 3));
		}
	}
	class MyCustomScroll : Scroll
	{
		private readonly Tilemap tilemap;

		public MyCustomScroll(Tilemap tilemap, (int x, int y) position, int size = 5, bool isVertical = true) :
			base(position, size, isVertical) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var scorllUpAng = (sbyte)(IsVertical ? 3 : 0);
			var scorllDownAng = (sbyte)(IsVertical ? 1 : 2);
			var scrollColor = Color.Gray.ToBright();
			tilemap.SetTile(Up.Position, new(Tile.ARROW, GetColor(Up, scrollColor), scorllUpAng));
			tilemap.SetTile(Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(Slider, scrollColor)));
			tilemap.SetTile(Down.Position, new(Tile.ARROW, GetColor(Down, scrollColor), scorllDownAng));
		}
	}

	public static void Run()
	{
		Window.Create(Window.Mode.Windowed);

		var aspectRatio = Window.MonitorAspectRatio;
		var tilemaps = new TilemapManager(3, (aspectRatio.width * 3, aspectRatio.height * 3));
		var back = tilemaps[0];
		var middle = tilemaps[1];
		var front = tilemaps[2];

		var panelVertical = new MyCustomPanel(back, middle, (16, 2))
		{
			Size = (9, 16),
			MinimumSize = (5, 5)
		};
		var listVertical = new MyCustomList(middle, default, List.Types.Vertical, itemCount: 15)
		{
			IsSingleSelecting = true,
			ItemGap = (0, 1),
			ItemMaximumSize = (7, 1)
		};

		var panelHorizontal = new MyCustomPanel(back, middle, (2, 20))
		{
			Size = (25, 3),
			IsResizable = false,
			IsMovable = false,
		};
		var listHorizontal = new MyCustomList(middle, (2, 20), List.Types.Horizontal, itemCount: 4);

		var elements = new List<Element>()
		{
			new MyCustomButton(middle, (2, 2)),
			new MyCustomCheckbox(middle, (2, 7)),
			new MyCustomInputBox(back, middle, front, (2, 10)) { Size = (12, 4) },
			new MyCustomSlider(middle, (2, 17), size: 7),
			new MyCustomPages(middle, (27, 2)) { Size = (18, 2) },
			new MyCustomNumericScroll(middle, (34, 6)) { Range = (-9, 13) },
			new MyCustomList(middle, (27, 5), List.Types.Dropdown, 15) { Size = (6, 9) },
			new MyCustomScroll(middle, (37, 6), 9),
			new MyCustomScroll(middle, (38, 15), 9, false),
			panelHorizontal, listHorizontal,
			panelVertical, listVertical,
			new MyCustomPalette(middle, (34, 20), brightnessLevels: 30),
		};

		while (Window.IsOpen)
		{
			Window.Activate(true);

			tilemaps.Fill();

			Element.ApplyInput(
				isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
				position: tilemaps.PointFrom(Mouse.CursorPosition, Window.Size),
				scrollDelta: Mouse.ScrollDelta,
				keysPressed: Keyboard.KeyIDsPressed,
				keysTyped: Keyboard.KeyTyped,
				tilemapSize: tilemaps.Size);

			StickListToPanel(panelVertical, listVertical);
			StickListToPanel(panelHorizontal, listHorizontal);

			back.SetRectangle((34, 17), (13, 6), new(Tile.SHADE_5, Color.Gray.ToDark()));

			for (int i = 0; i < elements.Count; i++)
				elements[i].Update();

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			for (int i = 0; i < tilemaps.Count; i++)
				Window.DrawTiles(tilemaps[i].ToBundle());

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