namespace Pure.Engine.Window;

using System.IO.Compression;

internal static class DefaultGraphics
{
    public static string PngToBase64String(string pngPath)
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

        for (var i = 0; i < rawBits.Length; i++)
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
            continue;

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
        if (isPacked)
            return Convert.ToBase64String(Compress(bytes.ToArray()));

        var raw = rawBits[^readIndexCounter..];
        raw = raw.PadLeft(RAW_BITS_COUNT, '0');
        var finalByteStr = $"0{raw}";
        var newByte = Convert.ToByte(finalByteStr, BINARY);

        AddByte(newByte);

        return Convert.ToBase64String(Compress(bytes.ToArray()));

        void AddByte(byte b)
        {
            bytes.Add(b);
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

        for (var i = 4; i < bytes.Length; i++)
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
                    var lastRawBits = bits[^lastLength..];
                    decodedBits += lastRawBits;
                    break;
                }

                var rawBits = bits[1..];
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
        return new(img) { Repeated = true };
    }

#region Backend
    // image format
    // each byte (8 bits) goes as
    // packed bit (1) | color bit | up to 64 repeats (6 bits)
    // not packed bit (0) | 7 literal sequence of raw bits

    private const string DEFAULT_GRAPHICS_BASE64 =
        "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";
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