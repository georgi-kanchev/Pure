namespace Pure.Examples.Systems;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

using static Pure.UserInterface.List;

public static class UserInterface
{
	class UI : Pure.UserInterface.UserInterface
	{
		private readonly Tilemap back, middle, front;
		private int buttonClickCount = 0;

		public UI(Tilemap back, Tilemap middle, Tilemap front)
		{
			this.back = back;
			this.middle = middle;
			this.front = front;
		}

		protected override void OnUserActionButton(string key, Button button, UserAction userAction)
		{
			if (key == "button" && userAction == UserAction.Trigger)
				buttonClickCount++;
		}

		protected override void OnUpdateButton(string key, Button button)
		{
			var e = button;
			if (key == "button")
			{
				e.Size = (e.Text.Length + 2, 3);

				middle.SetBorder(e.Position, e.Size,
					Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, GetColor(e, Color.Yellow).ToDark());
				middle.SetTextLine((e.Position.x + 1, e.Position.y + 1), e.Text, GetColor(e, Color.Yellow));
				middle.SetTextLine((e.Position.x + 1 + e.Size.width, e.Position.y + 1), $"{buttonClickCount}", Color.White);
			}
			else if (key == "checkbox")
			{
				e.Size = (e.Text.Length + 2, 1);

				var color = e.IsSelected ? Color.Green : Color.Red;
				var tile = new Tile(e.IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X, GetColor(button, color));
				middle.SetTile(e.Position, tile);
				middle.SetTextLine((e.Position.x + 2, e.Position.y), e.Text, GetColor(button, color));
			}
		}
		protected override void OnUpdateInputBox(string key, InputBox inputBox)
		{
			var e = inputBox;
			back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray));
			back.SetTextRectangle(e.Position, e.Size, e.Selection, e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
			middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

			if (string.IsNullOrWhiteSpace(e.Text) && e.CursorIndex == 0)
				middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);

