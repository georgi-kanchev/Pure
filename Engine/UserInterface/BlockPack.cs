namespace Pure.Engine.UserInterface;

using System.Text;

public class BlockPack
{
    public int Count
    {
        get => blocks.Count;
    }
    public Prompt? Prompt
    {
        get;
        set;
    }

    public Block this[int index]
    {
        get => blocks[index];
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
            var bblock = GetBytes(bytes, byteCount, ref offset);

            if (typeStr == nameof(Button)) Add(new Button(bblock));
            else if (typeStr == nameof(InputBox)) Add(new InputBox(bblock));
            else if (typeStr == nameof(FileViewer)) Add(new FileViewer(bblock));
            else if (typeStr == nameof(List)) Add(new List(bblock));
            else if (typeStr == nameof(Stepper)) Add(new Stepper(bblock));
            else if (typeStr == nameof(Pages)) Add(new Pages(bblock));
            else if (typeStr == nameof(Palette)) Add(new Palette(bblock));
            else if (typeStr == nameof(Panel)) Add(new Panel(bblock));
            else if (typeStr == nameof(Scroll)) Add(new Scroll(bblock));
            else if (typeStr == nameof(Slider)) Add(new Slider(bblock));
            else if (typeStr == nameof(Layout)) Add(new Layout(bblock));
        }

        return;

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
        }
    }

    public void Add(params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        foreach (var block in blocks)
            this.blocks.Add(block);
    }
    public void Remove(params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        foreach (var block in blocks)
            this.blocks.Remove(block);
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
        return block == null ? -1 : blocks.IndexOf(block);
    }
    public bool IsContaining(Block? block)
    {
        return block != null && blocks.Contains(block);
    }

    public void Update()
    {
        foreach (var e in blocks)
            e.Update();

        Prompt?.Update(this);
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(blocks.Count));

        foreach (var block in blocks)
        {
            var bytes = block.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return result.ToArray();
    }

#region Backend
    private readonly List<Block> blocks = new();

    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}