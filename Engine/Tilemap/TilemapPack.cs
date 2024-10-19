namespace Pure.Engine.Tilemap;

// should have common properties such as size, views and text configuration
// essentially - the tilemap pack should act as a single tilemap with multiple layers
public class TilemapPack
{
    public List<Tilemap> Tilemaps { get; } = [];

    public (int width, int height) Size { get; }
    public Area View
    {
        get => view;
        set
        {
            value.Width = Math.Max(value.Width, 1);
            value.Height = Math.Max(value.Height, 1);

            view = value;
            foreach (var map in Tilemaps)
                map.View = value;
        }
    }

    public TilemapPack()
    {
    }
    public TilemapPack(int count, (int width, int height) size)
    {
        for (var i = 0; i < Math.Max(count, 1); i++)
            Tilemaps.Add(new(size));

        Size = size;
        View = (0, 0, size.width, size.height);
    }
    public TilemapPack(byte[] bytes)
    {
        var b = Tilemap.Decompress(bytes);
        var offset = 0;
        var count = GetInt();
        Size = (GetInt(), GetInt());
        View = (GetInt(), GetInt(), GetInt(), GetInt());

        for (var i = 0; i < count; i++)
        {
            var bTilemap = Tilemap.GetBytesFrom(b, GetInt(), ref offset);
            Tilemaps.Add(new(bTilemap));
        }

        int GetInt()
        {
            return BitConverter.ToInt32(Tilemap.GetBytesFrom(b, 4, ref offset));
        }
    }
    public TilemapPack(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(Tilemaps.Count));
        result.AddRange(BitConverter.GetBytes(Size.width));
        result.AddRange(BitConverter.GetBytes(Size.height));
        result.AddRange(BitConverter.GetBytes(View.X));
        result.AddRange(BitConverter.GetBytes(View.Y));
        result.AddRange(BitConverter.GetBytes(View.Width));
        result.AddRange(BitConverter.GetBytes(View.Height));

        foreach (var t in Tilemaps)
        {
            var bytes = t.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return Tilemap.Compress(result.ToArray());
    }

    public void Flush()
    {
        foreach (var t in Tilemaps)
            t.Flush();
    }
    public void Fill(Area? mask = null, params Tile[]? tiles)
    {
        foreach (var t in Tilemaps)
            t.Fill(mask, tiles);
    }
    public void Flood((int x, int y) cell, bool exactTile, Area? mask = null, params Tile[] tiles)
    {
        foreach (var map in Tilemaps)
            map.Flood(cell, exactTile, mask, tiles);
    }

    public void ConfigureText(int lowercase = Tile.LOWERCASE_A, int uppercase = Tile.UPPERCASE_A, int numbers = Tile.NUMBER_0)
    {
        foreach (var map in Tilemaps)
            map.ConfigureText(lowercase, uppercase, numbers);
    }
    public void ConfigureText(string symbols, int firstTileId)
    {
        foreach (var map in Tilemaps)
            map.ConfigureText(symbols, firstTileId);
    }

    public bool IsOverlapping((int x, int y) cell)
    {
        return Tilemaps.Count > 0 && Tilemaps[0].IsOverlapping(cell);
    }

    public Tile[] TilesAt((int x, int y) cell)
    {
        var result = new Tile[Tilemaps.Count];
        for (var i = 0; i < Tilemaps.Count; i++)
            result[i] = Tilemaps[i].TileAt(cell);

        return result;
    }
    public int TileIdFrom(char symbol)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return Tilemaps[0].TileIdFrom(symbol);
    }
    public int[] TileIdsFrom(string text)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return Tilemaps[0].TileIdsFrom(text);
    }

    public Tilemap[] ViewUpdate()
    {
        var result = new Tilemap[Tilemaps.Count];
        for (var i = 0; i < Tilemaps.Count; i++)
            result[i] = Tilemaps[i].ViewUpdate();

        return result;
    }

    public TilemapPack Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator Tilemap[](TilemapPack tilemapPack)
    {
        return tilemapPack.Tilemaps.ToArray();
    }
    public static implicit operator byte[](TilemapPack tilemapPack)
    {
        return tilemapPack.ToBytes();
    }
    public static implicit operator TilemapPack(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private Area view;
#endregion
}