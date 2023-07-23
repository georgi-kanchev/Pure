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