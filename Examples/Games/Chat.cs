namespace Pure.Examples.Games;

using Engine.LocalAreaNetwork;

public static class Chat
{
    // TODO make this use a window & graphical UI
    public static void Run()
    {
        Log("[host/join]");
        var input = Console.ReadLine();

        if (input == "host")
        {
            Host();
            return;
        }

        Join();

        void Host()
        {
            var server = StartServer();
            Log("Started a server. Type a message or 'quit'.");
            var msg = Console.ReadLine();
            while (msg != "quit")
            {
                server.SendToAll(msg);
                msg = Console.ReadLine();
            }
        }
        void Join()
        {
            var client = ConnectClient();

            Log("Connected. Type a message or 'quit'.");
            var msg = Console.ReadLine();
            while (msg != "quit")
            {
                client.SendToAll(msg);
                msg = Console.ReadLine();
            }
        }
        Client ConnectClient()
        {
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
            client.OnReceive(data => Log(
                $"{(data.fromNickname == string.Empty ? "Server" : data.fromNickname)}: {data.message}"));

            Log("server ip:");
            var ip = Console.ReadLine();
            Log("server port:");
            int.TryParse(Console.ReadLine(), out var port);
            client.Connect(ip, port);

            return client;
        }
        Server StartServer()
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
            return server;
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }
}