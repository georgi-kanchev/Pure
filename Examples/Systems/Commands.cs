using Cmd = Pure.Commands.Commands;

namespace Pure.Examples.Systems;

public static class Commands
{
    public static void Run()
    {
        var cmd = new Cmd();

        cmd.Add("log", () =>
        {
            var text = cmd.GetNextValue<int[]>();
            Console.WriteLine(text);
            return default;
        });
        cmd.Execute("log 1|2|3|4|5|6");
    }
}