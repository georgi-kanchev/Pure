namespace Pure.Examples.ExampleApplications;

using Engine.LocalAreaNetwork;

public static class Chat
{
    public static void Run()
    {
        Log("[host/join]");

        if (Console.ReadLine() == "host")
        {
            var server = new Server();
            server.OnError(error =>
            {
                Log($"Error: {error}");
                Environment.Exit(0);
            });
            server.OnReceive(data => Log($"{data.fromNickname}: {data.message}"));
            server.OnClientConnect(nickname => Log($"'{nickname}' connected."));
            server.OnClientDisconnect(nickname => Log($"'{nickname}' disconnected."));
            server.Start(13000);

            Log("Started a server. Type a message or 'quit'.");

            var msg = Console.ReadLine();
            while (msg != "quit")
            {
                server.SendToAll(msg);
                msg = Console.ReadLine();
            }

            return;
        }

        Log("nickname:");
        var nickInput = Console.ReadLine();
        var nick = string.IsNullOrWhiteSpace(nickInput) ? "Chatter" : nickInput;
        var client = new Client(nick);
        client.OnError(error =>
        {
            Log($"Error: {error}");
            Environment.Exit(0);
        });
        client.OnClientConnect(nickname => Log($"'{nickname}' connected."));
        client.OnClientDisconnect(nickname => Log($"'{nickname}' disconnected."));
        client.OnLostConnection(() => Log("Lost connection."));
        client.OnReconnectionAttempt(() => Log("Trying to reconnect..."));
        client.OnReceive(data =>
            Log($"{(data.fromNickname == "" ? "Server" : data.fromNickname)}: {data.message}"));

        Log("server ip:");
        var ip = Console.ReadLine();
        Log("server port:");
        int.TryParse(Console.ReadLine(), out var port);
        client.Connect(ip, port);

        Log("Connected. Type a message or 'quit'.");
        var input = Console.ReadLine();
        while (input != "quit")
        {
            client.SendToAll(input);
            input = Console.ReadLine();
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }
}