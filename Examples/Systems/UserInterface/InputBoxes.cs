namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;
using Tilemap;
using static Utility;

public static class InputBoxes
{
    public static Element[] Create(TilemapManager maps)
    {
        var singleLine = new InputBox();
        var multiLine = new InputBox { IsMultiLine = true, Size = (12, 6) };

        singleLine.Align((0.5f, 0.3f));
        multiLine.Align((0.5f, 0.6f));
        singleLine.OnDisplay(() => DisplayInputBox(maps, singleLine, 0));
        multiLine.OnDisplay(() => DisplayInputBox(maps, multiLine, 0));

        return new Element[] { singleLine, multiLine };
    }

    public static void DisplayInputBox(TilemapManager maps, InputBox inputBox, int backgroundIndex)
    {
        var e = inputBox;
        var bgColor = Color.Gray.ToDark(0.4f);

        Clear(maps, inputBox);
        maps[backgroundIndex].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, bgColor));
        maps[backgroundIndex].SetTextRectangle(e.Position, e.Size, e.Selection,
            e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
        maps[backgroundIndex + 1].SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(e.Value))
            maps[backgroundIndex + 1].SetTextRectangle(e.Position, e.Size, e.Placeholder,
                tint: Color.Gray.ToBright(),
                isWordWrapping: false,
                alignment: Tilemap.Alignment.TopLeft);

        if (e.IsCursorVisible)
            maps[backgroundIndex + 2].SetTile(e.PositionFromIndices(e.CursorIndices),
                new(Tile.SHAPE_LINE, Color.White, 2));
    }
}