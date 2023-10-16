namespace Pure.Examples.ExamplesSystems;

using Engine.Utilities;

public static class UtilityExtensions
{
    public static void Run()
    {
        var expression = "(5 + 7) % 3 ^ 2".Calculate();
    }
}