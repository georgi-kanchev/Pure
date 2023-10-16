namespace Pure.EditorUserInterface;

internal class EditPanel : Panel
{
    public EditPanel((int x, int y) position) : base(position)
    {
        Text = "Edit";
        IsHidden = true;
        IsResizable = false;
        IsMovable = false;

        var disabled = new EditButton((0, 0)) { Text = "Disabled" };
        var hidden = new EditButton((0, 0)) { Text = "Hidden" };
        var text = new InputBox((0, 0)) { Placeholder = "Text�", [0] = "" };

        var selected = new EditButton((0, 0)) { Text = "Selected" };

        var placeholder = new InputBox((0, 0)) { Placeholder = "Placeholder�", [0] = "Type�" };
        var editable = new EditButton((0, 0)) { Text = "Editable" };

        var pagesCount = new Stepper((0, 0)) { Text = "Count", Range = (1, 99) };
        var currentPage = new Stepper((0, 0)) { Text = "Current" };

        var movable = new EditButton((0, 0)) { Text = "Movable" };
        var resizable = new EditButton((0, 0)) { Text = "Resizable" };
        var restricted = new EditButton((0, 0)) { Text = "Restricted" };

        var brightnessMax = new Stepper((0, 0)) { Text = "Levels", Range = (1, 99) };
        var brightness = new Stepper((0, 0)) { Text = "Brightness" };
        var opacity = new Stepper((0, 0)) { Text = "Opacity", Range = (0, 1), Step = 0.05f };

        var vertical = new EditButton((0, 0)) { Text = "Vertical" };
        var progress = new Stepper((0, 0)) { Text = "Progress", Range = (0, 1), Step = 0.05f };

        var step = new Stepper((0, 0)) { Text = "Step", Range = (0, 1), Step = 0.05f };

        var stepperStep = new Stepper((0, 0)) { Text = "Step", Step = 0.05f };
        var min = new Stepper((0, 0)) { Text = "Minimum", Step = 0.05f };
        var max = new Stepper((0, 0)) { Text = "Maximum", Step = 0.05f };
        var value = new Stepper((0, 0)) { Text = "Value", Step = 0.05f };

        var restore = new EditButton((0, 0)) { Text = "Restore" };
        var index = new Stepper((0, 0)) { Text = "Index" };
        var rate = new Stepper((0, 0)) { Text = "Rate", Range = (0, 1), Step = 0.05f };
        var cutTop = new EditButton((0, 0)) { Text = "Cut Top" };
        var cutLeft = new EditButton((0, 0)) { Text = "Cut Left" };
        var cutRight = new EditButton((0, 0)) { Text = "Cut Right" };
        var cutBottom = new EditButton((0, 0)) { Text = "Cut Bottom" };

        var type = new EditButton((0, 0)) { IsDisabled = true };
        var expanded = new EditButton((0, 0)) { Text = "Expanded" };
        var items = new InputBox((0, 0)) { Placeholder = "Empty�", [0] = "", Size = (0, 7) };
        var multiSelect = new EditButton((0, 0)) { Text = "Multi-Select" };
        var scroll = new Stepper((0, 0)) { Text = "Scroll", Range = (0, 1) };
        var itemWidth = new Stepper((0, 0)) { Text = "Width", Range = (1, int.MaxValue) };
        var itemHeight = new Stepper((0, 0)) { Text = "Height", Range = (1, int.MaxValue) };
        var itemGap = new Stepper((0, 0)) { Text = "Gap", Range = (0, int.MaxValue) };

        for (var i = 0; i < items.Size.height; i++)
            itemSelections.Add(new((0, 0)) { Text = "ItemSelect" });

        checkboxes.AddRange(new[]
        {
            disabled, hidden, selected, movable, resizable, restricted, vertical, multiSelect,
            expanded, editable
        });

        blocks = new()
        {
            { typeof(Block), new() { disabled, hidden, text } },
            { typeof(Button), new() { selected } },
            { typeof(InputBox), new() { editable, placeholder } },
            { typeof(Pages), new() { pagesCount, currentPage } },
            { typeof(Panel), new() { movable, resizable, restricted } },
            { typeof(Palette), new() { brightnessMax, brightness, opacity } },
            { typeof(Slider), new() { vertical, progress } },
            { typeof(Scroll), new() { vertical, progress, step } },
            { typeof(Layout), new() { restore, index, rate, cutTop, cutLeft, cutRight, cutBottom } },
            { typeof(FileViewer), new() { } },
            { typeof(Stepper), new() { min, max, value, stepperStep } },
            {
                typeof(List),
                new()
                {
                    type, expanded, scroll, items, multiSelect, itemWidth, itemHeight, itemGap
                }
            },
        };
    }

