namespace Pure.Examples.Systems;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using Window;
using static Pure.UserInterface.List;

public static class UserInterfacee
{
    private class UI : Pure.UserInterface.UserInterface
    {
        private readonly TilemapManager maps;
        private int buttonClickCount;

        public UI(TilemapManager tilemaps, Prompt prompt)
        {
            Prompt = prompt;
            maps = tilemaps;
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

            var promptBtnIndex = Prompt?.IndexOf(e);
            if (promptBtnIndex != -1)
            {
                var tile = new Tile(Tile.ICON_TICK, GetColor(e, Color.Green));
                if (promptBtnIndex == 1)
                {
                    tile.Id = Tile.ICON_CANCEL;
                    tile.Tint = GetColor(e, Color.Red);
                }

                Clear(maps[0], e);
                Clear(maps[1], e);
                maps[2].SetTile(e.Position, tile);
                Clear(maps[3], e);
                return;
            }

            if (e.Text == "Button")
            {
                var offW = w / 2 - Math.Min(e.Text.Length, w - 2) / 2;
                maps[1].SetBox(e.Position, e.Size, Tile.EMPTY,
                    Tile.BOX_DEFAULT_CORNER, Tile.BOX_DEFAULT_STRAIGHT,
                    GetColor(e, Color.Yellow).ToDark());
                maps[1].SetTextLine(
                    (e.Position.x + offW, e.Position.y + h / 2),
                    e.Text,
                    GetColor(e, Color.Yellow.ToDark()),
                    e.Size.width - 2);
                maps[1].SetTextLine((e.Position.x + 1 + e.Size.width, e.Position.y + 1),
                    $"{buttonClickCount}", Color.White);
            }
            else if (e.Text == "Checkbox")
            {
                e.Size = (e.Text.Length + 2, 1);

                var color = e.IsSelected ? Color.Orange : Color.Red;
                var tile = new Tile(e.IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X,
                    GetColor(e, color));
                SetBackground(maps[0], e);
                maps[1].SetTile(e.Position, tile);
                maps[1].SetTextLine((e.Position.x + 2, e.Position.y), e.Text, GetColor(button, color));
            }
        }
        protected override void OnDisplayInputBox(InputBox inputBox)
        {
            var e = inputBox;
            var isPrompt = Prompt?.IsOwning(e) ?? false;

            Clear(maps[1], e);
            Clear(maps[2], e);

            var bgColor = Color.Gray.ToDark();
            if (isPrompt)
            {
                bgColor = Color.Gray;
                Clear(maps[3], e);
            }

            maps[0].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, bgColor));
            maps[0].SetTextRectangle(e.Position, e.Size, e.Selection,
                e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
            maps[1].SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

            if (string.IsNullOrWhiteSpace(e.Value))
                maps[1].SetTextRectangle(e.Position, e.Size, e.Placeholder,
                    tint: isPrompt ? Color.White : Color.Gray.ToBright(),
                    isWordWrapping: isPrompt,
                    alignment: isPrompt ? Tilemap.Alignment.Center : Tilemap.Alignment.TopLeft);

            if (e.IsCursorVisible)
                maps[2].SetTile(e.PositionFromIndices(e.CursorIndices),
                    new(Tile.SHAPE_LINE, Color.White, 2));
        }
        protected override void OnDisplaySlider(Slider slider)
        {
            var e = slider;
            var (w, h) = e.Size;
            var text = $"{e.Progress:F2}";
            var isHandle = e.Handle.IsPressedAndHeld;

            SetBackground(maps[1], e);
            maps[2].SetBar(e.Handle.Position, Tile.BAR_DEFAULT_EDGE, Tile.BAR_DEFAULT_STRAIGHT,
                GetColor(isHandle ? e.Handle : e, Color.Magenta), e.Size.height, true);
            maps[2].SetTextLine((e.Position.x + w / 2 - text.Length / 2, e.Position.y + h / 2), text);
        }
        protected override void OnDisplayList(List list)
        {
            Clear(maps[1], list);
            Clear(maps[2], list);

            maps[0].SetRectangle(list.Position, list.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));

