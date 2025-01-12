using Pure.Engine.Tiles;
using Pure.Engine.Utility;

namespace Pure.Examples.Systems;

public static class Storages
{
    public enum SomeEnum
    {
        Value1, SecondValue, ValueNumber3
    }

    public class Test
    {
        // public readonly string c = "test";
        // public uint[] a = [123, 56, 12, 47, 31];
        // public int? b = null;
        // public SomeEnum someEnum = SomeEnum.ValueNumber3;
        //
        // public static string Something { get; set; } = "heeeelloooo";
        //
        // [SaveAtOrder(10)]
        // public float D { get; set; } = 33.6f;

        public (float chance, string[] tiles)[] PlainRegionClustersChanceTiles { get; private set; } = [];

        public Test()
        {
            PlainRegionClustersChanceTiles =
            [
                (0.2f, [
                    "*RegionBush1", "*RegionBush2", "*RegionBush3",
                    "*RegionBush4", "*RegionBush5", "*RegionBush6"
                ])
            ];
        }
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
        var array2D = new[,]
        {
            { 0, 3, 5, 12 },
            { 125, 5, 612, 3 }
        };
        var arr = new[] { 6, 7, 12, 4 };

        var list = new List<int> { 5, 12, 687, 123 };
        var dict = new Dictionary<string, int[]>
        {
            { "hello", [35, 4] },
            { "test", [12, 7] },
            { "fast", [1, 88] }
        };

        var a = new Test().ToTSV();
        var b = a.ToObject<Test>();
    }
}