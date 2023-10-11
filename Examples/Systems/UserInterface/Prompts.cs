namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using Window;
using static Utility;

public static class Prompts
{
    public static Prompt Create(TilemapManager maps)
    {
        var input = new InputBox
        {
            Size = (16, 1),
            Value = "",
            Placeholder = "Message…",
        };
        input.OnDisplay(() => InputBoxes.DisplayInputBox(maps, input, 4));
        var prompt = new Prompt();
        prompt.OnDisplay(buttons => DisplayPrompt(maps, prompt, buttons));
        Keyboard.OnKeyPress(Keyboard.Key.Enter, asText =>
        {
            var shouldLog = prompt.Element == input && prompt.IsOpened;
            prompt.Close();

            if (shouldLog)
                Console.WriteLine(input.Value);
        });
        Keyboard.OnKeyPress(Keyboard.Key.ShiftLeft, asText =>
        {
            input.IsFocused = true;
            input.SelectAll();

            prompt.Element = input;
            prompt.Message = $"Log a message?";
            prompt.Open(2, index =>
            {
                if (index == 0)
                    Console.WriteLine(input.Value);
            });
        });
        Keyboard.OnKeyPress(Keyboard.Key.ControlLeft, asText =>
        {
            prompt.Element = null;
            prompt.Message = $"This should be some{Environment.NewLine}important message!";
            prompt.Open();
        });

        return prompt;
    }
    private static void DisplayPrompt(TilemapManager maps, Prompt promptInput, Button[] buttons)
    {
        if (promptInput.IsOpened)
        {
            var tile = new Tile(Tile.SHADE_OPAQUE, new Color(0, 0, 0, 127));
            maps[3].SetRectangle((0, 0), maps.Size, tile);
            maps[3].SetBox(promptInput.Position, promptInput.Size,
                tileFill: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()),
                cornerTileId: Tile.BOX_CORNER_ROUND,
                borderTileId: Tile.SHADE_OPAQUE,
                borderTint: Color.Gray.ToDark());
        }

        var messageSize = (promptInput.Size.width, promptInput.Size.height - 1);
        maps[6].SetTextRectangle(promptInput.Position, messageSize, promptInput.Message,
            alignment: Tilemap.Alignment.Center);

        for (var i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            Clear(maps, btn);

            var tile = new Tile(Tile.ICON_TICK, GetColor(btn, Color.Green));
            var bg = new Tile(Tile.SHADE_OPAQUE, Color.Gray.ToDark());
            if (i == 1)
            {
                tile.Id = Tile.ICON_CANCEL;
                tile.Tint = GetColor(btn, Color.Red);
            }

            maps[3].SetTile(btn.Position, bg);
            maps[6].SetTile(btn.Position, tile);
        }
    }
}