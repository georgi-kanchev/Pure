namespace Pure.Examples.Systems;

using Tilemap;
using Pure.UserInterface;
using Utilities;
using Window;
using static Pure.UserInterface.List;

public static class UserInterface
{
    private class UI : Pure.UserInterface.UserInterface
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

        protected override void OnDisplayButton(Button button)
        {
            var e = button;
            var (w, h) = e.Size;
            if (e.Text == "Button")
            {
                var offW = w / 2 - Math.Min(e.Text.Length, w - 2) / 2;
                middle.SetBox(e.Position, e.Size, Tile.EMPTY,
                    Tile.BOX_DEFAULT_CORNER, Tile.BOX_DEFAULT_STRAIGHT,
                    GetColor(e, Color.Yellow).ToDark());
                middle.SetTextLine(
                    (e.Position.x + offW, e.Position.y + h / 2),
                    e.Text,
                    GetColor(e, Color.Yellow.ToDark()),
                    e.Size.width - 2);
                middle.SetTextLine((e.Position.x + 1 + e.Size.width, e.Position.y + 1),
                    $"{buttonClickCount}", Color.White);
            }
            else if (e.Text == "Checkbox")
            {
                e.Size = (e.Text.Length + 2, 1);

                var color = e.IsSelected ? Color.Orange : Color.Red;
                var tile = new Tile(e.IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X,
                    GetColor(e, color));
                SetBackground(back, e);
                middle.SetTile(e.Position, tile);
                middle.SetTextLine((e.Position.x + 2, e.Position.y), e.Text, GetColor(button, color));
            }
        }
        protected override void OnDisplayInputBox(InputBox inputBox)
        {
            var e = inputBox;

            back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
            back.SetTextRectangle(e.Position, e.Size, e.Selection,
                e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
            middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

            if (string.IsNullOrWhiteSpace(e.Text))
                middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);

            if (e.IsCursorVisible)
                front.SetTile(e.PositionFromIndices(e.CursorIndices),
                    new(Tile.SHAPE_LINE, Color.White, 2));
        }
        protected override void OnDisplaySlider(Slider slider)
        {
            var e = slider;
            var (w, h) = e.Size;
            var text = $"{e.Progress:F2}";
            var isHandle = e.Handle.IsPressedAndHeld;

            SetBackground(middle, e);
            front.SetBar(e.Handle.Position, Tile.BAR_DEFAULT_EDGE, Tile.BAR_DEFAULT_STRAIGHT,
                GetColor(isHandle ? e.Handle : e, Color.Magenta), e.Size.height, true);
            front.SetTextLine((e.Position.x + w / 2 - text.Length / 2, e.Position.y + h / 2), text);
        }
        protected override void OnDisplayList(List list)
        {
            back.SetRectangle(list.Position, list.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
            OnDisplayScroll(list.Scroll);

            var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(list, Color.Gray.ToBright()), 1);
            if (list.IsExpanded == false)
                front.SetTile((list.Position.x + list.Size.width - 1, list.Position.y), dropdownTile);
        }
        protected override void OnDisplayListItem(List list, Button item)
        {
            var color = item.IsSelected ? Color.Green : Color.Gray.ToBright(0.3f);
            var (x, y) = item.Position;
            var (_, h) = item.Size;

            SetBackground(middle, item, 0.25f);
            front.SetTextLine((x, y + h / 2), item.Text, GetColor(item, color), item.Size.width);
        }
        protected override void OnDisplayFileViewer(FileViewer fileViewer)
        {
            var e = fileViewer;
            var (x, y) = e.Position;

            back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
            OnDisplayScroll(e.FilesAndFolders.Scroll);
            front.SetTextLine((x, y - 1), e.CurrentDirectory);
        }
        protected override void OnDisplayFileViewerItem(FileViewer fileViewer, Button item)
        {
            var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
            var (x, y) = item.Position;
            var icon = fileViewer.IsFolder(item)
                ? new Tile(Tile.ICON_FOLDER, GetColor(item, Color.Yellow))
                : new(Tile.ICON_FILE, GetColor(item, Color.Gray.ToBright()));

            icon = item.Text == ".." ? Tile.ICON_BACK : icon;

            front.SetTile((x, y), icon);
            front.SetTextLine((x + 1, y), item.Text, GetColor(item, color), item.Size.width - 1);
        }
        protected override void OnDisplayPanel(Panel panel)
        {
            var e = panel;
            SetBackground(back, e, 0.75f);
            middle.SetRectangle(e.Position, e.Size, Tile.EMPTY);
            front.SetRectangle(e.Position, e.Size, Tile.EMPTY);

            front.SetBox(e.Position, e.Size, Tile.EMPTY, Tile.BOX_GRID_CORNER,
                Tile.BOX_GRID_STRAIGHT, Color.Blue);
            front.SetTextRectangle((e.Position.x, e.Position.y), (e.Size.width, 1), e.Text,
                alignment: Tilemap.Alignment.Center);
        }
        protected override void OnDisplayPages(Pages pages)
        {
            var e = pages;
            SetBackground(back, e);
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
        protected override void OnDisplayPagesPage(Pages pages, Button page)
        {
            var color = page.IsSelected ? Color.Green : Color.Gray;
            middle.SetTextLine(page.Position, page.Text, GetColor(page, color));

            if (pages.Count == 10)
                middle.SetTile((page.Position.x, page.Position.y + 1),
                    new(Tile.ICON_HOME + int.Parse(page.Text), GetColor(page, color)));
        }
        protected override void OnDisplayPalette(Palette palette)
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
        protected override void OnDisplayPalettePage(Palette palette, Button page)
        {
            OnDisplayPagesPage(palette.Brightness, page); // display the same kind of pages
        }
        protected override void OnUpdatePaletteSample(Palette palette, Button sample,
            uint color)
        {
            middle.SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
        }
        protected override void OnDisplayStepper(Stepper stepper)
        {
            var e = stepper;
            var text = MathF.Round(e.Value, 2).Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";

            SetBackground(back, stepper);

            middle.SetTile(e.Decrease.Position, new(Tile.ARROW, GetColor(e.Decrease, Color.Gray), 1));
            middle.SetTile(e.Increase.Position, new(Tile.ARROW, GetColor(e.Increase, Color.Gray), 3));
            middle.SetTextLine((e.Position.x + 2, e.Position.y), e.Text);
            middle.SetTextLine((e.Position.x + 2, e.Position.y + 1), text);
        }
        protected override void OnDisplayScroll(Scroll scroll)
        {
            var e = scroll;
            var scrollUpAng = (sbyte)(e.IsVertical ? 3 : 0);
            var scrollDownAng = (sbyte)(e.IsVertical ? 1 : 2);
            var scrollColor = Color.Gray.ToBright();
            var isHandle = e.Slider.Handle.IsPressedAndHeld;

            SetBackground(back, e);
            middle.SetTile(e.Increase.Position,
                new(Tile.ARROW, GetColor(e.Increase, scrollColor), scrollUpAng));
            middle.SetTile(e.Slider.Handle.Position,
                new(Tile.SHAPE_CIRCLE, GetColor(isHandle ? e.Slider.Handle : e.Slider, scrollColor)));
            middle.SetTile(e.Decrease.Position,
                new(Tile.ARROW, GetColor(e.Decrease, scrollColor), scrollDownAng));
        }
        protected override void OnDisplayLayout(Layout layout) { }
        protected override void OnDisplayLayoutSegment(Layout layout,
            (int x, int y, int width, int height) segment, int index)
        {
            //var colors = new uint[]
            //{
            //    Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Gray,
            //    Color.Orange, Color.Cyan, Color.Black, Color.Azure, Color.Brown,
            //    Color.Magenta, Color.Purple, Color.Pink, Color.Violet
            //};
            //var pos = (segment.x, segment.y);
            //var size = (segment.width, segment.height);
            //middle.SetRectangle(pos, size, new(Tile.SHADE_OPAQUE, colors[index]));

            var (sx, sy, sw, sh) = segment;
            var (w, h) = (0, 0);

            if (index == 0)
                w = 3;
            else if (index == 1)
            {
                middle.SetTextRectangle((sx, sy), (sw, sh),
                    "I am a text rectangle with some meaningful text inside.");
                return;
            }

            var element = this[index + 11];
            element.Position = (sx, sy);
            element.Size = (sw - w, sh - h);
        }

        // a special kind of action - the palette needs the tile color at a certain position
        // and we happen to have it in the tilemap
        // potentially inheriting Palette and receiving the action there would take priority over this one
        protected override uint OnPalettePick(Palette palette, (float x, float y) position)
        {
            var p = ((int)position.x, (int)position.y);
            var color = front.TileAt(p).Tint;

            if (color == default || color == Color.Black)
                color = middle.TileAt(p).Tint;
            if (color == default || color == Color.Black)
                color = back.TileAt(p).Tint;

            return color;
        }

        // simple "animation" for hovering and pressing buttons
        private static Color GetColor(Element element, Color baseColor)
        {
            if (element.IsDisabled) return baseColor;
            if (element.IsPressedAndHeld) return baseColor.ToDark();
            else if (element.IsHovered) return baseColor.ToBright();

            return baseColor;
        }
        private static void SetBackground(Tilemap map, Element element, float shade = 0.5f)
        {
            var e = element;
            var color = Color.Gray.ToDark(shade);
            var tile = new Tile(Tile.SHADE_OPAQUE, color);
            map.SetBox(e.Position, e.Size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
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
        userInterface.Add(new Stepper((37, 4)) { Range = (-9, 13) });
        userInterface.Add(new List((37, 7), 15, Types.Dropdown) { Size = (8, 6) });
        userInterface.Add(new Scroll((46, 7), 9));
        userInterface.Add(new Palette((34, 23), brightnessLevels: 30));
        userInterface.Add(new Scroll((37, 16), 9, false));
        userInterface.Add(new Pages((29, 1)) { Size = (18, 2) });
        userInterface.Add(new FileViewer((1, 18)) { Size = (16, 8) });
        userInterface.Add(new Panel((18, 19))
        {
            Size = (15, 7),
            IsResizable = false,
            IsMovable = false,
        });
        userInterface.Add(new List((19, 20), 10, Types.Horizontal)
        {
            Size = (13, 5),
            ItemSize = (5, 4),
        });

        // -------

        var panelIndex = userInterface.Count;
        userInterface.Add(new Panel((1, 1))
        {
            Size = (15, 15),
            SizeMinimum = (15, 6),
        });
        var layout = new Layout((0, 0));
        userInterface.Add(layout);

        layout.Cut(index: 0, side: Layout.CutSide.Right, rate: 0.4f);
        layout.Cut(index: 0, side: Layout.CutSide.Bottom, rate: 0.6f);
        layout.Cut(index: 1, side: Layout.CutSide.Top, rate: 0.25f);
        layout.Cut(index: 1, side: Layout.CutSide.Bottom, rate: 0.4f);

        userInterface.Add(new Button((0, 0)));
        userInterface.Add(new Button((37, 18)) { Text = "Checkbox" });
        userInterface.Add(new List((0, 0), 15)
        {
            Size = (8, 9),
            IsSingleSelecting = true,
            ItemGap = (0, 1),
        });
        userInterface.Add(new InputBox((0, 0)));
        userInterface.Add(new Slider((0, 0), 7));

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

            var p = userInterface[panelIndex];
            layout.Size = (p.Size.width - 2, p.Size.height - 2);
            layout.Position = (p.Position.x + 1, p.Position.y + 1);

            Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

            for (var i = 0; i < tilemaps.Count; i++)
                Window.DrawTiles(tilemaps[i].ToBundle());

            Window.Activate(false);
        }
    }
}