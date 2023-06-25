namespace Test;

using SFML.Graphics;

public static class Test
{
    private static void Main()
    {
        var window = new RenderWindow(new(800, 600), "Test");
        window.SetActive();

        while (window.IsOpen)
        {
            window.DispatchEvents();

            window.Clear();
            window.Display();
        }
    }
}