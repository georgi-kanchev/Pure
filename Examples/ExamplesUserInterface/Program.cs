global using static Pure.Default.Tilemapper.TilemapperUserInterface;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Tilemap;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using static Pure.Engine.Window.Keyboard;
global using Key = Pure.Engine.Window.Keyboard.Key;
global using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.ExamplesUserInterface;

public static class Program
{
    public static (TilemapPack, BlockPack) Initialize()
    {
        Window.Create();
        var (width, height) = Monitor.AspectRatio;
        var maps = new TilemapPack(7, (width * 3, height * 3));
        var blocks = new BlockPack();
        Input.TilemapSize = maps.Size;
        return (maps, blocks);
    }
    public static void Run(TilemapPack maps, BlockPack blocks)
    {
        var layer = new Layer(maps.Size);
        while (Window.IsOpen)
        {
            Window.Activate(true);

            Time.Update();
            maps.Flush();

            Input.Position = layer.PixelToWorld(Mouse.CursorPosition);
            Input.Update(
                isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
                scrollDelta: Mouse.ScrollDelta,
                keysPressed: KeyIDsPressed,
                keysTyped: KeyTyped);

            blocks.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            layer.Clear();
            for (var i = 0; i < maps.Count; i++)
                layer.DrawTilemap(maps[i].ToBundle());

            Window.DrawLayer(layer);
            Window.Activate(false);
        }
    }
}