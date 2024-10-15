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
        var tooltip = $"{"This is an".PadLeftAndRight(TARGET_TOOLTIP_WIDTH)}\n" +
                      $"{"example tooltip!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH)}\n" +
                      $"{"Oh my!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH)}\n" +
                      $"{"It keeps going!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH)}\n" +
                      "A very long one!".PadLeftAndRight(TARGET_TOOLTIP_WIDTH);

        while (Window.KeepOpen())
        {
            // var checkbox = ImGui.Checkbox((0, 5), "Checkbox");
            // if (checkbox.Once("on"))
            //     log = "checkbox: on";
            // if ((checkbox == false).Once("off"))
            //     log = "checkbox: off";
            //
            // if (ImGui.Button((0, 0), "Button"))
            //     log = "button: clicked";

            var input = ImGui.InputBox((0, 10, 10, 1), "Inputbox");
            if (input != null)
                log = $"inputbox: {input}";

            if (Keyboard.Key.ControlLeft.IsJustPressed())
                ImGui.Prompt();

            var choice = ImGui.PromptChoice("Are you ready to pick a\n" +
                                            "choice from all the choices?", 1);
            if (float.IsNaN(choice) == false)
                log = $"prompt choice: {(int)choice}";

            // ImGui.Tooltip = (tooltip, Side.Bottom, 0.5f);
            // var prompt = ImGui.PromptInput("Type in stuff:");
            // if (prompt != null)
            //     log = $"prompt input: {prompt}";
            // ImGui.Tooltip = default;

            if (ImGui.Button((0, 2), "Button2"))
                log = "button2: clicked";

            // var sliderHor = ImGui.Slider((0, 7), 9);
            // ImGui.Text((9, 7), percent, 2);
            // if (float.IsNaN(sliderHor) == false)
            // {
            //     percent = $"{sliderHor * 100f:F0}%";
            //     log = $"horizontal slider: {percent}";
            // }
            //
            // var sliderVer = ImGui.Slider((14, 1), 9, true);
            // if (float.IsNaN(sliderVer) == false)
            //     log = $"vertical slider: {sliderVer:F1}";
            //
            // ImGui.Tooltip = (tooltip, Side.Left, 0.5f);
            // var scrollVer = ImGui.Scroll((20, 1), 9, true);
            // if (float.IsNaN(scrollVer) == false)
            //     log = $"vertical scroll: {scrollVer:F1}";
            // ImGui.Tooltip = default;
            //
            // var scrollHer = ImGui.Scroll((0, 12), 11);
            // if (float.IsNaN(scrollHer) == false)
            //     log = $"horizontal scroll: {scrollHer:F1}";
            //
            // var pages = ImGui.Pages((25, 14));
            // if (float.IsNaN(pages) == false)
            //     log = $"pages: {pages:F1}";
            //
            // ImGui.Tooltip = (tooltip, Side.Left, 0.5f);
            // var stepper = ImGui.Stepper((15, 14), "Stepper", 1f, 999f, -999f);
            // if (float.IsNaN(stepper) == false)
            //     log = $"stepper: {stepper:F1}";
            // ImGui.Tooltip = default;
            //
            // var animals = new[] { "Dog", "Cat", "Rabbit", "Fox", "Bear" };
            // var dropdown = ImGui.List((0, 18, 6, 5), animals, true);
            // if (dropdown != null)
            //     log = $"dropdown list: {dropdown.ToString(", ")}";
            //
            // var items = new[] { "Stick", "Rock", "Leaf", "Bush", "Flower" };
            // var list = ImGui.List((10, 18, 6, 5), items);
            // if (list != null)
            //     log = $"regular list: {list.ToString(", ")}";
            //
            ImGui.Text((0, 0), log.Constrain(layer.Size, false, Alignment.Bottom));
            layer.DrawImGui();
        }
    }
}