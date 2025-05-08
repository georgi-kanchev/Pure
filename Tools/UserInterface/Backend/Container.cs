using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using Pure.Tools.Tiles;
using SizeI = (int width, int height);
using static Pure.Engine.UserInterface.Pivot;

namespace Pure.Tools.UserInterface;

internal class Container(Layout owner)
{
	public string Name { get; set; } = "";
	public string? Parent { get; set; }

	public (string x, string y, string w, string h) AreaDynamic { get; set; }
	public Area Area
	{
		get
		{
			var parent = owner.GetParent(this);
			var x = Layout.ToInt(AreaDynamic.x, "", parent?.Area);
			var y = Layout.ToInt(AreaDynamic.y, "", parent?.Area);
			var w = Layout.ToInt(AreaDynamic.w, "", parent?.Area);
			var h = Layout.ToInt(AreaDynamic.h, "", parent?.Area);
			return (x, y, w, h);
		}
	}
	public Pivot Pivot { get; set; } = Center;
	public SizeI Gap { get; set; } = (1, 1);
	public Wrap Wrap { get; set; } = Wrap.SingleRow;
	public (ushort tile, uint color) Background { get; set; } = (0, Color.Gray.ToDark(0.7f));
	public Dictionary<string, Block> Blocks { get; set; } = [];
	public Dictionary<string, string[]> BlocksData { get; set; } = [];

	public void Calculate()
	{
		if (Blocks.Count == 0)
			return;

		foreach (var (name, block) in Blocks)
		{
			foreach (var prop in BlocksData[name])
			{
				var keyValue = prop.Trim().Split(": ");
				var key = keyValue[0].Trim();
				var value = keyValue.Length > 1 ? keyValue[1].Trim() : "";
				var values = value.Split(",");
				int? items = block is List l ? l.Items.Count : null;

				for (var j = 0; j < values.Length; j++)
					values[j] = values[j].Trim();

				block.Mask = Area;

				if (key == nameof(Block.Text)) block.Text = keyValue[1]; // no trim
				else if (key == nameof(Block.Size) && values.Length == 2) block.Size = Int2(Area);
				else if (key == nameof(Block.IsDisabled)) block.IsDisabled = true;
				else if (key == nameof(Block.IsHidden)) block.IsHidden = true;
				// list props====================================================
				else if (block is List list)
				{
					if (key == nameof(List.Items))
					{
						list.Items.Clear();
						foreach (var _ in values)
							list.Items.Add(new());

						list.Edit(values);
					}
					else if (key == nameof(List.ItemSize)) list.ItemSize = Int2(block.Area);
					else if (key == nameof(List.ItemGap)) list.ItemGap = (int)value.Calculate();
					else if (key == nameof(List.IsSingleSelecting)) list.IsSingleSelecting = true;
				}
				// button props====================================================
				else if (block is Button btn)
				{
					if (key == "Icon") btn.OnDisplay += () => owner.TileMaps.SetButtonIcon(btn, (ushort)Layout.ToInt(value));
					else if (key is nameof(Button.IsToggle) or nameof(Button.IsSelected))
					{
						btn.IsToggle = true;
						btn.IsSelected = key == nameof(Button.IsSelected);
					}
					else if (key == "PadLeftRight" && values.Length == 2)
					{
						var pad = Int2(block.Area);
						block.Text = block.Text.PadLeft(pad.x).PadRight(pad.y);

						if (block is List list2)
							foreach (var item in list2.Items)
								item.Text = item.Text.PadLeft(pad.x).PadRight(pad.y);
					}
				}

				(int x, int y) Int2(Area area)
				{
					return (Layout.ToInt(values[0], block.Text, area, items),
						Layout.ToInt(values[1], block.Text, area, items));
				}
			}
		}

		if (Wrap is Wrap.SingleRow or Wrap.MultipleRows)
			Block.SortRow(Blocks.Values.ToArray(), Area, Pivot, Gap, Wrap == Wrap.MultipleRows);
	}
}