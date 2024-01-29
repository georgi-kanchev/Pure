namespace Pure.Examples.ExamplesSystems;

using Engine.Storage;

public static class Storages
{
    public static void Run()
    {
        var storage = new Storage();
        storage.Set("test", "hello, world");
        storage.Set("test2", new[] { "this|that", "test|fest" });
        storage.Set("test3", 35);
        storage.Set("test4", (5, "hi"));
        storage.Set("dict", new Dictionary<char, int> { { 'a', 25 }, { '/', 39 } });

        File.WriteAllBytes("storage.st", storage);
    }
}