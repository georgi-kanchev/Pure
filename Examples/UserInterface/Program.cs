global using static Pure.Tools.Tiles.TileMapperUI;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Tiles;
global using Pure.Engine.Utility;
global using Pure.Engine.Window;
global using Key = Pure.Engine.Window.Keyboard.Key;
global using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.UserInterface;

public static class Program
{
    public static Layer? Layer { get; private set; }

    public static (TileMapPack, BlockPack) Initialize()
    {
        var (width, height) = Monitor.Current.AspectRatio;
        var maps = new TileMapPack(7, (width * 3, height * 3));
        var blocks = new BlockPack();
        Input.TilemapSize = maps.Size;
        return (maps, blocks);
    }
    public static void Run(TileMapPack maps, BlockPack blocks)
    {
        Layer = new(maps.Size);
        while (Window.KeepOpen())
        {
            Time.Update();
            maps.Flush();

            Input.PositionPrevious = Input.Position;
            Input.Position = Layer.MouseCursorPosition;
            Input.Update(Mouse.ButtonIdsPressed, Mouse.ScrollDelta, Keyboard.KeyIdsPressed, Keyboard.KeyTyped);

            blocks.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            foreach (var map in maps.TileMaps)
                Layer.DrawTiles(map);

            Layer.DrawMouseCursor();
            Layer.Draw();
        }
    }
}