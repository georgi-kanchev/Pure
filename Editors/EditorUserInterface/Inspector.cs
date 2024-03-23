namespace Pure.Editors.EditorUserInterface;

internal class Inspector : Panel
{
    public Inspector((int x, int y) position) : base(position)
    {
        Text = "Edit";
        IsHidden = true;
        IsResizable = false;
        IsMovable = false;

        var disabled = new EditButton((0, 0)) { Text = "Disabled" };
        var hidden = new EditButton((0, 0)) { Text = "Hidden" };
        var text = new InputBox((0, 0)) { Placeholder = "Text…", [0] = "" };

        var selected = new EditButton((0, 0)) { Text = "Selected" };

        var placeholder = new InputBox((0, 0)) { Placeholder = "Placeholder…", [0] = "Type…" };
        var editable = new EditButton((0, 0)) { Text = "Editable" };

        var count = new Stepper((0, 0)) { Text = "Count", Range = (1, 9999) };
        var current = new Stepper((0, 0)) { Text = "Current" };
        var pageWidth = new Stepper((0, 0)) { Text = "Width", Range = (1, 9999) };
        var pageGap = new Stepper((0, 0)) { Text = "Gap", Range = (1, 9999) };

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
        var items = new InputBox((0, 0)) { Placeholder = "Items�", [0] = "", Size = (0, 7) };
        var multiSelect = new EditButton((0, 0)) { Text = "Multi-Select" };
        var scroll = new Stepper((0, 0)) { Text = "Scroll", Range = (0, 1) };
        var width = new Stepper((0, 0)) { Text = "Width", Range = (1, int.MaxValue) };
        var height = new Stepper((0, 0)) { Text = "Height", Range = (1, int.MaxValue) };
        var gap = new Stepper((0, 0)) { Text = "Gap", Range = (0, int.MaxValue) };

        var fileSelect = new EditButton((0, 0)) { Text = "File-Select" };

        for (var i = 0; i < items.Size.height; i++)
            itemSelections.Add(new((0, 0)) { Text = "ItemSelect" });

        checkboxes.AddRange(new[]
        {
            disabled, hidden, selected, movable, resizable, restricted, vertical, multiSelect,
            expanded, editable, fileSelect
        });

        blocks = new()
        {
            { typeof(Block), new() { disabled, hidden, text } },
            { typeof(Button), new() { selected } },
            { typeof(InputBox), new() { editable, placeholder } },
            { typeof(Pages), new() { count, current, pageWidth, pageGap } },
            { typeof(Panel), new() { movable, resizable, restricted } },
            { typeof(Palette), new() { brightnessMax, brightness, opacity } },
            { typeof(Slider), new() { vertical, progress } },
            { typeof(Scroll), new() { vertical, progress, step } },
            { typeof(Layout), new() { restore, index, rate, cutTop, cutLeft, cutRight, cutBottom } },
            { typeof(FileViewer), new() { fileSelect } },
            { typeof(Stepper), new() { min, max, value, stepperStep } },
            { typeof(List), new() { type, expanded, scroll, items, multiSelect, width, height, gap } },
        };

        OnDisplay(() =>
        {
            if (IsHovered)
                Input.CursorResult = MouseCursor.Arrow;

            if ((IsHidden == false).Once("on-show"))
                UpdatePanelValues();

            if (Program.selected != null && IsHidden == false)
            {
                if ((prevSelected != Program.selected).Once("on-select"))
                {
                    UpdateSelected();
                    UpdatePanelValues();
                }

                UpdatePanel();
                UpdatePanelBlocks();
                ReclampPanelValues();
            }

            prevSelected = Program.selected;
        });
    }

#region Backend
    private class EditButton : Button
    {
        public EditButton((int x, int y) position) : base(position)
        {
            OnInteraction(Interaction.Trigger, () =>
            {
                if (selected == null)
                    return;

                var prompt = editor.Prompt;
                var panel = panels[ui.IndexOf(selected)];
                var (uw, uh) = editor.MapsUi.View.Size;
                var (ew, eh) = editor.MapsEditor.View.Size;

                if (Text == "Remove")
                {
                    BlockRemove(selected);
                    inspector.IsHidden = true;
                }
                else if (Text == "To Top")
                    BlockToTop(selected);
                else if (Text == "Disabled")
                    selected.IsDisabled = IsSelected;
                else if (Text == "Hidden")
                    selected.IsHidden = IsSelected;
                else if (Text == "Align X")
                {
                    prompt.Text = "Left      Right";
                    prompt.Open(promptSlider, onButtonTrigger: i =>
                    {
                        prompt.Close();

                        if (i != 0)
                            return;

                        Input.TilemapSize = (ew, eh);
                        panel.AlignInside((promptSlider.Progress, float.NaN));
                        panel.Position = (panel.Position.x + 1, panel.Position.y);
                        Input.TilemapSize = (uw, uh);
                    });
                }
                else if (Text == "Align Y")
                {
                    prompt.Text = "Top      Bottom";
                    prompt.Open(promptSlider, onButtonTrigger: i =>
                    {
                        prompt.Close();

                        if (i != 0)
                            return;

                        Input.TilemapSize = (ew, eh);
                        panel.AlignInside((float.NaN, promptSlider.Progress));
                        panel.Position = (panel.Position.x, panel.Position.y + 1);
                        Input.TilemapSize = (uw, uh);
                    });
                }
                else if (Text == "ItemSelect")
                {
                    var isViewer = selected is FileViewer;
                    var items = isViewer ?
                        (InputBox)inspector.blocks[typeof(FileViewer)][0] :
                        (InputBox)inspector.blocks[typeof(List)][3];
                    var list = isViewer ? ((FileViewer)selected).FilesAndFolders : (List)selected;
                    var index = inspector.itemSelections.IndexOf(this);
                    var item = list[index + items.ScrollIndices.y];

                    if (list.IsSingleSelecting && list.IndexOf(list.ItemsSelected[0]) == index)
                        return;

                    list.Select(item, IsSelected);
                    inspector.UpdateSelected();
                }
                else if (Text.Contains("Cut") && selected is Layout l)
                {
                    var index = (int)((Stepper)inspector.blocks[typeof(Layout)][1]).Value;
                    var rate = ((Stepper)inspector.blocks[typeof(Layout)][2]).Value;

                    if (Text.Contains("Top")) l.Cut(index, Side.Top, rate);
                    else if (Text.Contains("Left")) l.Cut(index, Side.Left, rate);
                    else if (Text.Contains("Right")) l.Cut(index, Side.Right, rate);
                    else if (Text.Contains("Bottom")) l.Cut(index, Side.Bottom, rate);

                    inspector.UpdatePanelValues();
                }
                else if (Text == "Restore" && selected is Layout la)
                {
                    la.Restore();
                    inspector.UpdatePanelValues();
                }
            });
        }
    }

