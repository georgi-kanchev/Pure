namespace Pure.Examples.Systems;

using LocalAreaNetwork;

public static class ChatLAN
{
    private class Server : BaseServer
    {
        protected override void OnMessageReceive(string fromNickname, byte tag, string message)
        {
            Console.WriteLine($"{fromNickname}: {message}");
        }
        protected override void OnClientConnect(string clientNickname)
        {
            Console.WriteLine($"'{clientNickname}' connected.");
        }
        protected override void OnClientDisconnect(string clientNickname)
        {
            Console.WriteLine($"'{clientNickname}' disconnected.");
        }
    }

    private class Client : BaseClient
    {
        public Client(string nickname) : base(nickname) { }

        protected override void OnMessageReceive(string fromNickname, byte tag, string message)
        {
            fromNickname = fromNickname == "" ? "Server" : fromNickname;
            Console.WriteLine($"{fromNickname}: {message}");
        }
        protected override void OnClientConnect(string clientNickname)
        {
            Console.WriteLine($"'{clientNickname}' connected.");
        }
        protected override void OnClientDisconnect(string clientNickname)
        {
            Console.WriteLine($"'{clientNickname}' disconnected.");
        }
        protected override void OnLostConnection()
        {
            Console.WriteLine($"Lost connection.");
        }
        protected override void OnReconnectionAttempt()
        {
            Console.WriteLine($"Trying to reconnect...");
        }
    }

    public static void Run()
    {
        Console.WriteLine("[host/join]");
        var isHost = Console.ReadLine() == "host";

        if (isHost)
        {
            var server = new Server();
            server.Start(13000);

            Console.WriteLine("Started a server. Type a message or 'quit'.");

            var msg = Console.ReadLine();
            while (msg != "quit")
            {
                server.SendToAll(msg);
                msg = Console.ReadLine();
            }

            return;
        }

        Console.WriteLine("nickname:");
        var n = Console.ReadLine();
        var nick = string.IsNullOrWhiteSpace(n) ? "Chatter" : n;
        var client = new Client(nick);
        Console.WriteLine("server ip:");
        var ip = Console.ReadLine();
        Console.WriteLine("server port:");
        int.TryParse(Console.ReadLine(), out var port);
        client.Connect(ip, port);

        Console.WriteLine("Connected. Type a message or 'quit'.");
        var input = Console.ReadLine();
        while (input != "quit")
        {
            client.SendToAll(input);
            input = Console.ReadLine();
        }
    }
}