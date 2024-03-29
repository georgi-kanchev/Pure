namespace Pure.Examples.ExamplesUserInterface;

public static class Palettes
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Palettes Example";

        var palette = new Palette { Size = (13, 3) };
        palette.AlignInside((0.5f, 0.5f));
        palette.OnPick(position => maps[0].TileAt(((int)position.x, (int)position.y)).Tint);
        palette.OnDisplay(() =>
        {
            maps.SetPalette(palette);
            maps.SetPages(palette.Brightness);
            palette.OnSampleDisplay((sample, color) =>
                maps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color)));
            palette.Brightness.OnItemDisplay(item =>
                maps.SetPagesItem(palette.Brightness, item, zOrder: 2));
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
            var position = (palette.Position.x, palette.Position.y - 8);
            var size = (palette.Size.width, 6);
            var rect = (position.x, position.Item2, size.width, size.width);
            maps[0].SetTextRectangle(rect, "This text can be used to pick colors from",
                alignment: Alignment.Center);

            for (var i = 0; i < colors.Length; i++)
                maps[0].SetTextRectangleTint(rect, text: $"{(char)('a' + i)}", colors[i]);
        });

        return new Block[] { hack, palette };
    }
}