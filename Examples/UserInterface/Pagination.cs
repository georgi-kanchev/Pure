namespace Pure.Examples.UserInterface;

public static class Pagination
{
    public static Block[] Create(List<TileMap> maps)
    {
        Window.Title = "Pure - Pages Example";

        var pages = new Pages { Size = (19, 1), Count = 150 };
        pages.Current = pages.Count / 2;
        pages.ItemWidth = $"{pages.Count}".Length;
        pages.AlignInside((0.5f, 0.1f));
        pages.OnDisplay += () => maps.SetPages(pages);
        pages.OnItemDisplay += item => maps.SetPagesItem(pages, item);

        //====================================================

        var icons = new Tile[]
        {
            Tile.ICON_HOME, Tile.ICON_BOOK, Tile.ICON_BELL, Tile.ICON_INFO, Tile.ICON_LIGHTNING, Tile.ICON_STAR,
            Tile.ICON_CAMERA_MOVIE
        };
        var tiles = new Pages((0, 0), icons.Length) { Size = (9, 1), ItemGap = 1 };
        tiles.AlignInside((0.5f, 0.5f));
        tiles.OnDisplay += () => maps.SetPages(tiles);
        tiles.OnItemDisplay += item => maps.SetPagesItemTile(tiles, item, icons);

        //====================================================

        var buttons = new Pages { Size = (21, 3), Count = 100 };
        buttons.ItemWidth = $"{buttons.Count}".Length + 2;
        buttons.AlignInside((0.5f, 0.95f));
        buttons.OnDisplay += () => maps.SetPages(buttons);
        buttons.OnItemDisplay += item => maps.SetButton(item, 1, true);

        return [tiles, pages, buttons];
    }
}