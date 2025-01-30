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

    public static (List<TileMap>, List<Block>) Initialize()
    {
        var (width, height) = Monitor.Current.AspectRatio;
        var sz = (width * 3, height * 3);
        var maps = new List<TileMap>();
        var blocks = new List<Block>();
        Input.TilemapSize = sz;

        for (var i = 0; i < 7; i++)
            maps.Add(new(sz));

        return (maps, blocks);
    }
    public static void Run(List<TileMap> maps, List<Block> blocks)
    {
        Layer = new(maps[0].Size);
        while (Window.KeepOpen())
        {
            Time.Update();
            maps.ForEach(map => map.Flush());

            Input.PositionPrevious = Input.Position;
            Input.Position = Layer.MouseCursorPosition;
            Input.Update(Mouse.ButtonIdsPressed, Mouse.ScrollDelta, Keyboard.KeyIdsPressed, Keyboard.KeyTyped);

            blocks.ForEach(block => block.Update());

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            maps.ForEach(map => Layer.DrawTileMap(map));
            Layer.DrawMouseCursor();
            Layer.Draw();
        }
    }
}