namespace Pure.Examples.ExamplesSystems;

using Engine.Window;
using Engine.Tilemap;
using Engine.Utilities;

public static class DefaultGraphics
{
    public static void Run()
    {
        Window.Create();
        Window.Title = "Pure - Default Graphics Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);

        while (Window.KeepOpen())
        {
            tilemap.Flush();

            for (var i = 0; i < 26; i++)
                for (var j = 0; j < 26; j++)
                    tilemap.SetTile((j, i), new Indices(i, j).ToIndex(26));

            var (x, y) = layer.PixelToWorld(Mouse.CursorPosition);
            var id = tilemap.TileAt(((int)x, (int)y)).Id;
            tilemap.SetTextLine((27, 13), $"{id}");

            layer.DrawTilemap(tilemap.ToBundle());
            layer.DrawCursor();
            layer.Draw();
        }
    }
}