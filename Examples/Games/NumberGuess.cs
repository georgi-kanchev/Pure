using Pure.Engine.Hardware;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Tools.UserInterface;

namespace Pure.Examples.Games;

public static class NumberGuess
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Number Guess Example" };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var layer = new LayerTiles((w * 3, h * 3));
		var log = "";
		var tries = 0;
		var randomNumber = GenerateRandomNumber();

		while (window.KeepOpen())
		{
			var input = InstantBlock.InputBox((20, 20, 5, 1), symbolGroup: SymbolGroup.Integers, symbolLimit: 4);
			if (input is { Length: 4 })
			{
				log = tries == 0 ? "" : log;
				tries++;

				var bulls = 0;
				var cows = 0;
				for (var i = 0; i < randomNumber.Length; i++)
					if (input[i] == randomNumber[i])
						bulls++;
					else if (randomNumber.Contains(input[i]))
						cows++;
				var bullsAndCows = $"{new('●', bulls)}{new('○', cows)}";

				var result = input == randomNumber ? $"✓✓✓✓ ({tries} tries)" : bullsAndCows;
				log = $"{log}\n{input} {result}";

				if (input == randomNumber)
				{
					tries = 0;
					randomNumber = GenerateRandomNumber();
				}
			}

			log.Text((20, 19 - log.Count("\n")));

			InstantBlock.UpdateAndDraw(layer);
		}

		string GenerateRandomNumber()
		{
			var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var randomNumberStr = "";

			for (var i = 0; i < 4; i++)
			{
				var n = numbers.ChooseOne();
				numbers.Remove(n);
				randomNumberStr += $"{n}";
			}

			return randomNumberStr;
		}
	}
}