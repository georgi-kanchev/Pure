namespace Pure.Examples.Systems;

public static class Storage
{
    public static void Run()
    {
        var storage = new Pure.Storage.Storage();
        storage.Set("test", "hello, |world;");
        storage.Set("test2", new[] { "this|that", "test|fest" });
        storage.Set("test3", 35);
        storage.Set("dict", new Dictionary<string, int> { { "key1", 25 }, { "key2", 39 } });

        var text = storage.ToText();
        var load = new Pure.Storage.Storage();
        load.FromText(text);

        var test = load.GetAsObject<string>("test");
        var test2 = load.GetAsObject<string[]>("test2");
        var test3 = load.GetAsObject<int>("test3");
        var dict = load.GetAsObject<Dictionary<string, int>>("dict");
    }
}