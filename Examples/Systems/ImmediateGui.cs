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

        var textPrompt = false;
        var percent = "0%";
        var log = "";
        var tooltip = ("This is an\n" +
                       "example tooltip!\n" +
                       "A very long one!\n" +
                       "Oh my!\n" +
                       "It keeps going!").Constrain((16, 5), false, Alignment.Center);

        while (Window.KeepOpen())
        {
            var checkbox = ImGui.Checkbox((0, 5), "Checkbox");
            if (checkbox != null)
                log = $"checkbox:\n{checkbox}";

            if (ImGui.Button((0, 2), "Button"))
                log = "button:\nclicked";

            var input = ImGui.InputBox((0, 10, 10, 1), "Inputbox");
            if (input != null)
                log = $"inputbox:\n{input}";

            var palette = ImGui.Palette((20, 20));
            if (palette != null)
                log = $"palette:\n{new Color(palette ?? 0).ToBrush()}color";

            var fileViewer = ImGui.FileViewer((15, 4, 30, 10));
            if (fileViewer != null)
                log = $"fileViewer:\n{fileViewer.ToString(", ")}";

            var sliderHor = ImGui.Slider((0, 7), 9);
            ImGui.Text((9, 7), percent, 2);
            if (float.IsNaN(sliderHor) == false)
            {
                percent = $"{sliderHor * 100f:F0}%";
                log = $"horizontal slider:\n{percent}";
            }

            var sliderVer = ImGui.Slider((9, 14), 9, true);
            if (float.IsNaN(sliderVer) == false)
                log = $"vertical slider:\n{sliderVer:F1}";

            ImGui.Tooltip = (tooltip, Side.Left, 0.5f);
            var scrollVer = ImGui.Scroll((11, 14), 9, true);
            if (float.IsNaN(scrollVer) == false)
                log = $"vertical scroll:\n{scrollVer:F1}";
            ImGui.Tooltip = default;

            var scrollHer = ImGui.Scroll((0, 12), 11);
            if (float.IsNaN(scrollHer) == false)
                log = $"horizontal scroll:\n{scrollHer:F1}";

            var pages = ImGui.Pages((2, 24));
            if (float.IsNaN(pages) == false)
                log = $"pages: \n{pages:F1}";

            ImGui.Tooltip = (tooltip, Side.Left, 0.5f);
            var stepper = ImGui.Stepper((0, 14), "Stepper", 1f, 999f, -999f);
            if (float.IsNaN(stepper) == false)
                log = $"stepper:\n{stepper:F1}";
            ImGui.Tooltip = default;

            var animals = new[] { "Dog", "Cat", "Rabbit", "Fox", "Bear" };
            var dropdown = ImGui.List((0, 17, 6, 5), animals, true);
            if (dropdown != null)
                log = $"dropdown list:\n{dropdown.ToString(", ")}";

            var items = new[] { "Stick", "Rock", "Leaf", "Bush", "Flower" };
            var list = ImGui.List((40, 20, 6, 5), items);
            if (list != null)
                log = $"regular list:\n{list.ToString(", ")}";

            // = = = = = = = = = = = =
            // promts should be last
            // = = = = = = = = = = = =

            if (Keyboard.Key.ControlLeft.IsJustPressed())
            {
                ImGui.Prompt();
                textPrompt = true;
            }
            else if (Keyboard.Key.ShiftLeft.IsJustPressed())
            {
                ImGui.Prompt();
                textPrompt = false;
            }

            if (textPrompt)
            {
                var choice = ImGui.PromptChoice("Are you ready to pick a\n" +
                                                "choice from all the choices?", 3);
                if (float.IsNaN(choice) == false)
                    log = $"prompt choice:\n{(int)choice}";
            }
            else
            {
                ImGui.Tooltip = (tooltip, Side.Bottom, 0.5f);
                var prompt = ImGui.PromptInput("Type in stuff:");
                if (prompt != null)
                    log = $"prompt input:\n{prompt}";
                ImGui.Tooltip = default;
            }

            ImGui.Text((20, 0), "Left Control = Info Popup\n" +
                                "Left Shift = Input Popup");
            ImGui.Text((0, 0), log.Constrain(layer.Size, false, Alignment.Bottom));
            layer.DrawImGui();
        }
    }
}