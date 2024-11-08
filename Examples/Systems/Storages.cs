using Pure.Engine.Storage;

namespace Pure.Examples.Systems;

public static class Storages
{
    public enum SomeEnum
    {
        Value1, SecondValue, ValueNumber3
    }

    public class Test
    {
        public readonly string c = "test";
        public uint[] a = [123, 56, 12, 47, 31];
        public int? b = 3;
        public SomeEnum someEnum = SomeEnum.ValueNumber3;
    }

    public static void Run()
    {
        "".ToTable();
        var newArr = new Test().ToTSV();
    }
}