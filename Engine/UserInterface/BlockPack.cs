namespace Pure.Engine.UserInterface;

using System.Text;

public class BlockPack
{
    public List<Block> Blocks { get; } = [];

    public BlockPack()
    {
    }
    public BlockPack(byte[] bytes)
    {
        var b = Block.Decompress(bytes);
        var offset = 0;
        var count = GetInt();

        for (var i = 0; i < count; i++)
        {
            var byteCount = GetInt();

            // blockType string gets saved first for each block
            var typeStrBytesLength = GetInt();
            var typeStr = Encoding.UTF8.GetString(GetBytes(b, typeStrBytesLength, ref offset));

            // return the offset to where it was so the block can get loaded properly
            offset -= typeStrBytesLength + 4;
            var bBlock = GetBytes(b, byteCount, ref offset);

            if (typeStr == nameof(Block)) Blocks.Add(new(bBlock));
            else if (typeStr == nameof(Button)) Blocks.Add(new Button(bBlock));
            else if (typeStr == nameof(FileViewer)) Blocks.Add(new FileViewer(bBlock));
            else if (typeStr == nameof(InputBox)) Blocks.Add(new InputBox(bBlock));
            else if (typeStr == nameof(Layout)) Blocks.Add(new Layout(bBlock));
            else if (typeStr == nameof(List)) Blocks.Add(new List(bBlock));
            else if (typeStr == nameof(Pages)) Blocks.Add(new Pages(bBlock));
            else if (typeStr == nameof(Palette)) Blocks.Add(new Palette(bBlock));
            else if (typeStr == nameof(Panel)) Blocks.Add(new Panel(bBlock));
            else if (typeStr == nameof(Scroll)) Blocks.Add(new Scroll(bBlock));
            else if (typeStr == nameof(Slider)) Blocks.Add(new Slider(bBlock));
            else if (typeStr == nameof(Stepper)) Blocks.Add(new Stepper(bBlock));
            else if (typeStr == nameof(Tooltip)) Blocks.Add(new Tooltip(bBlock));
        }

        return;

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(b, 4, ref offset));
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
        result.AddRange(BitConverter.GetBytes(Blocks.Count));

        foreach (var block in Blocks)
        {
            var bytes = block.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return Block.Compress(result.ToArray());
    }

    public void BringToFront(params Block[]? blocks)
    {
        if (blocks == null || blocks.Length == 0)
            return;

        for (var i = blocks.Length - 1; i >= 0; i--)
        {
            Blocks.Remove(blocks[i]);
            Blocks.Add(blocks[i]);
        }
    }
    public void Stack((int x, int y) pivot, Side direction, float alignment = 0f, int gap = 0)
    {
        var opposites = new[] { Side.Right, Side.Left, Side.Bottom, Side.Top };
        var opposite = opposites[(int)direction];

        for (var i = 0; i < Blocks.Count; i++)
        {
            if (i == 0)
            {
                Blocks[i].Position = pivot;
                continue;
            }

            Blocks[i].AlignEdges(opposite, direction, Blocks[i - 1], alignment, gap + 1);
        }
    }

    public bool IsOverlapping((float x, float y) point)
    {
        foreach (var block in Blocks)
            if (block.IsOverlapping(point))
                return true;

        return false;
    }

    public void Update()
    {
        foreach (var e in Blocks)
        {
            var prevCount = Blocks.Count;

            e.Update();

            if (Blocks.Count != prevCount) // was the data modified by this update?
                return; // run away
        }
    }

    public BlockPack Duplicate()
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
    public static implicit operator Block[](BlockPack blockPack)
    {
        return blockPack.Blocks.ToArray();
    }

#region Backend
    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}