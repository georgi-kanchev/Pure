namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using static Utility;

public static class Palettes
{
    public static Element[] Create(TilemapManager maps)
    {
        var palette = new Palette { Size = (13, 3) };
        palette.Align((0.5f, 0.6f));
        palette.OnColorPick(position => maps[0].TileAt(((int)position.x, (int)position.y)).Tint);
        palette.OnDisplay(() =>
        {
            DisplayPalette(maps, palette, zOrder: 0);
            Pagination.DisplayPages(maps, palette.Brightness, zOrder: 1);
            palette.OnColorSampleDisplay((sample, color) =>
                maps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color)));
            palette.Brightness.OnItemDisplay(item =>
                Pagination.DisplayPagesItem(maps, palette.Brightness, item, zOrder: 2));
        });

        //===============

        var colors = new uint[]
        {
            Color.Red, Color.Blue, Color.Brown, Color.Violet, Color.Gray,
            Color.Orange, Color.Cyan, Color.Azure, Color.Purple,
            Color.Magenta, Color.Green, Color.Pink, Color.Yellow,
            Color.Red, Color.Blue, Color.Brown, Color.Violet, Color.Gray,
            Color.Orange, Color.Cyan, Color.Azure, Color.Purple,
            Color.Magenta, Color.Green, Color.Pink, Color.Yellow
        };
        var hack = new Button((int.MaxValue, int.MaxValue));
        hack.OnDisplay(() =>
        {
            var position = (palette.Position.x, palette.Position.y - 6);
            var size = (palette.Size.width, 6);
            maps[0].SetTextRectangle(
                position,
                size,
                "This text can be used to pick colors from with the + button!",
                alignment: Tilemap.Alignment.Center);

            for (var i = 0; i < colors.Length; i++)
                maps[0].SetTextRectangleTint(
                    position,
                    size,
                    text: $"{(char)('a' + i)}",
                    colors[i]);
        });

        return new Element[] { hack, palette };
    }
    public static void DisplayPalette(TilemapManager maps, Palette palette, int zOrder)
    {
        var p = palette;
        var tile = new Tile(Tile.SHADE_OPAQUE, GetColor(p.Opacity, Color.Gray.ToBright()));

        Clear(maps, p, zOrder: (zOrder, zOrder + 2));

        maps[zOrder].SetRectangle(
            position: p.Opacity.Position,
            size: p.Opacity.Size,
            tile: new(Tile.SHADE_5, Color.Gray.ToDark()));
        maps[zOrder + 1].SetBar(
            p.Opacity.Position,
            tileIdEdge: Tile.BAR_BIG_EDGE,
            tileId: Tile.BAR_BIG_STRAIGHT,
            p.SelectedColor,
            p.Opacity.Size.width);
        maps[zOrder + 1].SetTile(p.Opacity.Handle.Position, tile);

        maps[zOrder + 1].SetTile(p.Pick.Position, new(Tile.MATH_PLUS, GetColor(p.Pick, Color.Gray)));
    }
}