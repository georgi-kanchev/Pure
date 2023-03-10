using System.Net;
using System.Net.Sockets;

namespace Pure.LAN;

public class Client
{
	public string Nickname { get; set; }
	//{
	//	get => ipToNicknames[IP];
	//	set => ipToNicknames[IP] = string.IsNullOrWhiteSpace(value) ? "Player" : value;
	//}
	public string? IP
	{
		get
		{
			var ep = client?.Client.LocalEndPoint;
			return ep == null ? null : ((IPEndPoint)ep).Address.ToString();
		}
	}

	public Client(string nickname = "Player")
	{
		Nickname = nickname;
	}

	public string Connect(string serverIP = "127.0.0.1", int serverPort = 13000)
	{
		if (client != null)
			Disconnect();

		try
		{
			client = new TcpClient(serverIP, serverPort);
			networkStream = client.GetStream();
			this.serverIP = serverIP;

			thread = new(Run);
			thread.Start();

			if (IP != null)
			{
				var msg = new Message(IP, serverIP, (byte)Tag.ClientToServerNickname, Nickname);
				networkStream.Write(msg.Data);
			}

			return $"'{Nickname}' connected to server '{serverIP}:{serverPort}'";
		}
		catch (Exception e)
		{
			Disconnect();
			return $"'{Nickname}' error: {e.Message}";
		}
	}
	public void Disconnect()
	{
		client?.Dispose();
		client = null;

		networkStream?.Dispose();
		networkStream = null;

		thread = null;
	}

	public void WhenReceive(Action<Message> method)
	{
		methods.Add(method);
	}

	public string SendToServer(string message)
	{
		if (networkStream == null)
			return ErrorNoConnection;

		//var msg = new Message(IP, 0, (byte)Tag.ClientToServer, message);
		//networkStream.Write(msg.Data);

		return $"'{Nickname}' sent message to server: '{message}'";
	}
	public string SendToClient(string ip, string message)
	{
		if (networkStream == null)
			return ErrorNoConnection;

		if (ipToNicknames.ContainsKey(ip) == false)
			return $"'{Nickname}' tried to send message to '{ip}' but could not find them";

		//var msg = new Message(IP, ip, (byte)Tag.ClientToClient, message);
		//networkStream.Write(msg.Data);

		return $"'{Nickname}' sent message to '{ipToNicknames[ip]}': '{message}'";
	}

	#region Backend
	private string ErrorNoConnection => $"'{Nickname}' error: No connection!";

	private string? serverIP;
	private readonly List<Action<Message>> methods = new();
	private readonly Dictionary<string, string> ipToNicknames = new();
	private Thread? thread;
	private TcpClient? client;
	private Stream? networkStream;

	private void Run()
	{
		while (true)
		{
			if (client == null || networkStream == null)
				continue;

			var bSize = new byte[4];
			var r = networkStream.Read(bSize, 0, 4);
			var size = BitConverter.ToInt32(bSize);

			if (r == 0)
				continue;

			var bytes = new byte[size];
			networkStream.Read(bytes, 0, size);

			ParseMessage(new Message(bytes));
		}
	}
	private void ParseMessage(Message msg)
	{
		var isForMe = msg.ToIP == IP;

		// one of the clients has new free nickname
		if (msg.Tag == (byte)Tag.ServerToClientNickname &&
			msg.ToIP != null && msg.Value != null)
			ipToNicknames[msg.ToIP] = msg.Value;

		msg.FromNickname = msg.FromIP == null ||
			ipToNicknames.ContainsKey(msg.FromIP) == false ? null :
			ipToNicknames[msg.FromIP];
		msg.ToNickname = msg.ToIP == null ||
			ipToNicknames.ContainsKey(msg.ToIP) == false ? null :
			ipToNicknames[msg.ToIP];

		if (msg.Tag == (byte)Tag.ServerToClient ||
			msg.Tag == (byte)Tag.ClientToClient)
		{
			for (int i = 0; i < methods.Count; i++)
				methods[i].Invoke(msg);

			return;
		}
	}
	#endregion
}