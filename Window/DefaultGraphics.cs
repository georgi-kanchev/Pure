namespace Pure.Window;

using System.IO.Compression;

using SFML.Graphics;

internal static class DefaultGraphics
{
	// image format
	// each byte (8 bits) goes as
	// packed bit (1) | color bit | up to 64 repeats (6 bits)
	// not packed bit (0) | 7 literal sequence of raw bits

	private const string DEFAULT_GRAPHICS_BASE64 = "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";
	private const int BINARY = 2;
	private const int RAW_BITS_COUNT = 7;
	private const int BYTE_BITS_COUNT = 8;
	private const int PACKED_REPEAT_BITS_COUNT = 6;
	private const int PACKED_REPEAT_COUNT = 63;

	public static string PNGToBase64String(string pngPath)
	{
		var img = new Image(pngPath);
		var rawBits = "";
		for (uint y = 0; y < img.Size.Y; y++)
			for (uint x = 0; x < img.Size.X; x++)
			{
				var value = img.GetPixel(x, y).A > byte.MaxValue / 2 ? "1" : "0";
				rawBits += value;
			}

		var isPacked = false;
		var prevBit = rawBits.Length > 1 ? rawBits[0] : default;
		var sameBitSequence = 0;
		var readIndexCounter = 0;

		var w = Convert.ToString(img.Size.X, BINARY).PadLeft(BYTE_BITS_COUNT * 2, '0');
		var h = Convert.ToString(img.Size.Y, BINARY).PadLeft(BYTE_BITS_COUNT * 2, '0');
		var bytes = new List<byte>
			{
				Convert.ToByte(w[0..BYTE_BITS_COUNT], BINARY),
				Convert.ToByte(w[BYTE_BITS_COUNT..^0], BINARY),
				Convert.ToByte(h[0..BYTE_BITS_COUNT], BINARY),
				Convert.ToByte(h[BYTE_BITS_COUNT..^0], BINARY),
			};

		for (int i = 0; i < rawBits.Length; i++)
		{
			var bit = rawBits[i];
			var hasProcessed = false;

			if (bit == prevBit)
				sameBitSequence++;
			// if start of new sequence while packed
			else if (isPacked)
				ProcessPackedSequence();

			// if end of repeated sequence (max 63 bits)
			if (hasProcessed == false && sameBitSequence == PACKED_REPEAT_COUNT)
				ProcessPackedSequence();

			// if end of image on repeated sequence
			if (isPacked && hasProcessed == false && i == rawBits.Length - 1)
			{
				readIndexCounter++;
				ProcessPackedSequence();
				break;
			}

			isPacked = sameBitSequence >= RAW_BITS_COUNT;

			// if end of raw sequence (max 7 bits)
			if (hasProcessed == false && isPacked == false &&
				readIndexCounter == RAW_BITS_COUNT && sameBitSequence < RAW_BITS_COUNT)
				ProcessRawSequence();

			isPacked = sameBitSequence >= RAW_BITS_COUNT;
			prevBit = bit;
			readIndexCounter++;

			void ProcessPackedSequence()
			{
				var length = Convert.ToByte(readIndexCounter);
				var byteStr = Convert.ToString(length, BINARY);
				byteStr = byteStr.PadLeft(PACKED_REPEAT_BITS_COUNT, '0');
				var finalByteStr = $"1{prevBit}{byteStr}".PadLeft(BYTE_BITS_COUNT, '0');
				var newByte = Convert.ToByte(finalByteStr, BINARY);

				hasProcessed = true;
				AddByte(newByte);
			}
			void ProcessRawSequence()
			{
				var raw = rawBits[(i - RAW_BITS_COUNT)..i];
				var finalByteStr = $"0{raw}";
				var newByte = Convert.ToByte(finalByteStr, BINARY);

				AddByte(newByte);
			}
		}

		// if end of image on raw sequence
		if (isPacked == false)
		{
			var raw = rawBits[^readIndexCounter..^0];
			raw = raw.PadLeft(RAW_BITS_COUNT, '0');
			var finalByteStr = $"0{raw}";
			var newByte = Convert.ToByte(finalByteStr, BINARY);

			AddByte(newByte);
		}

		return Convert.ToBase64String(Compress(bytes.ToArray()));

		void AddByte(byte newByte)
		{
			bytes.Add(newByte);
			sameBitSequence = 0;
			readIndexCounter = 0;
		}
	}
	public static Texture CreateTexture()
	{
		var bytes = Decompress(Convert.FromBase64String(DEFAULT_GRAPHICS_BASE64));
		var width = Convert.ToUInt16($"{bytes[0]}{bytes[1]}");
		var height = Convert.ToUInt16($"{bytes[2]}{bytes[3]}");
		var total = width * height;
		var decodedBits = "";
		var img = new Image(width, height);

		for (int i = 4; i < bytes.Length; i++)
		{
			var curByte = bytes[i];
			var bits = Convert.ToString(curByte, BINARY);
			bits = bits.PadLeft(BYTE_BITS_COUNT, '0');
			var isPacked = bits[0] == '1';
			var isLast = i == bytes.Length - 1;

			if (isPacked)
			{
				var color = bits[1];
				var repCountBinary = bits[2..^0];
				var repeatCount = Convert.ToByte(repCountBinary, BINARY);

				decodedBits += new string(color, repeatCount);
			}
			else
			{
				if (isLast)
				{
					var lastLength = total - decodedBits.Length;
					var lastRawBits = bits[^lastLength..^0];
					decodedBits += lastRawBits;
					break;
				}

				var rawBits = bits[1..^0];
				decodedBits += rawBits;
			}
		}

		for (uint i = 0; i < decodedBits.Length; i++)
		{
			var bit = decodedBits[(int)i];
			var color = bit == '1' ? Color.White : Color.Transparent;
			var x = i % width;
			var y = i / width;

			img.SetPixel(x, y, color);
		}

		return new Texture(img);
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
}
