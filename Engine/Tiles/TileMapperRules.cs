namespace Pure.Engine.Tiles;

public class TileMapperRules
{
    public int Count
    {
        get => data.Count;
    }
    public int CountWithRotations
    {
        get => hashes.Count;
    }

    public void Add(Tile result, ushort?[] match3X3, bool withRotations = true, int atIndex = 0)
    {
        if (match3X3 is not { Length: 9 })
            return;

        atIndex = Math.Clamp(atIndex, 0, data.Count);

        TryAdd(result, match3X3, atIndex, 0);

        if (withRotations == false)
            return;

        var match = new[,]
        {
            { match3X3[0], match3X3[1], match3X3[2] },
            { match3X3[3], match3X3[4], match3X3[5] },
            { match3X3[6], match3X3[7], match3X3[8] }
        };
        for (var i = 1; i < 4; i++)
            TryAdd(result.Rotate(i), Flatten(Rotate(match, i)), atIndex, i);
    }
    public void Remove(int atIndex)
    {
        if (atIndex < 0 || data.Count <= atIndex)
            return;

        for (var i = 0; i < data[atIndex].Length; i++)
        {
            if (data[atIndex].GetValue(i) is not Rule rule)
                continue;

            var itemsHash = GetItemsHash(rule.match);
            hashes.Remove(itemsHash);
        }

        data.RemoveAt(atIndex);
    }
    public void Move(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || data.Count <= fromIndex)
            return;

        var rotations = data[fromIndex];
        data.Remove(rotations);
        data.Insert(toIndex, rotations);
    }

    public (Tile result, ushort?[] match3X3)? Get(int atIndex, int withRotation = 0)
    {
        if (atIndex < 0 || data.Count <= atIndex)
            return null;

        withRotation %= GetRotationCount(atIndex);
        var rule = data[atIndex][withRotation]!;
        return (rule.result, rule.match);
    }
    public int GetRotationCount(int atIndex)
    {
        if (atIndex < 0 || data.Count <= atIndex)
            return 0;

        var rotations = data[atIndex];
        var totalRotations = 1;

        for (var i = 1; i < rotations.Length; i++)
            if (rotations[i] != null)
                totalRotations++;

        return totalRotations;
    }

    public void Apply(TileMap tileMap, Area? area = null, Area? mask = null)
    {
        var result = new Dictionary<VecI, Tile>();
        var ar = area ?? new((0, 0), (tileMap.Size.width, tileMap.Size.height));

        for (var i = ar.X; i < ar.Height; i++)
            for (var j = ar.Y; j < ar.Width; j++)
                foreach (var rotations in data)
                    foreach (var rule in rotations)
                    {
                        if (rule == null || rule.match.Length != 9)
                            continue;

                        var isMatch = true;

                        for (var k = 0; k < 9; k++)
                        {
                            var (ox, oy) = (k % 3 - 1, k / 3 - 1);
                            var offTile = tileMap.TileAt((j + ox, i + oy));

                            if (rule.match[k] == null || rule.match[k] == offTile.Id)
                                continue;

                            isMatch = false;
                            break;
                        }

                        if (isMatch)
                            result[(j, i)] = rule.result;
                    }

        foreach (var (cell, tile) in result)
            tileMap.SetTile(cell, tile, mask);
    }

#region Backend
    private class Rule(Tile result, ushort?[] match)
    {
        public readonly Tile result = result;
        public readonly ushort?[] match = match;
    }

    private readonly List<Rule?[]> data = [];
    private readonly List<int> hashes = [];

    private void TryAdd(Tile tile, ushort?[] match, int atIndex, int rotation)
    {
        var itemsHash = GetItemsHash(match);

        if (hashes.Contains(itemsHash))
            return;

        hashes.Add(itemsHash);

        if (rotation == 0)
            data.Insert(atIndex, new Rule[4]);

        data[atIndex][rotation] = new(tile, match);
    }

    private static int GetItemsHash(ushort?[] array)
    {
        return array.Aggregate(17, (hash, val) => hash * 31 + (val?.GetHashCode() ?? ushort.MaxValue));
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