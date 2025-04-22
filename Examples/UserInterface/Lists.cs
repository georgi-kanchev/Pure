namespace Pure.Examples.UserInterface;

public static class Lists
{
    public static Block[] Create(List<TileMap> maps)
    {
        Window.Title = "Pure - Lists Example";

        var listVertical = new List
        {
            ItemGap = 1,
            Size = (7, 10),
            IsSingleSelecting = true
        };
        listVertical.AlignInside((0.05f, 0.5f));
        listVertical.OnDisplay += () =>
        {
            var (x, y) = listVertical.Position;
            maps[0].SetText((x, y - 1), "Single select");
            maps.SetList(listVertical);
        };
        listVertical.OnItemDisplay += item => maps.SetListItem(listVertical, item);

        //==============

        var listHorizontal = new List((0, 0), span: Span.Horizontal)
        {
            ItemSize = (7, 3),
            ItemGap = 1,
            Size = (11, 4)
        };
        listHorizontal.AlignInside((0.95f, 0.5f));
        listHorizontal.OnDisplay += () =>
        {
            var (x, y) = listHorizontal.Position;
            maps[0].SetText((x, y - 1), "Multi select");
            maps.SetList(listHorizontal);
        };
        listHorizontal.OnItemDisplay += item => maps.SetButton(item, 1, true);

        //==============

        var listDropdown = new List((0, 0), span: Span.Dropdown) { Size = (7, 10) };
        listDropdown.AlignInside((0.5f, 0.5f));
        listDropdown.OnDisplay += () =>
        {
            var (x, y) = listDropdown.Position;
            maps[0].SetText((x, y - 1), "Dropdown");
            maps.SetList(listDropdown);
        };
        listDropdown.OnItemDisplay += item => maps.SetListItem(listDropdown, item);

        return [listVertical, listHorizontal, listDropdown];
    }
}