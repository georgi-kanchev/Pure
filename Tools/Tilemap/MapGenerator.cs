using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;

namespace Pure.Tools.Tilemap;

public class MapGenerator
{
    public Engine.Tilemap.Tilemap? Tilemap { get; set; }
    public Dictionary<byte, Tile> DepthRanges { get; } = new();
    public int AffectedTileId { get; set; }

    public Noise Noise { get; set; } = Noise.ValueCubic;
    public float Scale { get; set; } = 10f;
    public int Seed { get; set; }
    public (int x, int y) Offset { get; set; }

    public void Apply()
    {
        if (Tilemap == null || DepthRanges.Count == 0)
            return;

        var (w, h) = Tilemap.Size;

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var currTileId = Tilemap.TileAt((x, y)).Id;

                if (AffectedTileId != currTileId)
                    continue;

                var value = new Point(x + Offset.x, y + Offset.y).ToNoise(Noise, Scale, Seed);
                var currBottom = 0f;
                foreach (var (rule, tile) in DepthRanges)
                {
                    var currTop = rule / 255f;

                    if (value.IsBetween((currBottom, currTop), (true, true)))
                    {
                        Tilemap.SetTile((x, y), tile);
                        break;
                    }

                    currBottom = currTop;
                }
            }
    }
}