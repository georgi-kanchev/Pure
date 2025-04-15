using Pure.Engine.Utility;

namespace Pure.Examples.Systems;

public static class Storages
{
    [Flags]
    public enum SomeEnum
    {
        Value1 = 1 << 0, SecondValue = 1 << 1, ValueNumber3 = 1 << 2
    }

    public class Test
    {
        [DoNotSave]
        public readonly string c = "test";
        public List<(string txt, uint id)> arr = [("m", 123), ("hmmm", 56), ("he", 12), ("she", 47), ("ai", 31)];
        public (int j, bool k, float l, string m) a = (3, true, 0.3f, "str\nso|me\nnew\nlines");
        public SomeEnum someEnum = SomeEnum.ValueNumber3 | SomeEnum.SecondValue;
        public static string Something { get; set; } = "something|something";
        public float D { get; set; } = 33.6f;

        public Dictionary<string, int[][]> dict = new()
        {
            { "hello", [[35, 4], [11, 2]] },
            { "test testing\nthis\nnewline", [[6, 9], [69, 3]] },
            { "fast", [[19, 77], [8, 1]] }
        };
        public int[][] jag2D =
        [
            [0, 3, 5, 12],
            [125, 5, 612]
        ];
        public int[,] arr2D =
        {
            { 21, 4, 87, 9 },
            { 6, 17, 41, 31 }
        };
    }

    public class Empty : Test
    {
        public (int x, string y, float z) tuple = (3, "hi\nhello", 0.3f);
    }

    public struct SomeStruct
    {
        public static int MyInt { get; set; } = 12;
    }

    public class OtherClass : Test
    {
        public float MyNumber { get; set; } = 87.1f;
    }

    public static void Run()
    {
        var jaggedArray = new int[][]
        {
            [0, 3, 5, 12],
            [25, 5, 61]
        };
        var arr = new[] { "f", "c", "d\nh", "y" };
        var arr3D = new[,,]
        {
            {
                { 1, 2 },
                { 3, 4 }
            },
            {
                { 5, 6 },
                { 7, 8 }
            }
        };
        var list = new List<int> { 5, 12, 687, 123 };
        var dict = new Dictionary<string, int>
        {
            { "hello", 3 },
            { "test\noh\nno", 1 },
            { "fast", 5 }
        };

        var a = new OtherClass().ToDataAsText();
        var b = a.ToRaw();
    }
}