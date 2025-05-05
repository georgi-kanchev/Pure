using Pure.Engine.Hardware;

namespace Pure.Examples.UserInterface;

public static class Prompts
{
	public static Block[] Create(Window window, Hardware hardware, List<TileMap> maps)
	{
		window.Title = "Pure - Prompts Example";

		const Keyboard.Key HOTKEY_LOG = Keyboard.Key.ShiftLeft;
		const Keyboard.Key HOTKEY_MSG = Keyboard.Key.ControlLeft;

		var info = new Button { Position = (int.MaxValue, 0) };
		var text = $"Press <{HOTKEY_LOG}> to type and log\n" +
		           $"Press <{HOTKEY_MSG}> to show a message";
		info.OnDisplay += () => maps[0].SetText((0, 0), text);

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

		hardware.Keyboard.OnPress(HOTKEY_LOG, () =>
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
		hardware.Keyboard.OnPress(HOTKEY_MSG, () =>
		{
			input.IsHidden = true;

			prompt.Text = "This should be some\nimportant message!";
			prompt.Open(btnCount: 1);
		});

		return [info, prompt];
	}
}