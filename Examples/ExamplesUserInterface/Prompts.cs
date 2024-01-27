namespace Pure.Examples.ExamplesUserInterface;

public static class Prompts
{
    public static Block[] Create(TilemapPack maps)
    {
        const Key HOTKEY_LOG = Key.ShiftLeft;
        const Key HOTKEY_MSG = Key.ControlLeft;

        var info = new Button { Position = (int.MaxValue, 0) };
        info.OnDisplay(() => maps[0].SetTextRectangle(
            position: (0, 0),
            size: (maps.Size.width, 2),
            text:
            $"Press <{HOTKEY_LOG}> to type and log{Environment.NewLine}" +
            $"Press <{HOTKEY_MSG}> to show a message"));

        var input = new InputBox
        {
            Size = (16, 1),
            Value = "",
            Placeholder = "Message…",
        };
        input.OnDisplay(() => maps.SetInputBox(input, zOrder: 4));

        var prompt = new Prompt();
        prompt.OnDisplay(() => maps.SetPrompt(prompt, zOrder: 3));
        prompt.OnItemDisplay(item => maps.SetPromptItem(prompt, item, zOrder: 5));

        Key.Enter.OnPress(() =>
        {
            var shouldLog = input.IsHidden == false && prompt.IsHidden == false;
            prompt.Close();

            if (shouldLog)
                Console.WriteLine(input.Value);
        });
        HOTKEY_LOG.OnPress(() =>
        {
            input.IsFocused = true;
            input.IsHidden = false;

            prompt.Text = "Log a message?";
            prompt.Open(input, onButtonTrigger: index =>
            {
                if (index == 0)
                    Console.WriteLine(input.Value);
            });
        });
        HOTKEY_MSG.OnPress(() =>
        {
            input.IsHidden = true;

            prompt.Text = $"This should be some{Environment.NewLine}important message!";
            prompt.Open(onButtonTrigger: _ => prompt.Close());
        });

        return new Block[] { info, prompt };
    }
}