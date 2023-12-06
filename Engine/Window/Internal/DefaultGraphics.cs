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