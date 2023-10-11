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
        var counter = 0;
        var button = new Button { Text = "Cool Button" };
        button.Size = (button.Text.Length + 2, 3);
        button.Align((0.5f, 0.3f));
        button.OnInteraction(Interaction.Trigger, () => counter++);
        button.OnDisplay(() =>
        {
            var b = button;

            var (w, h) = b.Size;
            var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
            var color = GetColor(b, Color.Yellow.ToDark());
            var colorBack = Color.Gray.ToDark(0.6f);

            maps[0].SetBox(b.Position, b.Size,
                tileFill: new(SHADE_OPAQUE, colorBack),
                cornerTileId: BOX_CORNER_ROUND,
                borderTileId: SHADE_OPAQUE,
                borderTint: colorBack);

            maps[1].SetBox(b.Position, b.Size,
                tileFill: EMPTY,
                cornerTileId: BOX_DEFAULT_CORNER,
                borderTileId: BOX_DEFAULT_STRAIGHT,
                borderTint: color);

            maps[1].SetTextLine(
                position: (b.Position.x + offsetW, b.Position.y + h / 2),
                text: b.Text,
                tint: color,
                maxLength: w - 2);

            maps[1].SetTextLine((0, 0), $"The {button.Text} was pressed {counter} times.");
        });

        // ==============

        var checkbox = new Button { Text = "Checkbox" };
        checkbox.Size = (checkbox.Text.Length + 2, 1);
        checkbox.Align((0.5f, 0.5f));
        checkbox.OnDisplay(() =>
        {
            var color = checkbox.IsSelected ? Color.Green : Color.Red;
            var tileId = checkbox.IsSelected ? ICON_TICK : UPPERCASE_X;
            var tile = new Tile(tileId, GetColor(checkbox, color));

            maps[0].SetTile(checkbox.Position, tile);

            maps[0].SetTextLine(
                position: (checkbox.Position.x + 2, checkbox.Position.y),
                text: checkbox.Text,
                tint: GetColor(checkbox, color));
        });

        // ==============

        var buttonDisabled = new Button { IsDisabled = true, Text = "Disabled Button" };
        buttonDisabled.Size = (buttonDisabled.Text.Length, 1);
        buttonDisabled.Align((0.5f, 0.8f));
        buttonDisabled.OnDisplay(() =>
        {
            maps[0].SetTextLine(buttonDisabled.Position, buttonDisabled.Text,
                tint: Color.Gray.ToDark(0.7f));
        });

        // ==============

        return new Element[] { button, buttonDisabled, checkbox };
    }
}