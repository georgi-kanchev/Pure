namespace Pure.Examples.ExamplesSystems;

using Engine.Utilities;

public static class UtilityExtensions
{
    public static void Run()
    {
        var expression = "(5 + 7) % 3 ^ 2 * 4 / 2".Calculate();
        var numbers = new[] { "1", "2", "3", "4", "5", "6" };
        numbers.Shift(99, "4", "3", "2");
    }
}