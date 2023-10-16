namespace Pure.Examples.ExamplesUserInterface;

public static class Palettes
{
    public static Block[] Create(TilemapPack maps)
    {
        var palette = new Palette { Size = (13, 3) };
        palette.Align((0.5f, 0.6f));
        palette.OnColorPick(position => maps[0].TileAt(((int)position.x, (int)position.y)).Tint);
        palette.OnDisplay(() =>
        {
            SetPalette(maps, palette, zOrder: 0);
            SetPages(maps, palette.Brightness, zOrder: 1);
            palette.OnColorSampleDisplay((sample, color) =>
                maps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color)));
            palette.Brightness.OnItemDisplay(item =>
                SetPagesItem(maps, palette.Brightness, item, zOrder: 2));
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

        return new Block[] { hack, palette };
    }
}