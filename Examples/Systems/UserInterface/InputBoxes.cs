namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;
using Tilemap;
using static Utility;

public static class InputBoxes
{
    public static Element[] Create(TilemapManager maps)
    {
        var line = Environment.NewLine;
        var messages = $"Welcome to the chat! :){line}" +
                       $"Type a message &{line}" +
                       $"press <Enter> to send it.{line}{line}";
        var chat = new InputBox
        {
            Size = (20, 1),
            Value = "",
            Placeholder = "Chat message…",
            IsSingleLine = true,
        };
        chat.Align((0.1f, 0.95f));
        chat.OnSubmit(() =>
        {
            var clock = $"[{Time.Clock.ToClock()}]";
            messages += $"{clock}{line}{chat.Value}" +
                        $"{line}{line}";
            chat.Value = "";
        });
        chat.OnDisplay(() =>
        {
            var (x, y) = chat.Position;
            maps[0].SetTextRectangle(
                position: (x, y - 10),
                size: (chat.Size.width, 10),
                text: messages,
                alignment: Tilemap.Alignment.BottomLeft,
                scrollProgress: 1f);
            DisplayInputBox(maps, chat, 0);
        });

        // ==========================

        var multiLine = new InputBox
        {
            Size = (12, 12),
            Value = "",
            Placeholder = "Type some text on multiple lines…",
        };
        multiLine.Align((0.1f, 0.1f));
        multiLine.OnDisplay(() => DisplayInputBox(maps, multiLine, 0));

        // ==========================

        var pass = "<Enter> to submit";
        var password = new InputBox
        {
            Size = (17, 1),
            Value = "",
            Placeholder = "Password…",
            IsSingleLine = true,
        };
        password.SymbolSet |= SymbolSet.Password;
        password.Align((0.95f, 0.1f));
        password.OnSubmit(() => { pass = password.Value; });
        password.OnDisplay(() =>
        {
            var pos = password.Position;
            maps[0].SetTextLine((pos.x, pos.y - 1), pass);
            DisplayInputBox(maps, password, 0);
        });

        // ==========================

        var mathResult = "<Enter> to calculate";
        var equation = new InputBox
        {
            Size = (20, 1),
            Value = "",
            Placeholder = "Math equation…",
            SymbolSet = SymbolSet.Math | SymbolSet.Digits,
            IsSingleLine = true,
        };
        equation.Align((0.95f, 0.9f));
        equation.OnSubmit(() => mathResult = $"{equation.Value.Calculate()}");
        equation.OnDisplay(() =>
        {
            var pos = equation.Position;
            maps[0].SetTextLine((pos.x, pos.y - 1), mathResult);
            DisplayInputBox(maps, equation, 0);
        });

        return new Element[] { multiLine, password, chat, equation };
    }

    public static void DisplayInputBox(TilemapManager maps, InputBox inputBox, int zOrder)
    {
        var ib = inputBox;
        var bgColor = Color.Gray.ToDark(0.4f);
        var selectColor = ib.IsFocused ? Color.Blue : Color.Blue.ToBright();

        Clear(maps, inputBox, (zOrder, zOrder + 2));
        maps[zOrder].SetRectangle(ib.Position, ib.Size, new(Tile.SHADE_OPAQUE, bgColor));
        maps[zOrder].SetTextRectangle(ib.Position, ib.Size, ib.Selection, selectColor, false);
        maps[zOrder + 1].SetTextRectangle(ib.Position, ib.Size, ib.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(ib.Value))
            maps[zOrder + 1].SetTextRectangle(ib.Position, ib.Size, ib.Placeholder,
                tint: Color.Gray.ToBright(),
                alignment: Tilemap.Alignment.TopLeft);

        if (ib.IsCursorVisible)
            maps[zOrder + 2].SetTile(ib.PositionFromIndices(ib.CursorIndices),
                new(Tile.SHAPE_LINE, Color.White, 2));
    }
}