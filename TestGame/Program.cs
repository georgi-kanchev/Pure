namespace TestGame;

using Pure.LAN;

public class Program
{
	// raycast texture mapping
	// - https://youtu.be/fSjc8vLMg8c?t=504
	// - https://www.youtube.com/watch?v=fRu8kjXvkdY
	//
	// https://chillmindscapes.itch.io/
	// tilemap editor collisions
	// loading cursor
	// checkbox do toggle button with bigger widths
	// sprite/tile 90 angle rotations + mirror H & V
	// sprite varying sizes/sprite collection?
	// smooth camera scroll with 1 tile margin outside the screen

	// <ProjectReference Include="..\Animation\Animation.csproj" />
	// <ProjectReference Include="..\Audio\Audio.csproj" />
	// <ProjectReference Include="..\Collision\Collision.csproj" />
	// <ProjectReference Include="..\Modding\Modding.csproj" />
	// <ProjectReference Include="..\Particles\Particles.csproj" />
	// <ProjectReference Include="..\Storage\Storage.csproj" />
	// <ProjectReference Include="..\Tilemap\Tilemap.csproj" />
	// <ProjectReference Include="..\UserInterface\UserInterface.csproj" />
	// <ProjectReference Include="..\Utilities\Utilities.csproj" />
	// <ProjectReference Include="..\Window\Window.csproj" />
	// <ProjectReference Include="..\Pathfinding\Pathfinding.csproj" />
	// <ProjectReference Include="..\Tracker\Tracker.csproj" />

	class Server : BaseServer
	{
		protected override void OnMessageReceive(string fromNickname, byte tag, string message)
		{
			Console.WriteLine($"{fromNickname}: {message}");
		}
		protected override void OnClientConnect(string clientNickname)
		{
			Console.WriteLine($"Client '{clientNickname}' connected.");
		}
		protected override void OnClientDisconnect(string clientNickname)
		{
			Console.WriteLine($"Client '{clientNickname}' disconnected.");
		}
	}
	class Client : BaseClient
	{
		public Client(string nickname) : base(nickname) { }

		protected override void OnMessageReceive(string fromNickname, byte tag, string message)
		{
			fromNickname = fromNickname == "" ? "Server" : fromNickname;
			Console.WriteLine($"{fromNickname}: {message}");
		}
		protected override void OnClientConnect(string clientNickname)
		{
			Console.WriteLine($"Client '{clientNickname}' connected.");
		}
		protected override void OnClientDisconnect(string clientNickname)
		{
			Console.WriteLine($"Client '{clientNickname}' disconnected.");
		}
		protected override void OnLostConnection()
		{
			Console.WriteLine($"Lost connection!");
		}
		protected override void OnReconnectionAttempt()
		{
			Console.WriteLine($"Trying to reconnect...");
		}
	}

	static void Main()
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
		var nick = Console.ReadLine() ?? "Chatter";
		var client = new Client(nick);
		Console.WriteLine("server ip:");
		var ip = Console.ReadLine();
		Console.WriteLine("server port:");
		int.TryParse(Console.ReadLine(), out var port);
		client.Connect(ip, port);

		var input = Console.ReadLine();
		while (input != "quit")
		{
			client.SendToAll(input);
			input = Console.ReadLine();
		}
	}
}
