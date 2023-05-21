namespace Pure.Tilemap;

public class TilemapManager
{
	public int Count => tilemaps.Length;
	public (int width, int height) Size { get; private set; }
	public (int x, int y) CameraPosition { get; set; }
	public (int width, int height) CameraSize { get; set; }

	public Tilemap this[int index] => tilemaps[index];

	public TilemapManager(string path)
	{
		try
		{
			var bytes = Tilemap.Decompress(File.ReadAllBytes(path));
			var tilemapList = new List<Tilemap>();
			tilemaps = Array.Empty<Tilemap>();

			while (true)
			{
				var currentTilemap = Tilemap.FromBytes(bytes, out var remainingBytes);
				if (remainingBytes == null || remainingBytes.Length == 0)
					break;

				tilemapList.Add(currentTilemap);
				bytes = remainingBytes;
			}
			tilemaps = tilemapList.ToArray();
		}
		catch (Exception)
		{ throw new Exception($"Could not load {nameof(Tilemap)} from '{path}'."); }
	}
	public TilemapManager(int count, (int width, int height) size)
	{
		Size = size;

		count = Math.Max(count, 1);

		tilemaps = new Tilemap[count];
		for (int i = 0; i < count; i++)
			tilemaps[i] = new(size);
	}

	public void Save(string path)
	{
		try
		{
			var result = new List<byte>();
			for (int i = 0; i < tilemaps.Length; i++)
				result.AddRange(Tilemap.ToBytes(tilemaps[i]));

			File.WriteAllBytes(path, Tilemap.Compress(result.ToArray()));
		}
		catch (Exception)
		{ throw new Exception($"Could not save {nameof(TilemapManager)} at '{path}'."); }
	}

	public Tilemap[] CameraUpdate()
	{
		var result = new Tilemap[Count];
		for (int i = 0; i < Count; i++)
		{
			var tmap = tilemaps[i];

			// keep the original tilemap camera props, use the manager ones
			var prevPos = tmap.CameraPosition;
			var prevSz = tmap.CameraSize;
			tmap.CameraPosition = CameraPosition;
			tmap.CameraSize = CameraSize;

			result[i] = tmap.CameraUpdate();

			// and revert
			tmap.CameraPosition = prevPos;
			tmap.CameraSize = prevSz;
		}
		return result;
	}
	public void Fill(Tile withTile = default)
	{
		for (int i = 0; i < tilemaps.Length; i++)
			tilemaps[i].Fill(withTile);
	}

	public Tile[] TilesAt((int x, int y) position)
	{
		var result = new Tile[Count];
		for (int i = 0; i < tilemaps.Length; i++)
			result[i] = tilemaps[i].TileAt(position);

		return result;
	}
	public (float x, float y) PointFrom((int x, int y) pixelPosition, (int width, int height) windowSize, bool isAccountingForCamera = true)
	{
		// cannot use first tilemap to not duplicate code since the used camera props are
		// the ones on the manager
		var x = Map(pixelPosition.x, 0, windowSize.width, 0, Size.width);
		var y = Map(pixelPosition.y, 0, windowSize.height, 0, Size.height);

		if (isAccountingForCamera)
		{
			x += CameraPosition.x;
			y += CameraPosition.y;
		}

		return (x, y);
	}

	public void ConfigureText(int lowercase = Tile.LOWERCASE_A, int uppercase = Tile.UPPERCASE_A, int numbers = Tile.NUMBER_0)
	{
		for (int i = 0; i < Count; i++)
			tilemaps[i].ConfigureText(lowercase, uppercase, numbers);
	}
	public void ConfigureText(string symbols, int startId)
	{
		for (int i = 0; i < symbols?.Length; i++)
			tilemaps[i].ConfigureText(symbols, startId);
	}

	public bool IsInside((int x, int y) position, (int width, int height) outsideMargin = default)
	{
		return tilemaps[0].IsInside(position, outsideMargin);
	}

	public int TileIDFrom(char symbol)
	{
		// not very reliable method since the user might configure text on each
		// tilemap individually but the idea of the tilemap manager is to bundle
		// multiple tilemaps with COMMON properties, therefore same text configuration
		return tilemaps[0].TileIDFrom(symbol);
	}
	public int[] TileIDsFrom(string text)
	{
		// not very reliable method since the user might configure text on each
		// tilemap individually but the idea of the tilemap manager is to bundle
		// multiple tilemaps with COMMON properties, therefore same text configuration
		return tilemaps[0].TileIDsFrom(text);
	}

	#region Backend
	// save format
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- tilemaps count
	// [4]						- camera x
	// [4]						- camera y
	// [4]						- camera width
	// [4]						- camera height
	// [width * height]			- tile bundles array

	private readonly Tilemap[] tilemaps;

	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	#endregion
}
