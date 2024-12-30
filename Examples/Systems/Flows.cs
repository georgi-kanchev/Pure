using System.Collections;

using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Flows
{
    public static void Run()
    {
        while (Window.KeepOpen())
        {
            Time.Update();

            if (Keyboard.Key.A.IsJustPressed())
                Flow.Start(MyNewFlow());
            if (Keyboard.Key.Space.IsJustPressed())
                Flow.End(MyNewFlow());
        }
    }

    private static IEnumerator MyNewFlow()
    {
        yield return Flow.Wait(5f, nameof(MyNewFlow));
        Console.WriteLine(Time.RuntimeClock);
        Flow.Start(MyNewFlow());
    }
}