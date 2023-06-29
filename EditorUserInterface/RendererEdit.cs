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

    public static void DrawGrid()
    {
        var tmapSz = tilemaps.Size;
        var color = Color.Gray.ToDark(0.66f);
        const int LAYER = (int)Layer.Grid;
        for (var i = 0; i < tmapSz.width; i += 10)
            tilemaps[LAYER].SetLine((i, 0), (i, tmapSz.height), new(Tile.SHADE_1, color));
        for (var i = 0; i < tmapSz.height; i += 10)
            tilemaps[LAYER].SetLine((0, i), (tmapSz.width, i), new(Tile.SHADE_1, color, 1));
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
        const int CORNER = Tile.BOX_GRID_CORNER;
        const int STRAIGHT = Tile.BOX_GRID_STRAIGHT;

        if (Selected != e)
            return;

        var back = tilemaps[(int)Layer.EditBack];
        var middle = tilemaps[(int)Layer.EditMiddle];
        back.SetBox(panel.Position, panel.Size, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Cyan);
        back.SetRectangle(textPos, (panel.Text.Length, 1), default);
        middle.SetTextLine(textPos, panel.Text, Color.Cyan);
    }
    protected override void OnPanelResize(Panel panel, (int width, int height) delta)
    {
        DisplayInfoText($"{panel.Text} {panel.Size.width - 2}x{panel.Size.height - 2}");
    }
    protected override void OnDragPanel(Panel panel, (int width, int height) delta)
    {
        DisplayInfoText($"{panel.Text} {panel.Position.x + 1}, {panel.Position.y + 1}");
    }
}