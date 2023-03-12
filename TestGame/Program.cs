namespace TestGame;

using System;
using Pure.LAN;
using Pure.Utilities;

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

	// pure;to TestGame;dotnet build -c Release;to bin/Release/net6.0;dotnet TestGame.dll

	// https://gist.github.com/define-private-public/cea29b56eebaf59714f6c858f26b46d0

	static void Main()
	{
		Console.Write("Enter 'host' or 'join': ");
		var isHost = Console.ReadLine() == "host";

		if (isHost)
		{
			Console.Write("Enter a port: ");
			int.TryParse(Console.ReadLine(), out var port);
			var server = new Server();

			Console.WriteLine(server.IPs.ToString(", "));
			server.WhenReceive((m) => Console.WriteLine($"{m.FromNickname}: {m.Value}"));

			while (server.Start(port) == false)
			{
				Console.WriteLine("Failed to start");
				Console.Write("Enter a port: ");
				int.TryParse(Console.ReadLine(), out var p);
				port = p;
			}
			Console.WriteLine("Started\nEnter 'quit' to exit");

			var input = Console.ReadLine();
			while (input != "quit")
			{
				if (string.IsNullOrWhiteSpace(input) == false)
				{
					server.SendToAllClients(input);
					Console.WriteLine($"Sent: {input}");
				}

				input = Console.ReadLine();
			}
			server.Stop();

			return;
		}

		Console.Write("Enter the host IP to connect or 'quit' to exit: ");
		var ip = Console.ReadLine();
		var client = new Client();

		client.WhenReceive((m) => Console.WriteLine($"{m.FromNickname}: {m.Value}"));

		while (ip == null || (ip != "quit" && client.Connect(ip) == false))
		{
			Console.WriteLine("Failed to connect");
			Console.Write("Enter the host IP to connect or 'quit' to exit: ");
			ip = Console.ReadLine();
		}
		if (ip != "quit")
		{
			Console.WriteLine("Connected\nEnter 'quit' to exit");

			var input = Console.ReadLine();
			while (input != "quit")
			{
				if (string.IsNullOrWhiteSpace(input) == false)
					client.SendToAllClients(input);

				input = Console.ReadLine();
			}
		}
		client.Disconnect();
	}
}
