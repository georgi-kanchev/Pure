namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

public class RendererEdit : UserInterface
{
	private readonly TilemapManager tilemaps;
	private readonly RendererUI ui;

	public RendererEdit(TilemapManager tilemaps, RendererUI ui)
	{
		this.tilemaps = tilemaps;
		this.ui = ui;
	}

	public void CreateElement(int index, (int x, int y) position)
	{
		var count = Count.ToString();
		var element = default(Element);
		this[count] = new Panel(position) { IsRestricted = false, MinimumSize = (3, 3) };
		switch (index)
		{
			case 1: element = new Button(position); break;
		}
		if (element == null)
			return;

		this[count].Size = (element.Size.width + 2, element.Size.height + 2);
		this[count].Text = element.Text;
		ui[count] = element;
	}

	protected override void OnUpdatePanel(string key, Panel panel)
	{
		var p = panel;
		var e = ui[key];

		e.Position = (p.Position.x + 1, p.Position.y + 1);
		e.Size = (p.Size.width - 2, p.Size.height - 2);

		var offset = (p.Size.width - p.Text.Length) / 2;
		offset = Math.Max(offset, 0);
		tilemaps[2].SetBorder(p.Position, p.Size, Tile.BORDER_GRID_CORNER, Tile.BORDER_GRID_STRAIGHT, Color.Cyan);
		tilemaps[2].SetTextLine((p.Position.x + offset, p.Position.y), p.Text, Color.White, p.Size.width);
	}
}
