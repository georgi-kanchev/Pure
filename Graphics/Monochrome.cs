using SFML.Graphics;

namespace Pure.Graphics
{
	public class Monochrome
	{
		public (uint, uint) Size => (img.Size.X, img.Size.Y);
		public uint PixelCount => Size.Item1 * Size.Item2;

		public Monochrome()
		{
			img = new(0, 0);
		}
		public Monochrome((uint, uint) size)
		{
			img = new Image(size.Item1, size.Item2);
		}

		public bool IsPixelActive((uint, uint) indices)
		{
			var color = img.GetPixel(indices.Item1, indices.Item2);
			return color == Color.White;
		}
		public bool IsPixelActive(uint index)
		{
			var x = index % img.Size.X - 1;
			var y = index / img.Size.X - 1;
			return IsPixelActive((x, y));
		}

		public void ActivatePixel((uint, uint) indices, bool isActive)
		{
			if(indices.Item1 < 0 || indices.Item1 >= img.Size.X ||
				indices.Item2 < 0 || indices.Item2 >= img.Size.Y)
				return;

			img.SetPixel(indices.Item1, indices.Item2, isActive ? Color.White : Color.Transparent);
		}
		public void ActivatePixel(uint index, bool isActive)
		{
			var x = index % img.Size.X - 1;
			var y = index / img.Size.X - 1;
			ActivatePixel((x, y), isActive);
		}

		public void ActivateAllPixels(bool areActive)
		{
			for(uint y = 0; y < Size.Item2; y++)
				for(uint x = 0; x < Size.Item1; x++)
					img.SetPixel(x, y, areActive ? Color.White : Color.Transparent);
		}

		// image format
		// packed (1) | color bit | up to 64 repeats (6 bits)
		// not packed (0) | 7 literal sequence of raw bits
		public void Save(string monochromePath)
		{
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

			File.WriteAllBytesAsync(monochromePath, bytes.ToArray());

			void AddByte(byte newByte)
			{
				bytes.Add(newByte);
				sameBitSequence = 0;
				readIndexCounter = 0;
			}
		}
		public void Load(string monochromePath)
		{
			var mono = File.ReadAllBytes(monochromePath);

			if(mono.Length < 4)
				return;

			var width = Convert.ToUInt16($"{mono[0]}{mono[1]}");
			var height = Convert.ToUInt16($"{mono[2]}{mono[3]}");
			var total = width * height;
			var decodedBits = "";

			img.Dispose();
			img = new(width, height);

			for(int i = 4; i < mono.Length; i++)
			{
				var curByte = mono[i];
				var bits = Convert.ToString(curByte, BINARY);
				bits = bits.PadLeft(BYTE_BITS_COUNT, '0');
				var isPacked = bits[0] == '1';
				var isLast = i == mono.Length - 1;

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
		}

		public void SaveAsImage(string imagePath)
		{
			img.SaveToFile(imagePath);
		}
		public void LoadFromImage(string imagePath)
		{
			img = new Image(imagePath);

			for(uint y = 0; y < img.Size.Y; y++)
				for(uint x = 0; x < img.Size.X; x++)
				{
					var color = img.GetPixel(x, y).A > byte.MaxValue / 2 ?
						Color.White : Color.Transparent;

					img.SetPixel(x, y, color);
				}
		}

		#region Backend
		private const int BINARY = 2;
		private const int RAW_BITS_COUNT = 7;
		private const int BYTE_BITS_COUNT = 8;
		private const int PACKED_REPEAT_BITS_COUNT = 6;
		private const int PACKED_REPEAT_COUNT = 63;

		private Image img;
		#endregion
	}
}
