namespace TestGame;

using Pure.LAN;
using Pure.Window;

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

	// pure;to TestGame;dotnet build -c Release;to bin/Release/net6.0;dotnet TestGame.dll

	class Server : BaseServer
	{
		protected override void OnMessageReceive(string fromNickname, byte tag, string message)
		{
			Console.WriteLine($"Server: from {fromNickname}, {tag} | {message}");
		}
	}
	class Client : BaseClient
	{
		public Client(string nickname) : base(nickname) { }

		protected override void OnMessageReceive(string fromNickname, byte tag, string message)
		{
			Console.WriteLine($"{Nickname}: from {fromNickname}, {tag} | {message}");
		}
	}

	static void Main()
	{
		var server = new Server();
		var client = new Client("pen4o");
		var client2 = new Client("stamat");
		var client3 = new Client("troyan");
		server.Start(13000);
		client.Connect("127.0.0.1", 13000);
		client2.Connect("127.0.0.1", 13000);
		client3.Connect("127.0.0.1", 13000);

		while (Window.IsExisting)
		{
			Window.Activate(true);
			Window.Activate(false);
		}
	}
}
