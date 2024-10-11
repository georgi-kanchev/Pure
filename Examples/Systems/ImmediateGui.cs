using Pure.Engine.UserInterface;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Pure.Tools.ImmediateGraphicalUserInterface;
using Monitor = Pure.Engine.Window.Monitor;
using ImGui = Pure.Tools.ImmediateGraphicalUserInterface.ImmediateGraphicalUserInterface;

namespace Pure.Examples.Systems;

public static class ImmediateGui
{
    public static void Run()
    {
        Window.Title = "Pure - Immediate Graphical User Interface Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var layer = new Layer((w * 3, h * 3));

        while (Window.KeepOpen())
        {
            if (ImGui.ShowButton((0, 0), "Button"))
                Console.WriteLine("button: clicked");

            var input = ImGui.ShowInputBox((10, 10, 10, 1), "Inputbox");
            if (input != "")
                Console.WriteLine($"inputbox: {input}");

            var checkbox = ImGui.ShowCheckbox((0, 5), "Checkbox");
            if (checkbox.Once("on"))
                Console.WriteLine("checkbox: on");
            if ((checkbox == false).Once("off"))
                Console.WriteLine("checkbox: off");

            layer.DrawImGui();
        }
    }
}