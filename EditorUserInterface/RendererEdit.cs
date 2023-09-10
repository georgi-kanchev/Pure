namespace Pure.EditorUserInterface;

using Tilemap;

using UserInterface;

using Utilities;

using static Program;

public class RendererEdit : UserInterface
{
	public void ElementCreate(int index, (int x, int y) position, List.Spans type = default)
	{
		var element = default(Element);
		var panel = new Panel(position) { IsRestricted = false, SizeMinimum = (3, 3) };
		if(index == 1) element = new Button(position);
		else if(index == 2) element = new InputBox(position);
		else if(index == 3)
		{
			element = new Pages(position);
			panel.SizeMinimum = (8, 3);
		}
		else if(index == 4)
		{
			element = new Panel(position);
			panel.SizeMinimum = (5, 5);
		}
		else if(index == 5)
		{
			element = new Palette(position);
			panel.SizeMinimum = (15, 5);
		}
		else if(index == 6)
		{
			element = new Slider(position);
			panel.SizeMinimum = (4, 3);
		}
		else if(index == 7)
		{
			element = new Scroll(position);
			panel.SizeMinimum = (3, 4);
		}
		else if(index == 8)
		{
			element = new Stepper(position);
			panel.SizeMinimum = (6, 4);
		}
		else if(index == 9)
			element = new Layout(position);
		else if(index == 10)
		{
			element = new List(position, 10, type);
			panel.SizeMinimum = (4, 4);
		}
		else if(index == 11)
		{
			element = new FileViewer(position);
			panel.SizeMinimum = (5, 5);
		}
		else if(index == 12)
		{
			element = new FileViewer(position) { Text = "FolderViewer" };
			panel.SizeMinimum = (5, 5);
		}

		if(element == null)
			return;

		panel.Size = (element.Size.width + 2, element.Size.height + 2);
		panel.Text = element.Text;

		ui.Add(element);
		Add(panel);

		DisplayInfoText("Added " + element.Text);
		Element.Focused = null;
	}
	public void ElementRemove(Element element)
	{
		var panel = this[ui.IndexOf(element)];
		Remove(panel);
		ui.Remove(element);

		if(Selected == element)
			Selected = null;
	}
	public void ElementToTop(Element element)
	{
		var panel = this[ui.IndexOf(element)];
		editUI.BringToTop(panel);
		ui.BringToTop(element);
		Selected = element;
	}

	public static void DrawGrid()
	{
		var tmapSz = tilemaps.Size;
		var color = Color.Gray.ToDark(0.66f);
		const int LAYER = (int)Layer.Grid;
		for(var i = 0; i < tmapSz.width; i += 10)
			tilemaps[LAYER].SetLine((i, 0), (i, tmapSz.height), new(Tile.SHADE_1, color));
		for(var i = 0; i < tmapSz.height; i += 10)
			tilemaps[LAYER].SetLine((0, i), (tmapSz.width, i), new(Tile.SHADE_1, color, 1));

		for(var i = 0; i < tmapSz.height; i += 20)
			for(var j = 0; j < tmapSz.width; j += 20)
			{
				tilemaps[LAYER].SetTile((j, i), new Tile(Tile.SHADE_OPAQUE, color));
				tilemaps[LAYER].SetTextLine((j + 1, i + 1), $"{j}, {i}", color);
			}
	}

	protected override void OnUserActionPanel(Panel panel, UserAction userAction)
	{
		if(userAction != UserAction.Press)
			return;

		var notOverEditPanel = editPanel.IsHovered == false || editPanel.IsHidden;
		var isHoveringMenu = false;
		foreach(var kvp in menus)
			if(kvp.Value is { IsHovered: true, IsHidden: false })
			{
				isHoveringMenu = true;
				break;
			}

		if(notOverEditPanel == false || isHoveringMenu)
			return;

		var index = IndexOf(panel);
		var e = ui[index];
		Selected = e;
		panel.IsHidden = false;
	}
	protected override void OnDisplayPanel(Panel panel)
	{
		var index = IndexOf(panel);
		var e = ui[index];

		e.Position = (panel.Position.x + 1, panel.Position.y + 1);
		e.Size = (panel.Size.width - 2, panel.Size.height - 2);

		var offset = (panel.Size.width - panel.Text.Length) / 2;
		offset = Math.Max(offset, 0);
		var textPos = (panel.Position.x + offset, panel.Position.y);
		const int CORNER = Tile.BOX_GRID_CORNER;
		const int STRAIGHT = Tile.BOX_GRID_STRAIGHT;

		if(Selected != e)
			return;

		var back = tilemaps[(int)Layer.EditBack];
		var middle = tilemaps[(int)Layer.EditMiddle];
		back.SetBox(panel.Position, panel.Size, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Cyan);
		back.SetRectangle(textPos, (panel.Text.Length, 1), default);
		middle.SetTextLine(textPos, panel.Text, Color.Cyan);

		var (x, y) = (panel.Position.x, panel.Position.y - 1);
		var curX = 0;
		if(Selected.IsDisabled)
		{
			back.SetTile((x, y), new(Tile.LOWERCASE_X, Color.Red));
			curX++;
		}

		if(Selected.IsHidden == false)
			return;

		var pos = (x + curX, y);
		back.SetTile(pos, new(Tile.ICON_EYE_OPENED, Color.Red));
	}
	protected override void OnPanelResize(Panel panel, (int width, int height) delta)
	{
		DisplayInfoText($"{panel.Text} {panel.Size.width - 2}x{panel.Size.height - 2}");
	}
	protected override void OnDragPanel(Panel panel, (int width, int height) delta)
	{
		DisplayInfoText($"{panel.Text} {panel.Position.x + 1}, {panel.Position.y + 1}");
	}
}