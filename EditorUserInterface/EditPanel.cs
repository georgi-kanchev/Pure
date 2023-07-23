using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;
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
        var selected = new EditButton((0, 0)) { Text = "Selected" };
        var text = new InputBox((0, 0)) { Placeholder = "Text…", [0] = "" };
        var placeholder = new InputBox((0, 0)) { Placeholder = "Placeholder…", [0] = "Type…" };

        checkboxes.AddRange(new[] { disabled, hidden, selected });

        elements = new()
        {
            { typeof(Element), new() { disabled, hidden, text } },
            { typeof(Button), new() { selected } },
            { typeof(InputBox), new() { placeholder } },
        };
    }

    protected override void OnDisplay()
    {
        if ((IsHidden == false).Once("on-show"))
            UpdatePanelElementsValues();
        if (IsHidden.Once("on-hide"))
            UpdateSelectedWithTexts();

        if (Selected != null && !IsHidden)
        {
            if ((prevSelected != Selected).Once("on-select"))
            {
                UpdateSelectedWithTexts();
                UpdatePanelElementsValues();
            }

            TryNotifyTextChange();
            RepositionPanel();
            UpdatePanel();
            UpdatePanelElements();
        }

        prevSelected = Selected;
    }

#region Backend
    private class EditButton : Button
    {
        public EditButton((int x, int y) position) : base(position) { }

        protected override void OnUserAction(UserAction userAction)
        {
            base.OnUserAction(userAction);

            if (userAction != UserAction.Trigger || Selected == null)
                return;

            var name = Selected.GetType().Name;
            var action = "";
            var panel = editUI[ui.IndexOf(Selected)];

            if (Text == "Remove")
            {
                editUI.ElementRemove(Selected);
                editPanel.Position = (int.MaxValue, int.MaxValue);
                action = "removed";
            }
            else if (Text == "To Top")
            {
                editUI.ElementToTop(Selected);
                action = "surfaced";
            }
            else if (Text == "Disabled")
            {
                Selected.IsDisabled = IsSelected;
                action = IsSelected ? "disabled" : "enabled";
            }
            else if (Text == "Hidden")
            {
                Selected.IsHidden = IsSelected;
                action = IsSelected ? "hidden" : "shown";
            }
            else if (Text == "Selected")
            {
                ((Button)Selected).IsSelected = IsSelected;
                action = IsSelected ? "selected" : "deselected";
            }
            else if (Text == "Center X")
            {
                panel.Position = (
                    CameraPosition.x + CameraSize.w / 2 - panel.Size.width / 2,
                    panel.Position.y);
                action = "centered horizontally";
            }
            else if (Text == "Center Y")
            {
                panel.Position = (
                    panel.Position.x,
                    CameraPosition.y + CameraSize.h / 2 - panel.Size.height / 2);
                action = "centered vertically";
            }

            DisplayInfoText(name + " " + action);
        }
    }

    private readonly List<EditButton> checkboxes = new();
    private readonly EditButton
        toTop = new((0, 0)) { Text = "To Top" },
        centerX = new((0, 0)) { Text = "Center X" },
        centerY = new((0, 0)) { Text = "Center Y" },
        remove = new((0, 0)) { Text = "Remove" };
    private readonly Dictionary<Type, List<Element>> elements;
    private string prevText = "", prevPlaceholder = "";
    private Element? prevSelected;

    private void TryNotifyTextChange()
    {
        if (Selected == null)
            return;

        var onTextChange = (prevText != Selected.Text).Once("on-text-change");
        if (onTextChange && Selected == prevSelected)
            DisplayInfoText($"{Selected.GetType().Name} text changed");

        if (Selected is InputBox inputBox)
        {
            var p = prevPlaceholder != inputBox.Placeholder;
            var onPlaceholderChange = p.Once("on-placeholder-change");
            if (onPlaceholderChange && inputBox == prevSelected)
                DisplayInfoText($"{Selected.GetType().Name} placeholder changed");
        }

        prevText = Selected.Text;

        if (Selected is InputBox i)
            prevPlaceholder = i.Placeholder;
    }

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
                    UpdateInputBox(i, (x, y));

                y++;
            }
        }
    }

    private void UpdateSelectedWithTexts()
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

        prevSelected.Text = text.Value;
    }
    private void UpdatePanelElementsValues()
    {
        if (Selected == null)
            return;

        ((Button)elements[typeof(Element)][0]).IsSelected = Selected.IsDisabled;
        ((Button)elements[typeof(Element)][1]).IsSelected = Selected.IsHidden;

        var value = Selected.Text;
        if (Selected is InputBox e)
            value = e.Value;

        var text = (InputBox)elements[typeof(Element)][2];

        text.Value = value;
        text.CursorIndices = (0, 0);
        text.SelectionIndices = (0, 0);
        text.ScrollIndices = (0, 0);

        if (Selected is Button b)
            ((Button)elements[typeof(Button)][0]).IsSelected = b.IsSelected;

        if (Selected is InputBox i)
        {
            var placeholder = ((InputBox)elements[typeof(InputBox)][0]);
            placeholder.Value = i.Placeholder;
            placeholder.CursorIndices = (0, 0);
            placeholder.SelectionIndices = (0, 0);
            placeholder.ScrollIndices = (0, 0);
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
        //SetBackground(front, e, color.ToDark());

        back.SetTextRectangle(e.Position, e.Size, e.Selection,
            e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
        middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(e.Text))
            middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, color.ToBright(), false);

        if (e.IsCursorVisible)
            front.SetTile(e.PositionFromIndices(e.CursorIndices), new(Tile.SHAPE_LINE, Color.White, 2));
    }

    private static Color GetColor(Element element, Color baseColor)
    {
        if (element.IsPressedAndHeld) return baseColor.ToDark();
        else if (element.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
    private static void SetBackground(Tilemap.Tilemap map, Element element, Color color)
    {
        var tile = new Tile(Tile.SHADE_OPAQUE, color);
        map.SetRectangle(element.Position, element.Size, tile);
    }
#endregion
}