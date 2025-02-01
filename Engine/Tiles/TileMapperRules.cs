namespace Pure.Engine.Tiles;

public class TileMapperRules
{
    public void Apply(TileMap tileMap, Area? area = null, Area? mask = null)
    {
        var result = new Dictionary<(int x, int y), Tile>();
        var ar = area ?? new((0, 0), (tileMap.Size.width, tileMap.Size.height));

        for (var i = ar.X; i < ar.Height; i++)
            for (var j = ar.Y; j < ar.Width; j++)
                foreach (var (tile, match) in rules)
                {
                    if (match.Length != 9)
                        continue;

                    var isMatch = true;

                    for (var k = 0; k < 9; k++)
                    {
                        var (ox, oy) = (k % 3 - 1, k / 3 - 1);
                        var offTile = tileMap.TileAt((j + ox, i + oy));

                        if (match[k] == null || match[k] == offTile.Id)
                            continue;

                        isMatch = false;
                        break;
                    }

                    if (isMatch)
                        result[(j, i)] = tile;
                }

        foreach (var (cell, tile) in result)
            tileMap.SetTile(cell, tile, mask);
    }

    public void AddRule(Tile result, ushort?[] match3X3, bool alsoRotations = true)
    {
        if (match3X3 is not { Length: 9 })
            return;

        TryAdd(result, match3X3);

        if (alsoRotations == false)
            return;

        var match = new[,]
        {
            { match3X3[0], match3X3[1], match3X3[2] },
            { match3X3[3], match3X3[4], match3X3[5] },
            { match3X3[6], match3X3[7], match3X3[8] }
        };
        for (var i = 1; i < 4; i++)
            TryAdd(result.Rotate(i), Flatten(Rotate(match, i)));
    }

#region Backend
    private readonly List<int> hashes = [];
    private readonly List<(Tile tile, ushort?[] match)> rules = [];

    private void TryAdd(Tile tile, ushort?[] match)
    {
        var itemsHash = GetItemsHash(match);

        if (hashes.Contains(itemsHash))
            return;

        hashes.Add(itemsHash);
        rules.Add((tile, match));
    }

    private static int GetItemsHash(ushort?[] array)
    {
        return array.Aggregate(17, (hash, val) => hash * 31 + (val?.GetHashCode() ?? 0));
    }
    private static T[,] Rotate<T>(T[,] matrix, int direction)
    {
        var dir = Math.Abs(direction) % 4;
        if (dir == 0)
            return matrix;

        var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
        var rotated = new T[n, m];

        if (direction > 0)
        {
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    rotated[i, j] = matrix[m - j - 1, i];
            direction--;
            return Rotate(rotated, direction);
        }

        for (var i = 0; i < n; i++)
            for (var j = 0; j < m; j++)
                rotated[i, j] = matrix[j, n - i - 1];

        direction++;
        return Rotate(rotated, direction);
    }
    private static T[] Flatten<T>(T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new T[rows * cols];
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
                result[i * cols + j] = matrix[i, j];
        return result;
    }
#endregion
}