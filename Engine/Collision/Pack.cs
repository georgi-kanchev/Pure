namespace Pure.Engine.Collision;

public abstract class Pack<T>
{
    public (float x, float y) Position { get; set; }
    public (float width, float height) Scale { get; set; } = (1f, 1f);
    public int Count
    {
        get => data.Count;
    }

    public T this[int index]
    {
        get => LocalToGlobal(data[index]);
        set => data[index] = value;
    }

    protected Pack(params T[] data)
    {
        Add(data);
    }

    public T[] ToArray()
    {
        return data.ToArray();
    }

    public void Add(params T[]? items)
    {
        if (items == null || items.Length == 0)
            return;

        data.AddRange(items);
    }
    public void RemoveAt(params int[]? indexes)
    {
        if (indexes == null || indexes.Length == 0)
            return;

        foreach (var i in indexes)
            data.RemoveAt(i);
    }

#region Backend
    protected readonly List<T> data = [];

    protected abstract T LocalToGlobal(T local);
#endregion
}