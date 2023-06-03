using System.IO.Compression;
using System.Text;

namespace Pure.Storage;

public class StorageRaw
{
	public byte[] this[string key]
	{
		get => key == null ? Array.Empty<byte>() : data[key];
		set
		{
			if (key == null)
				return;

			data[key] = value;
		}
	}

	public void Save(string path)
	{
		var result = new List<byte>();
		result.AddRange(BitConverter.GetBytes(data.Count));

		foreach (var kvp in data)
		{
			var bKey = BytesFromText(kvp.Key);
			result.AddRange(BitConverter.GetBytes(bKey.Length));
			result.AddRange(bKey);
			result.AddRange(BitConverter.GetBytes(kvp.Value.Length));
			result.AddRange(kvp.Value);
		}
		File.WriteAllBytes(path, Compress(result.ToArray()));
	}
	public void Load(string path)
	{
		var bytes = Decompress(File.ReadAllBytes(path));
		var offset = 0;
		var sectorCount = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));

		for (int i = 0; i < sectorCount; i++)
		{
			var keyLength = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
			var key = BytesToText(GetBytesFrom(bytes, keyLength, ref offset));
			var dataLength = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
			var data = GetBytesFrom(bytes, dataLength, ref offset);

			this.data[key] = data;
		}
	}

	#region Backend
	// save format in sectors
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- amount of sectors
	// = = = = = = (sector 1)
	// [4]						- key length
	// [key length]				- string key
	// [4]						- data length
	// [data length]			- data
	// = = = = = = (sector 2)
	// [4]						- key length
	// [key length]				- string key
	// [4]						- data length
	// [data length]			- data
	// = = = = = = (sector 3)
	// ...

	private Dictionary<string, byte[]> data = new();

	private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
	{
		var result = fromBytes[offset..(offset + amount)];
		offset += amount;
		return result;
	}

	private static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
			stream.Write(data, 0, data.Length);

		return output.ToArray();
	}
	private static byte[] Decompress(byte[] data)
	{
		var input = new MemoryStream(data);
		var output = new MemoryStream();
		using (var stream = new DeflateStream(input, CompressionMode.Decompress))
			stream.CopyTo(output);

		return output.ToArray();
	}

	private static byte[] BytesFromText(string text)
	{
		return Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
	}
	private static string BytesToText(byte[] bytes)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(Convert.ToBase64String(bytes)));
	}
	#endregion
}