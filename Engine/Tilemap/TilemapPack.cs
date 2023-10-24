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
    public (int x, int y, int width, int height) View
    {
        get => view;
        set
        {
            var (x, y, w, h) = value;
            var (sw, sh) = Size;

            w = Math.Clamp(w, 1, sw);
            h = Math.Clamp(h, 1, sh);
            x = Math.Clamp(x, 0, sw - w);
            y = Math.Clamp(y, 0, sh - h);
            view = (x, y, w, h);
        }
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
        View = (BitConverter.ToInt32(Get<int>()),
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
        View = (0, 0, size.width, size.height);

        count = Math.Max(count, 1);

        tilemaps = new Tilemap[count];
        for (var i = 0; i < count; i++)
            tilemaps[i] = new(size);
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(Count));
        result.AddRange(BitConverter.GetBytes(Size.width));
        result.AddRange(BitConverter.GetBytes(Size.height));
        result.AddRange(BitConverter.GetBytes(View.x));
        result.AddRange(BitConverter.GetBytes(View.y));
        result.AddRange(BitConverter.GetBytes(View.width));
        result.AddRange(BitConverter.GetBytes(View.height));

        foreach (var t in tilemaps)
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
        {
            var tmap = tilemaps[i];

            // keep the original tilemap view props, use the manager ones
            var prevCam = tmap.View;
            tmap.View = View;

            result[i] = tmap.ViewUpdate();

            // and revert
            tmap.View = prevCam;
        }

        return result;
    }

    public void Clear()
    {
        foreach (var t in tilemaps)
            t.Clear();
    }
    public void Fill(Tile tile = default)
    {
        foreach (var t in tilemaps)
            t.Fill(tile);
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
        // cannot use first tilemap to not duplicate code since the used view props are
        // the ones on the manager
        var (w, h) = isAccountingForCamera ? (View.width, View.height) : Size;
        var x = Map(pixelPosition.x, 0, windowSize.width, 0, w);
        var y = Map(pixelPosition.y, 0, windowSize.height, 0, h);

        return isAccountingForCamera == false ?
            (x, y) :
            (x + View.x, y + View.y);
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

#region Backend
    private readonly Tilemap[] tilemaps;
    private (int x, int y, int width, int height) view;

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