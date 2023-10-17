namespace Pure.Examples.ExamplesUserInterface;

public static class Pagination
{
    public static Block[] Create(TilemapPack maps)
    {
        var icons = new Pages { Size = (9, 1), ItemGap = 0 };
        icons.Align((0.05f, 0.5f));
        icons.OnDisplay(() => maps.SetPages(icons));
        icons.OnItemDisplay(item => maps.SetPagesIcon(item, Tile.ICON_HOME));

        //================

        var pages = new Pages { Size = (15, 1), Count = 150 };
        pages.ItemWidth = $"{pages.Count}".Length;
        pages.Align((0.95f, 0.5f));
        pages.OnDisplay(() => maps.SetPages(pages));
        pages.OnItemDisplay(item => maps.SetPagesItem(pages, item));

        //================

        var buttons = new Pages { Size = (21, 1), Count = 150 };
        buttons.Current = buttons.Count / 2;
        buttons.ItemWidth = $"{buttons.Count}".Length + 2;
        buttons.Align((0.5f, 0.1f));
        buttons.OnDisplay(() => maps.SetPages(buttons));
        buttons.OnItemDisplay(item => maps.SetButtonSelect(item));

        //================

        var buttonsBig = new Pages { Size = (21, 3), Count = 100 };
        buttonsBig.ItemWidth = $"{buttonsBig.Count}".Length + 2;
        buttonsBig.Align((0.5f, 0.9f));
        buttonsBig.OnDisplay(() => maps.SetPages(buttonsBig));
        buttonsBig.OnItemDisplay(item =>
            maps.SetButton(item, isDisplayingSelection: true));

        return new Block[] { icons, pages, buttons, buttonsBig };
    }
}