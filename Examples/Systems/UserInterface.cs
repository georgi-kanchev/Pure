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
        private int buttonClickCount;

        public UI(Tilemap back, Tilemap middle, Tilemap front)
        {
            this.back = back;
            this.middle = middle;
            this.front = front;
        }

        protected override void OnUserActionButton(Button button, UserAction userAction)
        {
            if (button.Text == "Button" && userAction == UserAction.Trigger)
                buttonClickCount++;
        }

        protected override void OnUpdateButton(Button button)
        {
            var e = button;
            if (button.Text == "Button")
            {
                e.Size = (e.Text.Length + 2, 3);
                middle.SetBox(e.Position, e.Size, Tile.SHADE_TRANSPARENT,
                    Tile.BOX_DEFAULT_CORNER, Tile.BOX_DEFAULT_STRAIGHT,
                    GetColor(e, Color.Yellow).ToDark());
                middle.SetTextLine((e.Position.x + 1, e.Position.y + 1), e.Text,
                    GetColor(e, Color.Yellow.ToDark()));
                middle.SetTextLine((e.Position.x + 1 + e.Size.width, e.Position.y + 1),
                    $"{buttonClickCount}", Color.White);
            }
            else if (button.Text == "Checkbox")
            {
                e.Size = (e.Text.Length + 2, 1);

                var color = e.IsSelected ? Color.Green : Color.Red;
                var tile = new Tile(e.IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X,
                    GetColor(button, color));
                SetBackground(e);
                middle.SetTile(e.Position, tile);
                middle.SetTextLine((e.Position.x + 2, e.Position.y), e.Text, GetColor(button, color));
            }
        }
        protected override void OnUpdateInputBox(InputBox inputBox)
        {
            var e = inputBox;
            back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.3f)));
            back.SetTextRectangle(e.Position, e.Size, e.Selection,
                e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
            middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

            if (string.IsNullOrWhiteSpace(e.Text) && e.CursorIndex == 0)
                middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);

            if (e.IsCursorVisible)
                front.SetTile(e.PositionFromIndex(e.CursorIndex), new(Tile.SHAPE_LINE, Color.White, 2));
        }
        protected override void OnUpdateSlider(Slider slider)
        {
            var e = slider;
            back.SetBox(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray), Tile.BOX_CORNER_ROUND,
                Tile.SHADE_OPAQUE, Color.Gray);
            middle.SetBar(e.Handle.Position, Tile.BAR_DEFAULT_EDGE, Tile.BAR_DEFAULT_STRAIGHT,
                GetColor(e, Color.Magenta), e.Size.height, true);
            middle.SetTextLine((e.Position.x + e.Size.width + 1, e.Position.y + 1), $"{e.Progress:F2}");
        }
        protected override void OnUpdateList(List list)
        {
            SetBackground(list);
            OnUpdateScroll(list.Scroll);
        }
        protected override void OnUpdateListItem(List list, Button item)
        {
            var color = item.IsSelected ? Color.Green : Color.Red;

            middle.SetTextLine(item.Position, item.Text, GetColor(item, color), item.Size.width);

            var (itemX, itemY) = item.Position;
            var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(item, color), 1);
            if (list.IsExpanded == false)
                middle.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
        }
        protected override void OnUpdatePanel(Panel panel)
        {
            var e = panel;
            SetBackground(e);
            front.SetBox(e.Position, e.Size, Tile.SHADE_TRANSPARENT, Tile.BOX_GRID_CORNER,
                Tile.BOX_GRID_STRAIGHT, Color.Blue);
            back.SetRectangle((e.Position.x + 1, e.Position.y), (e.Size.width - 2, 1),
                new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
            front.SetTextRectangle((e.Position.x, e.Position.y), (e.Size.width, 1), e.Text,
                alignment: Tilemap.Alignment.Center);
        }
        protected override void OnUpdatePages(Pages pages)
        {
            var e = pages;
            SetBackground(e);
            var colorFirst = GetColor(e.First, Color.Gray);
            var colorPrevious = GetColor(e.Previous, Color.Gray);
            var colorNext = GetColor(e.Next, Color.Gray);
            var colorLast = GetColor(e.Last, Color.Gray);
            middle.SetTile(e.First.Position, new(Tile.MATH_MUCH_LESS, colorFirst));
            middle.SetTile((e.First.Position.x, e.First.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorFirst));
            middle.SetTile(e.Previous.Position, new(Tile.MATH_LESS, colorPrevious));
            middle.SetTile((e.Previous.Position.x, e.Previous.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorPrevious));
            middle.SetTile(e.Next.Position, new(Tile.MATH_GREATER, colorNext));
            middle.SetTile((e.Next.Position.x, e.Next.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorNext));
            middle.SetTile(e.Last.Position, new(Tile.MATH_MUCH_GREATER, colorLast));
            middle.SetTile((e.Last.Position.x, e.Last.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorLast));
        }
        protected override void OnUpdatePagesPage(Pages pages, Button page)
        {
            var color = page.IsSelected ? Color.Green : Color.Gray;
            middle.SetTextLine(page.Position, page.Text, GetColor(page, color));

            if (pages.Count == 10)
                middle.SetTile((page.Position.x, page.Position.y + 1),
                    new(Tile.ICON_HOME + int.Parse(page.Text), GetColor(page, color)));
        }
        protected override void OnUpdatePalette(Palette palette)
        {
            var e = palette;
            var (x, y) = e.Position;
            var (w, h) = e.Size;
            var tile = new Tile(Tile.SHADE_OPAQUE, GetColor(e.Opacity, Color.Gray.ToBright()));
            var alpha = e.Opacity;

            back.SetRectangle((x, y - 3), (w, h + 3), new(Tile.SHADE_5, Color.Gray.ToDark()));
            middle.SetBar(alpha.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray,
                alpha.Size.width);
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
        protected override void OnUpdatePalettePage(Palette palette, Button page)
        {
            OnUpdatePagesPage(palette.Brightness, page); // display the same kind of pages
        }
        protected override void OnUpdatePaletteSample(Palette palette, Button sample,
            uint color)
        {
            middle.SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
        }
        protected override void OnUpdateStepper(Stepper stepper)
        {
            var e = stepper;
            SetBackground(e);
            middle.SetTile(e.Down.Position, new(Tile.ARROW, GetColor(e.Down, Color.Gray), 1));
            middle.SetTextLine((e.Position.x, e.Position.y + 1), $"{e.Value}");
            middle.SetTile(e.Up.Position, new(Tile.ARROW, GetColor(e.Up, Color.Gray), 3));
        }
        protected override void OnUpdateScroll(Scroll scroll)
        {
            var e = scroll;
            var scrollUpAng = (sbyte)(e.IsVertical ? 3 : 0);
            var scrollDownAng = (sbyte)(e.IsVertical ? 1 : 2);
            var scrollColor = Color.Gray.ToBright();

            SetBackground(e);
            middle.SetTile(e.Up.Position, new(Tile.ARROW, GetColor(e.Up, scrollColor), scrollUpAng));
            middle.SetTile(e.Slider.Handle.Position,
                new(Tile.SHAPE_CIRCLE, GetColor(e.Slider, scrollColor)));
            middle.SetTile(e.Down.Position,
                new(Tile.ARROW, GetColor(e.Down, scrollColor), scrollDownAng));
        }

        // a special kind of action - the palette needs the tile color at a certain position
        // and we happen to have it in the tilemap
        // potentially inheriting Palette and receiving the action there would take priority over this one
        protected override uint OnPalettePick(Palette palette, (float x, float y) position)
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
        private void SetBackground(Element element)
        {
            var tile = new Tile(Tile.SHADE_OPAQUE, Color.Gray.ToDark());
            back.SetRectangle(element.Position, element.Size, tile);
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
        var dropdown = new List((27, 5), 15, Types.Dropdown)
        {
            IsExpanded = true,
            Size = (6, 6)
        };
        dropdown.IsExpanded = false;

        var userInterface = new UI(back, middle, front);
        //userInterface.Add(new Button((2, 2)));
        //userInterface.Add(new Button((2, 7)) { Text = "Checkbox" });
        //userInterface.Add(new Slider((2, 15), 7) { Size = (7, 3) });
        //userInterface.Add(new Stepper((34, 6)) { Range = (-9, 13) });
        //userInterface.Add(dropdown);
        //userInterface.Add(new Scroll((37, 6), 9));
        //userInterface.Add(new Scroll((38, 15), 9, false));
        //userInterface.Add(new Palette((34, 23), brightnessLevels: 30));
        //userInterface.Add(new Pages((27, 2)) { Size = (18, 2) });
        //userInterface.Add(new Panel((18, 22))
        //{
        //	Size = (15, 4),
        //	IsResizable = false,
        //	IsMovable = false,
        //});
        //userInterface.Add(new List((19, 23), 10, Types.Horizontal)
        //{
        //	Size = (13, 2)
        //});
        var panelIndex = userInterface.Count;
        userInterface.Add(new Panel((0, 0))
        {
            Size = (9, 16),
            SizeMinimum = (5, 3),
        });
        //userInterface.Add(new List((17, 3), 15)
        //{
        //	Size = (7, 9),
        //	IsSingleSelecting = true,
        //	ItemGap = (0, 1),
        //	ItemMaximumSize = (7, 1)
        //});
        var inputIndex = userInterface.Count;
        userInterface.Add(new InputBox((0, 0)));

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

            userInterface.Update();

            Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

            var b = userInterface[inputIndex];
            var p = userInterface[panelIndex];
            b.Size = (p.Size.width - 2, p.Size.height - 2);
            b.Position = (p.Position.x + 1, p.Position.y + 1);

            for (var i = 0; i < tilemaps.Count; i++)
                Window.DrawTiles(tilemaps[i].ToBundle());

            Window.Activate(false);
        }
    }
}