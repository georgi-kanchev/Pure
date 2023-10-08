namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;

using Tilemap;

using static Utility;

public static class InputBoxes
{
	public static Element[] Create(TilemapManager maps)
	{
		var inputBox = new InputBox();

		inputBox.Align((0.5f, 0.5f));
		inputBox.OnDisplay(() => DisplayInputBox(maps, inputBox, 0));

		return new[] { inputBox };
	}

	public static void DisplayInputBox(TilemapManager maps, InputBox inputBox, int mapIndex)
	{
		var e = inputBox;
		var bgColor = Color.Gray.ToDark(0.4f);

		Clear(maps, inputBox);
		maps[mapIndex].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, bgColor));
		maps[mapIndex].SetTextRectangle(e.Position, e.Size, e.Selection,
			e.IsFocused ? Color.Blue : Color.Blue.ToBright(), false);
		maps[mapIndex + 1].SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

		if(string.IsNullOrWhiteSpace(e.Value))
			maps[mapIndex + 1].SetTextRectangle(e.Position, e.Size, e.Placeholder,
				tint: Color.Gray.ToBright(),
				isWordWrapping: false,
				alignment: Tilemap.Alignment.TopLeft);

		if(e.IsCursorVisible)
			maps[mapIndex + 2].SetTile(e.PositionFromIndices(e.CursorIndices),
				new(Tile.SHAPE_LINE, Color.White, 2));
	}
}