using static Pure.EditorUserInterface.Program;

namespace Pure.EditorUserInterface;

using Tilemap;
using UserInterface;
using Utilities;

public class RendererEdit : UserInterface
{
    public void ElementCreate(int index, (int x, int y) position)
    {
        var element = default(Element);
        var panel = new Panel(position) { IsRestricted = false, SizeMinimum = (3, 3) };
        if (index == 1) element = new Button(position);
        else if (index == 2) element = new InputBox(position);
        else if (index == 3)
        {
            element = new Pages(position);
            panel.SizeMinimum = (8, 3);
            panel.SizeMaximum = (int.MaxValue, 3);
        }
        else if (index == 4)
        {
            element = new Panel(position);
            panel.SizeMinimum = (5, 5);
        }
        else if (index == 5)
        {
            element = new Palette(position);
            panel.IsResizable = false;
        }
        else if (index == 6)
        {
            element = new Slider(position);
            panel.SizeMinimum = (4, 3);
            panel.SizeMaximum = (int.MaxValue, 3);
        }
        else if (index == 7)
        {
            element = new Scroll(position);
            panel.SizeMinimum = (3, 6);
            panel.SizeMaximum = (3, int.MaxValue);
        }
        else if (index == 8)
        {
            element = new Stepper(position);
            panel.IsResizable = false;
        }
        else if (index == 9)
            element = new List(position);

        if (element == null)
            return;

        panel.Size = (element.Size.width + 2, element.Size.height + 2);
        panel.Text = element.Text;

        ui.Add(element);
        Add(panel);

        Element.Focused = null;
    }
    public void ElementRemove(Element element)
    {
        var panel = this[ui.IndexOf(element)];
        Remove(panel);
        ui.Remove(element);

        if (Selected == element)
            Selected = null;
    }
    public void ElementToTop(Element element)
    {
        var panel = this[ui.IndexOf(element)];
        editUI.BringToTop(panel);
        ui.BringToTop(element);
        Selected = element;
    }

    protected override void OnUpdatePanel(Panel panel)
    {
        var index = IndexOf(panel);
        var e = ui[index];

        if (panel is { IsPressedAndHeld: true, IsHovered: true })
        {
            var isHoveringMenu = false;
            foreach (var kvp in menus)
                if (kvp.Value.IsHovered)
                {
                    isHoveringMenu = true;
                    break;
                }

            if (isHoveringMenu == false)
                Selected = e;
        }

        e.Position = (panel.Position.x + 1, panel.Position.y + 1);
        e.Size = (panel.Size.width - 2, panel.Size.height - 2);

        var offset = (panel.Size.width - panel.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (panel.Position.x + offset, panel.Position.y);
        const int corner = Tile.BORDER_GRID_CORNER;
        const int straight = Tile.BORDER_GRID_STRAIGHT;

        if (Selected != e)
            return;

        var back = tilemaps[(int)Layer.EditBack];
        var middle = tilemaps[(int)Layer.EditMiddle];
        back.SetBorder(panel.Position, panel.Size, corner, straight, Color.Cyan);
        back.SetRectangle(textPos, (panel.Text.Length, 1), default);
        middle.SetTextLine(textPos, panel.Text, Color.Cyan);
    }
    protected override void OnPanelResize(Panel panel, (int width, int height) delta)
    {
        SetInfoText($"{panel.Size.width}x{panel.Size.height}");
    }
    protected override void OnDragPanel(Panel panel, (int width, int height) delta)
    {
        SetInfoText($"{panel.Position.x}, {panel.Position.y}");
    }

    #region Backend
    private readonly Dictionary<string, List<(string, (int offX, int offY))>> pins = new();

    /*private void PinOverlapping(string parentKey)
    {
        var panel = this[parentKey];
        var element = ui[parentKey];
        var offset = (panel.Position.x - element.Position.x, panel.Position.y - element.Position.y);

        if (pins.ContainsKey(parentKey) == false)
            pins[parentKey] = new();
    }*/
    #endregion
}