namespace Pure.Examples.ExamplesUserInterface;

public static class InputBoxes
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Input Boxes Example";

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
        chat.AlignInside((0.1f, 0.95f));
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
                alignment: Alignment.BottomLeft,
                scrollProgress: 1f);
            maps.SetInputBox(chat);
        });

        // ==========================

        var multiLine = new InputBox
        {
            Size = (12, 12),
            Value = "",
            Placeholder = "Type some text on multiple lines…",
        };
        multiLine.AlignInside((0.1f, 0.1f));
        multiLine.OnDisplay(() => maps.SetInputBox(multiLine));

        // ==========================

        var pass = "<Enter> to submit";
        var password = new InputBox
        {
            Size = (17, 1),
            Value = "",
            Placeholder = "Password…",
            IsSingleLine = true,
            Mask = "#"
        };
        password.AlignInside((0.95f, 0.1f));
        password.OnSubmit(() => pass = password.Value);
        password.OnDisplay(() =>
        {
            var pos = password.Position;
            maps[0].SetTextLine((pos.x, pos.y - 1), pass);
            maps.SetInputBox(password);
        });

        // ==========================

        var mathResult = "<Enter> to calculate";
        var equation = new InputBox
        {
            Size = (20, 1),
            Value = "",
            Placeholder = "Math equation…",
            SymbolGroup = SymbolGroup.Math | SymbolGroup.Digits,
            IsSingleLine = true,
        };
        equation.AlignInside((0.95f, 0.9f));
        equation.OnSubmit(() => mathResult = $"{equation.Value.Calculate()}");
        equation.OnDisplay(() =>
        {
            var pos = equation.Position;
            maps[0].SetTextLine((pos.x, pos.y - 1), mathResult);
            maps.SetInputBox(equation);
        });

        Key.ControlLeft.OnPress(() => multiLine.Mask = "*");
        Key.ControlRight.OnPress(() => multiLine.Mask = null);

        return new Block[] { multiLine, password, chat, equation };
    }
}