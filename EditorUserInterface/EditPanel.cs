using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using static Pure.EditorUserInterface.Program;

namespace Pure.EditorUserInterface;

public class EditPanel : Panel
{
    public EditPanel((int x, int y) position) : base(position)
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

        var pagesCount = new EditStepper((0, 0)) { Text = "Count", Range = (1, 99) };
        var currentPage = new EditStepper((0, 0)) { Text = "Current" };

        var movable = new EditButton((0, 0)) { Text = "Movable" };
        var resizable = new EditButton((0, 0)) { Text = "Resizable" };
        var restricted = new EditButton((0, 0)) { Text = "Restricted" };

        var brightnessMax = new EditStepper((0, 0)) { Text = "Levels", Range = (1, 99) };
        var brightness = new EditStepper((0, 0)) { Text = "Brightness" };
        var opacity = new EditStepper((0, 0)) { Text = "Opacity", Range = (0, 1), Step = 0.05f };

        var vertical = new EditButton((0, 0)) { Text = "Vertical" };
        var progress = new EditStepper((0, 0)) { Text = "Progress", Range = (0, 1), Step = 0.05f };

        var step = new EditStepper((0, 0)) { Text = "Step", Range = (0, 1), Step = 0.05f };

        var stepperStep = new EditStepper((0, 0)) { Text = "Step", Step = 0.05f };
        var min = new EditStepper((0, 0)) { Text = "Minimum", Step = 0.05f };
        var max = new EditStepper((0, 0)) { Text = "Maximum", Step = 0.05f };
        var value = new EditStepper((0, 0)) { Text = "Value", Step = 0.05f };

        checkboxes.AddRange(new[]
            { disabled, hidden, selected, movable, resizable, restricted, vertical });

        elements = new()
        {
            { typeof(Element), new() { disabled, hidden, text } },
            { typeof(Button), new() { selected } },
            { typeof(InputBox), new() { placeholder } },
            { typeof(Pages), new() { pagesCount, currentPage } },
            { typeof(Panel), new() { movable, resizable, restricted } },
            { typeof(Palette), new() { brightnessMax, brightness, opacity } },
            { typeof(Slider), new() { vertical, progress } },
            { typeof(Scroll), new() { vertical, progress, step } },
            { typeof(Stepper), new() { min, max, value, stepperStep } },
        };
    }

    protected override void OnDisplay()
    {
        if ((IsHidden == false).Once("on-show"))
            UpdatePanelValues();
        if (IsHidden.Once("on-hide"))
            UpdateSelected();

        if (Selected != null && !IsHidden)
        {
            if ((prevSelected != Selected).Once("on-select"))
            {
                UpdateSelected();
                UpdatePanelValues();
            }

            RepositionPanel();
            UpdatePanel();
            UpdatePanelElements();
        }

        prevSelected = Selected;
    }

