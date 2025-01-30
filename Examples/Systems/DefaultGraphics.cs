using Pure.Engine.Window;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class DefaultGraphics
{
    public static void Run()
    {
        Window.Title = "Pure - Default Graphics Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new TileMap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);

        while (Window.KeepOpen())
        {
            tilemap.Flush();

            for (var i = 0; i < 26; i++)
                for (var j = 0; j < 26; j++)
                    tilemap.SetTile((j, i), (ushort)(i, j).ToIndex((26, 26)));

            var (x, y) = layer.PixelToPosition(Mouse.CursorPosition);
            var id = tilemap.TileAt(((int)x, (int)y)).Id;
            tilemap.SetText((27, 13), $"{id}");

            layer.DrawTileMap(tilemap.ToBundle());
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}