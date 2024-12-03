using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Animations
{
    public static void Run()
    {
        Window.Title = "Pure - Animation Example";

        var layer = new Layer((48, 27));

        var frames = new[] { (0f, 3f), (5f, 1f), (1.5f, 6f), (0.3f, 4.2f) };
        Time.CallFor(3f, Animation, true);

        while (Window.KeepOpen())
        {
            Time.Update();

            if (Keyboard.Key.A.IsJustPressed())
                Time.CancelCall(Animation);

            layer.DrawMouseCursor();
            layer.Draw();
        }

        void Animation(float unit)
        {
            var index = (int)Math.Min(unit * frames.Length, frames.Length - 1);
            Console.WriteLine(frames[index]);
        }
    }
}