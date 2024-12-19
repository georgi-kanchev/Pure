namespace Pure.Engine.Tiles;

// should have common properties such as size, views and text configuration
// essentially - the tile map pack should act as a single tile map with multiple layers
public class TileMapPack
{
    public List<TileMap> TileMaps { get; } = [];

    public (int width, int height) Size { get; }
    public Area View
    {
        get => view;
        set
        {
            value.Width = Math.Max(value.Width, 1);
            value.Height = Math.Max(value.Height, 1);

            view = value;
            foreach (var map in TileMaps)
                map.View = value;
        }
    }

    public TileMapPack()
    {
    }
    public TileMapPack(int count, (int width, int height) size)
    {
        for (var i = 0; i < Math.Max(count, 1); i++)
            TileMaps.Add(new(size));

        Size = size;
        View = (0, 0, size.width, size.height);
    }

    public void Flush()
    {
        foreach (var t in TileMaps)
            t.Flush();
    }

    public void ConfigureText(ushort lowercase = Tile.LOWERCASE_A, ushort uppercase = Tile.UPPERCASE_A, ushort numbers = Tile.NUMBER_0)
    {
        foreach (var map in TileMaps)
            map.ConfigureText(lowercase, uppercase, numbers);
    }
    public void ConfigureText(ushort firstTileId, string symbols)
    {
        foreach (var map in TileMaps)
            map.ConfigureText(firstTileId, symbols);
    }

    public bool IsOverlapping((int x, int y) cell)
    {
        return TileMaps.Count > 0 && TileMaps[0].IsContaining(cell);
    }

    public Tile[] TilesAt((int x, int y) cell)
    {
        var result = new Tile[TileMaps.Count];
        for (var i = 0; i < TileMaps.Count; i++)
            result[i] = TileMaps[i].TileAt(cell);

        return result;
    }
    public ushort TileIdFrom(char symbol)
    {
        // not very reliable method since the user might configure text on each
        // tile map individually but the idea of the tile map pack is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return TileMaps[0].TileIdFrom(symbol);
    }
    public ushort[] TileIdsFrom(string text)
    {
        // not very reliable method since the user might configure text on each
        // tile map individually but the idea of the tile map pack is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return TileMaps[0].TileIdsFrom(text);
    }

    public TileMap[] ViewUpdate()
    {
        var result = new TileMap[TileMaps.Count];
        for (var i = 0; i < TileMaps.Count; i++)
            result[i] = TileMaps[i].UpdateView();

        return result;
    }

    public static implicit operator TileMap[](TileMapPack tileMapPack)
    {
        return tileMapPack.TileMaps.ToArray();
    }

#region Backend
    private Area view;
#endregion
}