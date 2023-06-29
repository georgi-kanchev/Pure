namespace Pure.Window;

using System.IO.Compression;
using SFML.Graphics;

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

        img.SaveToFile("graphics.png");
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