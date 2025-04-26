namespace Pure.Examples.Systems;

public static class Commands
{
    public static void Run()
    {
        var cmd = new Engine.Execution.Commands();

        cmd.Create("logMessage", () =>
        {
            var text = cmd.GetNextValue<string>();
            var intArray = cmd.GetNextValue<int[]>();
            var boolArray = cmd.GetNextValue<bool[]>();

            // Console.WriteLine(text);
            return "result";
        });
        var results = cmd.Execute("Log-Message `hello, |;world!` 1|2|3|4 true|false|true ; log~MessagE `second command`");
    }
}