    protected void OnDisplay()
    {
        if (IsHovered)
            Input.MouseCursorResult = MouseCursor.Arrow;

        if ((IsHidden == false).Once("on-show"))
            UpdatePanelValues();

        if (Selected != null && IsHidden == false)
        {
            if ((prevSelected != Selected).Once("on-select"))
            {
                UpdateSelected();
                UpdatePanelValues();
            }

            RepositionPanel();
            UpdatePanel();
            UpdatePanelBlocks();
            ReclampPanelValues();
        }

        prevSelected = Selected;
    }

#region Backend
    private class EditButton : Button
    {
        public EditButton((int x, int y) position) : base(position)
        {
        }

        protected void OnUserAction(Interaction userAction)
        {
            if (userAction != Interaction.Trigger || Selected == null)
                return;

            var panel = editUI[ui.IndexOf(Selected)];

            if (Text == "Remove")
            {
                editUI.BlockRemove(Selected);
                editPanel.Position = (int.MaxValue, int.MaxValue);
            }
            else if (Text == "To Top")
            {
                editUI.BlockToTop(Selected);
            }
            else if (Text == "Disabled")
            {
                Selected.IsDisabled = IsSelected;
            }
            else if (Text == "Hidden")
            {
                Selected.IsHidden = IsSelected;
            }
            else if (Text == "Center X")
            {
                panel.Position = (
                    CameraPosition.x + CameraSize.w / 2 - panel.Size.width / 2,
                    panel.Position.y);
            }
            else if (Text == "Center Y")
            {
                panel.Position = (
                    panel.Position.x,
                    CameraPosition.y + CameraSize.h / 2 - panel.Size.height / 2);
            }
            else if (Text == "ItemSelect")
            {
                var isViewer = Selected is FileViewer;
                var items = isViewer ?
                    (InputBox)editPanel.blocks[typeof(FileViewer)][0] :
                    (InputBox)editPanel.blocks[typeof(List)][3];
                var list = isViewer ? ((FileViewer)Selected).FilesAndFolders : (List)Selected;
                var index = editPanel.itemSelections.IndexOf(this);

                //if (Selected is FileViewer { IsSelectingFolders: false } v && index < v.CountFolders)
                //    index += v.CountFolders;

                var item = list[index + items.ScrollIndices.y];

                list.Select(item, IsSelected);
                editPanel.UpdateSelected();
            }
            else if (Text.Contains("Cut") && Selected is Layout l)
            {
                var index = (int)((Stepper)editPanel.blocks[typeof(Layout)][1]).Value;
                var rate = ((Stepper)editPanel.blocks[typeof(Layout)][2]).Value;

                if (Text.Contains("Top")) l.Cut(index, Layout.CutSide.Top, rate);
                else if (Text.Contains("Left")) l.Cut(index, Layout.CutSide.Left, rate);
                else if (Text.Contains("Right")) l.Cut(index, Layout.CutSide.Right, rate);
                else if (Text.Contains("Bottom")) l.Cut(index, Layout.CutSide.Bottom, rate);

                editPanel.UpdatePanelValues();
            }
            else if (Text == "Restore" && Selected is Layout la)
            {
                la.Restore();
            }
        }
    }

    private readonly List<EditButton> checkboxes = new(), itemSelections = new();
    private readonly EditButton
        toTop = new((0, 0)) { Text = "To Top" },
        centerX = new((0, 0)) { Text = "Center X" },
        centerY = new((0, 0)) { Text = "Center Y" },
        remove = new((0, 0)) { Text = "Remove" };
    private readonly Dictionary<Type, List<Block>> blocks;
    private Block? prevSelected;

