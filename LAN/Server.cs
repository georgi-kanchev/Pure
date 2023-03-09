using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pure.LAN;

public class Server
{
	public string Name { get; set; }

	public Server(string name = "Server")
	{
		Name = name;
	}

	public string Start(string ip = "127.0.0.1", int port = 13000)
	{
		if (server != null)
			Stop();

		try
		{
			server = new TcpListener(IPAddress.Parse(ip), port);
			server.Start();

			thread = new(Run);
			thread.Start();

			return $"Server started on '{ip}:{port}'";
		}
		catch (Exception e) { return $"Server error: {e.Message}"; }
	}
	public void Stop()
	{
		nicknames.Clear();
		isStopping = true;

		server = null;
		thread = null;
	}

	public void WhenReceive(Action<Message> method)
	{
		methods.Add(method);
	}
	public void SendToAll()
	{

	}
	public void SendToClient()
	{

	}

	#region Backend
	private readonly List<Action<Message>> methods = new();
	private readonly Dictionary<byte, string> nicknames = new();
	private TcpListener? server;
	private Thread? thread;
	private bool isStopping;
	private byte currID;

	private void Run()
	{
		while (true)
		{
			if (isStopping)
				break;

			if (server == null)
				continue;

			using var client = server.AcceptTcpClient();
			using var stream = client.GetStream();
			var bSize = new byte[4];
			stream.Read(bSize, 0, 4);
			var size = BitConverter.ToInt32(bSize);

			System.Console.WriteLine(client.Client.LocalEndPoint.ToString());

			var bytes = new byte[size];
			stream.Read(bytes, 0, size);

			var msg = new Message(bytes);

			if (msg.Tag == (byte)Tag.ClientToServerConnect)
			{
				currID++;
				nicknames[currID] = msg.Value == null ? "Player" : msg.Value;

				var response = new Message(0, 0, (byte)Tag.ServerToClientID, $"{currID}");
				stream.Write(response.Data);
				return;
			}

			msg.FromNickname = nicknames[msg.FromID];

			if (msg.Tag == (byte)Tag.ClientToServer)
			{
				for (int i = 0; i < methods.Count; i++)
					methods[i]?.Invoke(msg);

				return;
			}

		}
	}
	#endregion
}