            if (list.Scroll.IsHidden == false)
                OnDisplayScroll(list.Scroll);

            var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(list, Color.Gray.ToBright()), 1);
            if (list.IsCollapsed)
                maps[2].SetTile((list.Position.x + list.Size.width - 1, list.Position.y), dropdownTile);
        }
        protected override void OnDisplayListItem(List list, Button item)
        {
            var color = item.IsSelected ? Color.Green : Color.Gray.ToBright(0.3f);
            var (x, y) = item.Position;
            var (_, h) = item.Size;

            SetBackground(maps[1], item, 0.25f);
            maps[2].SetTextLine((x, y + h / 2), item.Text, GetColor(item, color), item.Size.width);
        }
        protected override void OnDisplayFileViewer(FileViewer fileViewer)
        {
            var e = fileViewer;
            maps[0].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));

            if (e.FilesAndFolders.Scroll.IsHidden == false)
                OnDisplayScroll(e.FilesAndFolders.Scroll);

            var color = GetColor(e.Back, Color.Gray);
            var (x, y) = e.Back.Position;
            maps[2].SetTile((x, y), new(Tile.ICON_BACK, color));
            maps[2].SetTextLine((x + 1, y), e.CurrentDirectory, color, -e.Back.Size.width + 2);
        }
        protected override void OnDisplayFileViewerItem(FileViewer fileViewer, Button item)
        {
            var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
            var (x, y) = item.Position;
            var icon = fileViewer.IsFolder(item)
                ? new Tile(Tile.ICON_FOLDER, GetColor(item, Color.Yellow))
                : new(Tile.ICON_FILE, GetColor(item, Color.Gray.ToBright()));

            icon = item.Text == ".." ? Tile.ICON_BACK : icon;

            maps[2].SetTile((x, y), icon);
            maps[2].SetTextLine((x + 1, y), item.Text, GetColor(item, color), item.Size.width - 1);
        }
        protected override void OnDisplayPanel(Panel panel)
        {
            var e = panel;

            if (Prompt != null && Prompt.IsOwning(panel))
            {
                var color = new Color(0, 0, 0, 200);
                maps[3].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, color));
                maps[3].SetRectangle(Prompt.Position, Prompt.Size, Tile.SHADE_TRANSPARENT);
                return;
            }

            SetBackground(maps[0], e, 0.75f);

            maps[1].SetRectangle(e.Position, e.Size, Tile.EMPTY);
            maps[2].SetRectangle(e.Position, e.Size, Tile.EMPTY);

            maps[2].SetBox(e.Position, e.Size, Tile.EMPTY, Tile.BOX_GRID_CORNER,
                Tile.BOX_GRID_STRAIGHT, Color.Blue);
            maps[2].SetTextRectangle((e.Position.x, e.Position.y), (e.Size.width, 1), e.Text,
                alignment: Tilemap.Alignment.Center);
        }
        protected override void OnDisplayPages(Pages pages)
        {
            var e = pages;
            SetBackground(maps[0], e);
            var colorFirst = GetColor(e.First, Color.Gray);
            var colorPrevious = GetColor(e.Previous, Color.Gray);
            var colorNext = GetColor(e.Next, Color.Gray);
            var colorLast = GetColor(e.Last, Color.Gray);
            maps[1].SetTile(e.First.Position, new(Tile.MATH_MUCH_LESS, colorFirst));
            maps[1].SetTile((e.First.Position.x, e.First.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorFirst));
            maps[1].SetTile(e.Previous.Position, new(Tile.MATH_LESS, colorPrevious));
            maps[1].SetTile((e.Previous.Position.x, e.Previous.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorPrevious));
            maps[1].SetTile(e.Next.Position, new(Tile.MATH_GREATER, colorNext));
            maps[1].SetTile((e.Next.Position.x, e.Next.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorNext));
            maps[1].SetTile(e.Last.Position, new(Tile.MATH_MUCH_GREATER, colorLast));
            maps[1].SetTile((e.Last.Position.x, e.Last.Position.y + 1),
                new(Tile.PUNCTUATION_PIPE, colorLast));
        }
        protected override void OnDisplayPagesPage(Pages pages, Button page)
        {
            var color = page.IsSelected ? Color.Green : Color.Gray;
            maps[1].SetTextLine(page.Position, page.Text, GetColor(page, color));
            maps[1].SetTile((page.Position.x, page.Position.y + 1),
                new(Tile.ICON_HOME + int.Parse(page.Text), GetColor(page, color)));
        }
        protected override void OnDisplayPalette(Palette palette)
        {
            var e = palette;
            var (x, y) = e.Position;
            var (w, h) = e.Size;
            var tile = new Tile(Tile.SHADE_OPAQUE, GetColor(e.Opacity, Color.Gray.ToBright()));
            var alpha = e.Opacity;

            Clear(maps[0], e);
            Clear(maps[2], e);

            maps[0].SetRectangle((x, y), (w, h), new(Tile.SHADE_5, Color.Gray.ToDark()));
            maps[1].SetBar(alpha.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, e.SelectedColor,
                alpha.Size.width);
            maps[1].SetTile(alpha.Handle.Position, tile);

            var first = e.Brightness.First;
            var previous = e.Brightness.Previous;
            var next = e.Brightness.Next;
            var last = e.Brightness.Last;

            maps[1].SetTile(first.Position, new(Tile.MATH_MUCH_LESS, GetColor(first, Color.Gray)));
            maps[1].SetTile(previous.Position, new(Tile.MATH_LESS, GetColor(previous, Color.Gray)));
            maps[1].SetTile(next.Position, new(Tile.MATH_GREATER, GetColor(next, Color.Gray)));
            maps[1].SetTile(last.Position, new(Tile.MATH_MUCH_GREATER, GetColor(last, Color.Gray)));

            maps[1].SetTile(e.Pick.Position, new(Tile.MATH_PLUS, GetColor(e.Pick, Color.Gray)));

            //maps[1].SetRectangle((x, y - 3), (e.Size.width, 3), new(Tile.SHADE_OPAQUE, e.SelectedColor));
        }
        protected override void OnDisplayPalettePage(Palette palette, Button page)
        {
            var color = page.IsSelected ? Color.Green : Color.Gray;
            maps[1].SetTextLine(page.Position, page.Text, GetColor(page, color));
        }
        protected override void OnUpdatePaletteSample(Palette palette, Button sample,
            uint color)
        {
            maps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
        }
        protected override void OnDisplayStepper(Stepper stepper)
        {
            var e = stepper;
            var text = MathF.Round(e.Step, 2).Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";

            SetBackground(maps[0], stepper);

            maps[1].SetTile(e.Decrease.Position, new(Tile.ARROW, GetColor(e.Decrease, Color.Gray), 1));
            maps[1].SetTile(e.Increase.Position, new(Tile.ARROW, GetColor(e.Increase, Color.Gray), 3));
            maps[1].SetTextLine((e.Position.x + 2, e.Position.y), e.Text);
            maps[1].SetTextLine((e.Position.x + 2, e.Position.y + 1), text);

            maps[1].SetTile(e.Minimum.Position,
                new(Tile.MATH_MUCH_LESS, GetColor(e.Minimum, Color.Gray)));
            maps[1].SetTile(e.Middle.Position,
                new(Tile.PUNCTUATION_PIPE, GetColor(e.Middle, Color.Gray)));
            maps[1].SetTile(e.Maximum.Position,
                new(Tile.MATH_MUCH_GREATER, GetColor(e.Maximum, Color.Gray)));
        }
        protected override void OnDisplayScroll(Scroll scroll)
        {
            var e = scroll;
            var scrollUpAng = (sbyte)(e.IsVertical ? 3 : 0);
            var scrollDownAng = (sbyte)(e.IsVertical ? 1 : 2);
            var scrollColor = Color.Gray.ToBright();
            var isHandle = e.Slider.Handle.IsPressedAndHeld;

            SetBackground(maps[0], e);
            maps[1].SetTile(e.Increase.Position,
                new(Tile.ARROW, GetColor(e.Increase, scrollColor), scrollUpAng));
            maps[1].SetTile(e.Slider.Handle.Position,
                new(Tile.SHAPE_CIRCLE, GetColor(isHandle ? e.Slider.Handle : e.Slider, scrollColor)));
            maps[1].SetTile(e.Decrease.Position,
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
            //maps[1].SetRectangle(pos, size, new(Tile.SHADE_OPAQUE, colors[index]));

            var (sx, sy, sw, sh) = segment;
            var (w, h) = (0, 0);

            if (index == 0)
                w = 3;
            else if (index == 1)
            {
                maps[1].SetTextRectangle((sx, sy), (sw, sh),
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
            var color = maps[2].TileAt(p).Tint;

            if (color == default || color == Color.Black)
                color = maps[1].TileAt(p).Tint;
            if (color == default || color == Color.Black)
                color = maps[0].TileAt(p).Tint;

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
        private static void Clear(Tilemap map, Element element)
        {
            map.SetRectangle(element.Position, element.Size, Tile.SHADE_TRANSPARENT);
        }
    }

    public static void Run()
    {
        Window.Create(3);

        var (width, height) = Window.MonitorAspectRatio;
        var tilemaps = new TilemapManager(4, (width * 3, height * 3));
        var promptInputBox = new InputBox { Size = (16, 1) };
        var prompt = new Prompt();
        var ui = new UI(tilemaps, prompt);

        ui.Add(new Stepper((37, 4)) { Range = (-9, 13) });
        ui.Add(new List((37, 7), 15, Spans.Dropdown) { Size = (8, 6) });
        ui.Add(new Scroll((46, 7), 9));
        ui.Add(new Palette((34, 23), brightnessLevels: 30));
        ui.Add(new Scroll((37, 16), 9, false));
        ui.Add(new Pages((29, 1)) { Size = (18, 2) });
        ui.Add(new FileViewer((1, 18)) { Size = (16, 8) });
        ui.Add(new Panel((18, 19))
        {
            Size = (15, 7),
            IsResizable = false,
            IsMovable = false,
        });
        ui.Add(new List((19, 20), 10, Spans.Horizontal)
        {
            Size = (13, 5),
            ItemSize = (5, 4),
        });

        // -------

        var panelIndex = ui.Count;
        ui.Add(new Panel((1, 1))
        {
            Size = (15, 15),
            SizeMinimum = (15, 6),
        });
        var layout = new Layout((0, 0));
        ui.Add(layout);

        layout.Cut(index: 0, side: Layout.CutSide.Right, rate: 0.4f);
        layout.Cut(index: 0, side: Layout.CutSide.Bottom, rate: 0.6f);
        layout.Cut(index: 1, side: Layout.CutSide.Top, rate: 0.25f);
        layout.Cut(index: 1, side: Layout.CutSide.Bottom, rate: 0.4f);

        ui.Add(new Button((0, 0)));
        ui.Add(new Button((37, 18)) { Text = "Checkbox" });
        ui.Add(new List((0, 0), 6)
        {
            Size = (8, 9),
            IsSingleSelecting = true,
            ItemGap = (0, 1),
        });
        ui.Add(new InputBox((0, 0)));
        ui.Add(new Slider((0, 0), 7));

        while (Window.IsOpen)
        {
            Window.Activate(true);

            Time.Update();
            tilemaps.Fill();

            Element.ApplyInput(
                isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
                position: tilemaps.PointFrom(Mouse.CursorPosition, Window.Size),
                scrollDelta: Mouse.ScrollDelta,
                keysPressed: Keyboard.KeyIDsPressed,
                keysTyped: Keyboard.KeyTyped,
                tilemapSize: tilemaps.Size);

            ui.Update();

            var p = ui[panelIndex];
            layout.Size = (p.Size.width - 2, p.Size.height - 2);
            layout.Position = (p.Position.x + 1, p.Position.y + 1);

            Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

            for (var i = 0; i < tilemaps.Count; i++)
                Window.DrawTiles(tilemaps[i].ToBundle());

            Window.Activate(false);
        }
    }
}