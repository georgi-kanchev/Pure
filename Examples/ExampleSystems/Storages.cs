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
            new Dictionary<(string, int), int> { { ("key1", 3), 25 }, { ("key2", 6), 39 } });

        var xtest = storage.GetAsObject<string>("test");
        var xtest2 = storage.GetAsObject<(string, bool)[]>("test2");
        var xtest3 = storage.GetAsObject<int>("test3");
        var xtest4 = storage.GetAsObject<(int, string)>("test4");
        var xdict = storage.GetAsObject<Dictionary<(string, int), int>>("dict");

        var text = storage.ToBase64();
        var load = new Storage(text, true);

        var test = load.GetAsObject<string>("test");
        var test2 = load.GetAsObject<(string, bool)[]>("test2");
        var test3 = load.GetAsObject<int>("test3");
        var test4 = load.GetAsObject<(int, string)>("test4");
        var dict = load.GetAsObject<Dictionary<(string, int), int>>("dict");

        var t = load.ToText();
    }
}