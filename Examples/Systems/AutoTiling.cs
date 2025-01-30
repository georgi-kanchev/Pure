using Pure.Engine.Execution;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class AutoTiling
{
    public static void Run()
    {
        var (w, h) = Monitor.Current.AspectRatio;
        var layer = new Layer((w * 3, h * 3));
        var original = new TileMap(layer.Size);
        var autoTiled = new TileMap(layer.Size);
        var flag = false;
        var rules = new TileMapperRules();

        original.SetArea((10, 10, 15, 10), Tile.FULL);
        original.SetArea((18, 5, 10, 10), Tile.FULL);

        autoTiled.SetTiles((0, 0), original);
        autoTiled.SetText((0, 0), "<LMB> to remove rules:");

        original.SetText((0, 0), "<LMB> to apply rules:");

        Mouse.Button.Left.OnPress(() => flag = !flag);

        rules.AddRule(Tile.SHADE_3, [
            new(), new(), new(),
            new(), Tile.FULL, Tile.FULL,
            new(), Tile.FULL, Tile.FULL
        ]);
        rules.AddRule(Tile.SHADE_3, [
            new(), new(), new(),
            new(), Tile.FULL, Tile.FULL,
            new(), Tile.FULL, Tile.FULL
        ]);
        rules.AddRule(Tile.SHADE_4, [
            Tile.FULL, Tile.FULL, Tile.FULL,
            Tile.FULL, Tile.FULL, Tile.FULL,
            Tile.FULL, Tile.FULL, Tile.FULL
        ]);
        rules.Apply(autoTiled);

        while (Window.KeepOpen())
        {
            Time.Update();
            Flow.Update(Time.Delta);

            layer.DrawTileMap(flag ? original : autoTiled);
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}