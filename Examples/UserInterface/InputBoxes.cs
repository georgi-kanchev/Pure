namespace Pure.Examples.UserInterface;

public static class InputBoxes
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Input Boxes Example";

        var messages = $"{Color.Azure.ToBrush()}Welcome to the chat! :)\n" +
                       $"Type a message & press <Enter> to send it.\n";
        var chat = new InputBox
        {
            Size = (20, 1),
            Value = string.Empty,
            Placeholder = "Chat message…"
        };
        var scroll = new Scroll { IsHidden = true, IsDisabled = true };
        chat.AlignInside((0.1f, 0.95f));
        var (x, y, w, h) = (chat.Position.x, chat.Position.y - 10, chat.Size.width, 10);
        chat.OnInteraction(Interaction.Select, () =>
        {
            var clock = $"{Color.Gray.ToBrush()}[{Time.Clock.ToClock()}]";
            messages += $"{clock}\n{Color.White.ToBrush()}{chat.Value}\n";
            var lines = messages.Replace("\r", "").Split("\n").Length;
            chat.Value = string.Empty;
            scroll.Slider.Progress = 1f;
            scroll.IsHidden = lines < h;
            scroll.IsDisabled = scroll.IsHidden;
            scroll.Step = 1f / lines;
        });
        chat.OnDisplay(() =>
        {
            var text = messages.Constrain((w, h), alignment: Alignment.BottomRight,
                scrollProgress: scroll.Slider.Progress);
            maps.Tilemaps[0].SetText((x, y), text);
            maps.SetInputBox(chat);
        });
        scroll.OnDisplay(() => maps.SetScroll(scroll));
        scroll.AlignEdges(Side.Left, Side.Right, chat, 1f, 2);

        // ==========================

        var multiLine = new InputBox
        {
            Size = (12, 12),
            Value = string.Empty,
            Placeholder = "Type some text on multiple lines…"
        };
        multiLine.AlignInside((0.1f, 0.1f));
        multiLine.OnDisplay(() => maps.SetInputBox(multiLine));

        // ==========================

        var pass = "<Enter> to submit";
        var password = new InputBox
        {
            Size = (17, 1),
            Value = string.Empty,
            Placeholder = "Password…",
            SymbolMask = "#"
        };
        password.AlignInside((0.95f, 0.1f));
        password.OnInteraction(Interaction.Select, () => pass = password.Value);
        password.OnDisplay(() =>
        {
            var pos = password.Position;
            maps.Tilemaps[0].SetText((pos.x, pos.y - 1), pass);
            maps.SetInputBox(password);
        });

        // ==========================

        var mathResult = "<Enter> to calculate";
        var equation = new InputBox
        {
            Size = (20, 1),
            Value = string.Empty,
            Placeholder = "Math equation…",
            SymbolGroup = SymbolGroup.Math | SymbolGroup.Decimals
        };
        equation.AlignInside((0.95f, 0.9f));
        equation.OnInteraction(Interaction.Select, () => mathResult = $"{equation.Value.Calculate()}");
        equation.OnDisplay(() =>
        {
            var pos = equation.Position;
            maps.Tilemaps[0].SetText((pos.x, pos.y - 1), mathResult);
            maps.SetInputBox(equation);
        });

        return [multiLine, password, chat, scroll, equation];
    }
}