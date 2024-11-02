using Pure.Engine.Utilities;

namespace Pure.Tests;

public class UtilityExtensions
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ToBytes()
    {
        var obj = new int[3, 257];
        var bytes = obj.ToBytes();
        CollectionAssert.AreEqual(bytes, new byte[3, 0, 0, 0, 255, 1, 0, 0]);
    }
}