    private void RepositionPanel()
    {
        if (Selected == null)
            return;

        var x = Selected.Position.x + Selected.Size.width / 2;
        var cx = CameraPosition.x + CameraSize.w / 2;

        Size = (14, CameraSize.h);
        Position = (x > cx ? CameraPosition.x : CameraPosition.x + CameraSize.w - Size.width,
            CameraPosition.y);
    }
    private void UpdatePanel()
    {
        var offset = (Size.width - Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (Position.x + offset, Position.y);
        const int CORNER = Tile.BOX_HOLLOW_CORNER;
        const int STRAIGHT = Tile.BOX_HOLLOW_STRAIGHT;
        var front = tilemaps[(int)Layer.EditFront];
        var color = Color.Gray.ToDark(0.66f);
        var (bottomX, bottomY) = (Position.x + 1, Position.y + Size.height - 2);
        var (topX, topY) = (Position.x + 1, Position.y + 1);

        SetClear(Layer.EditBack, this);
        SetClear(Layer.EditMiddle, this);
        SetClear(Layer.EditFront, this);

        SetBackground(tilemaps[(int)Layer.EditBack], this, color);

        front.SetBox(Position, Size, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Yellow);
        front.SetTextLine(textPos, Text, Color.Yellow);

        UpdateButton(toTop, (topX, topY));
        UpdateButton(centerX, (topX, topY + 1));
        UpdateButton(centerY, (topX, topY + 2));
        UpdateButton(remove, (bottomX, bottomY));
    }
    private void UpdatePanelBlocks()
    {
        var (x, y) = (Position.x + 1, Position.y + 4);
        foreach (var kvp in blocks)
        {
            var type = Selected?.GetType();

            if (type == null || type != kvp.Key && type.IsSubclassOf(kvp.Key) == false)
                continue;

            y++;
            foreach (var block in kvp.Value)
            {
                if (block is Button b)
                {
                    var prev = b.IsSelected;
                    UpdateButton(b, (x, y));

                    if (prev != b.IsSelected)
                        UpdateSelected();
                }
                else if (block is InputBox i)
                {
                    y += 2;

                    var prev = i.Value;
                    UpdateInputBox(i, (x, y));
                    if (prev != i.Value)
                        UpdateSelected();

                    y += i.Size.height - 1;
                }
                else if (block is Stepper s)
                {
                    var prev = s.Value;
                    UpdateStepper(s, (x, y));

                    if (Math.Abs(prev - s.Value) > 0.01f)
                        UpdateSelected();

                    y++;
                }

                y++;
            }
        }
    }

    private void UpdateSelected()
    {
        if (prevSelected == null)
            return;

        var panel = editUI[ui.IndexOf(Selected)];
        var disabled = (EditButton)blocks[typeof(Block)][0];
        var hidden = (EditButton)blocks[typeof(Block)][1];
        var text = (InputBox)blocks[typeof(Block)][2];

        prevSelected.IsDisabled = disabled.IsSelected;
        prevSelected.IsHidden = hidden.IsSelected;

        if (prevSelected is InputBox i)
        {
            var editable = (Button)blocks[typeof(InputBox)][0];
            var placeholder = (InputBox)blocks[typeof(InputBox)][1];
            i.IsEditable = editable.IsSelected;
            i.Value = text.Value;
            i.Placeholder = placeholder.Value;
        }
        else if (prevSelected is Button b)
        {
            b.IsSelected = ((EditButton)blocks[typeof(Button)][0]).IsSelected;
        }
        else if (prevSelected is Pages p)
        {
            var count = (Stepper)blocks[typeof(Pages)][0];
            var current = (Stepper)blocks[typeof(Pages)][1];
            p.Count = (int)count.Value;
            p.Current = (int)current.Value;
        }
        else if (prevSelected is Panel pa)
        {
            var movable = (Button)blocks[typeof(Panel)][0];
            var resizable = (Button)blocks[typeof(Panel)][1];
            var restricted = (Button)blocks[typeof(Panel)][2];
            pa.IsMovable = movable.IsSelected;
            pa.IsResizable = resizable.IsSelected;
            pa.IsRestricted = restricted.IsSelected;
        }
        else if (prevSelected is Palette pl)
        {
            var brightnessMax = (Stepper)blocks[typeof(Palette)][0];
            var brightness = (Stepper)blocks[typeof(Palette)][1];
            var opacity = (Stepper)blocks[typeof(Palette)][2];
            pl.Brightness.Count = (int)brightnessMax.Value;
            pl.Brightness.Current = (int)brightness.Value;
            pl.Opacity.Progress = opacity.Value;
        }
        else if (prevSelected is Slider s)
        {
            var vertical = (EditButton)blocks[typeof(Slider)][0];
            var progress = (Stepper)blocks[typeof(Slider)][1];
            s.Progress = progress.Value;
            s.IsVertical = vertical.IsSelected;
            panel.SizeMinimum = vertical.IsSelected ? (3, 4) : (4, 3);
        }
        else if (prevSelected is Scroll sc)
        {
            var vertical = (EditButton)blocks[typeof(Scroll)][0];
            var progress = (Stepper)blocks[typeof(Scroll)][1];
            var step = (Stepper)blocks[typeof(Scroll)][2];
            sc.Step = step.Value;
            sc.Slider.Progress = progress.Value;
            sc.IsVertical = vertical.IsSelected;
            panel.SizeMinimum = vertical.IsSelected ? (3, 4) : (4, 3);
        }
        else if (prevSelected is Stepper st)
        {
            var min = (Stepper)blocks[typeof(Stepper)][0];
            var max = (Stepper)blocks[typeof(Stepper)][1];
            var value = (Stepper)blocks[typeof(Stepper)][2];
            var step = (Stepper)blocks[typeof(Stepper)][3];
            st.Range = (min.Value, max.Value);
            st.Value = value.Value;
            st.Step = step.Value;
        }
        else if (prevSelected is List l)
        {
            var expanded = (EditButton)blocks[typeof(List)][1];
            var scroll = (Stepper)blocks[typeof(List)][2];
            var items = (InputBox)blocks[typeof(List)][3];
            var multi = (EditButton)blocks[typeof(List)][4];
            var itemWidth = (Stepper)blocks[typeof(List)][5];
            var itemHeight = (Stepper)blocks[typeof(List)][6];
            var itemGap = (Stepper)blocks[typeof(List)][7];

            l.IsCollapsed = expanded.IsSelected;
            l.Scroll.Slider.Progress = scroll.Value;
            l.ItemSize = ((int)itemWidth.Value, (int)itemHeight.Value);
            l.ItemGap = (int)itemGap.Value;
            l.IsSingleSelecting = multi.IsSelected == false;

            var prev = new List<bool>();
            for (var j = 0; j < l.Count; j++)
                prev.Add(l[j].IsSelected);

            l.Clear();
            var split = items.Value.Split(Environment.NewLine);
            l.Add(split.Length);
            for (var index = 0; index < split.Length; index++)
                l[index].Text = split[index];

            for (var j = 0; j < l.Count; j++)
            {
                if (j >= prev.Count)
                    break;

                l.Select(l[j], prev[j]);
            }
        }
        else if (prevSelected is FileViewer v)
        {
        }

        prevSelected.Text = text.Value;
    }
    private void UpdatePanelValues()
    {
        if (Selected == null)
            return;

        var disabled = (EditButton)blocks[typeof(Block)][0];
        var hidden = (EditButton)blocks[typeof(Block)][1];
        var text = (InputBox)blocks[typeof(Block)][2];

        disabled.IsSelected = Selected.IsDisabled;
        hidden.IsSelected = Selected.IsHidden;

        var valueText = Selected.Text;
        if (Selected is InputBox e)
            valueText = e.Value;

        text.Placeholder = "Text�";
        text.Value = valueText;
        text.CursorIndices = (0, 0);
        text.SelectionIndices = (0, 0);
        text.ScrollIndices = (0, 0);

        if (Selected is Button b)
            ((Button)blocks[typeof(Button)][0]).IsSelected = b.IsSelected;
        else if (Selected is InputBox i)
        {
            text.Placeholder = "Value�";

            var editable = (Button)blocks[typeof(InputBox)][0];
            var placeholder = (InputBox)blocks[typeof(InputBox)][1];
            editable.IsSelected = i.IsEditable;
            placeholder.Value = i.Placeholder;
            placeholder.CursorIndices = (0, 0);
            placeholder.SelectionIndices = (0, 0);
            placeholder.ScrollIndices = (0, 0);
        }
        else if (Selected is Pages p)
        {
            var count = (Stepper)blocks[typeof(Pages)][0];
            var current = (Stepper)blocks[typeof(Pages)][1];
            count.Value = p.Count;
            current.Range = (1, count.Value);
            current.Value = p.Current;
        }
        else if (Selected is Panel pa)
        {
            var movable = (Button)blocks[typeof(Panel)][0];
            var resizable = (Button)blocks[typeof(Panel)][1];
            var restricted = (Button)blocks[typeof(Panel)][2];
            movable.IsSelected = pa.IsMovable;
            resizable.IsSelected = pa.IsResizable;
            restricted.IsSelected = pa.IsRestricted;
        }
        else if (Selected is Palette pl)
        {
            var brightnessMax = (Stepper)blocks[typeof(Palette)][0];
            var brightness = (Stepper)blocks[typeof(Palette)][1];
            var opacity = (Stepper)blocks[typeof(Palette)][2];
            brightnessMax.Value = pl.Brightness.Count;
            brightness.Range = (1, brightnessMax.Value);
            brightness.Value = pl.Brightness.Current;
            opacity.Value = pl.Opacity.Progress;
        }
        else if (Selected is Slider s)
        {
            var vertical = (Button)blocks[typeof(Slider)][0];
            var progress = (Stepper)blocks[typeof(Slider)][1];
            vertical.IsSelected = s.IsVertical;
            progress.Value = s.Progress;
        }
        else if (Selected is Scroll sc)
        {
            var vertical = (Button)blocks[typeof(Scroll)][0];
            var progress = (Stepper)blocks[typeof(Scroll)][1];
            var step = (Stepper)blocks[typeof(Scroll)][2];
            vertical.IsSelected = sc.IsVertical;
            progress.Value = sc.Slider.Progress;
            step.Value = sc.Step;
        }
        else if (Selected is Stepper st)
        {
            var min = (Stepper)blocks[typeof(Stepper)][0];
            var max = (Stepper)blocks[typeof(Stepper)][1];
            var value = (Stepper)blocks[typeof(Stepper)][2];
            var step = (Stepper)blocks[typeof(Stepper)][3];
            min.Value = st.Range.minimum;
            max.Value = st.Range.maximum;
            value.Value = st.Value;
            value.Range = (min.Value, max.Value);
            step.Value = st.Step;

            min.Step = step.Value;
            max.Step = step.Value;
            value.Step = step.Value;
        }
        else if (Selected is Layout la)
        {
            var index = (Stepper)blocks[typeof(Layout)][1];
            index.Range = (0, la.Count - 1);
        }
        else if (Selected is FileViewer v)
        {
        }
        else if (Selected is List l)
        {
            var type = blocks[typeof(List)][0];
            var expanded = (EditButton)blocks[typeof(List)][1];
            var scroll = (Stepper)blocks[typeof(List)][2];
            var items = (InputBox)blocks[typeof(List)][3];
            var multi = (EditButton)blocks[typeof(List)][4];
            var itemWidth = (Stepper)blocks[typeof(List)][5];
            var itemHeight = (Stepper)blocks[typeof(List)][6];
            var itemGap = (Stepper)blocks[typeof(List)][7];

            type.Text = $"{l.Span}";
            expanded.IsSelected = l.IsCollapsed;
            expanded.IsDisabled = l.Span != List.Spans.Dropdown;
            scroll.Value = l.Scroll.Slider.Progress;
            multi.IsSelected = l.IsSingleSelecting == false;
            multi.IsDisabled = l.Span == List.Spans.Dropdown;
            itemWidth.Value = l.ItemSize.width;
            itemHeight.Value = l.ItemSize.height;
            itemGap.Value = l.ItemGap;

            var value = "";
            for (var j = 0; j < l.Count; j++)
                value += $"{(j > 0 ? Environment.NewLine : "")}{l[j].Text}";

            items.IsEditable = true;
            items.Value = value;
            items.SelectionIndices = (0, 0);
            items.CursorIndices = (0, 0);
            items.CursorScroll();
        }
    }

    private void UpdateButton(Button btn, (int x, int y) position)
    {
        var front = tilemaps[(int)Layer.EditFront];

        btn.Position = position;
        btn.Size = (Size.width - 2, 1);

        btn.Update();

        var color = checkboxes.Contains(btn) && btn.IsSelected ? Color.Green.ToBright() : Color.Yellow;
        color = btn.IsDisabled ? Color.White : color;

        front.SetTextRectangle(btn.Position, btn.Size, btn.Text, GetColor(btn, color.ToDark()),
            alignment: Tilemap.Alignment.Center);
    }
    private void UpdateInputBox(InputBox inputBox, (int x, int y) position)
    {
        var e = inputBox;
        var back = tilemaps[(int)Layer.EditBack];
        var middle = tilemaps[(int)Layer.EditMiddle];
        var front = tilemaps[(int)Layer.EditFront];
        var color = Color.Gray;
        var isListItems = e.Placeholder.Contains("Empty");

        position = isListItems ? (position.x + 1, position.y) : position;

        e.Position = position;
        e.Size = (Size.width - 2 - (isListItems ? 1 : 0), e.Size.height);

        e.Update();

        back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, color.ToDark()));
        SetClear(Layer.EditMiddle, e);

