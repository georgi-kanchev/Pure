using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
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
	public uint Color { get; set; } = Engine.Utility.Color.Gray.ToDark(0.7f);
	public ushort Tile { get; set; } = 10;
	public Dictionary<string, Block> Blocks { get; set; } = [];
	public Dictionary<string, (string w, string h)> BlockDynamicSizes { get; set; } = [];
	public Dictionary<string, (string l, string r)> BlockDynamicPads { get; set; } = [];

	public void Align()
	{
		if (Blocks.Count == 0)
			return;

		//size ====================================================
		foreach (var (name, _) in Blocks)
		{
			SetDynamicSize(name);
			SetDynamicPad(name);
		}

		// position ====================================================
		var first = Blocks.First();
		var targetArea = first.Value.Area;
		var pivot = TopLeft;

		pivot = Pivot is Left or Center or Right ? Left : pivot;
		pivot = Pivot is BottomLeft or Bottom or BottomRight ? BottomLeft : pivot;
		first.Value.AlignInside(GetBoundingBox(), pivot);

		foreach (var (_, block) in Blocks.Skip(1))
			if (Wrap == Wrap.SingleRow)
			{
				block.AlignX((Left, Right), targetArea, Gap.width);
				block.AlignY((Left, Right), targetArea);
				targetArea = block.Area;
			}
	}

#region Backend
	private Area GetBoundingBox()
	{
		if (Blocks.Count == 0)
			return default;

		var (rw, rh) = (0, int.MinValue);
		var (x, y, w, h) = Area.ToBundle();

		foreach (var (_, block) in Blocks)
		{
			rw += block.Width + Gap.width;
			rh = block.Height > rh ? block.Height : rh;
		}

		rw -= Gap.width;

		if (Pivot is Top or Center or Bottom) x = x + w / 2 - rw / 2;
		else if (Pivot is TopRight or Right or BottomRight) x = x + w - rw;

		return (x, y, rw, h);
	}

	private void SetDynamicSize(string blockName)
	{
		var has = BlockDynamicSizes.TryGetValue(blockName, out var sz);
		var block = Blocks[blockName];
		int? items = block is List list ? list.Items.Count : null;
		var w = Layout.ToInt(sz.w, block.Text, Area, items);
		var h = Layout.ToInt(sz.h, block.Text, Area, items);

		block.Size = has ? (w, h) : (1, 1);
	}
	private void SetDynamicPad(string blockName)
	{
		var has = BlockDynamicPads.TryGetValue(blockName, out var pad);
		if (has == false)
			return;

		var block = Blocks[blockName];
		int? items = block is List list ? list.Items.Count : null;
		var l = Layout.ToInt(pad.l, block.Text, Area, items);
		var r = Layout.ToInt(pad.r, block.Text, Area, items);

		block.Text = block.Text.PadLeft(l).PadRight(r);
	}
#endregion
}