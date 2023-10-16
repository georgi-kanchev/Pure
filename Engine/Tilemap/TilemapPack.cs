namespace Pure.Engine.Tilemap;

using System.Runtime.InteropServices;

public class TilemapPack
{
    public int Count
    {
        get => tilemaps.Length;
    }
    public (int width, int height) Size
    {
        get;
        private set;
    }
    public (int x, int y, int width, int height) Camera
    {
        get;
        set;
    }

    public Tilemap this[int index]
    {
        get => tilemaps[index];
    }

    public TilemapPack(byte[] bytes)
    {
        var b = Tilemap.Decompress(bytes);
        var offset = 0;
        tilemaps = new Tilemap[BitConverter.ToInt32(Get<int>())];
        Size = (BitConverter.ToInt32(Get<int>()), BitConverter.ToInt32(Get<int>()));
        Camera = (BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()));

        for (var i = 0; i < tilemaps.Length; i++)
        {
            var tmapByteCount = BitConverter.ToInt32(Get<int>());
            tilemaps[i] = new Tilemap(GetBytesFrom(b, tmapByteCount, ref offset));
        }

        return;

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public TilemapPack(int count, (int width, int height) size)
    {
        Size = size;

        count = Math.Max(count, 1);

        tilemaps = new Tilemap[count];
        for (var i = 0; i < count; i++)
            tilemaps[i] = new(size);
    }

    public Tilemap[] CameraUpdate()
    {
        var result = new Tilemap[Count];
        for (var i = 0; i < Count; i++)
        {
            var tmap = tilemaps[i];

            // keep the original tilemap camera props, use the manager ones
            var prevCam = tmap.Camera;
            tmap.Camera = Camera;

            result[i] = tmap.CameraUpdate();

            // and revert
            tmap.Camera = prevCam;
        }

        return result;
    }
    public void Fill(Tile withTile = default)
    {
        foreach (var t in tilemaps)
            t.Fill(withTile);
    }

    public Tile[] TilesAt((int x, int y) position)
    {
        var result = new Tile[Count];
        for (var i = 0; i < tilemaps.Length; i++)
            result[i] = tilemaps[i].TileAt(position);

        return result;
    }
    public (float x, float y) PointFrom(
        (int x, int y) pixelPosition,
        (int width, int height) windowSize,
        bool isAccountingForCamera = true)
    {
        // cannot use first tilemap to not duplicate code since the used camera props are
        // the ones on the manager
        var x = Map(pixelPosition.x, 0, windowSize.width, 0, Size.width);
        var y = Map(pixelPosition.y, 0, windowSize.height, 0, Size.height);

        if (isAccountingForCamera)
        {
            x += Camera.x;
            y += Camera.y;
        }

        return (x, y);
    }

    public void ConfigureText(
        int lowercase = Tile.LOWERCASE_A,
        int uppercase = Tile.UPPERCASE_A,
        int numbers = Tile.NUMBER_0)
    {
        for (var i = 0; i < Count; i++)
            tilemaps[i].ConfigureText(lowercase, uppercase, numbers);
    }
    public void ConfigureText(string symbols, int startId)
    {
        for (var i = 0; i < symbols.Length; i++)
            tilemaps[i].ConfigureText(symbols, startId);
    }

    public bool IsInside((int x, int y) position, (int width, int height) outsideMargin = default)
    {
        return tilemaps[0].IsContaining(position, outsideMargin);
    }

    public int TileIdFrom(char symbol)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return tilemaps[0].TileIdFrom(symbol);
    }
    public int[] TileIDsFrom(string text)
    {
        // not very reliable method since the user might configure text on each
        // tilemap individually but the idea of the tilemap manager is to bundle
        // multiple tilemaps with COMMON properties, therefore same text configuration
        return tilemaps[0].TileIdsFrom(text);
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(Count));
        result.AddRange(BitConverter.GetBytes(Size.width));
        result.AddRange(BitConverter.GetBytes(Size.height));
        result.AddRange(BitConverter.GetBytes(Camera.x));
        result.AddRange(BitConverter.GetBytes(Camera.y));
        result.AddRange(BitConverter.GetBytes(Camera.width));
        result.AddRange(BitConverter.GetBytes(Camera.height));

        foreach (var t in tilemaps)
        {
            var bytes = t.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return Tilemap.Compress(result.ToArray());
    }

#region Backend
    // save format in sectors
    // [amount of bytes]		- data
    // --------------------------------
    // [4]						- tilemaps count
    // [4]						- width
    // [4]						- height
    // [4]						- camera x
    // [4]						- camera y
    // [4]						- camera width
    // [4]						- camera height
    // = = = = = = (sector 1)
    // [4]						- tile bundles size
    // [tile bundles size]		- tile bundles array
    // = = = = = = (sector 2)
    // [4]						- tile bundles size
    // [tile bundles size]		- tile bundles array
    // = = = = = = (sector 3)
    // ... up to sector [tilemaps count]

    private readonly Tilemap[] tilemaps;

    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}