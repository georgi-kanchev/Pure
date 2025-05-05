using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Tools.Tiles;

namespace Pure.Tools.UserInterface;

public enum Wrap { SingleRow, SingleColumn, MultipleRows, MultipleColumns }

[DoNotSave]
public class Layout
{
	public List<TileMap> TileMaps { get; } = [];
	public Tile Cursor { get; set; } = new(546, 3789677055);

	public Layout(string data)
	{
		var lines = data.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
		var container = default(Container);
		var lastContainerLine = 0;

		containers.Clear();

		for (var i = 0; i < lines.Length; i++)
		{
			var line = lines[i];
			var props = line.Trim().Split("\t", StringSplitOptions.RemoveEmptyEntries);
			var block = default(Block);
			var toggle = line.Contains("IsToggle");
			var selected = line.Contains(nameof(Button.IsSelected));

			foreach (var prop in props)
			{
				var keyValue = prop.Trim().Split(": ");
				var key = keyValue[0].Trim();
				var value = keyValue.Length > 1 ? keyValue[1].Trim() : "";
				var values = value.Split(",");

				for (var j = 0; j < values.Length; j++)
					values[j] = values[j].Trim();

				if (key == nameof(Container))
				{
					container = new(this) { Name = value };
					containers.Add(container);
					lastContainerLine = i;
					continue;
				}

				if (container == null)
					continue;

				if (lastContainerLine == i)
				{
					SetPropertyContainer(container, key, values);
					continue;
				}

				// create ====================================================
				if (key == nameof(Button))
				{
					var button = new Button { Text = "", IsSelected = selected };
					if (line.Contains("Icon: ") == false)
						button.OnDisplay += () => TileMaps.SetButton(button, 1, toggle || selected);
					container.Blocks.Add(value, button);
					block = button;
					continue;
				}
				else if (key is nameof(Span.Column) or nameof(Span.Row) or nameof(Span.Dropdown) or nameof(Span.Menu))
				{
					var span = Span.Column;
					span = key == nameof(Span.Row) ? Span.Row : span;
					span = key == nameof(Span.Dropdown) ? Span.Dropdown : span;
					span = key == nameof(Span.Menu) ? Span.Menu : span;

					var list = new List(default, 0, span) { Text = "" };
					list.OnDisplay += () => TileMaps.SetList(list);
					list.OnItemDisplay += item => TileMaps.SetListItem(list, item);
					container.Blocks.Add(value, list);
					block = list;
					continue;
				}

				// block props ====================================================
				if (block == null)
					continue;

				if (key == nameof(Block.Text)) block.Text = keyValue[1]; // no trim
				else if (key == nameof(Block.Size))
				{
					int? items = block is List list ? list.Items.Count : null;

					block.Size = (
						ToInt(values[0], block.Text, container.Area, items),
						ToInt(values[1], block.Text, container.Area, items));
				}
				else if (key == nameof(Block.IsDisabled)) block.IsDisabled = true;
				else if (key == nameof(Block.IsHidden)) block.IsHidden = true;
				// list props====================================================
				else if (block is List list)
				{
					if (key == nameof(List.Items))
					{
						foreach (var _ in values)
							list.Items.Add(new());

						list.Edit(values);
					}
					else if (key == nameof(List.ItemSize))
						list.ItemSize = (
							ToInt(values[0], block.Text, block.Area),
							ToInt(values[1], block.Text, block.Area));
					else if (key == nameof(List.ItemGap)) list.ItemGap = (int)value.Calculate();
					else if (key == nameof(List.IsSingleSelecting)) list.IsSingleSelecting = true;
				}
				// button props====================================================
				else if (block is Button btn)
				{
					if (key == "Icon")
						btn.OnDisplay += () => TileMaps.SetButtonIcon(btn, (ushort)ToInt(value), 1, toggle || selected);
					else if (key == nameof(string.PadLeft)) btn.Text = btn.Text.PadLeft(ToInt(value, btn.Text, btn.Area));
					else if (key == nameof(string.PadRight)) btn.Text = btn.Text.PadRight(ToInt(value, btn.Text, btn.Area));
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
		if (TileMaps.Count == 0 || layerTiles.Size != TileMaps[0].Size)
		{
			TileMaps.Clear();
			for (var i = 0; i < 7; i++)
				TileMaps.Add(new(layerTiles.Size));
		}

		var screen = (0, 0, Input.Bounds.width, Input.Bounds.height);
		foreach (var container in containers)
		{
			foreach (var (_, block) in container.Blocks)
			{
				if (block.IsOverlapping(screen) == false)
					block.Fit(screen);

				block.Update();
			}

			TileMaps[0].SetArea(container.Area, [new(container.Tile, container.Color)]);
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

	private static void SetPropertyContainer(Container container, string key, string[] values)
	{
		if (key == nameof(Container.Parent))
			container.Parent = values[0];
		else if (key == nameof(Container.Pivot))
			container.Pivot = values[0].ToPrimitive<Pivot>() ?? Pivot.Center;
		else if (key == nameof(Container.Wrap))
			container.Wrap = values[0].ToPrimitive<Wrap>() ?? Wrap.SingleRow;
		else if (key == nameof(Container.Area) && values.Length == 4)
			container.AreaDynamic = (values[0], values[1], values[2], values[3]);
		else if (key == nameof(Container.Gap) && values.Length == 2)
			container.Gap = ((int)values[0].Calculate(), (int)values[1].Calculate());
		else if (key == nameof(Container.Color) && values.Length == 4)
		{
			var r = (byte)Math.Clamp(ToInt(values[0]), byte.MinValue, byte.MaxValue);
			var g = (byte)Math.Clamp(ToInt(values[1]), byte.MinValue, byte.MaxValue);
			var b = (byte)Math.Clamp(ToInt(values[2]), byte.MinValue, byte.MaxValue);
			var a = (byte)Math.Clamp(ToInt(values[3]), byte.MinValue, byte.MaxValue);
			container.Color = new Color(r, g, b, a);
		}
		else if (key == nameof(Container.Tile))
			container.Tile = (ushort)ToInt(values[0]);
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