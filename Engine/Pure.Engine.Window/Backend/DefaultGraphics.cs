namespace Pure.Engine.Window;

using System.IO.Compression;

internal static class DefaultGraphics
{
    public static string PngToBase64String(string pngPath)
    {
        var img = new Image(pngPath);
        var rawBits = string.Empty;
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
            if (hasProcessed == false &&
                isPacked == false &&
                readIndexCounter == RAW_BITS_COUNT &&
                sameBitSequence < RAW_BITS_COUNT)
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
        var width = (ushort)(bytes[0] << 8 | bytes[1]);
        var height = (ushort)(bytes[2] << 8 | bytes[3]);
        var total = width * height;
        var decodedBits = new List<bool>();
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
                var repCountBinary = bits[2..];
                var repeatCount = Convert.ToByte(repCountBinary, BINARY);

                for (var j = 0; j < repeatCount; j++)
                    decodedBits.Add(color == '1');
            }
            else
            {
                if (isLast)
                {
                    var lastLength = total - decodedBits.Count;
                    var lastRawBits = bits[^lastLength..];
                    foreach (var bit in lastRawBits)
                        decodedBits.Add(bit == '1');

                    break;
                }

                var rawBits = bits[1..];
                foreach (var bit in rawBits)
                    decodedBits.Add(bit == '1');
            }
        }

        for (uint i = 0; i < decodedBits.Count; i++)
        {
            var bit = decodedBits[(int)i];
            var color = bit ? Color.White : Color.Transparent;
            var x = i % width;
            var y = i / width;

            img.SetPixel(x, y, color);
        }

        var result = new Texture(img) { Repeated = true };
        img.Dispose();
        return result;
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