			if (e.IsCursorVisible)
				front.SetTile(e.CursorPosition, new(Tile.SHAPE_LINE, Color.White, 2));
		}
		protected override void OnUpdateSlider(string key, Slider slider)
		{
			var e = slider;
			middle.SetBar(e.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, e.Size.width);
			middle.SetTile(e.Handle.Position, new(Tile.SHADE_OPAQUE, GetColor(e, Color.Yellow)));
			middle.SetTextLine((e.Position.x + e.Size.width + 1, e.Position.y), $"{e.Progress:F2}");
		}
		protected override void OnUpdateList(string key, List list) => OnUpdateScroll(key, list.Scroll);
		protected override void OnUpdateListItem(string key, List list, Button item)
		{
			var color = item.IsSelected ? Color.Green : Color.Red;

			middle.SetTextLine(item.Position, item.Text, GetColor(item, color), item.Size.width);

			var (itemX, itemY) = item.Position;
			var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(item, color), 1);
			if (list.IsExpanded == false)
				middle.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
		}
		protected override void OnUpdatePanel(string key, Panel panel)
		{
			var e = panel;
			back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
			front.SetBorder(e.Position, e.Size, Tile.BORDER_GRID_CORNER, Tile.BORDER_GRID_STRAIGHT, Color.Blue);
			back.SetRectangle((e.Position.x + 1, e.Position.y), (e.Size.width - 2, 1), new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
			front.SetTextRectangle((e.Position.x, e.Position.y), (e.Size.width, 1), e.Text, alignment: Tilemap.Alignment.Center);
		}
		protected override void OnUpdatePages(string key, Pages pages)
		{
			var e = pages;
			middle.SetTile(e.First.Position, new(Tile.MATH_MUCH_LESS, GetColor(e.First, Color.Gray)));
			middle.SetTile(e.Previous.Position, new(Tile.MATH_LESS, GetColor(e.Previous, Color.Gray)));
			middle.SetTile(e.Next.Position, new(Tile.MATH_GREATER, GetColor(e.Next, Color.Gray)));
			middle.SetTile(e.Last.Position, new(Tile.MATH_MUCH_GREATER, GetColor(e.Last, Color.Gray)));
		}
		protected override void OnUpdatePagesPage(string key, Pages pages, Button page)
		{
			var color = page.IsSelected ? Color.Green : Color.Gray;
			middle.SetTextLine(page.Position, page.Text, GetColor(page, color));
		}
		protected override void OnUpdatePalette(string key, Palette palette)
		{
			var e = palette;
			var (x, y) = e.Position;
			var tile = new Tile(Tile.SHADE_OPAQUE, GetColor(e.Opacity, Color.Gray.ToBright()));
			var alpha = e.Opacity;
			middle.SetBar(alpha.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, alpha.Size.width);
			middle.SetTile(alpha.Handle.Position, tile);

			var first = e.Brightness.First;
			var previous = e.Brightness.Previous;
			var next = e.Brightness.Next;
			var last = e.Brightness.Last;
			middle.SetTile(first.Position, new(Tile.MATH_MUCH_LESS, GetColor(first, Color.Gray)));
			middle.SetTile(previous.Position, new(Tile.MATH_LESS, GetColor(previous, Color.Gray)));
			middle.SetTile(next.Position, new(Tile.MATH_GREATER, GetColor(next, Color.Gray)));
			middle.SetTile(last.Position, new(Tile.MATH_MUCH_GREATER, GetColor(last, Color.Gray)));

			middle.SetTile(e.Pick.Position, new(Tile.MATH_PLUS, GetColor(e.Pick, Color.Gray)));

			middle.SetRectangle((x, y - 3), (e.Size.width, 3), new(Tile.SHADE_OPAQUE, e.SelectedColor));
		}
		protected override void OnUpdatePalettePage(string key, Palette palette, Button page)
		{
			OnUpdatePagesPage(key, palette.Brightness, page); // display the same kind of pages
		}
		protected override void OnUpdatePaletteSample(string key, Palette palette, Button sample, uint color)
		{
			middle.SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
		}
		protected override void OnUpdateNumericScroll(string key, NumericScroll numericScroll)
		{
			var e = numericScroll;
			middle.SetTile(e.Down.Position, new(Tile.ARROW, GetColor(e.Down, Color.Gray), 1));
			middle.SetTextLine((e.Position.x, e.Position.y + 1), $"{e.Value}");
			middle.SetTile(e.Up.Position, new(Tile.ARROW, GetColor(e.Up, Color.Gray), 3));
		}
		protected override void OnUpdateScroll(string key, Scroll scroll)
		{
			var e = scroll;
			var scorllUpAng = (sbyte)(e.IsVertical ? 3 : 0);
			var scorllDownAng = (sbyte)(e.IsVertical ? 1 : 2);
			var scrollColor = Color.Gray.ToBright();
			middle.SetTile(e.Up.Position, new(Tile.ARROW, GetColor(e.Up, scrollColor), scorllUpAng));
			middle.SetTile(e.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(e.Slider, scrollColor)));
			middle.SetTile(e.Down.Position, new(Tile.ARROW, GetColor(e.Down, scrollColor), scorllDownAng));
		}

		// a special kind of action - the palette needs the tile color at a certain position
		// and we happen to have it in the tilemap
		// potentially inheriting Palette and receiving the action there would take priority over this one
		protected override uint OnPalettePick(string key, Palette palette, (float x, float y) position)
		{
			return middle.TileAt(((int)position.x, (int)position.y)).Tint;
		}

		// simple "animation" for hovering and pressing buttons
		private static Color GetColor(Element element, Color baseColor)
		{
			if (element.IsPressedAndHeld) return baseColor.ToDark();
			else if (element.IsHovered) return baseColor.ToBright();

			return baseColor;
		}
	}

	public static void Run()
	{
		Window.Create(3);

		var (width, height) = Window.MonitorAspectRatio;
		var tilemaps = new TilemapManager(3, (width * 3, height * 3));
		var back = tilemaps[0];
		var middle = tilemaps[1];
		var front = tilemaps[2];

		var userInterface = new UI(back, middle, front);
		userInterface["button"] = new Button((2, 2));
		userInterface["checkbox"] = new Button((2, 7)) { Text = "Checkbox" };
		userInterface["inputbox"] = new InputBox((2, 10)) { Size = (12, 4) };
		userInterface["slider"] = new Slider((2, 17), 7);

		userInterface["numeric-scroll"] = new NumericScroll((34, 6)) { Range = (-9, 13) };
		userInterface["list-dropdown"] = new List((27, 5), 15, Types.Dropdown) { Size = (6, 9) };
		userInterface["scroll-vertical"] = new Scroll((37, 6), 9);
		userInterface["scroll-horizontal"] = new Scroll((38, 15), 9, false);
		userInterface["palette"] = new Palette((34, 20), brightnessLevels: 30);
		userInterface["pages"] = new Pages((27, 2)) { Size = (18, 2) };
		userInterface["panel-horizontal"] = new Panel((2, 20))
		{
			Size = (25, 4),
			IsResizable = false,
			IsMovable = false,
		};
		userInterface["list-horizontal"] = new List((2, 20), 10, Types.Horizontal);
		userInterface["panel-vertical"] = new Panel((16, 2))
		{
			Size = (9, 16),
			MinimumSize = (5, 5),
		};
		userInterface["list-vertical"] = new List(default, 15, Types.Vertical)
		{
			IsSingleSelecting = true,
			ItemGap = (0, 1),
			ItemMaximumSize = (7, 1)
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

			StickListToPanel(userInterface["panel-vertical"], userInterface["list-vertical"]);
			StickListToPanel(userInterface["panel-horizontal"], userInterface["list-horizontal"]);

			back.SetRectangle((34, 17), (13, 6), new(Tile.SHADE_5, Color.Gray.ToDark()));

			userInterface.Update();

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			for (int i = 0; i < tilemaps.Count; i++)
				Window.DrawTiles(tilemaps[i].ToBundle());

			Window.Activate(false);
		}

		static void StickListToPanel(Element panel, Element list)
		{
			list.Position = (panel.Position.x + 1, panel.Position.y + 1);
			list.Size = (panel.Size.width - 2, panel.Size.height - 2);
		}
	}
}