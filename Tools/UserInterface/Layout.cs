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

		containers.Clear();

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

				for (var i = 0; i < subVals.Length; i++)
					subVals[i] = subVals[i].Trim();

				if (key == nameof(Container))
				{
					container = new(this) { Name = value };
					containers.Add(container);
				}

				if (container == null)
					continue;

				if (key == nameof(Container.Parent)) container.Parent = value;
				else if (key == nameof(Container.Pivot)) container.Pivot = value.ToPrimitive<Pivot>() ?? Pivot.Center;
				else if (key == nameof(Container.Wrap)) container.Wrap = value.ToPrimitive<Wrap>() ?? Wrap.SingleRow;
				else if (key == nameof(Container.Area) && subVals.Length == 4)
					container.AreaDynamic = (subVals[0], subVals[1], subVals[2], subVals[3]);
				else if (key == nameof(Container.Gap) && subVals.Length == 2)
					container.Gap = ((int)subVals[0].Calculate(), (int)subVals[1].Calculate());
				//====================================================
				else if (key == nameof(Button))
				{
					var button = new Button();
					button.OnDisplay += () => TileMaps.SetButton(button);
					container.Blocks.Add(value, button);
					block = button;
				}
				else if (key == nameof(Span.Dropdown))
				{
					var dropdown = new List(default, 0, Span.Dropdown);
					dropdown.OnDisplay += () => TileMaps.SetList(dropdown);
					dropdown.OnItemDisplay += item => TileMaps.SetListItem(dropdown, item);
					container.Blocks.Add(value, dropdown);
					block = dropdown;
				}

				if (block == null)
					continue;

				if (key == nameof(Block.Text)) block.Text = value;
				else if (key == nameof(Block.Size))
				{
					int? items = block is List list ? list.Items.Count : null;

					block.Size = (
						ToInt(subVals[0], block.Text, container.Area, items),
						ToInt(subVals[1], block.Text, container.Area, items));
				}
				//====================================================
				else if (block is List list)
				{
					if (key == nameof(List.Items))
					{
						for (var i = 0; i < subVals.Length; i++)
							list.Items.Add(new());

						list.Edit(subVals);
					}
					else if (key == nameof(List.ItemSize))
						list.ItemSize = (
							ToInt(subVals[0], block.Text, block.Area),
							ToInt(subVals[1], block.Text, block.Area));
					else if (key == nameof(List.ItemGap))
						list.ItemGap = (int)value.Calculate();
				}
			}
		}
	}

	public void Resize()
	{
		foreach (var c in containers)
			c.Align();
	}
	public T? GetBlock<T>(string name) where T : Block
	{
		foreach (var container in containers)
			foreach (var (n, block) in container.Blocks)
				if (n == name)
					return (T?)block;

		return default;
	}

	public void DrawGUI(LayerTiles layerTiles)
	{
		var screen = (0, 0, Input.Bounds.width, Input.Bounds.height);
		foreach (var container in containers)
			foreach (var (_, block) in container.Blocks)
			{
				if (block.IsOverlapping(screen) == false)
					block.Fit(screen);

				block.Update();
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

		if (loadedWithSize == Input.Bounds)
			return;

		Resize();
		loadedWithSize = Input.Bounds;
	}

#region Backend
	private readonly List<Container> containers = [];
	private (int width, int height) loadedWithSize;

	internal Container? GetParent(Container child)
	{
		foreach (var container in containers)
			if (container.Name == child.Parent)
				return container;

		return null;
	}

	internal static int ToInt(string value, string text = "", Area? area = null, int? items = null)
	{
		area ??= (0, 0, Input.Bounds.width, Input.Bounds.height);

		var result = value.Trim()
			.Replace("left", $"{area.Value.X}")
			.Replace("top", $"{area.Value.Y}")
			.Replace("right", $"{area.Value.Width}")
			.Replace("bottom", $"{area.Value.Height}")
			.Replace("width", $"{area.Value.Width}")
			.Replace("height", $"{area.Value.Height}")
			.Replace("items", $"{items}")
			.Replace("text", $"{text.Length}");

		return (int)result.Calculate();
	}
#endregion
}