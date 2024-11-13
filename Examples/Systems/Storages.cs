using Pure.Engine.Storage;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Storages
{
    public enum SomeEnum
    {
        Value1, SecondValue, ValueNumber3
    }

    public class Test
    {
        public readonly string c = "test";
        public uint[] a = [123, 56, 12, 47, 31];
        public int? b = null;
        public SomeEnum someEnum = SomeEnum.ValueNumber3;

        [SaveAtOrder(10)]
        public float D { get; set; } = 33.6f;
    }

    public static void Run()
    {
        var jaggedArray = new int[][]
        {
            [0, 3, 5, 12],
            [25, 5, 61]
        };
        var array2D = new[,]
        {
            { 0, 3, 5, 12 },
            { 125, 5, 612, 3 }
        };
        var arr = new[] { 6, 7, 12, 4 };

        var list = new List<int> { 5, 12, 687, 123 };
        var dict = new Dictionary<string, int[]>
        {
            { "hello", [35, 4] },
            { "test", [12, 7] },
            { "fast", [1, 88] }
        };

        var tilemap = new Tilemap((48, 27));
        tilemap.SetEllipse((48 / 2, 27 / 2), (10, 10), true, null, Tile.NUMBER_1, Tile.NUMBER_2);

        var a = array2D.ToTSV();
        var b = a.ToObject<List<int[,]>>();

        var layer = new Layer(tilemap.Size);
        // var ticker = (float progress) =>
        // {
        //     var t = progress.Animate(Animation.Bounce, AnimationCurve.Out);
        //     layer.DrawTiles((48 / 2f + progress * 10, 27 / 2f + t * 10), new Tile(Tile.FULL));
        // };
        // ticker.CallFor(2f, true);

        var points = new List<(float x, float y)>();
        for (var i = 0; i < 12; i++)
            points.Add((i * 4, 10));

        while (Window.KeepOpen())
        {
            Time.Update();

            var (mx, my) = layer.PixelToPosition(Mouse.CursorPosition);
            if (Mouse.Button.Right.IsPressed())
                for (var i = 0; i < points.Count; i++)
                    if (new Point(points[i]).Distance((mx, my)) < 0.5f)
                        points[i] = (mx, my);

            const int POINTS = 500;
            for (var i = 0f; i < POINTS; i++)
            {
                var x = i / POINTS;
                var t = x.AnimateSpline(Extensions.BounceOut);
                layer.DrawPoints((t.x * 15f, t.y * 15f, Color.White));
            }

            // for (var i = 0f; i < POINTS; i++)
            // {
            //     var x = i / POINTS;
            //     var t = x.AnimateSpline(points.ToArray());
            //     layer.DrawPoints((t.x, t.y, Color.Red));
            // }

            // for (var i = 0; i < points.Count; i++)
            // {
            //     layer.DrawTiles((points[i].x, points[i].y), new Tile(Tile.NUMBER_0 + i));
            //     layer.DrawPoints((points[i].x, points[i].y, Color.Green));
            // }

            layer.DrawCursor();
            layer.Draw();
        }
    }
}