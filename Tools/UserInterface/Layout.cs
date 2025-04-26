using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;

namespace Pure.Tools.UserInterface;

public class Layout
{
    public string Data
    {
        get => data;
        private set
        {
            data = value;
            containers = value.ToObject<Dictionary<string, (string? parent, Container container)>>() ?? [];
        }
    }

    public Layout()
    {
    }
    public Layout(string data)
    {
        Data = data;
    }

    public void SetContainer(string name, string? parentName = null, Area? area = null, Pivot pivot = Pivot.Center)
    {
        containers.TryAdd(name, (parentName, new()));

        var c = containers[name].container;
        c.Area = area;
        c.Pivot = pivot;

        data = containers.ToDataAsText();
    }
    public void SetBlock(string containerName, string name, Block block)
    {
        containers[containerName].container.Blocks[name] = (block.GetType().Name, block);
        containers[containerName].container.Align();
    }

    public void Update()
    {
        foreach (var (_, container) in containers)
        {
            foreach (var (_, b) in container.container.Blocks)
                b.block.Update();

            if (container.container.IsAlwaysAligning)
                container.container.Align();
        }
    }

#region Backend
    private Dictionary<string, (string? parent, Container container)> containers = [];
    private string data = "";
#endregion
}