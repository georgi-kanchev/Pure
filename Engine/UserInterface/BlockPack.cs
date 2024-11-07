namespace Pure.Engine.UserInterface;

public class BlockPack
{
    public List<Block> Blocks { get; } = [];

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

    public static implicit operator Block[](BlockPack blockPack)
    {
        return blockPack.Blocks.ToArray();
    }
}