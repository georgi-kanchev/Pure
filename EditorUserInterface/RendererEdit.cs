using Pure.Window;

namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

public class RendererEdit : UserInterface
{
    public void CreateElement(int index, (int x, int y) position)
    {
        var count = Count.ToString();
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
            element = new List(position);

        if (element == null)
            return;

        panel.Size = (element.Size.width + 2, element.Size.height + 2);
        panel.Text = element.Text;

        Program.ui[count] = element;
        this[count] = panel;

        Element.Focused = null;
    }
    public void RemoveElement(Element element)
    {
        var key = Program.ui.KeyOf(element);
        if (key == null)
            return;

        Program.ui.Remove(key);
        Remove(key);
    }

    protected override void OnUpdatePanel(string key, Panel panel)
    {
        var e = Program.ui[key];

        if (panel is { IsPressedAndHeld: true, IsHovered: true })
            Program.Selected = e;

        e.Position = (panel.Position.x + 1, panel.Position.y + 1);
        e.Size = (panel.Size.width - 2, panel.Size.height - 2);

        var offset = (panel.Size.width - panel.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (panel.Position.x + offset, panel.Position.y);
        const int corner = Tile.BORDER_GRID_CORNER;
        const int straight = Tile.BORDER_GRID_STRAIGHT;

        if (Program.Selected != e)
            return;

        var back = Program.tilemaps[(int)Program.Layer.EditBack];
        var middle = Program.tilemaps[(int)Program.Layer.EditMiddle];
        back.SetBorder(panel.Position, panel.Size, corner, straight, Color.Cyan);
        back.SetRectangle(textPos, (panel.Text.Length, 1), default);
        middle.SetTextLine(textPos, panel.Text, Color.Cyan);
    }

    #region Backend
    private readonly Dictionary<string, List<(string, (int offX, int offY))>> pins = new();

    private void PinOverlapping(string parentKey)
    {
        var panel = this[parentKey];
        var element = Program.ui[parentKey];
        var offset = (panel.Position.x - element.Position.x, panel.Position.y - element.Position.y);

        if (pins.ContainsKey(parentKey) == false)
            pins[parentKey] = new();
    }
    #endregion
}