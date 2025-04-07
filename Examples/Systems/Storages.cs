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
        public readonly string c = "test";
        // public List<(string, uint)> arr = [("m", 123), ("hmmm", 56), ("he", 12), ("she", 47), ("ai", 31)];
        public (int j, bool k, float l) a = (3, true, 0.3f);
        public SomeEnum someEnum = SomeEnum.ValueNumber3 | SomeEnum.SecondValue;
        public static string Something { get; set; } = "heeeelloooo";
        public float D { get; set; } = 33.6f;

        public Dictionary<string, int[][]> dict = new()
        {
            { "hello", [[35, 4], [11, 2]] },
            { "test", [[6, 9], [69, 3]] },
            { "fast", [[19, 77], [8, 1]] }
        };
        public int[][] jag2D =
        [
            [0, 3, 5, 12],
            [125, 5, 612, 3]
        ];
        public int[,] arr2D =
        {
            { 21, 4, 87, 9 },
            { 6, 17, 41, 31 }
        };
    }

    public struct SomeStruct
    {
        public static int MyInt { get; set; } = 12;
    }

    public static class StaticClass
    {
        public static float MyNumber { get; set; } = 87.1f;
    }

    public static void Run()
    {
        var jaggedArray = new int[][]
        {
            [0, 3, 5, 12],
            [25, 5, 61]
        };
        var arr = new[] { 6, 7, 12, 4 };

        var list = new List<int> { 5, 12, 687, 123 };
        var dict = new Dictionary<string, int[]>
        {
            { "hello", [35, 4] },
            { "test", [12, 7] },
            { "fast", [1, 88] }
        };

        var a = new Test().ToPipes();
        var b = a.ToObj<Test>();
        File.WriteAllText("data.tsv", a);
    }
}

// c
//  |test
//
// a
//  |m|3
//  |j|true
//  |k|0.3
//
// someEnum
//  |ValueNumber3 SecondValue
//
// Something
//  |heeeelloooo
//
// D
//  |33.6
//
// dict
//  |hello
//  | |0|35
//  | |1|4
//  |test
//  | |0|12
//  | |1|7
//  |fast
//  | |0|1
//  | |1|88
//
// arr2D
//  |0|0|0
//  |0|1|3
//  |0|2|5
//  |0|3|12
//  |1|0|125
//  |1|1|5
//  |1|2|612
//  |1|3|3