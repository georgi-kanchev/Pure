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