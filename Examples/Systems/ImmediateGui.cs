using Pure.Engine.UserInterface;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Pure.Tools.ImmediateGraphicalUserInterface;
using Monitor = Pure.Engine.Window.Monitor;

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
        const int TARGET_TOOLTIP_WIDTH = 16;

        ImGui.Tooltip = (Side.Left, 0.5f);

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
            if (input != null)
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

            var scrollVer = ImGui.ShowScroll((20, 1), 9, true,
                "This is an".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "example tooltip!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "Oh my!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "Keeps going!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "A very long one!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH));
            if (float.IsNaN(scrollVer) == false)
                log = $"vertical scroll: {scrollVer:F1}";

            var scrollHer = ImGui.ShowScroll((0, 12), 11);
            if (float.IsNaN(scrollHer) == false)
                log = $"horizontal scroll: {scrollHer:F1}";

            var pages = ImGui.ShowPages((25, 14));
            if (float.IsNaN(pages) == false)
                log = $"pages: {pages:F1}";

            var stepper = ImGui.ShowStepper((15, 14), "Stepper", 1f, 999f, -999f,
                "This is an".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "example tooltip!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "Oh my!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "Keeps going!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH) +
                "\n" +
                "A very long one!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH));
            if (float.IsNaN(stepper) == false)
                log = $"stepper: {stepper:F1}";

            var animals = new[] { "Dog", "Cat", "Rabbit", "Fox", "Bear" };
            var dropdown = ImGui.ShowList((0, 18, 6, 5), animals, true);
            if (dropdown != null)
                log = $"dropdown list: {dropdown.ToString(", ")}";

            var items = new[] { "Stick", "Rock", "Leaf", "Bush", "Flower" };
            var list = ImGui.ShowList((10, 18, 6, 5), items);
            if (list != null)
                log = $"regular list: {list.ToString(", ")}";

            ImGui.ShowText((0, 0), log.Constrain(layer.Size, false, Alignment.Bottom));
            layer.DrawImGui();
        }
    }
}