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
        input.OnDisplay(() => maps.SetInputBox(input, zOrder: 4));

        var prompt = new Prompt();
        prompt.OnDisplay(() => maps.SetPrompt(prompt, zOrder: 3));
        prompt.OnItemDisplay(item => maps.SetPromptItem(prompt, item, zOrder: 5));

        Keyboard.OnKeyPress(Keyboard.Key.Enter, asText =>
        {
            var shouldLog = input.IsHidden == false && prompt.IsHidden == false;
            prompt.Close();

            if (shouldLog)
                Console.WriteLine(input.Value);
        });
        Keyboard.OnKeyPress(Keyboard.Key.ShiftLeft, asText =>
        {
            input.IsFocused = true;
            input.IsHidden = false;

            prompt.Text = "Log a message?";
            prompt.ButtonCount = 2;
            prompt.Open(input, index =>
            {
                if (index == 0)
                    Console.WriteLine(input.Value);
            });
        });
        Keyboard.OnKeyPress(Keyboard.Key.ControlLeft, asText =>
        {
            input.IsHidden = true;

            prompt.Text = $"This should be some{Environment.NewLine}important message!";
            prompt.ButtonCount = 1;
            prompt.Open();
        });

        return prompt;
    }
}