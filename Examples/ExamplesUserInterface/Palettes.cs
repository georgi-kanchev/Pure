namespace Pure.Examples.ExamplesUserInterface;

public static class Palettes
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Palettes Example";

        var palette = new Palette { Size = (13, 3) };
        palette.AlignInside((0.5f, 0.5f));
        palette.OnPick(position => maps[1].TileAt(((int)position.x, (int)position.y)).Tint);
        palette.OnDisplay(() =>
        {
            maps.SetPalette(palette);
            maps.SetPages(palette.Brightness);
        });
        palette.OnSampleDisplay((sample, color) =>
            maps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color)));
        palette.Brightness.OnItemDisplay(item =>
            maps.SetPagesItem(palette.Brightness, item, zOrder: 2));

        //===============

        var hack = new Button((int.MaxValue, int.MaxValue));
        hack.OnDisplay(() =>
        {
            var (x, y) = (palette.Position.x, palette.Position.y - 10);
            var (w, h) = (palette.Size.width, 10);
            var rect = (x, y, w, h);
            var text = $"{Color.Blue.ToBrush()}This " +
                       $"{Color.Red.ToBrush()}text " +
                       $"{Color.Green.ToBrush()}can " +
                       $"{Color.Orange.ToBrush()}be " +
                       $"{Color.Azure.ToBrush()}used " +
                       $"{Color.Purple.ToBrush()}to " +
                       $"{Color.Pink.ToBrush()}pick " +
                       $"{Color.Yellow.ToBrush()}colors " +
                       $"{Color.Brown.ToBrush()}from";

            maps[1].SetTextArea(rect, text, symbolProgress: Time.RuntimeClock / 5f,
                alignment: Alignment.Center);
            maps[1].SetTextAreaTint(rect, text);
        });

        return new Block[] { hack, palette };
    }
}