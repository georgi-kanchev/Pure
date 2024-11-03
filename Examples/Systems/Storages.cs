using Pure.Engine.Tilemap;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

using Engine.Storage;

public static class Storages
{
    public class Test
    {
        [SaveAtOrder(5)]
        public string c;
        public uint a;
        public int? b = 3;

        public char D { get; set; }
    }

    public static void Run()
    {
        // var storage = new Storage();
        // storage.Set("test", "hello, world");
        // storage.Set("test2", new[] { "this|that", "test|fest" });
        // storage.Set("test3", 35);
        // storage.Set("test4", (5, "hi"));
        // storage.Set("dict", new Dictionary<char, int> { { 'a', 25 }, { '/', 39 } });
        //
        // // LYxRCoAgDIatR0+xAwSG5kN0mUkaCkKgg148fM5i8LF/37ZJCAFTBzIORpsZkltV2FCohLACxpDzvcBzl+wR5BB6GIqpNoqOsI1pu8bNt2J4xdg/bZzsgTGx9+n8fjtU2irsZfYX
        // var base64 = storage.ToBase64();
        // // Y2RgYFBgBBIJIMIaRNQwgQguEFO/CCRTklpckqBgoJCQkZqTk6+jUJ5flJOSoMAFljACy5RkZBbXlGQkliTUgEVr0sB6IEqMQUqMTaE8ExDP1DohIxMkn5KZDDE7MUHfyFQ/AQiNLRmgQACImaFsFiAOSa0oYYCKCUDFQI7lhMrpwBSwo2lmA2K/0tykVJB/GLiQtIN8yQ2XhRvAh6QEZAgvQgmEAgA=
        // var b64 =
        //     "Y2RgYFBgBBIJIMIaRNQwgQguEFO/CCRTklpckqBgoJCQkZqTk6+jUJ5flJOSoMAFljACy5RkZBbXlGQkliTUgEVr0sB6IEqMQUqMTaE8ExDP1DohIxMkn5KZDDE7MUHfyFQ/AQiNLRmgQACImaFsFiAOSa0oYYCKCUDFQI7lhMrpwBSwo2lmA2K/0tykVJB/GLiQtIN8yQ2XhRvAh6QEZAgvQgmEAgA=\n";
        // var load = new Storage(b64, true);

        // var tilemap = bytes.ToObject<Tilemap>();
        var tilemap = new Tilemap((48, 27));
        var layer = new Layer(tilemap.Size);

        tilemap.SetEllipse((48 / 2, 27 / 2), (10, 10), true, null, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        var bTilemap = Extensions.ToBytes(tilemap);
        var newTilemap = bTilemap.ToObject<Tilemap>();

        while (Window.KeepOpen())
        {
            layer.DrawTilemap(newTilemap);
            layer.DrawCursor();
            layer.Draw();
        }
    }
}