global using static Pure.Tools.Tilemapper.TilemapperUserInterface;
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
        var (width, height) = Monitor.Current.AspectRatio;
        var maps = new TilemapPack(7, (width * 3, height * 3));
        var blocks = new BlockPack();
        Input.TilemapSize = maps.Size;
        return (maps, blocks);
    }
    public static void Run(TilemapPack maps, BlockPack blocks)
    {
        var layer = new Layer(maps.Size);
        while (Window.KeepOpen())
        {
            Time.Update();
            maps.Flush();

            Input.PositionPrevious = Input.Position;
            Input.Position = layer.PixelToWorld(Mouse.CursorPosition);
            Input.Update(
                buttonsPressed: Mouse.ButtonIdsPressed,
                scrollDelta: Mouse.ScrollDelta,
                keysPressed: KeyIdsPressed,
                keysTyped: KeyTyped);

            blocks.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            for (var i = 0; i < maps.Count; i++)
                layer.DrawTilemap(maps[i].ToBundle());

            layer.DrawCursor();
            layer.Draw();
        }
    }
}