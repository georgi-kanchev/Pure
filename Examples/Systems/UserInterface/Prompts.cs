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
        var inputBox = new InputBox { Size = (20, 1), Value = "" };
        var prompt = new Prompt
        {
            Element = inputBox,
            Message = $"Would you press{Environment.NewLine}on the green tick?"
        };

        inputBox.OnDisplay(() => InputBoxes.DisplayInputBox(maps, inputBox, 4));
        prompt.OnDisplay(buttons =>
        {
            if (prompt.IsOpened)
            {
                var tile = new Tile(Tile.SHADE_OPAQUE, new Color(0, 0, 0, 127));
                var bg = new Tile(Tile.SHADE_OPAQUE, Color.Gray.ToDark());
                maps[3].SetRectangle((0, 0), maps.Size, tile);
                maps[3].SetBox(prompt.Position, prompt.Size, bg, Tile.BOX_CORNER_ROUND,
                    Tile.SHADE_OPAQUE, Color.Gray.ToDark());
            }

            var messageSize = (prompt.Size.width, prompt.Size.height - 1);
            maps[6].SetTextRectangle(prompt.Position, messageSize, prompt.Message,
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
        });

        Keyboard.OnKeyPress(Keyboard.Key.ShiftLeft, asText =>
        {
            inputBox.IsFocused = true;
            inputBox.SelectAll();

            prompt.Open(2, index =>
            {
                if (index == 0)
                    Console.WriteLine(inputBox.Value);

                prompt.Close();
            });
        });

        return prompt;
    }
}