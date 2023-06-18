namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

public class RendererEdit : UserInterface
{
    private readonly TilemapManager tilemaps;
    private readonly RendererUI ui;

    public RendererEdit(TilemapManager tilemaps, RendererUI ui)
    {
        this.tilemaps = tilemaps;
        this.ui = ui;
    }

    public void CreateElement(int index, (int x, int y) position)
    {
        var count = Count.ToString();
        var element = default(Element);
        var panel = new Panel(position) { IsRestricted = false, SizeMinimum = (3, 3) };
        switch (index)
        {
            case 1:
                element = new Button(position);
                break;
            case 2:
                element = new InputBox(position);
                break;
            case 3:
            {
                element = new Pages(position);
                panel.SizeMinimum = (8, 3);
                panel.SizeMaximum = (int.MaxValue, 3);
                break;
            }
            case 4:
            {
                element = new Panel(position);
                panel.SizeMinimum = (5, 5);
                break;
            }
            case 5:
            {
                element = new Palette(position);
                panel.IsResizable = false;
                break;
            }
            case 6:
            {
                element = new Slider(position);
                panel.SizeMinimum = (4, 3);
                panel.SizeMaximum = (int.MaxValue, 3);
                break;
            }
            case 7:
            {
                element = new Scroll(position);
                panel.SizeMinimum = (3, 6);
                panel.SizeMaximum = (3, int.MaxValue);
                break;
            }
            case 8:
            {
                element = new List(position);
                break;
            }
        }

        if (element == null)
            return;

        panel.Size = (element.Size.width + 2, element.Size.height + 2);
        panel.Text = element.Text;

        ui[count] = element;
        this[count] = panel;
    }

    protected override void OnUpdatePanel(string key, Panel panel)
    {
        var p = panel;
        var e = ui[key];

        e.Position = (p.Position.x + 1, p.Position.y + 1);
        e.Size = (p.Size.width - 2, p.Size.height - 2);

        var offset = (p.Size.width - p.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (p.Position.x + offset, p.Position.y);
        const int corner = Tile.BORDER_GRID_CORNER;
        const int straight = Tile.BORDER_GRID_STRAIGHT;

        if (panel.IsHovered == false)
            return;

        tilemaps[3].SetBorder(p.Position, p.Size, corner, straight, Color.Cyan);
        tilemaps[3].SetRectangle(textPos, (p.Text.Length, 1), default);
        tilemaps[4].SetTextLine(textPos, p.Text, Color.White);
    }
}