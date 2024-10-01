namespace Pure.Examples.Systems;

using Engine.Execution;

public static class Commands
{
    public static void Run()
    {
        var cmd = new CommandPack();

        cmd.Add("log", () =>
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