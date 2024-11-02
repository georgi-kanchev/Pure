namespace Pure.Examples.Systems;

using Engine.Storage;

public static class Storages
{
    public class Test
    {
        [SaveAtOrder(5)]
        public string c = "test";
        public uint a = 3;
        public int? b = null;
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

        var data = new int[3, 2, 2];
        data[0, 0, 0] = 12;
        data[0, 1, 0] = 6;

        data[1, 0, 0] = 5;
        data[1, 1, 0] = 46;

        data[2, 0, 0] = 38;
        data[2, 1, 0] = 24;

        var test = data.ToBytes().ToObject<int[,,]>();
    }
}