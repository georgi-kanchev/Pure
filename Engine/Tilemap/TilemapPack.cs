namespace Pure.Engine.Tilemap;

// should have common properties such as size, views and text configuration
// essentially - the tilemap pack should act as a single tilemap with multiple layers
public class TilemapPack
{
    public int Count
    {
        get => data.Count;
    }
    public (int width, int height) Size { get; }
    public Area View
    {
        get => view;
        set
        {
            value.Width = Math.Max(value.Width, 1);
            value.Height = Math.Max(value.Height, 1);

            view = value;
            foreach (var map in data)
                map.View = value;
        }
    }

    public Tilemap this[int index]
    {
        get => data[index];
    }

    public TilemapPack()
    {
    }
    public TilemapPack(int count, (int width, int height) size)
    {
        for (var i = 0; i < Math.Max(count, 1); i++)
            data.Add(new(size));

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
            var bTmap = Tilemap.GetBytesFrom(b, GetInt(), ref offset);
            data.Add(new(bTmap));
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
        result.AddRange(BitConverter.GetBytes(Count));
        result.AddRange(BitConverter.GetBytes(Size.width));
        result.AddRange(BitConverter.GetBytes(Size.height));
        result.AddRange(BitConverter.GetBytes(View.X));
        result.AddRange(BitConverter.GetBytes(View.Y));
        result.AddRange(BitConverter.GetBytes(View.Width));
        result.AddRange(BitConverter.GetBytes(View.Height));

        foreach (var t in data)
        {
            var bytes = t.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return Tilemap.Compress(result.ToArray());
    }

    public void Shift(int offset, params Tilemap[]? tilemaps)
    {
        if (tilemaps == null || tilemaps.Length == 0 || offset == 0)
            return;

        Shift(data, offset, tilemaps);
    }

    public void Add(params Tilemap[]? tilemaps)
    {
        if (tilemaps == null || tilemaps.Length == 0)
            return;

        ValidateMaps(tilemaps);
        data.AddRange(tilemaps);
    }
    public void Insert(int index, params Tilemap[]? tilemaps)
    {
        if (tilemaps == null || tilemaps.Length == 0)
            return;

        ValidateMaps(tilemaps);
        data.InsertRange(index, tilemaps);
    }
    public void Remove(params Tilemap[]? tilemaps)
    {
        if (tilemaps == null || tilemaps.Length == 0)
            return;

        foreach (var map in tilemaps)
            data.Remove(map);
    }
    public void Clear()
    {
        data.Clear();
    }

    public void Flush()
    {
        foreach (var t in data)
            t.Flush();
    }
    public void Fill(Area? mask = null, params Tile[]? tiles)
    {
        foreach (var t in data)
            t.Fill(mask, tiles);
    }
    public void Flood((int x, int y) cell, bool exactTile, Area? mask = null, params Tile[] tiles)
    {
        foreach (var map in data)
            map.Flood(cell, exactTile, mask, tiles);
    }

    public void ConfigureText(int lowercase = Tile.LOWERCASE_A, int uppercase = Tile.UPPERCASE_A, int numbers = Tile.NUMBER_0)
    {
        for (var i = 0; i < Count; i++)
            data[i].ConfigureText(lowercase, uppercase, numbers);
    }
    public void ConfigureText(string symbols, int leftmostTileId)
    {
        for (var i = 0; i < symbols.Length; i++)
            data[i].ConfigureText(symbols, leftmostTileId);
    }

    public int IndexOf(Tilemap? tilemap)
    {
        return tilemap == null || data.Contains(tilemap) == false ? -1 : data.IndexOf(tilemap);
    }
    public bool IsOverlapping((int x, int y) cell)
    {
        return data.Count > 0 && data[0].IsOverlapping(cell);
    }
    public bool IsContaining(Tilemap? tilemap)
    {
        return tilemap != null && data.Contains(tilemap);
    }

    public Tile[] TilesAt((int x, int y) cell)
    {
        var result = new Tile[Count];
        for (var i = 0; i < data.Count; i++)
            result[i] = data[i].TileAt(cell);

        return result;
    }
    public int TileIdFrom(char symbol)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return data[0].TileIdFrom(symbol);
    }
    public int[] TileIdsFrom(string text)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return data[0].TileIdsFrom(text);
    }

    public Tilemap[] ViewUpdate()
    {
        var result = new Tilemap[Count];
        for (var i = 0; i < Count; i++)
            result[i] = data[i].ViewUpdate();

        return result;
    }

    public TilemapPack Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator Tilemap[](TilemapPack tilemapPack)
    {
        return tilemapPack.data.ToArray();
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
    private readonly List<Tilemap> data = new();

    private void ValidateMaps(Tilemap[] tilemaps)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < tilemaps.Length; i++)
        {
            var map = tilemaps[i];
            if (Count > 0 && map.Size != Size)
            {
                var newMap = new Tilemap(Size) { View = View };
                newMap.SetGroup((0, 0), map);
                map = newMap;
            }

            map.View = View;
        }
    }
    private static void Shift<T>(IList<T> collection, int offset, params T[]? items)
    {
        if (items == null || items.Length == 0 || items.Length == collection.Count || offset == 0)
            return;

        if (collection is List<T> list)
        {
            var results = new List<int>();
            var indexes = new List<int>();
            var itemList = items.ToList();
            var prevTargetIndex = -1;
            var max = list.Count - 1;

            foreach (var item in items)
                indexes.Add(list.IndexOf(item));

            indexes.Sort();

            if (offset > 0)
                indexes.Reverse();

            foreach (var currIndex in indexes)
            {
                var item = list[currIndex];

                if (item == null || list.Contains(item) == false)
                    continue;

                var index = list.IndexOf(item);
                var targetIndex = Math.Clamp(index + offset, 0, max);

                // prevent items order change
                if (index > 0 &&
                    index < max &&
                    itemList.Contains(list[index + (offset > 0 ? 1 : -1)]))
                    continue;

                // prevent overshooting of multiple items which would change the order
                var isOvershooting = (targetIndex == 0 && prevTargetIndex == 0) ||
                                     (targetIndex == max && prevTargetIndex == max) ||
                                     results.Contains(targetIndex);
                var i = indexes.IndexOf(list.IndexOf(item));
                var result = isOvershooting ? offset < 0 ? i : max - i : targetIndex;

                list.Remove(item);
                list.Insert(result, item);
                prevTargetIndex = targetIndex;
                results.Add(result);
            }

            return;
        }

        // if not a list then convert it
        var tempList = collection.ToList();
        Shift(tempList, offset, items);

        for (var i = 0; i < tempList.Count; i++)
            collection[i] = tempList[i];
    }
#endregion
}