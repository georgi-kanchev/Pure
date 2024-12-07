using Pure.Engine.Execution;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
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
        var tooltip = ("This is an\n" +
                       "example tooltip!\n" +
                       "A very long one!\n" +
                       "Oh my!\n" +
                       "It keeps going!").Constrain((16, 5), false, Alignment.Center);

        var promptStates = new StateMachine();
        promptStates.Add(null, PromptChoice, PromptInput, PromptInfo);

        while (Window.KeepOpen())
        {
            var checkbox = GUI.Checkbox((0, 5), "Checkbox");
            if (checkbox != null)
                log = $"checkbox:\n{checkbox}";

            if (GUI.Button((0, 1, 8, 3), "Button"))
                log = "button:\nclicked";

            var input = GUI.InputBox((0, 10, 10, 1), "Inputbox");
            if (input != null)
                log = $"inputbox:\n{input}";

            var palette = GUI.Palette((20, 20));
            if (palette != null)
                log = $"palette:\n{new Color(palette ?? 0).ToBrush()}color";

            var fileViewer = GUI.FileViewer((15, 4, 30, 10));
            if (fileViewer != null)
                log = $"fileViewer:\n{fileViewer.ToString(", ")}";

            var sliderHor = GUI.Slider((0, 7), 9);
            percent.Text((9, 7), 2);
            if (float.IsNaN(sliderHor) == false)
            {
                percent = $"{sliderHor * 100f:F0}%";
                log = $"horizontal slider:\n{percent}";
            }

            var sliderVer = GUI.Slider((9, 14), 9, vertical: true);
            if (float.IsNaN(sliderVer) == false)
                log = $"vertical slider:\n{sliderVer:F1}";

            GUI.Tooltip = (tooltip, Side.Left, 0.5f);
            var scrollVer = GUI.Scroll((11, 14), 9, vertical: true);
            if (float.IsNaN(scrollVer) == false)
                log = $"vertical scroll:\n{scrollVer:F1}";
            GUI.Tooltip = default;

            var scrollHer = GUI.Scroll((0, 12), 11);
            if (float.IsNaN(scrollHer) == false)
                log = $"horizontal scroll:\n{scrollHer:F1}";

            var pages = GUI.Pages((2, 24));
            if (float.IsNaN(pages) == false)
                log = $"pages: \n{pages:F1}";

            GUI.Tooltip = (tooltip, Side.Left, 0.5f);
            var stepper = GUI.Stepper((0, 14), "Stepper", 1f, 999f, -999f);
            if (float.IsNaN(stepper) == false)
                log = $"stepper:\n{stepper:F1}";
            GUI.Tooltip = default;

            var animals = new[] { "Dog", "Cat", "Rabbit", "Fox", "Bear" };
            var dropdown = GUI.List((0, 17, 6, 5), animals, true);
            if (dropdown != null)
                log = $"dropdown list:\n{dropdown.ToString(", ")}";

            var items = new[] { "Stick", "Rock", "Leaf", "Bush", "Flower" };
            var list = GUI.List((40, 20, 6, 5), items);
            if (list != null)
                log = $"regular list:\n{list.ToString(", ")}";

            // prompts should be last

            if (Keyboard.Key.ControlLeft.IsJustPressed())
            {
                GUI.Prompt();
                promptStates.GoTo(PromptChoice);
            }
            else if (Keyboard.Key.ShiftLeft.IsJustPressed())
            {
                GUI.Prompt();
                promptStates.GoTo(PromptInput);
            }
            else if (Keyboard.Key.AltLeft.IsJustPressed())
            {
                GUI.Prompt();
                promptStates.GoTo(PromptInfo);
            }

            promptStates.Update();

            ("Left Control = Info Popup\n" +
             "Left Shift = Input Popup").Text((20, 0));
            log.Constrain(layer.Size, false, Alignment.Bottom).Text((0, -1));

            layer.DrawGUI();
        }

        void PromptInfo()
        {
            var choice = GUI.PromptChoice("This is some useful info!\n" +
                                          " And some more useful info. ", 1);
            if (float.IsNaN(choice) == false)
                log = $"prompt info:\nclosed";
        }

        void PromptChoice()
        {
            var choice = GUI.PromptChoice("Are you ready to pick a\n" +
                                          "choice from all the choices?", 3);
            if (float.IsNaN(choice) == false)
                log = $"prompt choice:\n{(int)choice}";
        }

        void PromptInput()
        {
            GUI.Tooltip = (tooltip, Side.Bottom, 0.5f);
            var prompt = GUI.PromptInput("Type in stuff:");
            if (prompt != null)
                log = $"prompt input:\n{prompt}";
            GUI.Tooltip = default;
        }
    }
}