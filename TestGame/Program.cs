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

	// https://github.com/ygoe/AsyncTcpClient

	class Server : BaseServer
	{
		protected override void OnMessageReceive(string clientIP, string message)
		{
			Console.WriteLine($"{clientIP}: {message}");
		}
	}

	static void Main()
	{
		var server = new Server();
		var client = new BaseClient();
		server.Start(13000);
		client.Connect("127.0.0.1", 13000);

		client.SendMessage("helloooo");

		while (Window.IsExisting)
		{
			Window.Activate(true);
			Window.Activate(false);
		}

		//Console.WriteLine("Server IP:");
		//var ip = Console.ReadLine();
		//var client = new Client("niiiick");
		//client.Connect(ip == null ? "" : ip, 13000);
	}
}