        back.SetTextRectangle(e.Position, e.Size, e.Selection,
            e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
        middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);
        middle.SetTextLine((Position.x + 1, e.Position.y - 1), e.Placeholder, color);

        if (string.IsNullOrWhiteSpace(e.Text))
            middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, color.ToBright(), false);

        if (e.IsCursorVisible)
            front.SetTile(e.PositionFromIndices(e.CursorIndices), new(Tile.SHAPE_LINE, Color.White, 2));

        if (isListItems == false || Selected is not List list)
            return;

        UpdateListItems(list, inputBox);
    }
    private void UpdateStepper(Stepper stepper, (int x, int y) position)
    {
        var e = stepper;
        var color = Color.Gray;
        var middle = tilemaps[(int)Layer.EditMiddle];
        var front = tilemaps[(int)Layer.EditFront];
        var value = e.Step.Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";
        e.Position = position;
        e.Size = (Size.width - 2, 2);

        var prev = e.Value;
        e.Update();
        if (prev != e.Value)
            UpdateSelected();

        SetBackground(middle, stepper, color.ToDark());
        SetBackground(front, stepper, color.ToDark());

        front.SetTile(e.Increase.Position, new(Tile.ARROW_NO_TAIL, GetColor(e.Increase, color), 3));
        front.SetTile(e.Decrease.Position, new(Tile.ARROW_NO_TAIL, GetColor(e.Decrease, color), 1));
        front.SetTextLine((e.Position.x + 2, e.Position.y), e.Text, color);
        front.SetTextLine((e.Position.x + 2, e.Position.y + 1), value);

        front.SetTile(e.Minimum.Position, new(Tile.MATH_MUCH_LESS, GetColor(e.Minimum, color)));
        front.SetTile(e.Middle.Position, new(Tile.PUNCTUATION_PIPE, GetColor(e.Middle, color)));
        front.SetTile(e.Maximum.Position, new(Tile.MATH_MUCH_GREATER, GetColor(e.Maximum, color)));
    }
    private void UpdateListItems(List list, InputBox inputBoxEditor)
    {
        var middle = tilemaps[(int)Layer.EditMiddle];
        var length = Math.Min(list.Count, itemSelections.Count);

        for (var i = 0; i < length; i++)
        {
            var index = i + inputBoxEditor.ScrollIndices.y;
            var item = list[index];
            var btn = itemSelections[i];
            var c = GetColor(btn, (item.IsSelected ? Color.Green : Color.Red).ToDark());
            var tile = new Tile(item.IsSelected ? Tile.ICON_TICK : Tile.LOWERCASE_X, c);

            btn.IsDisabled = false;
            btn.Size = (1, 1);
            btn.Position = (inputBoxEditor.Position.x - 1, inputBoxEditor.Position.y + i);
            btn.IsSelected = item.IsSelected;

            // double clicking a button will enter a different folder so this
            // loop is iterating over an old data/length - so abandon
            var prevSize = list.Count;
            btn.Update();

            if (prevSize != list.Count)
                return;

            middle.SetTile(btn.Position, tile);
        }
    }

    private void ReclampPanelValues()
    {
        var count = (Stepper)blocks[typeof(Pages)][0];
        var current = (Stepper)blocks[typeof(Pages)][1];
        var brightnessMax = (Stepper)blocks[typeof(Palette)][0];
        var brightness = (Stepper)blocks[typeof(Palette)][1];

        var min = (Stepper)blocks[typeof(Stepper)][0];
        var max = (Stepper)blocks[typeof(Stepper)][1];
        var value = (Stepper)blocks[typeof(Stepper)][2];
        var step = (Stepper)blocks[typeof(Stepper)][3];

        current.Range = (1, count.Value);
        brightness.Range = (1, brightnessMax.Value);
        value.Range = (min.Value, max.Value);

        min.Step = step.Value;
        max.Step = step.Value;
        value.Step = step.Value;

        if (Selected is not List list)
            return;

        var scroll = (Stepper)blocks[typeof(List)][2];
        scroll.Step = list.Scroll.Step;
    }

    private static Color GetColor(Block block, Color baseColor)
    {
        if (block.IsDisabled) return baseColor;
        if (block.IsPressedAndHeld) return baseColor.ToDark(0.3f);
        else if (block.IsHovered) return baseColor.ToBright(0.3f);

        return baseColor;
    }
    private static void SetBackground(Tilemap map, Block block, Color color)
    {
        var tile = new Tile(Tile.SHADE_OPAQUE, color);
        var pos = block.Position;
        var size = block.Size;

        map.SetBox(pos, size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
    private static void SetClear(Layer layer, Block block)
    {
        tilemaps[(int)layer].SetBox(block.Position, block.Size, 0, 0, 0, 0);
    }
#endregion
}