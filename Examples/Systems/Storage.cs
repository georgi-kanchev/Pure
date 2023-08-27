namespace Pure.Examples.Systems;

using StorageObj = Pure.Storage.Storage;

public static class Storage
{
    public static void Run()
    {
        var storage = new StorageObj();
        storage.Set("test", "hello, world;");
        storage.Set("test2", "this|that");
        storage.Set("test3", 35);

        var text = storage.ToText();
        var load = new StorageObj();
        load.FromText(text);

        var test = load.GetAsObject<string>("test");
        var test2 = load.GetAsObject<string>("test2");
        var test3 = load.GetAsObject<int>("test3");
    }
}