    private readonly List<EditButton> checkboxes = new(), itemSelections = new();
    private readonly EditButton
        toTop = new((0, 0)) { Text = "To Top" },
        alignX = new((0, 0)) { Text = "Align X" },
        alignY = new((0, 0)) { Text = "Align Y" },
        remove = new((0, 0)) { Text = "Remove" };
    private readonly Dictionary<Type, List<Block>> blocks;
    private Block? prevSelected;

    private void UpdatePanel()
    {
        var offset = (Size.width - Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (Position.x + offset, Position.y);
        const int CORNER = Tile.BOX_HOLLOW_CORNER;
        const int STRAIGHT = Tile.BOX_HOLLOW_STRAIGHT;
        var front = editor.MapsUi[(int)Editor.LayerMapsUi.Front];
        var color = Color.Gray.ToDark(0.66f);
        var (bottomX, bottomY) = (Position.x + 1, Position.y + Size.height - 2);
        var (topX, topY) = (Position.x + 1, Position.y + 1);

        SetClear(Editor.LayerMapsUi.Back, this);
        SetClear(Editor.LayerMapsUi.Middle, this);
        SetClear(Editor.LayerMapsUi.Front, this);

        SetBackground(editor.MapsUi[(int)Editor.LayerMapsUi.Back], this, color);

        front.SetBox(Area, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Yellow);
        front.SetTextLine(textPos, Text, Color.Yellow);

        UpdateButton(toTop, (topX, topY));
        UpdateButton(alignX, (topX, topY + 1));
        UpdateButton(alignY, (topX, topY + 2));
        UpdateButton(remove, (bottomX, bottomY));
    }
    private void UpdatePanelBlocks()
    {
        var (x, y) = (Position.x + 1, Position.y + 4);
        foreach (var kvp in blocks)
        {
            var type = selected?.GetType();

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

        var panel = panels[ui.IndexOf(selected)];
        var disabled = (EditButton)blocks[typeof(Block)][0];
        var hidden = (EditButton)blocks[typeof(Block)][1];
        var text = (InputBox)blocks[typeof(Block)][2];

        prevSelected.IsDisabled = disabled.IsSelected;
        prevSelected.IsHidden = hidden.IsSelected;

        if (prevSelected is InputBox i)
        {
            var editable = (Button)blocks[typeof(InputBox)][0];
            var placeholder = (InputBox)blocks[typeof(InputBox)][1];
            i.IsReadOnly = editable.IsSelected;
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
            var width = (Stepper)blocks[typeof(Pages)][2];
            var gap = (Stepper)blocks[typeof(Pages)][3];
            p.Count = (int)count.Value;
            p.Current = (int)current.Value;
            p.ItemWidth = (int)width.Value;
            p.ItemGap = (int)gap.Value;
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
            foreach (var t in split)
                l.Add(new Button { Text = t });

            for (var j = 0; j < l.Count; j++)
            {
                if (j >= prev.Count)
                    break;

                l.Select(l[j], prev[j]);
            }
        }
        else if (prevSelected is FileViewer v)
        {
            var fileSelect = (EditButton)blocks[typeof(FileViewer)][0];
            v.IsSelectingFolders = fileSelect.IsSelected == false;
        }

        prevSelected.Text = text.Value;
    }
    private void UpdatePanelValues()
    {
        if (selected == null)
            return;

        var disabled = (EditButton)blocks[typeof(Block)][0];
        var hidden = (EditButton)blocks[typeof(Block)][1];
        var text = (InputBox)blocks[typeof(Block)][2];

        disabled.IsSelected = selected.IsDisabled;
        hidden.IsSelected = selected.IsHidden;

        var valueText = selected.Text;
        if (selected is InputBox e)
            valueText = e.Value;

        text.Placeholder = "Text�";
        text.Value = valueText;
        text.CursorIndices = (0, 0);
        text.SelectionIndices = (0, 0);
        text.ScrollIndices = (0, 0);

        if (selected is Button b)
            ((Button)blocks[typeof(Button)][0]).IsSelected = b.IsSelected;
        else if (selected is InputBox i)
        {
            text.Placeholder = "Value�";

            var editable = (Button)blocks[typeof(InputBox)][0];
            var placeholder = (InputBox)blocks[typeof(InputBox)][1];
            editable.IsSelected = i.IsReadOnly;
            placeholder.Value = i.Placeholder;
            placeholder.CursorIndices = (0, 0);
            placeholder.SelectionIndices = (0, 0);
            placeholder.ScrollIndices = (0, 0);
        }
        else if (selected is Pages p)
        {
            var count = (Stepper)blocks[typeof(Pages)][0];
            var current = (Stepper)blocks[typeof(Pages)][1];
            var width = (Stepper)blocks[typeof(Pages)][2];
            var gap = (Stepper)blocks[typeof(Pages)][3];
            count.Value = p.Count;
            current.Range = (1, count.Value);
            current.Value = p.Current;
            width.Value = p.ItemWidth;
            gap.Value = p.ItemGap;
        }
        else if (selected is Panel pa)
        {
            var movable = (Button)blocks[typeof(Panel)][0];
            var resizable = (Button)blocks[typeof(Panel)][1];
            var restricted = (Button)blocks[typeof(Panel)][2];
            movable.IsSelected = pa.IsMovable;
            resizable.IsSelected = pa.IsResizable;
            restricted.IsSelected = pa.IsRestricted;
        }
        else if (selected is Palette pl)
        {
            var brightnessMax = (Stepper)blocks[typeof(Palette)][0];
            var brightness = (Stepper)blocks[typeof(Palette)][1];
            var opacity = (Stepper)blocks[typeof(Palette)][2];
            brightnessMax.Value = pl.Brightness.Count;
            brightness.Range = (1, brightnessMax.Value);
            brightness.Value = pl.Brightness.Current;
            opacity.Value = pl.Opacity.Progress;
        }
        else if (selected is Slider s)
        {
            var vertical = (Button)blocks[typeof(Slider)][0];
            var progress = (Stepper)blocks[typeof(Slider)][1];
            vertical.IsSelected = s.IsVertical;
            progress.Value = s.Progress;
        }
        else if (selected is Scroll sc)
        {
            var vertical = (Button)blocks[typeof(Scroll)][0];
            var progress = (Stepper)blocks[typeof(Scroll)][1];
            var step = (Stepper)blocks[typeof(Scroll)][2];
            vertical.IsSelected = sc.IsVertical;
            progress.Value = sc.Slider.Progress;
            step.Value = sc.Step;
        }
        else if (selected is Stepper st)
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
        else if (selected is Layout la)
        {
            var index = (Stepper)blocks[typeof(Layout)][1];
            index.Range = (0, la.Count - 1);
        }
        else if (selected is FileViewer v)
        {
            var fileSelect = (EditButton)blocks[typeof(FileViewer)][0];
            fileSelect.IsSelected = v.IsSelectingFolders == false;
        }
        else if (selected is List l)
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
            expanded.IsDisabled = l.Span != Span.Dropdown;
            scroll.Value = l.Scroll.Slider.Progress;
            multi.IsSelected = l.IsSingleSelecting == false;
            multi.IsDisabled = l.Span == Span.Dropdown;
            itemWidth.Value = l.ItemSize.width;
            itemHeight.Value = l.ItemSize.height;
            itemGap.Value = l.ItemGap;

            var value = "";
            for (var j = 0; j < l.Count; j++)
                value += $"{(j > 0 ? Environment.NewLine : "")}{l[j].Text}";

            items.Value = value;
            items.SelectionIndices = (0, 0);
            items.CursorIndices = (0, 0);
            items.CursorScroll();
        }
    }

    private void UpdateButton(Button btn, (int x, int y) position)
    {
        var front = editor.MapsUi[(int)Editor.LayerMapsUi.Front];

        btn.Position = position;
        btn.Size = (Size.width - 2, 1);

        btn.Update();

        var color = checkboxes.Contains(btn) && btn.IsSelected ? Color.Green.ToBright() : Color.Yellow;
        color = btn.IsDisabled ? Color.White : color;

        var (x, y) = btn.Position;
        var (w, h) = btn.Size;
        front.SetTextRectangle((x, y, w, h), btn.Text, GetColor(btn, color.ToDark()),
            alignment: Alignment.Center);
    }
    private void UpdateInputBox(InputBox inputBox, (int x, int y) position)
    {
        var e = inputBox;
        var back = editor.MapsUi[(int)Editor.LayerMapsUi.Back];
        var middle = editor.MapsUi[(int)Editor.LayerMapsUi.Middle];
        var front = editor.MapsUi[(int)Editor.LayerMapsUi.Front];
        var color = Color.Gray;
        var isListItems = e.Placeholder.Contains("Items");
        var (x, y) = e.Position;
        var (w, h) = e.Size;

        position = isListItems ? (position.x + 1, position.y) : position;

        e.Position = position;
        e.Size = (Size.width - 2 - (isListItems ? 1 : 0), e.Size.height);

        e.Update();

        back.SetArea((x, y, w, h), null, new Tile(Tile.SHADE_OPAQUE, color.ToDark()));
        SetClear(Editor.LayerMapsUi.Middle, e);

        (x, y) = e.Position;
        (w, h) = e.Size;

        back.SetTextRectangle((x, y, w, h), e.Selection,
            e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
        middle.SetTextRectangle((x, y, w, h), e.Text, isWordWrapping: false);
        middle.SetTextLine((Position.x + 1, e.Position.y - 1), e.Placeholder, color);

        if (string.IsNullOrWhiteSpace(e.Text))
            middle.SetTextRectangle((x, y, w, h), e.Placeholder, color.ToBright(), false);

        if (e.IsCursorVisible)
            front.SetTile(e.PositionFromIndices(e.CursorIndices), new(Tile.SHAPE_LINE, Color.White, 2));

        if (isListItems == false || selected is not List list)
            return;

        UpdateListItems(list, inputBox);
    }
    private void UpdateStepper(Stepper stepper, (int x, int y) position)
    {
        var e = stepper;
        var color = Color.Gray;
        var middle = editor.MapsUi[(int)Editor.LayerMapsUi.Middle];
        var front = editor.MapsUi[(int)Editor.LayerMapsUi.Front];
        var value = e.Step.Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";
        e.Position = position;
        e.Size = (Size.width - 2, 2);

        var prev = e.Value;
        e.Update();
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (prev != e.Value)
            UpdateSelected();

        SetBackground(middle, stepper, color.ToDark());
        SetBackground(front, stepper, color.ToDark());

        front.SetTile(e.Increase.Position,
            new(Tile.ARROW_TAILLESS_ROUND, GetColor(e.Increase, color), 3));
        front.SetTile(e.Decrease.Position,
            new(Tile.ARROW_TAILLESS_ROUND, GetColor(e.Decrease, color), 1));
        front.SetTextLine((e.Position.x + 2, e.Position.y), e.Text, color);
        front.SetTextLine((e.Position.x + 2, e.Position.y + 1), value);

        front.SetTile(e.Minimum.Position, new(Tile.MATH_MUCH_LESS, GetColor(e.Minimum, color)));
        front.SetTile(e.Middle.Position, new(Tile.PUNCTUATION_PIPE, GetColor(e.Middle, color)));
        front.SetTile(e.Maximum.Position, new(Tile.MATH_MUCH_GREATER, GetColor(e.Maximum, color)));
    }
    private void UpdateListItems(List list, InputBox inputBoxEditor)
    {
        var middle = editor.MapsUi[(int)Editor.LayerMapsUi.Middle];
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

        if (selected is not List list)
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
        map.SetBox(block.Area, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
    private static void SetClear(Editor.LayerMapsUi layer, Block block)
    {
        editor.MapsUi[(int)layer].SetArea(block.Area, null, 0);
    }
#endregion
}