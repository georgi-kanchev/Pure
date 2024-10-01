global using static Pure.Tools.Tilemapper.TilemapperUI;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Tilemap;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using Key = Pure.Engine.Window.Keyboard.Key;
global using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.UserInterface;

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
            Input.Position = layer.PixelToPosition(Mouse.CursorPosition);
            Input.Update(Mouse.ButtonIdsPressed, Mouse.ScrollDelta, Keyboard.KeyIdsPressed, Keyboard.KeyTyped);

            blocks.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            foreach (var map in maps.Tilemaps)
                layer.DrawTilemap(map);

            layer.DrawCursor();
            layer.Draw();
        }
    }
}