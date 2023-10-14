namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using static Utility;

public static class Panels
{
    public static Element[] Create(TilemapManager maps)
    {
        var panelText = new Panel { Size = (15, 15) };
        panelText.Align((0.1f, 0.5f));
        panelText.OnDisplay(() => DisplayPanel(maps, panelText));

        //============

        var panelButton = new Panel { Size = (15, 15) };
        panelButton.Align((0.9f, 0.5f));
        panelButton.OnDisplay(() => DisplayPanel(maps, panelButton));

        return new Element[] { panelText, panelButton };
    }
    public static void DisplayPanel(TilemapManager maps, Panel panel)
    {
        var e = panel;
        SetBackground(maps[0], e, 0.6f);

        maps[1].SetRectangle(e.Position, e.Size, Tile.EMPTY);
        maps[2].SetRectangle(e.Position, e.Size, Tile.EMPTY);

        maps[2].SetBox(e.Position, e.Size, Tile.EMPTY, Tile.BOX_GRID_CORNER,
            Tile.BOX_GRID_STRAIGHT, Color.Blue);
        maps[2].SetTextRectangle((e.Position.x, e.Position.y), (e.Size.width, 1), e.Text,
            alignment: Tilemap.Alignment.Center);
    }
}