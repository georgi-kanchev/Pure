using SFML.Graphics;

namespace Graphics
{
	public class PureImage
	{
		// packed (1) | color bit | up to 64 repeats (6 bits)
		// not packed (0) | 7 literal sequence of raw bits

		public PureImage(string imageFile)
		{
			img = new Image(imageFile);
		}
		public PureImage(byte[] purifiedImage)
		{
			img = new(0, 0);

			if(purifiedImage.Length < 2)
				return;

			var width = (uint)purifiedImage[0];
			var height = (uint)purifiedImage[1];

			img.Dispose();
			img = new(width, height);

			var decodedBits = "";

			for(int i = 2; i < purifiedImage.Length; i++)
			{
				var curByte = purifiedImage[i];
				var bits = Convert.ToString(curByte, BINARY);
				bits = bits.PadLeft(BYTE_BITS_COUNT, '0');
				var isPacked = bits[0] == '1';

				if(isPacked)
				{
					var color = bits[1];
					var repCountBinary = bits[2..^0];
					var repeatCount = Convert.ToByte(repCountBinary, BINARY);

					decodedBits += new string(color, repeatCount);
				}
				else
				{
					var rawBitsBinary = bits[1..^0];
					decodedBits += rawBitsBinary;
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

		public byte[] Purify()
		{
			if(img.Size.X > byte.MaxValue || img.Size.Y > byte.MaxValue)
				return Array.Empty<byte>();

			var rawBits = "";
			for(uint y = 0; y < img.Size.Y; y++)
				for(uint x = 0; x < img.Size.X; x++)
				{
					var value = img.GetPixel(x, y).A > byte.MaxValue / 2 ? "1" : "0";
					rawBits += value;
				}

			var isPacked = false;
			var prevBit = rawBits[0];
			var sameBitSequence = 0;
			var readIndexCounter = 0;
			var byteList = new List<byte>();

			byteList.Add((byte)img.Size.X);
			byteList.Add((byte)img.Size.Y);

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
				void AddByte(byte newByte)
				{
					byteList.Add(newByte);
					sameBitSequence = 0;
					readIndexCounter = 0;
				}
			}

			return byteList.ToArray();
		}

		#region Backend
		private const int BINARY = 2;
		private const int RAW_BITS_COUNT = 7;
		private const int BYTE_BITS_COUNT = 8;
		private const int PACKED_REPEAT_BITS_COUNT = 6;
		private const int PACKED_REPEAT_COUNT = 63;

		private readonly Image img;
		#endregion
	}
}
