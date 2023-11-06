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
	public (int width, int height) Size
	{
		get => Count == 0 ? default : data[0].Size;
	}
	public (int x, int y) ViewPosition
	{
		get => Count == 0 ? default : data[0].ViewPosition;
		set
		{
			foreach(var map in data)
				map.ViewPosition = value;
		}
	}
	public (int width, int height) ViewSize
	{
		get => Count == 0 ? default : data[0].ViewSize;
		set
		{
			foreach(var map in data)
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
		for(var i = 0; i < Math.Max(count, 1); i++)
			data.Add(new(size));

		ViewSize = (size.width, size.height);
	}
	public TilemapPack(byte[] bytes)
	{
		var b = Tilemap.Decompress(bytes);
		var offset = 0;
		var count = BitConverter.ToInt32(Get<int>());

		for(var i = 0; i < count; i++)
		{
			var tmapByteCount = BitConverter.ToInt32(Get<int>());
			data.Add(new(GetBytesFrom(b, tmapByteCount, ref offset)));
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

		foreach(var t in data)
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
		for(var i = 0; i < Count; i++)
			result[i] = data[i].ViewUpdate();

		return result;
	}

	public void Add(params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;

		ValidateMaps(tilemaps);
		data.AddRange(tilemaps);
	}
	public void Insert(int index, params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;

		ValidateMaps(tilemaps);
		data.InsertRange(index, tilemaps);
	}
	public void Remove(params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;

		foreach(var map in tilemaps)
			data.Remove(map);
	}
	public void Clear()
	{
		data.Clear();
	}

	public void BringUp(params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;

		var maps = tilemaps.ToList();
		for(var i = 0; i < maps.Count; i++)
		{
			var map = maps[i];
			var index = data.IndexOf(map);
			if(index < 1) // skip if not present or topmost
				continue;

			if(maps.Contains(data[index - 1]))
				continue; // keep the order of the provided maps

			// swap
			var cache = data[index - 1];
			data[index - 1] = map;
			data[index] = cache;
		}
	}
	public void BringDown(params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;

		var maps = tilemaps.ToList();
		for(var i = 0; i < maps.Count; i++)
		{
			var map = maps[i];
			var index = data.IndexOf(map);
			if(index > data.Count - 1) // skip if not present or downmost
				continue;

			if(maps.Contains(data[index + 1]))
				continue; // keep the order of the provided maps

			// swap
			var cache = data[index + 1];
			data[index + 1] = map;
			data[index] = cache;
		}
	}
	public void BringToTop(params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;
	}
	public void BringToBottom(params Tilemap[]? tilemaps)
	{
		if(tilemaps == null || tilemaps.Length == 0)
			return;
	}

	public void Flush()
	{
		foreach(var t in data)
			t.Flush();
	}
	public void Fill(Tile tile = default)
	{
		foreach(var t in data)
			t.Fill(tile);
	}
	public void Flood((int x, int y) position, Tile tile)
	{
		foreach(var map in data)
			map.Flood(position, tile);
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
		for(var i = 0; i < data.Count; i++)
			result[i] = data[i].TileAt(position);

		return result;
	}

	public void ConfigureText(
		int lowercase = Tile.LOWERCASE_A,
		int uppercase = Tile.UPPERCASE_A,
		int numbers = Tile.NUMBER_0)
	{
		for(var i = 0; i < Count; i++)
			data[i].ConfigureText(lowercase, uppercase, numbers);
	}
	public void ConfigureText(string symbols, int startId)
	{
		for(var i = 0; i < symbols.Length; i++)
			data[i].ConfigureText(symbols, startId);
	}

	public bool IsInside((int x, int y) position)
	{
		return data[0].IsContaining(position);
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
	private readonly List<Tilemap> data = new();

	private void ValidateMaps(Tilemap[] tilemaps)
	{
		// ReSharper disable once ForCanBeConvertedToForeach
		for(var i = 0; i < tilemaps.Length; i++)
		{
			var map = tilemaps[i];
			if(Count > 0 && map.Size != Size)
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
	#endregion
}