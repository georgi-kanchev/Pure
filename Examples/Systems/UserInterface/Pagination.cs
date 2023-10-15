namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using static Utility;

public static class Pagination
{
    public static Element[] Create(TilemapManager maps)
    {
        var icons = new Pages { Size = (9, 1), ItemGap = 0 };
        icons.Align((0.05f, 0.5f));
        icons.OnDisplay(() => DisplayPages(maps, icons, zOrder: 0));
        icons.OnItemDisplay(item => DisplayPagesIcon(maps, item, zOrder: 1, tileId: Tile.ICON_HOME));

        //================

        var pages = new Pages { Size = (15, 1), Count = 150 };
        pages.ItemWidth = $"{pages.Count}".Length;
        pages.Align((0.95f, 0.5f));
        pages.OnDisplay(() => DisplayPages(maps, pages, zOrder: 0));
        pages.OnItemDisplay(item => DisplayPagesItem(maps, pages, item, zOrder: 1));

        //================

        var buttons = new Pages { Size = (21, 1), Count = 150 };
        buttons.Current = buttons.Count / 2;
        buttons.ItemWidth = $"{buttons.Count}".Length + 2;
        buttons.Align((0.5f, 0.1f));
        buttons.OnDisplay(() => DisplayPages(maps, buttons, zOrder: 0));
        buttons.OnItemDisplay(item => ButtonsAndCheckboxes.DisplayButtonSelect(maps, item, zOrder: 1));

        //================

        var buttonsBig = new Pages { Size = (21, 3), Count = 100 };
        buttonsBig.ItemWidth = $"{buttonsBig.Count}".Length + 2;
        buttonsBig.Align((0.5f, 0.9f));
        buttonsBig.OnDisplay(() => DisplayPages(maps, buttonsBig, zOrder: 0));
        buttonsBig.OnItemDisplay(item =>
            ButtonsAndCheckboxes.DisplayButton(maps, item, zOrder: 1, isDisplayingSelection: true));

        return new Element[] { icons, pages, buttons, buttonsBig };
    }
    public static void DisplayPages(TilemapManager maps, Pages pages, int zOrder)
    {
        var p = pages;

        SetBackground(maps[zOrder], p);
        DisplayButton(p.First, Tile.MATH_MUCH_LESS, GetColor(p.First, Color.Red));
        DisplayButton(p.Previous, Tile.MATH_LESS, GetColor(p.Previous, Color.Yellow));
        DisplayButton(p.Next, Tile.MATH_GREATER, GetColor(p.Next, Color.Yellow));
        DisplayButton(p.Last, Tile.MATH_MUCH_GREATER, GetColor(p.Last, Color.Red));

        return;

        void DisplayButton(Button button, int tileId, Color color)
        {
            maps[zOrder].SetBar(
                button.Position,
                tileIdEdge: Tile.BAR_BIG_EDGE,
                tileId: Tile.SHADE_OPAQUE,
                tint: color.ToDark(0.75f),
                button.Size.height,
                isVertical: true);
            maps[zOrder + 1].SetTile(
                position: (button.Position.x, button.Position.y + button.Size.height / 2),
                tile: new(tileId, color));
        }
    }
    public static void DisplayPagesItem(TilemapManager maps, Pages pages, Button page, int zOrder)
    {
        var color = GetColor(page, page.IsSelected ? Color.Green : Color.Gray.ToBright(0.2f));
        var text = page.Text.ToNumber().PadZeros(-pages.ItemWidth);
        maps[zOrder].SetTextLine(page.Position, text, color);
    }
    public static void DisplayPagesIcon(TilemapManager maps, Button page, int zOrder, int tileId)
    {
        var color = GetColor(page, page.IsSelected ? Color.Green : Color.Gray.ToBright(0.2f));
        maps[zOrder].SetTile(
            page.Position,
            tile: new(tileId + int.Parse(page.Text), color));
    }
}