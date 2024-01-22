namespace Pure.Examples.ExamplesSystems;

using Engine.Storage;

public static class Storages
{
    public static void Run()
    {
        var storage = new Storage();
        storage.Set("test", "hello, |world;");
        storage.Set("test2", new[] { "this|that", "test|fest" });
        storage.Set("test3", 35);
        storage.Set("dict", new Dictionary<string, int> { { "key1", 25 }, { "key2", 39 } });

        var xtest = storage.GetAsObject<string>("test");
        var xtest2 = storage.GetAsObject<string[]>("test2");
        var xtest3 = storage.GetAsObject<int>("test3");
        var xdict = storage.GetAsObject<Dictionary<string, int>>("dict");

        var text = storage.ToBase64();
        var load = new Storage(text, true);

        var test = load.GetAsObject<string>("test");
        var test2 = load.GetAsObject<string[]>("test2");
        var test3 = load.GetAsObject<int>("test3");
        var dict = load.GetAsObject<Dictionary<string, int>>("dict");
    }
}