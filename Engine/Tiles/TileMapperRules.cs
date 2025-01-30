namespace Pure.Engine.Tiles;

public class TileMapperRules
{
    public void Apply(TileMap tileMap, Area? area = null, Area? mask = null)
    {
        var result = new Dictionary<(int x, int y), Tile>();
        var ar = area ?? new((0, 0), (tileMap.Size.width, tileMap.Size.height));

        for (var i = ar.X; i < ar.Height; i++)
            for (var j = ar.Y; j < ar.Width; j++)
                foreach (var tiles in rules)
                {
                    if (tiles.Length != 10)
                        continue;

                    var isMatch = true;

                    if (i == 10 && j == 10)
                        ;

                    for (var k = 0; k < 9; k++)
                    {
                        var (ox, oy) = (k % 3 - 1, k / 3 - 1);
                        var offTile = tileMap.TileAt((j + ox, i + oy));

                        if (tiles[k] == null || tiles[k] == offTile)
                            continue;

                        isMatch = false;
                        break;
                    }

                    if (isMatch)
                        result[(j, i)] = tiles[9]!.Value;
                }

        foreach (var (cell, tile) in result)
            tileMap.SetTile(cell, tile, mask);
    }

    public void AddRule(Tile result, Tile?[] match3X3)
    {
        var items = match3X3.Concat([result]).ToArray();
        var itemsHash = GetItemsHash(items);

        if (hashes.Contains(itemsHash))
            return;

        hashes.Add(itemsHash);
        rules.Add(items);
    }
    public void AddRulesForRegion()
    {
    }
    public void AddRulesForPath()
    {
    }

#region Backend
    private readonly List<int> hashes = []; //    items hash
    private readonly List<Tile?[]> rules = []; // match3X3[0...8] + result[9]

    private static int GetItemsHash(Tile?[] array)
    {
        return array.Aggregate(17, (hash, val) => hash * 31 + (val?.GetHashCode() ?? 0));
    }
#endregion
}