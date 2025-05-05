using Pure.Engine.Utility;
using Pure.Engine.Collision;
using Pure.Engine.Tiles;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class LineOfSightAndLights
{
    public static void Run()
    {
        Window.Title = "Pure - Sights/Lights Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new TileMap((w * 3, h * 3));
        var solidMap = new SolidMap();
        var angle = 0f;
        var opaque = new Tile(Tile.FULL, Color.Green);
        var effect = new Effect { EffectLight = Light.Flat };
        var layer = new LayerTiles(tilemap.Size) { Effect = effect };

        tilemap.SetEllipse((21, 8), (10, 7), true, [opaque]);
        tilemap.SetEllipse((5, 9), (4, 7), true, [opaque]);
        tilemap.SetEllipse((32, 20), (9, 3), true, [opaque]);
        tilemap.SetLine((0, 0), (48, 27), [Tile.SHADE_1]);
        tilemap.SetLine((0, 1), (48, 27), [Tile.SHADE_1]);
        tilemap.SetLine((1, 0), (48, 27), [Tile.SHADE_1]);

        solidMap.AddSolids(Tile.FULL, [new(0, 0, 1, 1)]);
        solidMap.Update(tilemap);

        Window.BackgroundColor = Color.Gray.ToDark(0.65f);
        layer.BackgroundColor = Color.Gray;

        while (Window.KeepOpen())
        {
            Time.Update();
            angle += Time.Delta * 60;

            var (mx, my) = layer.PositionFromPixel(Mouse.CursorPosition);

            effect.AddLightObstacles(solidMap);
            effect.AddLight([(mx, my)], 5f, (360f, angle));

            layer.DrawTileMap(tilemap);
            layer.DrawMouseCursor();
            layer.Render();
        }
    }
}