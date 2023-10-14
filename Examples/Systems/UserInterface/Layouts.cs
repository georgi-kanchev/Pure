using Pure.Window;

namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;
using Tilemap;
using static Utility;

public static class Layouts
{
    public static Element[] Create(TilemapManager maps)
    {
        var panel = new Panel { Size = (21, 21) };
        panel.Align((0.9f, 0.5f));

        //============

        var layoutElements = new Element[] { new Button(), new Button(), new Slider(), new InputBox() };
        var layoutFull = new Layout { Size = (20, 20) };
        layoutFull.Align((0.9f, 0.5f));
        layoutFull.Cut(index: 0, side: Layout.CutSide.Right, rate: 0.4f);
        layoutFull.Cut(index: 0, side: Layout.CutSide.Bottom, rate: 0.3f);
        layoutFull.Cut(index: 2, side: Layout.CutSide.Top, rate: 0.5f);
        layoutFull.Cut(index: 1, side: Layout.CutSide.Top, rate: 0.4f);
        layoutFull.OnDisplaySegment((segment, index) =>
        {
            if (index == 0)
            {
                DisplaySegment(maps, segment, index);
                maps[1].SetTextRectangle(
                    position: (segment.x, segment.y),
                    size: (segment.width, segment.height),
                    text: "A very meaningful text",
                    alignment: Tilemap.Alignment.Center);
                return;
            }

            var e = layoutElements[index - 1];
            e.Position = (segment.x, segment.y);
            e.Size = (segment.width, segment.height);

            if (e is Button button)
                ButtonsAndCheckboxes.DisplayButton(maps, button);
            else if (e is Slider slider)
                SlidersAndScrolls.DisplaySlider(maps, slider);
            else if (e is InputBox inputBox)
                InputBoxes.DisplayInputBox(maps, inputBox, 0);
        });

        //============

        var layoutEmpty = new Layout();
        layoutEmpty.Align((0.1f, 0.5f));
        layoutEmpty.Cut(index: 0, side: Layout.CutSide.Right, rate: 0.4f);
        layoutEmpty.Cut(index: 0, side: Layout.CutSide.Bottom, rate: 0.6f);
        layoutEmpty.Cut(index: 1, side: Layout.CutSide.Top, rate: 0.25f);
        layoutEmpty.Cut(index: 1, side: Layout.CutSide.Bottom, rate: 0.4f);
        layoutEmpty.OnDisplaySegment((segment, index) => DisplaySegment(maps, segment, index));

        var elements = new List<Element>
        {
            layoutEmpty,
            layoutFull
        };
        elements.AddRange(layoutElements);
        return elements.ToArray();
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