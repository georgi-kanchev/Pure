namespace Pure.Examples.ExamplesUserInterface;

public static class Pagination
{
    public static Block[] Create(TilemapPack maps)
    {
        var icons = new Pages { Size = (9, 1), ItemGap = 0 };
        icons.Align((0.05f, 0.5f));
        icons.OnDisplay(() => SetPages(maps, icons, zOrder: 0));
        icons.OnItemDisplay(item => SetPagesIcon(maps, item, zOrder: 1, tileId: Tile.ICON_HOME));

        //================

        var pages = new Pages { Size = (15, 1), Count = 150 };
        pages.ItemWidth = $"{pages.Count}".Length;
        pages.Align((0.95f, 0.5f));
        pages.OnDisplay(() => SetPages(maps, pages, zOrder: 0));
        pages.OnItemDisplay(item => SetPagesItem(maps, pages, item, zOrder: 1));

        //================

        var buttons = new Pages { Size = (21, 1), Count = 150 };
        buttons.Current = buttons.Count / 2;
        buttons.ItemWidth = $"{buttons.Count}".Length + 2;
        buttons.Align((0.5f, 0.1f));
        buttons.OnDisplay(() => SetPages(maps, buttons, zOrder: 0));
        buttons.OnItemDisplay(item => SetButtonSelect(maps, item, zOrder: 1));

        //================

        var buttonsBig = new Pages { Size = (21, 3), Count = 100 };
        buttonsBig.ItemWidth = $"{buttonsBig.Count}".Length + 2;
        buttonsBig.Align((0.5f, 0.9f));
        buttonsBig.OnDisplay(() => SetPages(maps, buttonsBig, zOrder: 0));
        buttonsBig.OnItemDisplay(item =>
            SetButton(maps, item, zOrder: 1, isDisplayingSelection: true));

        return new Block[] { icons, pages, buttons, buttonsBig };
    }
}