using Pure.Engine.Execution;

namespace Pure.Examples.Systems;

public static class Commands
{
    public static void Run()
    {
        var cmd = new Engine.Execution.Commands();

        cmd.Create("log", () =>
        {
            var text = cmd.GetNextValue<string>();
            var intArray = cmd.GetNextValue<int[]>();
            var boolArray = cmd.GetNextValue<bool[]>();

            Console.WriteLine(text);
            return "result";
        });
        var results =
            cmd.Execute("log `hello, |;world!` 1|2|3|4 true|false|true ; log `second command`");
    }
}