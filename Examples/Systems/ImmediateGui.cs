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

        var percent = "0%";
        var log = "";
        while (Window.KeepOpen())
        {
            var checkbox = ImGui.ShowCheckbox((0, 5), "Checkbox");
            if (checkbox.Once("on"))
                log = "checkbox: on";
            if ((checkbox == false).Once("off"))
                log = "checkbox: off";

            if (ImGui.ShowButton((0, 0), "Button"))
                log = "button: clicked";

            var input = ImGui.ShowInputBox((0, 10, 10, 1), "Inputbox");
            if (input != "")
                log = $"inputbox: {input}";

            if (ImGui.ShowButton((0, 2), "Button2"))
                log = "button2: clicked";

            var sliderHor = ImGui.ShowSlider((0, 7), 9);
            ImGui.ShowText((9, 7), percent, 2);
            if (float.IsNaN(sliderHor) == false)
            {
                percent = $"{sliderHor * 100f:F0}%";
                log = $"horizontal slider: {percent}";
            }

            var sliderVer = ImGui.ShowSlider((14, 1), 9, true);
            if (float.IsNaN(sliderVer) == false)
                log = $"vertical slider: {sliderVer:F1}";

            var scrollVer = ImGui.ShowScroll((16, 1), 9, true);
            if (float.IsNaN(scrollVer) == false)
                log = $"vertical scroll: {scrollVer:F1}";

            var scrollHer = ImGui.ShowScroll((0, 12), 11);
            if (float.IsNaN(scrollHer) == false)
                log = $"horizontal scroll: {scrollHer:F1}";

            ImGui.ShowText((0, 0), log.Constrain(layer.Size, false, Alignment.Bottom));
            layer.DrawImGui();
        }
    }
}