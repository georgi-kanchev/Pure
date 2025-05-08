namespace Pure.Examples.UserInterface;

public static class Lists
{
	public static Block[] Create(List<TileMap> maps)
	{
		Window.Title = "Pure - Lists Example";

		var row = new List(default, 8)
		{
			ItemGap = 1,
			Size = (6, 5),
			IsSingleSelecting = true
		};
		row.AlignInside((0.05f, 0.2f));
		row.OnDisplay += () =>
		{
			maps[0].SetText((row.Position.x, row.Position.y - 1), "Column Single");
			maps.SetList(row);
		};
		row.OnItemDisplay += item => maps.SetListItem(row, item);

		//====================================================

		var column = new List(default, span: Span.Row)
		{
			ItemSize = (7, 3),
			ItemGap = 1,
			Size = (11, 4)
		};
		column.AlignInside((0.05f, 0.5f));
		column.OnDisplay += () =>
		{
			maps[0].SetText((column.Position.x, column.Position.y - 1), "Row Multi");
			maps.SetList(column);
		};
		column.OnItemDisplay += item => maps.SetButton(item);

		//====================================================

		var dropdown = new List(default, span: Span.Dropdown) { Size = (13, 3), ItemSize = (13, 1) };
		dropdown.AlignInside((0.7f, 0.2f));
		dropdown.OnDisplay += () =>
		{
			maps[0].SetText((dropdown.Position.x, dropdown.Position.y - 1), "Dropdown Multi");
			maps.SetList(dropdown);
		};
		dropdown.OnItemDisplay += item => maps.SetListItem(dropdown, item);

		//====================================================

		var dropdownSingle = new List((0, 0), 8, Span.Dropdown) { Size = (8, 6), IsSingleSelecting = true };
		dropdownSingle.AlignInside((0.7f, 0.95f));
		dropdownSingle.OnDisplay += () =>
		{
			maps[0].SetText((dropdownSingle.Position.x, dropdownSingle.Position.y - 1), "Dropdown Single");
			maps.SetList(dropdownSingle);
		};
		dropdownSingle.OnItemDisplay += item => maps.SetListItem(dropdownSingle, item);

		//====================================================

		var checkboxes = new List((0, 0), 8) { Size = (8, 6), ItemSize = (7, 1) };
		checkboxes.AlignInside((0.7f, 0.5f));
		checkboxes.OnDisplay += () =>
		{
			var (x, y) = checkboxes.Position;
			maps[0].SetText((x, y - 1), "Checkboxes Multi");
			maps.SetList(checkboxes);
		};
		checkboxes.OnItemDisplay += item => maps.SetCheckbox(item);
//====================================================

		var menu = new List((0, 0), 8, Span.Menu) { Text = "My Menu", Size = (8, 6), ItemSize = (7, 1), IsSingleSelecting = true };
		menu.AlignInside((0.1f, 0.9f));
		menu.OnDisplay += () =>
		{
			maps[0].SetText((menu.Position.x, menu.Position.y - 1), "Menu Single");
			maps.SetList(menu);
		};
		menu.OnItemDisplay += item => maps.SetListItem(menu, item);
		menu.OnInteraction(Interaction.Hover, () => menu.IsFolded = false);
		menu.OnInteraction(Interaction.Unhover, () => menu.IsFolded = true);

		return [row, column, dropdown, dropdownSingle, checkboxes, menu];
	}
}