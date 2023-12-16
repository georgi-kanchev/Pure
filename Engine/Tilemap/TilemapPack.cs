namespace Pure.Engine.Tilemap;

using System.Runtime.InteropServices;

// should have common properties such as size, views and text configuration
// essentially - the tilemap pack should act as a single tilemap with multiple layers
public class TilemapPack
{
    public int Count
    {
        get => data.Count;
    }
    public (int width, int height) Size { get; }
    public (int x, int y) ViewPosition
    {
        get => viewPos;
        set
        {
            viewPos = value;
            foreach (var map in data)
                map.ViewPosition = value;
        }
    }
    public (int width, int height) ViewSize
    {
        get => viewSz;
        set
        {
            var (w, h) = value;
            var (sw, sh) = Size;

            w = Math.Clamp(w, 1, sw);
            h = Math.Clamp(h, 1, sh);
            viewSz = (w, h);

            foreach (var map in data)
                map.ViewSize = value;
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
        ViewSize = (size.width, size.height);
    }
    public TilemapPack(byte[] bytes)
    {
        var b = Tilemap.Decompress(bytes);
        var offset = 0;
        var count = BitConverter.ToInt32(Get<int>());

        for (var i = 0; i < count; i++)
        {
            var tmapByteCount = BitConverter.ToInt32(Get<int>());
            var bTmap = GetBytesFrom(b, tmapByteCount, ref offset);
            data.Add(new(bTmap));
        }

        return;

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(Count));

        foreach (var t in data)
        {
            var bytes = t.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return Tilemap.Compress(result.ToArray());
    }

    public Tilemap[] ViewUpdate()
    {
        var result = new Tilemap[Count];
        for (var i = 0; i < Count; i++)
            result[i] = data[i].ViewUpdate();

        return result;
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
    public void Fill(params Tile[]? tiles)
    {
        foreach (var t in data)
            t.Fill(tiles);
    }
    public void Flood((int x, int y) position, bool isExactTile, params Tile[] tiles)
    {
        foreach (var map in data)
            map.Flood(position, isExactTile, tiles);
    }

    public int IndexOf(Tilemap? tilemap)
    {
        return tilemap == null || data.Contains(tilemap) == false ? -1 : data.IndexOf(tilemap);
    }
    public bool IsContaining(Tilemap? tilemap)
    {
        return tilemap != null && data.Contains(tilemap);
    }

    public Tile[] TilesAt((int x, int y) position)
    {
        var result = new Tile[Count];
        for (var i = 0; i < data.Count; i++)
            result[i] = data[i].TileAt(position);

        return result;
    }

    public void ConfigureText(
        int lowercase = Tile.LOWERCASE_A,
        int uppercase = Tile.UPPERCASE_A,
        int numbers = Tile.NUMBER_0)
    {
        for (var i = 0; i < Count; i++)
            data[i].ConfigureText(lowercase, uppercase, numbers);
    }
    public void ConfigureText(string symbols, int startId)
    {
        for (var i = 0; i < symbols.Length; i++)
            data[i].ConfigureText(symbols, startId);
    }

    public bool IsInside((int x, int y) position)
    {
        return data[0].IsOverlapping(position);
    }

    public int TileIdFrom(char symbol)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return data[0].TileIdFrom(symbol);
    }
    public int[] TileIDsFrom(string text)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return data[0].TileIdsFrom(text);
    }

    public TilemapPack Copy()
    {
        var result = new TilemapPack { ViewPosition = ViewPosition, ViewSize = ViewSize };
        result.data.Clear();
        result.data.AddRange(data);
        return result;
    }

    public static implicit operator Tilemap[](TilemapPack tilemapPack)
    {
        return tilemapPack.data.ToArray();
    }

#region Backend
    private (int, int) viewSz, viewPos;
    private readonly List<Tilemap> data = new();

    private void ValidateMaps(Tilemap[] tilemaps)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < tilemaps.Length; i++)
        {
            var map = tilemaps[i];
            if (Count > 0 && map.Size != Size)
            {
                var newMap = new Tilemap(Size) { ViewPosition = ViewPosition, ViewSize = ViewSize };
                newMap.SetGroup((0, 0), map);
                map = newMap;
            }

            map.ViewPosition = ViewPosition;
            map.ViewSize = ViewSize;
        }
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
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
                if (index > 0 && index < max &&
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