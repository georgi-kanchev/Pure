namespace TestGame;

using Pure.LAN;

public class Program
{
	// raycast texture mapping https://youtu.be/fSjc8vLMg8c?t=504
	// https://chillmindscapes.itch.io/
	// tilemap editor collisions
	// loading cursor
	// checkbox do toggle button with bigger widths
	// sprite/tile mirror H & V

	static void Main()
	{
		var server = new Server();
		var client = new Client();
		var s = server.Start();
		var c = client.Connect("172.29.130.209");

		System.Console.WriteLine(s);
		System.Console.WriteLine(c);
		//client.SendToServer("helloooooo, world! :D");
	}
}
