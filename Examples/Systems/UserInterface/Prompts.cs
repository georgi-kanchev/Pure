namespace Pure.Examples.Systems.UserInterface;

using Utilities;
using Window;
using Tilemap;
using Pure.UserInterface;
using static Utility;

public static class Prompts
{
    public static void Run()
    {
        Window.Create(3);

        var (width, height) = Window.MonitorAspectRatio;
        var tilemaps = new TilemapManager(3, (width * 3, height * 3));
        var inputBox = new InputBox { Size = (16, 1) };
        var prompt = new Prompt { Message = "Your message:", Element = inputBox };
        var ui = new UserInterface { Prompt = prompt };
        var button = new Button();
        var pressCount = 0;

        ui.Add(button);
        button.OnDisplay(() =>
        {
            button.Text = $"pressed {pressCount} times";
            button.Size = (button.Text.Length, 1);
            tilemaps[0].SetTextLine(button.Position, button.Text, GetColor(button, Color.Blue));
        });
        button.OnUserAction(UserAction.Trigger, () => pressCount++);

        inputBox.OnDisplay(() =>
        {
            var e = inputBox;
            var bgColor = Color.Gray.ToDark();

            Clear(tilemaps, inputBox);
            tilemaps[0].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, bgColor));
            tilemaps[0].SetTextRectangle(e.Position, e.Size, e.Selection,
                e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
            tilemaps[1].SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

            if (string.IsNullOrWhiteSpace(e.Value))
                tilemaps[1].SetTextRectangle(e.Position, e.Size, e.Placeholder,
                    tint: Color.Gray.ToBright(),
                    isWordWrapping: false,
                    alignment: Tilemap.Alignment.TopLeft);

            if (e.IsCursorVisible)
                tilemaps[2].SetTile(e.PositionFromIndices(e.CursorIndices),
                    new(Tile.SHAPE_LINE, Color.White, 2));
        });
        prompt.OnDisplay(buttons =>
        {
            if (prompt.IsOpened)
            {
                var tile = new Tile(Tile.SHADE_OPAQUE, new Color(0, 0, 0, 127));
                tilemaps[2].SetRectangle((0, 0), tilemaps.Size, tile);
            }

            var messageSize = (prompt.Size.width, prompt.Size.height - 2);
            tilemaps[2].SetTextRectangle(prompt.Position, messageSize, prompt.Message,
                alignment: Tilemap.Alignment.Center);

            for (var i = 0; i < buttons.Length; i++)
            {
                var btn = buttons[i];
                Clear(tilemaps, btn);

                var tile = new Tile(Tile.ICON_TICK, GetColor(btn, Color.Green));
                if (i == 1)
                {
                    tile.Id = Tile.ICON_CANCEL;
                    tile.Tint = GetColor(btn, Color.Red);
                }

                tilemaps[2].SetTile(btn.Position, tile);
            }
        });

        Keyboard.OnKeyPressed(Keyboard.Key.ShiftLeft, asText =>
        {
            inputBox.IsFocused = true;
            inputBox.SelectAll();

            prompt.Open(2, index =>
            {
                if (index == 0)
                    Console.WriteLine(inputBox.Value);
                else if (index == 2)
                    Window.Close();
                prompt.Close();
            });
        });

        RunWindow(ui, tilemaps);
    }
}