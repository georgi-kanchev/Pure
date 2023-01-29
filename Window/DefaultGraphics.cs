using System.IO.Compression;

using SFML.Graphics;

namespace Pure.Window
{
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
			for(uint y = 0; y < img.Size.Y; y++)
				for(uint x = 0; x < img.Size.X; x++)
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

			for(int i = 0; i < rawBits.Length; i++)
			{
				var bit = rawBits[i];
				var hasProcessed = false;

				if(bit == prevBit)
					sameBitSequence++;
				// if start of new sequence while packed
				else if(isPacked)
					ProcessPackedSequence();

				// if end of repeated sequence (max 63 bits)
				if(hasProcessed == false && sameBitSequence == PACKED_REPEAT_COUNT)
					ProcessPackedSequence();

				// if end of image on repeated sequence
				if(isPacked && hasProcessed == false && i == rawBits.Length - 1)
				{
					readIndexCounter++;
					ProcessPackedSequence();
					break;
				}

				isPacked = sameBitSequence >= RAW_BITS_COUNT;

				// if end of raw sequence (max 7 bits)
				if(hasProcessed == false && isPacked == false &&
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
			if(isPacked == false)
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

			for(int i = 4; i < bytes.Length; i++)
			{
				var curByte = bytes[i];
				var bits = Convert.ToString(curByte, BINARY);
				bits = bits.PadLeft(BYTE_BITS_COUNT, '0');
				var isPacked = bits[0] == '1';
				var isLast = i == bytes.Length - 1;

				if(isPacked)
				{
					var color = bits[1];
					var repCountBinary = bits[2..^0];
					var repeatCount = Convert.ToByte(repCountBinary, BINARY);

					decodedBits += new string(color, repeatCount);
				}
				else
				{
					if(isLast)
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

			for(uint i = 0; i < decodedBits.Length; i++)
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
			using(var stream = new DeflateStream(output, CompressionLevel.Optimal))
				stream.Write(data, 0, data.Length);

			return output.ToArray();
		}
		private static byte[] Decompress(byte[] data)
		{
			var input = new MemoryStream(data);
			var output = new MemoryStream();
			using(var stream = new DeflateStream(input, CompressionMode.Decompress))
				stream.CopyTo(output);

			return output.ToArray();
		}
	}
}
