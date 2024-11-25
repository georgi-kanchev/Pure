using System.Reflection;
using Pure.Engine.Utilities;

namespace Pure.Tools.Storage;

public class FileTable
{
    public FileTable(string filePath)
    {
        this.filePath = filePath;
        try { cache = File.ReadAllText(filePath).ToObject<Dictionary<string, string>>()!; }
        catch { cache = new(); }
    }

    public void Set(string key, object data)
    {
        cache[key] = data.ToTSV();
    }
    public T? Get<T>(string key, T? defaultData)
    {
        return cache.TryGetValue(key, out var value) ? value.ToObject<T>() : defaultData;
    }

    public void Apply(Type classType, object? classInstance)
    {
        var props = classType.GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var prop in props)
            if (prop.GetSetMethod(true) != null)
                prop.SetValue(classInstance, prop.GetValue(classInstance));
    }
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? "");
        File.WriteAllText(filePath, cache.ToTSV());
    }

#region Backend
    private readonly string filePath;
    private readonly Dictionary<string, string> cache = new();
#endregion
}