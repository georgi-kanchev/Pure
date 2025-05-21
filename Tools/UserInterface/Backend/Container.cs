using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using Pure.Engine.Window;
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
			var parent = owner.GetParent(this)?.Area;
			var x = ToInt(AreaDynamic.x, parent: parent);
			var y = ToInt(AreaDynamic.y, parent: parent);
			var w = ToInt(AreaDynamic.w, parent: parent);
			var h = ToInt(AreaDynamic.h, parent: parent);
			return (x, y, w, h);
		}
	}
	public Pivot Pivot { get; set; } = Center;
	public SizeI Gap { get; set; } = (1, 1);
	public Wrap Wrap { get; set; } = Wrap.SingleRow;
	public (ushort tile, uint color) Background { get; set; } = (0, Color.Gray.ToDark(0.7f));
	public Dictionary<string, Block> Blocks { get; set; } = [];
	public Dictionary<string, string[]> BlocksData { get; set; } = [];
	public Scroll ScrollH { get; } = new((0, 0), false);
	public Scroll ScrollV { get; } = new();

	public void Calculate()
	{
		if (Blocks.Count == 0)
			return;

		var parent = owner.GetParent(this)?.Area;
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

				if (key == nameof(Block.Text)) block.Text = keyValue[1]; // no trim
				else if (key == nameof(Block.Size) && values.Length == 2)
				{
					block.Size = Int2(block.Area);
					block.Height = block is List { Span: Span.Dropdown or Span.Menu } ? 1 : block.Height; // handle folding
				}
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
					else if (key == nameof(List.ItemSize)) list.ItemSize = Int2(list.Area);
					else if (key == nameof(List.ItemGap)) list.ItemGap = (int)value.Calculate();
					else if (key == nameof(List.IsSingleSelecting)) list.IsSingleSelecting = true;
					else if (key == nameof(List.SelectedItems))
					{
						list.Deselect();

						foreach (var v in values)
							foreach (var item in list.Items)
								if (item.Text == v)
									list.Select(item);
					}
				}
				// button props====================================================
				else if (block is Button btn)
				{
					if (key == "Icon") btn.OnDisplay += () => owner.TileMaps.SetButtonIcon(btn, (ushort)ToInt(value));
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

				(int x, int y) Int2(Area self)
				{
					return (ToInt(values[0], self, Area, parent, block.Text, items),
						ToInt(values[1], self, Area, parent, block.Text, items));
				}
			}
		}

		var area = Area;
		var bounds = Block.GetBounds(Blocks.Values.ToArray());
		ScrollH.IsHidden = bounds.width <= Area.Width;
		ScrollV.IsHidden = bounds.height <= Area.Height;
		area.Width -= ScrollV.IsHidden ? 0 : 1;
		area.Height -= ScrollH.IsHidden ? 0 : 1;

		if (Wrap is Wrap.SingleRow or Wrap.MultipleRows)
			Block.SortRow(Blocks.Values.ToArray(), area, Pivot, Gap, Wrap == Wrap.MultipleRows);
		else if (Wrap is Wrap.SingleColumn or Wrap.MultipleColumns)
			Block.SortColumn(Blocks.Values.ToArray(), area, Pivot, Gap, Wrap == Wrap.MultipleColumns);

		ScrollH.Slider.Progress = Pivot is Top or Center or Bottom ? 0.5f : ScrollH.Slider.Progress;
		ScrollH.Slider.Progress = Pivot is TopRight or Right or BottomRight ? 1f : ScrollH.Slider.Progress;

		ScrollV.Slider.Progress = Pivot is Left or Center or Right ? 0.5f : ScrollV.Slider.Progress;
		ScrollV.Slider.Progress = Pivot is BottomLeft or Bottom or BottomRight ? 1f : ScrollV.Slider.Progress;

		localPositions.Clear();
		foreach (var (_, block) in Blocks)
			localPositions.Add((block.Position.x - Area.X, block.Position.y - Area.Y));
	}

	public void Update(LayerTiles layer)
	{
		foreach (var (_, block) in Blocks)
		{
			block.Mask = Area;

			if (block is List { IsFolded: false })
				block.Mask = (0, 0, Input.Bounds.width, Input.Bounds.height);

			block.Update();
		}

		if (Background.tile != 0)
			owner.TileMaps[0].SetArea(Area, [new(Background.tile, Background.color)]);

		if (ScrollH.IsHidden && ScrollV.IsHidden)
			return;

		var bounds = Block.GetBounds(Blocks.Values.ToArray());
		var (ax, ay, aw, ah) = (Area.X, Area.Y, Area.Width, Area.Height);
		var (v, h) = (ScrollV.IsHidden ? 0 : 1, ScrollH.IsHidden ? 0 : 1);

		aw -= v;
		ah -= h;

		if (ScrollH.IsHidden == false)
		{
			ScrollH.Ratio = (float)aw / bounds.width;
			ScrollH.Size = (Area.Width - 1, 1);
			ScrollH.Position = (ax, ay + ah);
			ScrollH.Update();
			owner.TileMaps.SetScroll(ScrollH);
		}

		if (ScrollV.IsHidden == false)
		{
			ScrollV.Ratio = (float)ah / bounds.height;
			ScrollV.Size = (1, Area.Height - 1);
			ScrollV.Position = (ax + aw, ay);
			ScrollV.Update();
			owner.TileMaps.SetScroll(ScrollV);
		}

		//====================================================

		var i = 0;
		foreach (var (_, block) in Blocks)
		{
			var (lx, ly) = localPositions[i];

			if (ScrollH.IsHidden == false)
			{
				block.X = (int)MathF.Ceiling(Map(ScrollH.Slider.Progress, (0, 1), (lx, lx - bounds.width + aw))) + ax;
				block.X += Pivot is TopRight or Right or BottomRight ? bounds.width - aw : 0;
				block.X += Pivot is Top or Center or Bottom ? (bounds.width - aw) / 2 : 0;
			}

			if (ScrollV.IsHidden == false)
			{
				block.Y = (int)MathF.Ceiling(Map(ScrollV.Slider.Progress, (0, 1), (ly, ly - bounds.height + ah))) + ay;
				block.Y += Pivot is BottomLeft or Bottom or BottomRight ? bounds.height - ah : 0;
				block.Y += Pivot is Left or Center or Right ? (bounds.height - ah) / 2 : 0;
			}

			i++;
		}

		//====================================================

		var (mx, my) = layer.MouseCell;
		var hover = mx >= ax && my >= ay && mx <= ax + aw && my <= ay + ah;
		var scrollHover = ScrollH.IsHovered || ScrollV.IsHovered;

		if (hover == false || Mouse.ScrollDelta == 0 || scrollHover)
			return;

		var shift = Keyboard.Key.ShiftLeft.IsPressed() || Keyboard.Key.ShiftRight.IsPressed();

		if (shift && ScrollH.IsHidden == false)
			ScrollH.Slider.MoveHandle(Mouse.ScrollDelta);
		else if (ScrollV.IsHidden == false)
			ScrollV.Slider.MoveHandle(Mouse.ScrollDelta);
	}

