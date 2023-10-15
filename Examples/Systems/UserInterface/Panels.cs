namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using static Utility;

public static class Panels
{
    public static Element[] Create(TilemapManager maps)
    {
        var nl = Environment.NewLine;
        var text = $"- Useful for containing other elements{nl}{nl}" +
                   $"- Title{nl}{nl}" +
                   $"- Can be optionally moved and/or resized{nl}{nl}" +
                   $"- Cannot be resized or moved outside the window{nl}{nl}" +
                   $"- Minimum sizes";
        var panelText = new Panel { Text = "Cool Title", Size = (19, 19), SizeMinimum = (4, 2) };
        panelText.Align((0.1f, 0.5f));
        panelText.OnDisplay(() =>
        {
            var (x, y) = panelText.Position;
            var (w, h) = panelText.Size;

            DisplayPanel(maps, panelText, zOrder: 0);
            maps[1].SetTextRectangle(
                position: (x + 1, y + 1),
                size: (w - 2, h - 2),
                text,
                tint: Color.Green);
        });

        //============

        var button = new Button { Text = "CLICK ME!" };
        var panelButton = new Panel { Size = (15, 9), SizeMinimum = (5, 5) };
        panelButton.Align((0.95f, 0.5f));
        panelButton.OnDisplay(() =>
        {
            var (x, y) = panelButton.Position;
            var (w, h) = panelButton.Size;

            DisplayPanel(maps, panelButton, zOrder: 2);
            button.Position = (x + 1, y + 1);
            button.Size = (w - 2, h - 2);
            ButtonsAndCheckboxes.DisplayButton(maps, button, zOrder: 2);
        });

        return new Element[] { panelText, panelButton, button };
    }
    public static void DisplayPanel(TilemapManager maps, Panel panel, int zOrder)
    {
        var e = panel;
        var (x, y) = e.Position;
        var (w, _) = e.Size;

        Clear(maps, panel, (zOrder, zOrder + 1));
        SetBackground(maps[zOrder], e, 0.6f);

        maps[zOrder + 1].SetBox(e.Position, e.Size, Tile.EMPTY, Tile.BOX_GRID_CORNER,
            Tile.BOX_GRID_STRAIGHT, Color.Blue);
        maps[zOrder + 1].SetTextLine(
            position: (x + w / 2 - e.Text.Length / 2, y),
            e.Text,
            maxLength: Math.Min(w, e.Text.Length));
    }
}