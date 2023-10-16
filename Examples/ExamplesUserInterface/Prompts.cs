namespace Pure.Examples.ExamplesUserInterface;

public static class Prompts
{
    public static Prompt Create(TilemapPack maps)
    {
        var input = new InputBox
        {
            Size = (16, 1),
            Value = "",
            Placeholder = "Message…",
        };
        input.OnDisplay(() => SetInputBox(maps, input, 4));
        var prompt = new Prompt();
        prompt.OnDisplay(buttons => SetPrompt(maps, prompt, buttons, 3));
        Keyboard.OnKeyPress(Keyboard.Key.Enter, asText =>
        {
            var shouldLog = prompt.Block == input && prompt.IsOpened;
            prompt.Close();

            if (shouldLog)
                Console.WriteLine(input.Value);
        });
        Keyboard.OnKeyPress(Keyboard.Key.ShiftLeft, asText =>
        {
            input.IsFocused = true;
            input.SelectAll();

            prompt.Block = input;
            prompt.Message = $"Log a message?";
            prompt.Open(2, index =>
            {
                if (index == 0)
                    Console.WriteLine(input.Value);
            });
        });
        Keyboard.OnKeyPress(Keyboard.Key.ControlLeft, asText =>
        {
            prompt.Block = null;
            prompt.Message = $"This should be some{Environment.NewLine}important message!";
            prompt.Open();
        });

        return prompt;
    }
}