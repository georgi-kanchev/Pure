using Pure.Engine.UserInterface;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Pure.Tools.ImmediateGraphicalUserInterface;

namespace Pure.Examples.Games;

public static class EquationGuess
{
    public static void Run()
    {
        Window.Title = "Pure - Equation Guess Example";
        var page = new Layer((48, 27));
        var equation = "";

        while (Window.KeepOpen())
        {
            var input = ImGui.InputBox((10, 10, 10, 1), symbolGroup: SymbolGroup.Integers | SymbolGroup.Math, symbolLimit: 8);
            if (input != null)
                equation = input;

            if (ImGui.Button((21, 10, 10, 3), "Submit"))
                Console.WriteLine(equation.Calculate());

            page.DrawImGui();
        }
    }
}