namespace Pure.Examples.ExamplesUserInterface;

public static class Palettes
{
    public static Block[] Create(TilemapPack maps)
    {
        Window.Title = "Pure - Palettes Example";

        var palette = new Palette { Size = (13, 3) };
        palette.AlignInside((0.5f, 0.5f));
        palette.OnPick(position => maps.Tilemaps[1].TileAt(((int)position.x, (int)position.y)).Tint);
        palette.OnDisplay(() => maps.SetPalette(palette));
        palette.OnSampleDisplay((sample, color) =>
            maps.Tilemaps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color)));

        //===============

        var hack = new Button((int.MaxValue, int.MaxValue));
        hack.OnDisplay(() =>
        {
            var (x, y) = (palette.Position.x, palette.Position.y - 10);
            var (w, h) = (palette.Size.width, 10);
            var text = $"{Color.Cyan.ToBrush()}This " +
                       $"{Color.Red.ToBrush()}text " +
                       $"{Color.Green.ToBrush()}can " +
                       $"{Color.Orange.ToBrush()}be " +
                       $"{Color.Azure.ToBrush()}used " +
                       $"{Color.Magenta.ToBrush()}for " +
                       $"{Color.Pink.ToBrush()}color " +
                       $"{Color.Yellow.ToBrush()}picking";

            var progress = Time.RuntimeClock / 2f;
            text = text.Constrain((w, h), alignment: Alignment.Center, symbolProgress: progress);
            maps.Tilemaps[1].SetText((x, y), text);
        });

        return new Block[] { hack, palette };
    }
}