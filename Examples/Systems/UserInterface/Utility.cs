namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Tilemap;
using Utilities;
using Window;

public static class Utility
{
    public static void Run(UserInterface userInterface, TilemapManager tilemaps)
    {
        while (Window.IsOpen)
        {
            Window.Activate(true);

            Time.Update();
            tilemaps.Fill();

            Input.TilemapSize = tilemaps.Size;
            Input.Update(
                isPressed: Mouse.IsButtonPressed(Mouse.Button.Left),
                position: tilemaps.PointFrom(Mouse.CursorPosition, Window.Size),
                scrollDelta: Mouse.ScrollDelta,
                keysPressed: Keyboard.KeyIDsPressed,
                keysTyped: Keyboard.KeyTyped);

            userInterface.Update();

            Mouse.CursorGraphics = (Mouse.Cursor)Input.MouseCursorResult;

            for (var i = 0; i < tilemaps.Count; i++)
                Window.DrawTiles(tilemaps[i].ToBundle());

            Window.Activate(false);
        }
    }
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
    public static void Clear(TilemapManager tilemaps, Element element, (int from, int to) zOrder)
    {
        for (var i = zOrder.from; i < zOrder.to; i++)
            tilemaps[i].SetRectangle(element.Position, element.Size, Tile.SHADE_TRANSPARENT);
    }
}