#region Backend
    private class EditButton : Button
    {
        public EditStepper? parent;

        public EditButton((int x, int y) position) : base(position) { }

        protected override void OnUserAction(UserAction userAction)
        {
            base.OnUserAction(userAction);

            if (userAction != UserAction.Trigger || Selected == null)
                return;

            var panel = editUI[ui.IndexOf(Selected)];

            if (Text == "Remove")
            {
                editUI.ElementRemove(Selected);
                editPanel.Position = (int.MaxValue, int.MaxValue);
            }
            else if (Text == "To Top")
            {
                editUI.ElementToTop(Selected);
            }
            else if (Text == "Disabled")
            {
                Selected.IsDisabled = IsSelected;
            }
            else if (Text == "Hidden")
            {
                Selected.IsHidden = IsSelected;
            }
            else if (Text == "Selected")
            {
                ((Button)Selected).IsSelected = IsSelected;
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
            else if (Text == "Movable")
            {
                var p = (Panel)Selected;
                p.IsMovable = IsSelected;
            }
            else if (Text == "Resizable")
            {
                var p = (Panel)Selected;
                p.IsResizable = IsSelected;
            }
            else if (Text == "Restricted")
            {
                var p = (Panel)Selected;
                p.IsRestricted = IsSelected;
            }
            else if (Text == "Vertical" && Selected is Slider s)
            {
                s.IsVertical = IsSelected;
                panel.SizeMinimum = IsSelected ? (3, 4) : (4, 3);
            }
            else if (Text == "Vertical" && Selected is Scroll sc)
            {
                sc.IsVertical = IsSelected;
                panel.SizeMinimum = IsSelected ? (3, 4) : (4, 3);
            }
            else if (Text == "Minimum" && parent != null)
            {
                parent.Value = parent.Range.minimum;
            }
            else if (Text == "Middle" && parent != null)
            {
                var max = parent.Range.maximum;
                parent.Value = Snap(float.IsPositiveInfinity(max) ? 0 : max / 2, parent.Step);
            }
            else if (Text == "Maximum" && parent != null)
            {
                parent.Value = parent.Range.maximum;
            }
        }
    }

    private class EditStepper : Stepper
    {
        public EditButton Minimum { get; }
        public EditButton Middle { get; }
        public EditButton Maximum { get; }

        public EditStepper((int x, int y) position, float value = 0) : base(position, value)
        {
            Minimum = new((0, 0)) { Text = "Minimum", Size = (1, 1) };
            Middle = new((0, 0)) { Text = "Middle", Size = (1, 1) };
            Maximum = new((0, 0)) { Text = "Maximum", Size = (1, 1) };
        }
    }

    private readonly List<EditButton> checkboxes = new();
    private readonly EditButton
        toTop = new((0, 0)) { Text = "To Top" },
        centerX = new((0, 0)) { Text = "Center X" },
        centerY = new((0, 0)) { Text = "Center Y" },
        remove = new((0, 0)) { Text = "Remove" };
    private readonly Dictionary<Type, List<Element>> elements;
    private Element? prevSelected;

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
        var back = tilemaps[(int)Layer.EditBack];
        var middle = tilemaps[(int)Layer.EditMiddle];
        var front = tilemaps[(int)Layer.EditFront];
        var color = Color.Gray.ToDark(0.66f);
        var (bottomX, bottomY) = (Position.x + 1, Position.y + Size.height - 2);
        var (topX, topY) = (Position.x + 1, Position.y + 1);

        SetBackground(back, this, color);
        SetBackground(middle, this, color);
        SetBackground(front, this, color);

        front.SetBox(Position, Size, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Yellow);
        front.SetTextLine(textPos, Text, Color.Yellow);

        UpdateButton(toTop, (topX, topY));
        UpdateButton(centerX, (topX, topY + 1));
        UpdateButton(centerY, (topX, topY + 2));
        UpdateButton(remove, (bottomX, bottomY));

        var count = (Stepper)elements[typeof(Pages)][0];
        var current = (Stepper)elements[typeof(Pages)][1];
        var brightnessMax = (Stepper)elements[typeof(Palette)][0];
        var brightness = (Stepper)elements[typeof(Palette)][1];

        current.Range = (1, count.Value);
        brightness.Range = (1, brightnessMax.Value);
    }
    private void UpdatePanelElements()
    {
        var (x, y) = (Position.x + 1, Position.y + 4);
        foreach (var kvp in elements)
        {
            var type = Selected?.GetType();

            if (type == null || type != kvp.Key && type.IsSubclassOf(kvp.Key) == false)
                continue;

            y++;
            foreach (var element in kvp.Value)
            {
                if (element is Button b)
                    UpdateButton(b, (x, y));
                else if (element is InputBox i)
                {
                    y += 2;
                    UpdateInputBox(i, (x, y));
                }
                else if (element is EditStepper s)
                {
                    UpdateStepper(s, (x, y));
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

        var text = (InputBox)elements[typeof(Element)][2];

        if (prevSelected is InputBox i)
        {
            var placeholder = (InputBox)elements[typeof(InputBox)][0];
            i.Value = text.Value;
            i.Placeholder = placeholder.Value;
        }
        else if (prevSelected is Pages p)
        {
            var count = (Stepper)elements[typeof(Pages)][0];
            var current = (Stepper)elements[typeof(Pages)][1];
            p.Count = (int)count.Value;
            p.Current = (int)current.Value;
        }
        else if (prevSelected is Palette pl)
        {
            var brightnessMax = (Stepper)elements[typeof(Palette)][0];
            var brightness = (Stepper)elements[typeof(Palette)][1];
            var opacity = (Stepper)elements[typeof(Palette)][2];
            pl.Brightness.Count = (int)brightnessMax.Value;
            pl.Brightness.Current = (int)brightness.Value;
            pl.Opacity.Progress = opacity.Value;
        }
        else if (prevSelected is Slider s)
        {
            var progress = (Stepper)elements[typeof(Slider)][1];
            s.Progress = progress.Value;
        }
        else if (prevSelected is Scroll sc)
        {
            var progress = (Stepper)elements[typeof(Slider)][1];
            var step = (Stepper)elements[typeof(Scroll)][2];
            sc.Step = step.Value;
            sc.Slider.Progress = progress.Value;
        }
        else if (prevSelected is Stepper st)
        {
            var min = (Stepper)elements[typeof(Stepper)][0];
            var max = (Stepper)elements[typeof(Stepper)][1];
            var value = (Stepper)elements[typeof(Stepper)][2];
            var step = (Stepper)elements[typeof(Stepper)][3];
            st.Range = (min.Value, max.Value);
            st.Value = value.Value;
            st.Step = step.Value;
        }

        prevSelected.Text = text.Value;
    }
    private void UpdatePanelValues()
    {
        if (Selected == null)
            return;

        ((Button)elements[typeof(Element)][0]).IsSelected = Selected.IsDisabled;
        ((Button)elements[typeof(Element)][1]).IsSelected = Selected.IsHidden;

        var valueText = Selected.Text;
        if (Selected is InputBox e)
            valueText = e.Value;

        var text = (InputBox)elements[typeof(Element)][2];

        text.Placeholder = "Text…";
        text.Value = valueText;
        text.CursorIndices = (0, 0);
        text.SelectionIndices = (0, 0);
        text.ScrollIndices = (0, 0);

        if (Selected is Button b)
            ((Button)elements[typeof(Button)][0]).IsSelected = b.IsSelected;
        else if (Selected is InputBox i)
        {
            text.Placeholder = "Value…";

            var placeholder = (InputBox)elements[typeof(InputBox)][0];
            placeholder.Value = i.Placeholder;
            placeholder.CursorIndices = (0, 0);
            placeholder.SelectionIndices = (0, 0);
            placeholder.ScrollIndices = (0, 0);
        }
        else if (Selected is Pages p)
        {
            var count = (Stepper)elements[typeof(Pages)][0];
            var current = (Stepper)elements[typeof(Pages)][1];
            count.Value = p.Count;
            current.Range = (1, count.Value);
            current.Value = p.Current;
        }
        else if (Selected is Panel pa)
        {
            var movable = (Button)elements[typeof(Panel)][0];
            var resizable = (Button)elements[typeof(Panel)][1];
            var restricted = (Button)elements[typeof(Panel)][2];
            movable.IsSelected = pa.IsMovable;
            resizable.IsSelected = pa.IsResizable;
            restricted.IsSelected = pa.IsRestricted;
        }
        else if (Selected is Palette pl)
        {
            var brightnessMax = (Stepper)elements[typeof(Palette)][0];
            var brightness = (Stepper)elements[typeof(Palette)][1];
            var opacity = (Stepper)elements[typeof(Palette)][2];
            brightnessMax.Value = pl.Brightness.Count;
            brightness.Range = (1, brightnessMax.Value);
            brightness.Value = pl.Brightness.Current;
            opacity.Value = pl.Opacity.Progress;
        }
        else if (Selected is Slider s)
        {
            var vertical = (Button)elements[typeof(Slider)][0];
            var progress = (Stepper)elements[typeof(Slider)][1];
            vertical.IsSelected = s.IsVertical;
            progress.Value = s.Progress;
        }
        else if (Selected is Scroll sc)
        {
            var vertical = (Button)elements[typeof(Scroll)][0];
            var progress = (Stepper)elements[typeof(Scroll)][1];
            var step = (Stepper)elements[typeof(Scroll)][2];
            vertical.IsSelected = sc.IsVertical;
            progress.Value = sc.Slider.Progress;
            step.Value = sc.Step;
        }
        else if (prevSelected is Stepper st)
        {
            var min = (Stepper)elements[typeof(Stepper)][0];
            var max = (Stepper)elements[typeof(Stepper)][1];
            var value = (Stepper)elements[typeof(Stepper)][2];
            var step = (Stepper)elements[typeof(Stepper)][3];
            min.Value = st.Range.minimum;
            max.Value = st.Range.maximum;
            value.Value = st.Value;
            step.Value = st.Step;
        }
    }

    private void UpdateButton(Button btn, (int x, int y) position)
    {
        var front = tilemaps[(int)Layer.EditFront];

        btn.Position = position;
        btn.Size = (Size.width - 2, 1);

        btn.Update();

        var color = checkboxes.Contains(btn) && btn.IsSelected ? Color.Green.ToBright() : Color.Yellow;

        front.SetTextRectangle(btn.Position, btn.Size, btn.Text, GetColor(btn, color.ToDark()),
            alignment: Tilemap.Tilemap.Alignment.Center);
    }
    private void UpdateInputBox(InputBox inputBox, (int x, int y) position, int height = 1)
    {
        var e = inputBox;
        var back = tilemaps[(int)Layer.EditBack];
        var middle = tilemaps[(int)Layer.EditMiddle];
        var front = tilemaps[(int)Layer.EditFront];
        var color = Color.Gray;

        e.Position = position;
        e.Size = (Size.width - 2, height);

        e.Update();

        SetBackground(back, e, color.ToDark());
        SetBackground(middle, e, color.ToDark());

        back.SetTextRectangle(e.Position, e.Size, e.Selection,
            e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
        middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);
        middle.SetTextLine((e.Position.x, e.Position.y - 1), e.Placeholder, color);

        if (string.IsNullOrWhiteSpace(e.Text))
            middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, color.ToBright(), false);

        if (e.IsCursorVisible)
            front.SetTile(e.PositionFromIndices(e.CursorIndices), new(Tile.SHAPE_LINE, Color.White, 2));
    }
    private void UpdateStepper(Stepper stepper, (int x, int y) position)
    {
        var e = (EditStepper)stepper;
        var color = Color.Gray;
        var middle = tilemaps[(int)Layer.EditMiddle];
        var front = tilemaps[(int)Layer.EditFront];
        var value = e.Value.Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";
        var (x, y) = e.Position;
        var (w, h) = e.Size;

        e.Position = position;
        e.Size = (Size.width - 2, 2);

        e.Minimum.parent = e;
        e.Middle.parent = e;
        e.Maximum.parent = e;

        e.Minimum.Position = (x + w - 3, y + h - 1);
        e.Middle.Position = (x + w - 2, y + h - 1);
        e.Maximum.Position = (x + w - 1, y + h - 1);

        e.Update();

        e.Minimum.Update();
        e.Middle.Update();
        e.Maximum.Update();

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

    private static Color GetColor(Element element, Color baseColor)
    {
        if (element.IsPressedAndHeld) return baseColor.ToDark(0.3f);
        else if (element.IsHovered) return baseColor.ToBright(0.3f);

        return baseColor;
    }
    private static void SetBackground(Tilemap.Tilemap map, Element element, Color color)
    {
        var tile = new Tile(Tile.SHADE_OPAQUE, color);
        var pos = element.Position;
        var size = element.Size;

        map.SetBox(pos, size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
    private static float Snap(float number, float interval)
    {
        if (Math.Abs(interval) < 0.001f)
            return number;

        // this prevents -0
        var value = number - (number < 0 ? interval : 0);
        value -= number % interval;
        return value;
    }
#endregion
}