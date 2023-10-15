namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using static Utility;
using Utilities;

public static class Lists
{
    public static Element[] Create(TilemapManager maps)
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
            DisplayList(maps, listVertical, 0);
        });
        listVertical.OnItemDisplay(item => DisplayListItem(maps, listVertical, item, 1));

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
            DisplayList(maps, listHorizontal, 0);
        });
        listHorizontal.OnItemDisplay(item => DisplayListItem(maps, listHorizontal, item, 1));

        //==============

        var listDropdown = new List(span: List.Spans.Dropdown) { Size = (6, 10) };
        listDropdown.Align((0.5f, 0.5f));
        listDropdown.OnDisplay(() => DisplayList(maps, listDropdown, 0));
        listDropdown.OnItemDisplay(item => DisplayListItem(maps, listDropdown, item, 1));

        return new Element[] { listVertical, listHorizontal, listDropdown };
    }
    public static void DisplayList(TilemapManager maps, List list, int zOrder)
    {
        Clear(maps, list, (zOrder, zOrder + 2));

        maps[zOrder].SetRectangle(
            list.Position,
            list.Size,
            tile: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));

        if (list.Scroll.IsHidden == false)
            SlidersAndScrolls.DisplayScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
            maps[zOrder + 2].SetTile(
                position: (list.Position.x + list.Size.width - 1, list.Position.y),
                tile: new(Tile.MATH_GREATER, GetColor(list, Color.Gray.ToBright()), angle: 1));
    }
    public static void DisplayListItem(TilemapManager maps, List list, Button item, int zOrder)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright(0.3f);
        var (x, y) = item.Position;
        var (_, h) = item.Size;
        var isLeftCrop =
            list.Span == List.Spans.Horizontal &&
            item.Size.width < list.ItemSize.width &&
            item.Position == list.Position;

        SetBackground(maps[zOrder], item, 0.25f);
        maps[zOrder + 1].SetTextLine(
            position: (x, y + h / 2),
            item.Text,
            GetColor(item, color),
            maxLength: item.Size.width * (isLeftCrop ? -1 : 1));
    }
}