namespace Pure.Engine.Storage;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SaveAtOrder(uint order) : Attribute
{
    public uint Value { get; } = order;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DoNotSave : Attribute
{
}