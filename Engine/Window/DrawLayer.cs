namespace Pure.Engine.Window;

using SFML.System;

internal class DrawLayer
{
    // apparently SFML.Texture.Size calls are a bit costly hence cache
    public Vector2u graphicsSize;

    public string graphicsPath = "default";
    public int tileIdFull = 10;
    public (int w, int h) tileGap, tileSize = (8, 8);
    public (float x, float y) offset;
    public float zoom = 1f;

    public (int w, int h) tilemapSize;
    public (int w, int h) tilemapCellCount;
}