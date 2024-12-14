using Pure.Engine.Tiles;
using Pure.Engine.Utility;

namespace Pure.Tools.Tiles;

public class MapGenerator
{
    public Noise Noise { get; set; } = Noise.ValueCubic;
    public float Scale { get; set; } = 10f;
    public int Seed { get; set; }
    public (int x, int y) Offset { get; set; }
    public int TargetTileId { get; set; }

    public SortedDictionary<byte, Tile> Elevations { get; } = new();

    public void Apply(TileMap? tilemap)
    {
        if (tilemap == null || Elevations.Count == 0)
            return;

        var (w, h) = tilemap.Size;

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var currTileId = tilemap.TileAt((x, y)).Id;

                if (TargetTileId != currTileId)
                    continue;

                var value = ((float)x + Offset.x, (float)y + Offset.y).ToNoise(Noise, Scale, Seed);
                var currBottom = 0f;
                foreach (var (rule, tile) in Elevations)
                {
                    var currTop = rule / 255f;

                    if (value.IsBetween((currBottom, currTop), (true, true)))
                    {
                        tilemap.SetTile((x, y), tile);
                        break;
                    }

                    currBottom = currTop;
                }
            }
    }
}