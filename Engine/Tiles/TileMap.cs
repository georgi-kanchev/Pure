using System.Diagnostics.CodeAnalysis;

namespace Pure.Engine.Tiles;

public class TileMap
{
    public (int width, int height) Size { get; }
    public Area View { get; set; }

    public TileMap((int width, int height) size)
    {
        var (w, h) = (Math.Max(size.width, 1), Math.Max(size.height, 1));

        Size = (w, h);
        data = new Tile[w, h];
        bundleCache = new (ushort id, uint tint, byte pose)[w, h];
        ids = new ushort[w, h];
        View = (0, 0, w, h);
    }
    public TileMap(Tile[,]? tileData)
    {
        tileData ??= new Tile[0, 0];

        var w = tileData.GetLength(0);
        var h = tileData.GetLength(1);
        Size = (w, h);
        data = Duplicate(tileData);
        bundleCache = new (ushort id, uint tint, byte pose)[w, h];
        ids = new ushort[w, h];
        View = (0, 0, w, h);

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                bundleCache[j, i] = tileData[j, i];
                ids[j, i] = tileData[j, i].Id;
            }
    }

    public (ushort id, uint tint, byte pose)[,] ToBundle()
    {
        TryRestoreCache();
        return bundleCache;
    }

    public void Flush()
    {
        Array.Clear(data);
        TryRestoreCache();
        Array.Clear(ids);
        Array.Clear(bundleCache);
    }
    public void SetTile((int x, int y) cell, Tile tile, Area? mask = null)
    {
        if (IndicesAreValid(cell, mask) == false)
            return;

        data[cell.x, cell.y] = tile;
        TryRestoreCache();
        ids[cell.x, cell.y] = tile.Id;
        bundleCache[cell.x, cell.y] = tile;
    }

    public bool IsContaining((int x, int y) cell)
    {
        return cell is { x: >= 0, y: >= 0 } &&
               cell.x <= Size.width - 1 &&
               cell.y <= Size.height - 1;
    }

    public Tile TileAt((int x, int y) cell)
    {
        return IndicesAreValid(cell, null) ? data[cell.x, cell.y] : default;
    }
    public Tile[,] TilesIn(Area area)
    {
        var (rx, ry) = (area.X, area.Y);
        var (rw, rh) = (area.Width, area.Height);
        var xStep = rw < 0 ? -1 : 1;
        var yStep = rh < 0 ? -1 : 1;
        var result = new Tile[Math.Abs(rw), Math.Abs(rh)]; // Fixed array dimensions

        for (var x = 0; x < Math.Abs(rw); x++)
            for (var y = 0; y < Math.Abs(rh); y++)
            {
                var currentX = rx + x * xStep - (rw < 0 ? 1 : 0);
                var currentY = ry + y * yStep - (rh < 0 ? 1 : 0);

                result[x, y] = TileAt((currentX, currentY));
            }

        return result;
    }

    public TileMap UpdateView()
    {
        var (vx, vy, vw, vh, _) = View.ToBundle();
        var result = new TileMap((View.Width, View.Height));
        var i = 0;
        for (var x = vx; x != vx + vw; x++)
        {
            var j = 0;
            for (var y = vy; y != vy + vh; y++)
            {
                result.SetTile((i, j), TileAt((x, y)));
                j++;
            }

            i++;
        }

        return result;
    }

    public static implicit operator TileMap?(Tile[,]? data)
    {
        return data == null ? null : new(data);
    }
    public static implicit operator Tile[,]?(TileMap? tileMap)
    {
        return tileMap == null ? null : Duplicate(tileMap.data);
    }
    public static implicit operator (ushort id, uint tint, byte pose)[,]?(TileMap? tileMap)
    {
        return tileMap?.ToBundle();
    }
    public static implicit operator Area(TileMap tileMap)
    {
        return (0, 0, tileMap.Size.width, tileMap.Size.height);
    }
    public static implicit operator Area?(TileMap? tileMap)
    {
        return tileMap == null ? null : (0, 0, tileMap.Size.width, tileMap.Size.height);
    }
    public static implicit operator ushort[,]?(TileMap? tileMap)
    {
        return tileMap?.ids;
    }
    public static implicit operator TileMap?(ushort[,]? ids)
    {
        if (ids == null)
            return null;

        var result = new TileMap((ids.GetLength(0), ids.GetLength(1)));
        for (var i = 0; i < ids.GetLength(0); i++)
            for (var j = 0; j < ids.GetLength(1); j++)
                result.SetTile((i, j), ids[i, j]);

        return result;
    }

#region Backend
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    internal class DoNotSave : Attribute;

    private readonly Tile[,] data;
    [DoNotSave]
    private (ushort id, uint tint, byte pose)[,]? bundleCache;
    [DoNotSave]
    private ushort[,]? ids;

    public static (int, int) FromIndex(int index, (int width, int height) size)
    {
        index = index < 0 ? 0 : index;
        index = index > size.width * size.height - 1 ? size.width * size.height - 1 : index;

        return (index % size.width, index / size.width);
    }
    private bool IndicesAreValid((int x, int y) indices, Area? mask)
    {
        var (x, y) = indices;
        var (w, h) = Size;
        mask ??= new(0, 0, Size.width, Size.height);
        var (mx, my, mw, mh, _) = mask.Value.ToBundle();
        var isInMask = x >= mx && y >= my && x < mx + mw && y < my + mh;

        return isInMask && x >= 0 && y >= 0 && x < w && y < h;
    }
    private static T[,] Duplicate<T>(T[,] array)
    {
        var copy = new T[array.GetLength(0), array.GetLength(1)];
        Array.Copy(array, copy, array.Length);
        return copy;
    }

    [MemberNotNull(nameof(bundleCache), nameof(ids))]
    private void TryRestoreCache()
    {
        if (ids != null && bundleCache != null)
            return;

        var w = data.GetLength(0);
        var h = data.GetLength(1);
        bundleCache = new (ushort id, uint tint, byte pose)[w, h];
        ids = new ushort[w, h];

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                bundleCache[j, i] = data[j, i];
                ids[j, i] = data[j, i].Id;
            }
    }
#endregion
}