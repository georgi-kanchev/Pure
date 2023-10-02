namespace Pure.Examples.Systems.UserInterface;

using Utilities;
using Tilemap;
using Pure.UserInterface;

public static class Utility
{
    public static Color GetColor(Element element, Color baseColor)
    {
        if (element.IsDisabled) return baseColor;
        if (element.IsPressedAndHeld) return baseColor.ToDark();
        else if (element.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
    public static void SetBackground(Tilemap map, Element element, float shade = 0.5f)
    {
        var e = element;
        var color = Color.Gray.ToDark(shade);
        var tile = new Tile(Tile.SHADE_OPAQUE, color);

        map.SetBox(e.Position, e.Size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
}