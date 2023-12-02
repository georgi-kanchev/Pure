namespace Pure.Examples.ExamplesUserInterface;

public static class Lists
{
    public static Block[] Create(TilemapPack maps)
    {
        var listVertical = new List
        {
            ItemGap = 1,
            Size = (7, 10),
            IsSingleSelecting = true
        };
        listVertical.Align((0.05f, 0.5f));
        listVertical.OnDisplay(() =>
        {
            var (x, y) = listVertical.Position;
            maps[0].SetTextLine((x, y - 1), "Single select");
            maps.SetList(listVertical);
        });
        listVertical.OnItemDisplay(item => maps.SetListItem(listVertical, item));

        //==============

        var listHorizontal = new List(span: Span.Horizontal)
        {
            ItemSize = (6, 3),
            ItemGap = 1,
            Size = (11, 4),
        };
        listHorizontal.Align((0.95f, 0.5f));
        listHorizontal.OnDisplay(() =>
        {
            var (x, y) = listHorizontal.Position;
            maps[0].SetTextLine((x, y - 1), "Multi select");
            maps.SetList(listHorizontal);
        });
        listHorizontal.OnItemDisplay(item => maps.SetListItem(listHorizontal, item));

        //==============

        var listDropdown = new List(span: Span.Dropdown) { Size = (7, 10) };
        listDropdown.Align((0.5f, 0.5f));
        listDropdown.OnDisplay(() => maps.SetList(listDropdown));
        listDropdown.OnItemDisplay(item => maps.SetListItem(listDropdown, item));

        return new Block[] { listVertical, listHorizontal, listDropdown };
    }
}