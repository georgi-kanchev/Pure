namespace Pure.Examples.ExamplesUserInterface;

public static class Lists
{
    public static Block[] Create(TilemapPack maps)
    {
        var listVertical = new List
        {
            ItemGap = 1,
            Size = (6, 10),
            IsSingleSelecting = true
        };
        listVertical.Align((0.05f, 0.5f));
        listVertical.OnDisplay(() =>
        {
            var (x, y) = listVertical.Position;
            maps[0].SetTextLine((x, y - 1), "Single select");
            SetList(maps, listVertical, 0);
        });
        listVertical.OnItemDisplay(item => SetListItem(maps, listVertical, item, 1));

        //==============

        var listHorizontal = new List(span: List.Spans.Horizontal)
        {
            ItemSize = (5, 3),
            ItemGap = 1,
            Size = (11, 4),
        };
        listHorizontal.Align((0.95f, 0.5f));
        listHorizontal.OnDisplay(() =>
        {
            var (x, y) = listHorizontal.Position;
            maps[0].SetTextLine((x, y - 1), "Multi select");
            SetList(maps, listHorizontal, 0);
        });
        listHorizontal.OnItemDisplay(item => SetListItem(maps, listHorizontal, item, 1));

        //==============

        var listDropdown = new List(span: List.Spans.Dropdown) { Size = (6, 10) };
        listDropdown.Align((0.5f, 0.5f));
        listDropdown.OnDisplay(() => SetList(maps, listDropdown, 0));
        listDropdown.OnItemDisplay(item => SetListItem(maps, listDropdown, item, 1));

        return new Block[] { listVertical, listHorizontal, listDropdown };
    }
}