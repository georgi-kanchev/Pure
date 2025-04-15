namespace Pure.Engine.UserInterface;

public class Container : Block
{
    public Container() : this((0, 0))
    {
    }
    public Container(PointI position) : base(position)
    {
        Size = (15, 15);
    }
}