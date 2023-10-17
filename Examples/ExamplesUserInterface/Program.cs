global using static Pure.Default.RendererUserInterface.Default;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Tilemap;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;

namespace Pure.Examples.ExamplesUserInterface;

public static class Program
{
    public static (TilemapPack, BlockPack) Initialize()
    {
        Window.Create(3);
        var (width, height) = Window.MonitorAspectRatio;
        var maps = new TilemapPack(7, (width * 3, height * 3));
        var blocks = new BlockPack();
        Input.TilemapSize = maps.Size;
        return (maps, blocks);
    }
    public static void Run(TilemapPack maps, BlockPack blocks)
    {
        while (Window.IsOpen)
        {
            Window.Activate(true);

            Time.Update();
            maps.Clear();

            Input.TilemapSize = maps.Size;
            Input.Update(
                isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
                position: maps.PointFrom(Mouse.CursorPosition, Window.Size),
                scrollDelta: Mouse.ScrollDelta,
                keysPressed: Keyboard.KeyIDsPressed,
                keysTyped: Keyboard.KeyTyped);

            blocks.Update();

            Mouse.CursorGraphics = (Mouse.Cursor)Input.MouseCursorResult;

            for (var i = 0; i < maps.Count; i++)
                Window.DrawTiles(maps[i].ToBundle());

            Window.Activate(false);
        }
    }
}