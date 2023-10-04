namespace Pure.Examples.Systems.UserInterface;

using Tilemap;
using Pure.UserInterface;
using Utilities;
using Window;
using static Utility;
using static Tilemap.Tile;

public static class ButtonsAndCheckboxes
{
    public static void Run()
    {
        Window.Create(3);
        var (width, height) = Window.MonitorAspectRatio;
        var tilemaps = new TilemapManager(2, (width * 3, height * 3));
        var ui = new UserInterface();

        var button = new Button((12, 5)) { Text = "Cool Button" };
        var buttonDisabled = new Button((12, 15)) { IsDisabled = true, Text = "Disabled Button" };
        var checkbox = new Button((12, 10)) { Text = "Checkbox" };
        var counter = 0;

        button.OnUserAction(UserAction.Trigger, () => counter++);

        buttonDisabled.OnDisplay(() =>
        {
            tilemaps[0].SetTextLine(buttonDisabled.Position, buttonDisabled.Text,
                tint: Color.Gray.ToDark(0.7f));
        });
        button.OnDisplay(() =>
        {
            var b = button;
            b.Size = (b.Text.Length + 2, 3);

            var (w, h) = b.Size;
            var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
            var color = GetColor(b, Color.Yellow.ToDark());
            var colorBack = Color.Gray.ToDark(0.6f);

            tilemaps[0].SetBox(b.Position, b.Size,
                tileFill: new(SHADE_OPAQUE, colorBack),
                cornerTileId: BOX_CORNER_ROUND,
                borderTileId: SHADE_OPAQUE,
                borderTint: colorBack);

            tilemaps[1].SetBox(b.Position, b.Size,
                tileFill: EMPTY,
                cornerTileId: BOX_DEFAULT_CORNER,
                borderTileId: BOX_DEFAULT_STRAIGHT,
                borderTint: color);

            tilemaps[1].SetTextLine(
                position: (b.Position.x + offsetW, b.Position.y + h / 2),
                text: b.Text,
                tint: color,
                maxLength: w - 2);

            tilemaps[1].SetTextLine((0, 0), $"The {button.Text} was pressed {counter} times.");
        });
        checkbox.OnDisplay(() =>
        {
            checkbox.Size = (checkbox.Text.Length + 2, 1);

            var color = checkbox.IsSelected ? Color.Green : Color.Red;
            var tileId = checkbox.IsSelected ? ICON_TICK : UPPERCASE_X;
            var tile = new Tile(tileId, GetColor(checkbox, color));

            tilemaps[0].SetTile(checkbox.Position, tile);

            tilemaps[0].SetTextLine(
                position: (checkbox.Position.x + 2, checkbox.Position.y),
                text: checkbox.Text,
                tint: GetColor(checkbox, color));
        });

        ui.Add(button);
        ui.Add(buttonDisabled);
        ui.Add(checkbox);

        RunWindow(ui, tilemaps);
    }
}