namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using static Tilemap.Tile;
using static Utility;

public static class ButtonsAndCheckboxes
{
    public static Element[] Create(TilemapManager maps)
    {
        var buttonSelect = new Button { Text = "Button Select" };
        buttonSelect.Size = (buttonSelect.Text.Length + 4, 1);
        buttonSelect.Align((0.5f, 0.2f));
        buttonSelect.OnDisplay(() => DisplayButtonSelect(maps, buttonSelect, zOrder: 0));

        // ==============

        var counter = 0;
        var button = new Button { Text = "Cool Button" };
        button.Size = (button.Text.Length + 2, 3);
        button.Align((0.5f, 0.4f));
        button.OnInteraction(Interaction.Trigger, () => counter++);
        button.OnDisplay(() =>
        {
            DisplayButton(maps, button, zOrder: 0);
            maps[1].SetTextLine((0, 0), $"The {button.Text} was pressed {counter} times.");
        });

        // ==============

        var checkbox = new Button { Text = "Checkbox" };
        checkbox.Size = (checkbox.Text.Length + 2, 1);
        checkbox.Align((0.5f, 0.6f));
        checkbox.OnDisplay(() => DisplayCheckbox(maps, checkbox, 0));

        // ==============

        var buttonDisabled = new Button { IsDisabled = true, Text = "Disabled Button" };
        buttonDisabled.Size = (buttonDisabled.Text.Length, 1);
        buttonDisabled.Align((0.5f, 0.8f));
        buttonDisabled.OnDisplay(() =>
        {
            maps[0].SetTextLine(buttonDisabled.Position, buttonDisabled.Text,
                tint: Color.Gray.ToDark(0.7f));
        });

        return new Element[] { buttonSelect, button, checkbox, buttonDisabled };
    }
    public static void DisplayCheckbox(TilemapManager maps, Button checkbox, int zOrder)
    {
        var color = checkbox.IsSelected ? Color.Green : Color.Red;
        var tileId = checkbox.IsSelected ? ICON_TICK : UPPERCASE_X;
        var tile = new Tile(tileId, GetColor(checkbox, color));

        maps[zOrder].SetTile(checkbox.Position, tile);

        maps[zOrder].SetTextLine(
            position: (checkbox.Position.x + 2, checkbox.Position.y),
            text: checkbox.Text,
            tint: GetColor(checkbox, color));
    }
    public static void DisplayButton(
        TilemapManager maps,
        Button button,
        int zOrder,
        bool isDisplayingSelection = false)
    {
        var b = button;

        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var c = b.IsSelected && isDisplayingSelection ? Color.Green : Color.Yellow;
        var color = GetColor(b, c.ToDark());
        var colorBack = Color.Gray.ToDark(0.6f);

        maps[zOrder].SetBox(b.Position, b.Size,
            tileFill: new(SHADE_OPAQUE, colorBack),
            cornerTileId: BOX_CORNER_ROUND,
            borderTileId: SHADE_OPAQUE,
            borderTint: colorBack);

        maps[zOrder + 1].SetBox(b.Position, b.Size,
            tileFill: EMPTY,
            cornerTileId: BOX_DEFAULT_CORNER,
            borderTileId: BOX_DEFAULT_STRAIGHT,
            borderTint: color);

        maps[zOrder + 2].SetTextLine(
            position: (b.Position.x + offsetW, b.Position.y + h / 2),
            text: b.Text,
            tint: color,
            maxLength: w - 2);
    }
    public static void DisplayButtonSelect(TilemapManager maps, Button button, int zOrder)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var selColor = b.IsSelected ? Color.Green : Color.Gray;

        maps[zOrder].SetBar(b.Position,
            tileIdEdge: BAR_BIG_EDGE,
            tileId: SHADE_OPAQUE,
            tint: GetColor(b, Color.Brown.ToDark(0.3f)),
            size: w);
        maps[zOrder + 1].SetTextLine(
            position: (b.Position.x + offsetW, b.Position.y + h / 2),
            text: b.Text,
            tint: GetColor(b, selColor),
            maxLength: w - 2);
    }
}