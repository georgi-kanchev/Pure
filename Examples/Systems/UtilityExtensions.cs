using Pure.Utilities;

namespace Pure.Examples.Systems;

public static class UtilityExtensions
{
    public static void Run()
    {
        var expression = "(5 + 7) % 3 ^ 2".Calculate();
    }
}