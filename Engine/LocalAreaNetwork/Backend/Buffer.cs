namespace Pure.Engine.LocalAreaNetwork;

using System;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Dynamic byte buffer
/// </summary>
internal class Buffer
{
    /// <summary>
    /// Is the buffer empty?
    /// </summary>
    public bool IsEmpty
    {
        get => Data == null || Size == 0;
    }
    /// <summary>
    /// Bytes memory buffer
    /// </summary>
    public byte[] Data { get; private set; }
    /// <summary>
    /// Bytes memory buffer capacity
    /// </summary>
    public long Capacity
    {
        get => Data.Length;
    }
    /// <summary>
    /// Bytes memory buffer size
    /// </summary>
    public long Size { get; private set; }
    /// <summary>
    /// Bytes memory buffer offset
    /// </summary>
    public long Offset { get; private set; }

    /// <summary>
    /// Buffer indexer operator
    /// </summary>
    public byte this[long index]
    {
        get => Data[index];
    }

    /// <summary>
    /// Initialize a new expandable buffer with zero capacity
    /// </summary>
    public Buffer()
    {
        Data = [];
        Size = 0;
        Offset = 0;
    }
    /// <summary>
    /// Initialize a new expandable buffer with the given capacity
    /// </summary>
    public Buffer(long capacity)
    {
        Data = new byte[capacity];
        Size = 0;
        Offset = 0;
    }
    /// <summary>
    /// Initialize a new expandable buffer with the given data
    /// </summary>
    public Buffer(byte[] data)
    {
        Data = data;
        Size = data.Length;
        Offset = 0;
    }

#region Memory buffer methods
    /// <summary>
    /// Get a span of bytes from the current buffer
    /// </summary>
    public Span<byte> AsSpan()
    {
        return new(Data, (int)Offset, (int)Size);
    }

    /// <summary>
    /// Get a string from the current buffer
    /// </summary>
    public override string ToString()
    {
        return ExtractString(0, Size);
    }

    // Clear the current buffer and its offset
    public void Clear()
    {
        Size = 0;
        Offset = 0;
    }

    /// <summary>
    /// Extract the string from buffer of the given offset and size
    /// </summary>
    public string ExtractString(long off, long sz)
    {
        Debug.Assert(off + sz <= Size, "Invalid offset & size!");
        if (off + sz > Size)
            throw new ArgumentException("Invalid offset & size!", nameof(off));

        return Encoding.UTF8.GetString(Data, (int)off, (int)sz);
    }

    /// <summary>
    /// Remove the buffer of the given offset and size
    /// </summary>
    public void Remove(long off, long sz)
    {
        Debug.Assert(off + sz <= Size, "Invalid offset & size!");
        if (off + sz > Size)
            throw new ArgumentException("Invalid offset & size!", nameof(off));

        Array.Copy(Data, off + sz, Data, off, Size - sz - off);
        Size -= sz;
        if (Offset >= off + sz)
            Offset -= sz;
        else if (Offset >= off)
        {
            Offset -= Offset - off;
            if (Offset > Size)
                Offset = Size;
        }
    }

    /// <summary>
    /// Reserve the buffer of the given capacity
    /// </summary>
    public void Reserve(long capacity)
    {
        Debug.Assert(capacity >= 0, "Invalid reserve capacity!");

        if (capacity <= Capacity)
            return;

        var data = new byte[Math.Max(capacity, 2 * Capacity)];
        Array.Copy(Data, 0, data, 0, Size);
        Data = data;
    }

    // Resize the current buffer
    public void Resize(long sz)
    {
        Reserve(sz);
        Size = sz;
        if (Offset > Size)
            Offset = Size;
    }

    // Shift the current buffer offset
    public void Shift(long off)
    {
        Offset += off;
    }
    // Unshift the current buffer offset
    public void Unshift(long off)
    {
        Offset -= off;
    }
#endregion

#region Buffer I/O methods
    /// <summary>
    /// Append the single byte
    /// </summary>
    /// <param name="value">Byte value to append</param>
    /// <returns>Count of append bytes</returns>
    public long Append(byte value)
    {
        Reserve(Size + 1);
        Data[Size] = value;
        Size += 1;
        return 1;
    }

    /// <summary>
    /// Append the given buffer
    /// </summary>
    /// <param name="buffer">Buffer to append</param>
    /// <returns>Count of append bytes</returns>
    public long Append(byte[] buffer)
    {
        Reserve(Size + buffer.Length);
        Array.Copy(buffer, 0, Data, Size, buffer.Length);
        Size += buffer.Length;
        return buffer.Length;
    }

    /// <summary>
    /// Append the given buffer fragment
    /// </summary>
    /// <param name="buffer">Buffer to append</param>
    /// <param name="off">Buffer offset</param>
    /// <param name="sz">Buffer size</param>
    /// <returns>Count of append bytes</returns>
    public long Append(byte[] buffer, long off, long sz)
    {
        Reserve(Size + sz);
        Array.Copy(buffer, off, Data, Size, sz);
        Size += sz;
        return sz;
    }

    /// <summary>
    /// Append the given span of bytes
    /// </summary>
    /// <param name="buffer">Buffer to append as a span of bytes</param>
    /// <returns>Count of append bytes</returns>
    public long Append(ReadOnlySpan<byte> buffer)
    {
        Reserve(Size + buffer.Length);
        buffer.CopyTo(new(Data, (int)Size, buffer.Length));
        Size += buffer.Length;
        return buffer.Length;
    }

    /// <summary>
    /// Append the given buffer
    /// </summary>
    /// <param name="buffer">Buffer to append</param>
    /// <returns>Count of append bytes</returns>
    public long Append(Buffer buffer)
    {
        return Append(buffer.AsSpan());
    }

    /// <summary>
    /// Append the given text in UTF-8 encoding
    /// </summary>
    /// <param name="text">Text to append</param>
    /// <returns>Count of append bytes</returns>
    public long Append(string text)
    {
        var length = Encoding.UTF8.GetMaxByteCount(text.Length);
        Reserve(Size + length);
        long result = Encoding.UTF8.GetBytes(text, 0, text.Length, Data, (int)Size);
        Size += result;
        return result;
    }

    /// <summary>
    /// Append the given text in UTF-8 encoding
    /// </summary>
    /// <param name="text">Text to append as a span of characters</param>
    /// <returns>Count of append bytes</returns>
    public long Append(ReadOnlySpan<char> text)
    {
        var length = Encoding.UTF8.GetMaxByteCount(text.Length);
        Reserve(Size + length);
        long result = Encoding.UTF8.GetBytes(text, new(Data, (int)Size, length));
        Size += result;
        return result;
    }
#endregion
}