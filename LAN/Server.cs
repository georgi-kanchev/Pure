using System.Net;
using System.Net.Sockets;
using System.Text;

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

	public void ReceiveMessage(Action<string> method)
	{
		msgStrings.Add(method);
	}

	#region Backend
	private readonly List<Action<string>> msgStrings = new();
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
			client.ReceiveBufferSize = 4096;
			var stream = client.GetStream();
			var bytes = new byte[client.ReceiveBufferSize];

			stream.Read(bytes, 0, bytes.Length);

			var id = default(byte);
			var tag = default(byte);
			if (bytes.Length > 0)
				id = bytes[0];
			if (bytes.Length > 1)
				tag = bytes[1];

			var bMessage = new byte[bytes.Length - 2];
			Array.Copy(bytes, 2, bMessage, 0, bMessage.Length);

			if (tag == (byte)Tag.ClientToServerConnect)
			{

			}
			else if (tag == (byte)Tag.ClientToServerNickname)
			{
				var nick = Utils.Decompress(bMessage);
				//nicknames(nicknames);
				var response = new byte[] { 0, (byte)Tag.ServerToClientID, (byte)nicknames.Count };
				stream.Write(response);
			}
		}
	}
	#endregion
}