#region Backend
	private readonly List<(int x, int y)> localPositions = [];

	private static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
	{
		var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) + targetRange.a;
		return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
	}

	private static int ToInt(string value, Area? self = null, Area? container = null, Area? parent = null, string text = "", int? items = null)
	{
		var result = value.Trim()
			.Replace("ui_left", "0")
			.Replace("ui_top", "0")
			.Replace("ui_right", $"{Input.Bounds.width}")
			.Replace("ui_bottom", $"{Input.Bounds.height}")
			.Replace("ui_width", $"{Input.Bounds.width}")
			.Replace("ui_height", $"{Input.Bounds.height}");

		if (self != null)
			result = result
				.Replace("self_left", $"{self.Value.X}")
				.Replace("self_top", $"{self.Value.Y}")
				.Replace("self_right", $"{self.Value.X + self.Value.Width}")
				.Replace("self_bottom", $"{self.Value.Y + self.Value.Height}")
				.Replace("self_width", $"{self.Value.Width}")
				.Replace("self_height", $"{self.Value.Height}")
				.Replace("self_items", $"{items}")
				.Replace("self_text", $"{text.Length}");

		if (container != null)
			result = result
				.Replace("owner_left", $"{container.Value.X}")
				.Replace("owner_top", $"{container.Value.Y}")
				.Replace("owner_right", $"{container.Value.X + container.Value.Width}")
				.Replace("owner_bottom", $"{container.Value.Y + container.Value.Height}")
				.Replace("owner_width", $"{container.Value.Width}")
				.Replace("owner_height", $"{container.Value.Height}");

		if (parent != null)
			result = result
				.Replace("parent_left", $"{parent.Value.X}")
				.Replace("parent_top", $"{parent.Value.Y}")
				.Replace("parent_right", $"{parent.Value.X + parent.Value.Width}")
				.Replace("parent_bottom", $"{parent.Value.Y + parent.Value.Height}")
				.Replace("parent_width", $"{parent.Value.Width}")
				.Replace("parent_height", $"{parent.Value.Height}");

		var calculation = result.Calculate();
		return (int)(float.IsNaN(calculation) ? 0 : calculation);
	}
#endregion
}