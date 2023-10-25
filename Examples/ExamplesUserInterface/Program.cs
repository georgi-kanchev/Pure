global using static Pure.Default.RendererUserInterface.Default;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Tilemap;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using static Pure.Engine.Window.Keyboard;
global using Key = Pure.Engine.Window.Keyboard.Key;

namespace Pure.Examples.ExamplesUserInterface;

public static class Program
{
    public static (TilemapPack, BlockPack) Initialize()
    {
        Window.Create();
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
            maps.Flush();

            Input.Position = Mouse.PixelToWorld(Mouse.CursorPosition);
            Input.Update(
                isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
                scrollDelta: Mouse.ScrollDelta,
                keysPressed: KeyIDsPressed,
                keysTyped: KeyTyped);

            blocks.Update();

            Mouse.CursorGraphics = (Mouse.Cursor)Input.CursorResult;

            for (var i = 0; i < maps.Count; i++)
                Window.DrawTiles(maps[i].ToBundle());

            Window.Activate(false);
        }
    }
}