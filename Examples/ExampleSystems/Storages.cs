namespace Pure.Examples.ExamplesSystems;

using Engine.Storage;

public static class Storages
{
    public static void Run()
    {
        var storage = new Storage();
        storage.Set("test", "hello, |world;");
        storage.Set("test2", new[] { ("this|that", true), ("test|fest", false) });
        storage.Set("test3", 35);
        storage.Set("test4", (5, "hi"));
        storage.Set("dict",
            new Dictionary<(char, int), int> { { ('a', 3), 25 }, { ('/', 6), 39 } });

        var xtest = storage.GetObject<string>("test");
        var xtest2 = storage.GetObject<(string, bool)[]>("test2");
        var xtest3 = storage.GetObject<int>("test3");
        var xtest4 = storage.GetObject<(int, string)>("test4");
        var xdict = storage.GetObject<Dictionary<(char, int), int>>("dict");

        var text = storage.ToText();
        var load = new Storage(text);

        var test = load.GetObject<string>("test");
        var test2 = load.GetObject<(string, bool)[]>("test2");
        var test3 = load.GetObject<int>("test3");
        var test4 = load.GetObject<(int, string)>("test4");
        var dict = load.GetObject<Dictionary<(char, int), int>>("dict");

        var t = load.ToText();
        var raw = load.ToBytes();
        var b64 = load.ToBase64();
    }
}