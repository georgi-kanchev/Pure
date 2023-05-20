namespace Pure.Window;

using System.IO.Compression;

using SFML.Graphics;

internal static class DefaultGraphics
{
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

		//img.SaveToFile("graphics.png");
		return new Texture(img);
	}

	#region Backend
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

	#endregion
}
