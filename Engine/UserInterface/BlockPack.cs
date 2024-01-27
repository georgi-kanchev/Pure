﻿namespace Pure.Engine.UserInterface;

using System.Text;

public class BlockPack
{
    public int Count
    {
        get => data.Count;
    }

    public Block this[int index]
    {
        get => data[index];
    }

    public BlockPack()
    {
    }
    public BlockPack(byte[] bytes)
    {
        var offset = 0;
        var count = GetInt();

        for (var i = 0; i < count; i++)
        {
            var byteCount = GetInt();

            // blockType string gets saved first for each block
            var typeStrBytesLength = GetInt();
            var typeStr = Encoding.UTF8.GetString(GetBytes(bytes, typeStrBytesLength, ref offset));

            // return the offset to where it was so the block can get loaded properly
            offset -= typeStrBytesLength + 4;
            var bBlock = GetBytes(bytes, byteCount, ref offset);

            if (typeStr == nameof(Button)) Add(new Button(bBlock));
            else if (typeStr == nameof(FileViewer)) Add(new FileViewer(bBlock));
            else if (typeStr == nameof(InputBox)) Add(new InputBox(bBlock));
            else if (typeStr == nameof(Layout)) Add(new Layout(bBlock));
            else if (typeStr == nameof(List)) Add(new List(bBlock));
            else if (typeStr == nameof(Pages)) Add(new Pages(bBlock));
            else if (typeStr == nameof(Palette)) Add(new Palette(bBlock));
            else if (typeStr == nameof(Panel)) Add(new Panel(bBlock));
            else if (typeStr == nameof(Scroll)) Add(new Scroll(bBlock));
            else if (typeStr == nameof(Slider)) Add(new Slider(bBlock));
            else if (typeStr == nameof(Stepper)) Add(new Stepper(bBlock));
        }

        return;

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
        }
    }
    public BlockPack(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(data.Count));

        foreach (var block in data)
        {
            var bytes = block.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return result.ToArray();
    }

    public void Add(params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        data.AddRange(blocks);
    }
    public void Insert(int index, params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        data.InsertRange(index, blocks);
    }
    public void Remove(params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        foreach (var block in blocks)
            data.Remove(block);
    }
    public void Clear()
    {
        data.Clear();
    }
    public void BringToTop(params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        foreach (var block in blocks)
        {
            Remove(block);
            Add(block);
        }
    }

    public int IndexOf(Block? block)
    {
        return block == null ? -1 : data.IndexOf(block);
    }
    public bool IsContaining(Block? block)
    {
        return block != null && data.Contains(block);
    }

    public void Update()
    {
        foreach (var e in data)
        {
            var prevCount = data.Count;

            e.Update();

            if (data.Count != prevCount) // was the data modified by this update?
                return; // run away
        }
    }

    public BlockPack Copy()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](BlockPack blockPack)
    {
        return blockPack.ToBytes();
    }
    public static implicit operator BlockPack(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private readonly List<Block> data = new();

    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}