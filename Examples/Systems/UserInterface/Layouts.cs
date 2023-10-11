namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;
using Tilemap;
using static Utility;

public static class Layouts
{
    public static Element[] Create(TilemapManager maps)
    {
        var layoutEmpty = new Layout();
        layoutEmpty.Cut(index: 0, side: Layout.CutSide.Right, rate: 0.4f);
        layoutEmpty.Cut(index: 0, side: Layout.CutSide.Bottom, rate: 0.6f);
        layoutEmpty.Cut(index: 1, side: Layout.CutSide.Top, rate: 0.25f);
        layoutEmpty.Cut(index: 1, side: Layout.CutSide.Bottom, rate: 0.4f);
        layoutEmpty.OnDisplaySegment((segment, index) => DisplaySegment(maps, segment, index));

        return new Element[] { layoutEmpty };
    }

    private static void DisplaySegment(
        TilemapManager maps,
        (int x, int y, int width, int height) segment,
        int index)
    {
        var colors = new uint[]
        {
            Color.Red, Color.Blue, Color.Brown, Color.Violet, Color.Gray,
            Color.Orange, Color.Cyan, Color.Black, Color.Azure, Color.Purple,
            Color.Magenta, Color.Green, Color.Pink, Color.Yellow
        };
        maps[0].SetBox(
            position: (segment.x, segment.y),
            size: (segment.width, segment.height),
            tileFill: new(Tile.SHADE_OPAQUE, colors[index]),
            cornerTileId: Tile.BOX_CORNER_ROUND,
            borderTileId: Tile.SHADE_OPAQUE,
            borderTint: colors[index]);

        maps[1].SetTextRectangle(
            position: (segment.x, segment.y),
            size: (segment.width, segment.height),
            text: index.ToString(),
            alignment: Tilemap.Alignment.Center);
    }
}