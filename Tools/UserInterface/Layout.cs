using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Tools.Tiles;

namespace Pure.Tools.UserInterface;

public enum Wrap { SingleRow, SingleColumn, MultipleRows, MultipleColumns }

public class Layout
{
    public List<TileMap> TileMaps { get; } = [];
    public Tile Cursor { get; set; } = new(546, 3789677055);

    public Layout(string data)
    {
        var lines = data.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var container = default(Container);
        var block = default(Block);

        foreach (var line in lines)
        {
            var props = line.Trim().Split("\t", StringSplitOptions.RemoveEmptyEntries);

            foreach (var prop in props)
            {
                var keyValue = prop.Trim().Split(": ");
                if (keyValue.Length != 2)
                    continue;

                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();
                var subVals = value.Split(",");

                if (key == nameof(Container))
                {
                    container = new() { Name = value };
                    containers.Add(container);
                }

                if (container == null)
                    continue;

                if (key == nameof(Container.Parent)) container.Parent = value;
                else if (key == nameof(Container.Pivot)) container.Pivot = value.ToPrimitive<Pivot>() ?? Pivot.Center;
                else if (key == nameof(Container.Wrap)) container.Wrap = value.ToPrimitive<Wrap>() ?? Wrap.SingleRow;
                else if (key == nameof(Container.Area) && subVals.Length == 4)
                    container.Area = (ToInt(subVals[0]), ToInt(subVals[1]), ToInt(subVals[2]), ToInt(subVals[3]));
                else if (key == nameof(Container.Gap) && subVals.Length == 2)
                {
                    var (x, y) = (subVals[0].Trim().ToPrimitive<int>(), subVals[1].Trim().ToPrimitive<int>());
                    if (x != null && y != null)
                        container.Gap = (x ?? 0, y ?? 0);
                }

                //====================================================

                if (key == nameof(Button))
                {
                    var button = new Button();
                    button.OnDisplay += () => TileMaps.SetButton(button);
                    container.Blocks.Add(value, (nameof(Button), button));
                    block = button;
                }

                if (block == null)
                    continue;

                else if (key == nameof(Block.Text)) block.Text = value;
                else if (key == nameof(Block.Size))
                {
                    var (x, y) = (subVals[0].Trim().ToPrimitive<int>(), subVals[1].Trim().ToPrimitive<int>());
                    if (x != null && y != null)
                        block.Size = (x ?? 0, y ?? 0);
                }
            }
        }
    }

    public T? GetBlock<T>(string name) where T : Block
    {
        foreach (var container in containers)
            foreach (var (n, b) in container.Blocks)
                if (n == name)
                    return (T?)b.block;

        return default;
    }

    public void DrawGUI(LayerTiles layerTiles)
    {
        foreach (var container in containers)
        {
            foreach (var (_, b) in container.Blocks)
                b.block.Update();

            container.Align();
        }

        if (TileMaps.Count == 0 || layerTiles.Size != TileMaps[0].Size)
        {
            TileMaps.Clear();
            for (var i = 0; i < 7; i++)
                TileMaps.Add(new(layerTiles.Size));
        }

        Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

        Input.ApplyMouse(layerTiles.Size, layerTiles.MousePosition, Mouse.ButtonIdsPressed, Mouse.ScrollDelta);
        Input.ApplyKeyboard(Keyboard.KeyIdsPressed, Keyboard.KeyTyped, Window.Clipboard);

        TileMaps.ForEach(map => layerTiles.DrawTileMap(map));
        layerTiles.DrawMouseCursor(Cursor.Id, Cursor.Tint);
        layerTiles.Render();
        TileMaps.ForEach(map => map.Flush());
    }

#region Backend
    private readonly List<Container> containers = [];

    private int ToInt(string value)
    {
        var result = value.Trim()
            .Replace("left", "0")
            .Replace("top", "0")
            .Replace("right", Input.Bounds.width.ToString())
            .Replace("bottom", Input.Bounds.height.ToString())
            .Replace("width", Input.Bounds.width.ToString())
            .Replace("height", Input.Bounds.height.ToString())
            .Calculate();

        return (int)result;
    }
#endregion
}