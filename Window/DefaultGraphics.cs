using SFML.Graphics;

namespace Pure.Window
{
	internal static class DefaultGraphics
	{
		// image format
		// each byte (8 bits) goes as
		// packed bit (1) | color bit | up to 64 repeats (6 bits)
		// not packed bit (0) | 7 literal sequence of raw bits

		private const string DEFAULT_GRAPHICS_BASE64 = "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VR8RI0QIIFRdHwhFJSoUfD4gUiJDcShkdQwPTzd6JHxJEQcDRWl4OBQRGm0zM0ECKhUPRSMwAFAcEw9IF3olAiIfFyEDMXhEXSJNTAd4cHw+Hw9ABIhAcEA+BB9hAEF5OhwRCENHeXi/v7+eYYlFK10+VQdAnGwwi0AgEgECA3F5AEAEAEAjIDFZRpJ4mXBsOCIMBgMICBgfCEgEYTAMIlEPXWpVelR4iXwIGw1HRjNVaHUBAkIReTBUHCMMKBIwnn8OCENCEVh8dzkdDlAQJCIRGQ1GZmN5CkQ7VSt1KDiIfAgbDUdHO1VodQ8ORjMZCFQcHAwJRAFZRpNgmHBsODMMBgMJeXB3O0kGIyBAICQgFS51elQQoWwwIgQCARBwYCIRDohCv7+/mHw+Hw9HY3F4fD4fD0djcXh8Ph8PR2NxeHw+Hw9HZlolVkkuWm92Wy1+XSRfa1d7LVZrNVprVWt9Enk/WmtVaiVWayRaaRRKdVZJP19pFEstEl0uWmsXei06fz9fb3ZbfX5/P19vd3s1Rkk7X291a31+fz9abTd6dQJjP19vd3p9Ol0nV292G31GVTVXLDd6dXp3P1hsNhp1RmMxUCx2Gw1iYyBYbnc7LW5VNV1sNht9RkkfD0djcXh8Ph8PR2NxeHw+Hw9HY3F4fD4fD0djcXi/v7+Sfz9fb3d7fBA2DgIBAzBwEIl8HQQVIY18HQQVIQd6fT5dLlcjR3hwOBQkRQJAQXh8CB8HAQIRGBAqCh9vd3t9fn8fH29zcQkCaxEHA0d4IHwICgUIMEEIEHc/XW93OnV+fz9faBQKdQIcDhtDQWBwKBQ1BQJBI31+fz9fb3d5eVY+MUhNMhAgfB8OBwNAQQgmFAoFD3drdTpdLkcDQEBwKBQEBQNDcHg4HA4FBCEQUCgUP19vd3t9fj4EBwEDcCA4CB8fZ3NxeHw+P09nY3F4v7+/osiQyJJ4kXiKYIlgAgBAP2C0fsiJfkBAB0dwAHhCDAYDAUAEAb8eH19zYgoCGD8/YwQUBDA8DAmJQBC9eH7IEhBQE2N7fiQhICdDYRBIAYhAu3h+yBIQUBNje34kISAnR3ERBAGIQLt4fsgeEFARQ3t+GCEgL2dyCQQBiEC/D294AX0Binh+inhCPz9vaAgEAb+QyJDIkniReADIiMgAyr9AIAQHYEB4ikCQcDAcA7+jYDAEAQBBBIlQBjAECCACCL+fcDgEAQBCCkKOSAkQBAO/qHg8B0EHehMZCAICD3AGA7+ofD4fYQBCIkIIBAEJFAICAb+hcD8PYQBCQpBAAQQQCItAv5tYMAdhAEEEiVBgAw4IIAIIv6JCiXF4iXiKQJpgcGC/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7+/v7++";

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

			return Convert.ToBase64String(bytes.ToArray());

			void AddByte(byte newByte)
			{
				bytes.Add(newByte);
				sameBitSequence = 0;
				readIndexCounter = 0;
			}
		}
		public static Texture CreateTexture()
		{
			var bytes = Convert.FromBase64String(DEFAULT_GRAPHICS_BASE64);
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
	}
}
