namespace Pure.Examples.UserInterface;

public static class Prompts
{
    public static Block[] Create(TileMapPack maps)
    {
        Window.Title = "Pure - Prompts Example";

        const Key HOTKEY_LOG = Key.ShiftLeft;
        const Key HOTKEY_MSG = Key.ControlLeft;

        var info = new Button { Position = (int.MaxValue, 0) };
        var text = $"Press <{HOTKEY_LOG}> to type and log\n" +
                   $"Press <{HOTKEY_MSG}> to show a message";
        info.OnDisplay += () => maps.TileMaps[0].SetText((0, 0), text);

        var input = new InputBox
        {
            Size = (16, 1),
            Value = string.Empty,
            Placeholder = "Message…"
        };
        input.OnDisplay += () => maps.SetInputBox(input, 4);

        var prompt = new Prompt();
        prompt.OnDisplay += () => maps.SetPrompt(prompt, 3);
        prompt.OnItemDisplay += item => maps.SetPromptItem(prompt, item, 5);

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

            prompt.Text = $"This should be some\nimportant message!";
            prompt.Open(btnCount: 1);
        });

        return [info, prompt];
    }
}