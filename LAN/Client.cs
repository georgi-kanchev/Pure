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
	public bool IsConnected { get; private set; }

	public Client(string nickname = "Player")
	{
		Nickname = nickname;
	}

	public bool Connect(string serverIP = "127.0.0.1", int serverPort = 13000)
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

			connectionTimer = new() { AutoReset = true, Interval = 2000 };
			connectionTimer.Elapsed += (s, e) => SendConnectionMessage();
			connectionTimer.Start();

			timeout = new() { AutoReset = true, Interval = 5000 };
			timeout.Elapsed += (s, e) => Disconnect();
			timeout.Start();

			IsConnected = true;

			if (IP != null)
			{
				var msg = new Message(IP, serverIP, (byte)Tag.CLIENT_TO_SERVER_NICKNAME, Nickname);
				networkStream.Write(msg.Data);
			}

			return true;
		}
		catch (Exception) { Disconnect(); return false; }
	}
	public void Disconnect()
	{
		IsConnected = false;

		connectionTimer?.Dispose();
		connectionTimer = null;
		ipToNicknames.Clear();

		Thread.Sleep(1);
		thread = null;

		client?.Dispose();
		client = null;

		networkStream?.Dispose();
		networkStream = null;
	}

	public void WhenReceive(Action<Message> method)
	{
		methodsMsgReceive.Add(method);
	}

	public void SendToServer(string message)
	{
		if (networkStream == null)
			return;

		var msg = new Message(IP, serverIP, Tag.CLIENT_TO_SERVER, message);
		networkStream.Write(msg.Data);
	}
	public void SendToClient(string nickname, string message)
	{
		if (networkStream == null)
			return;

		foreach (var kvp in ipToNicknames)
			if (nickname == kvp.Value)
			{
				var msg = new Message(IP, kvp.Key, Tag.CLIENT_TO_CLIENT, message);
				networkStream.Write(msg.Data);
			}
	}
	public void SendToAllClients(string message)
	{
		if (networkStream == null)
			return;

		var msg = new Message(IP, serverIP, Tag.CLIENT_TO_ALL, message);
		networkStream.Write(msg.Data);
	}

	#region Backend
	private System.Timers.Timer? connectionTimer, timeout;

	private string? serverIP;
	private readonly List<Action<Message>> methodsMsgReceive = new();
	private readonly Dictionary<string, string> ipToNicknames = new();
	private Thread? thread;
	private TcpClient? client;
	private Stream? networkStream;

	private void Run()
	{
		while (IsConnected)
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
		// one of the clients has new free nickname
		if (msg.Tag == Tag.SERVER_TO_CLIENT_NICKNAME &&
			msg.ToIP != null && msg.Value != null)
			ipToNicknames[msg.ToIP] = msg.Value;

		// after nickname change...
		msg.FromNickname = msg.FromIP == null ||
			ipToNicknames.ContainsKey(msg.FromIP) == false ? null :
			ipToNicknames[msg.FromIP];
		msg.ToNickname = msg.ToIP == null ||
			ipToNicknames.ContainsKey(msg.ToIP) == false ? null :
			ipToNicknames[msg.ToIP];

		// not for me?
		if (msg.ToIP != IP)
			return;

		// regular messagess...
		if (msg.Tag == Tag.SERVER_TO_CLIENT ||
			msg.Tag == Tag.CLIENT_TO_CLIENT)
		{
			for (int i = 0; i < methodsMsgReceive.Count; i++)
				methodsMsgReceive[i].Invoke(msg);
		}
		// server says they are still alive, do not timeout
		else if (msg.Tag == Tag.SERVER_TO_CLIENT_CONNECTION)
		{
			timeout?.Stop();
			timeout?.Start();
		}
		// server stopped, time to leave
		else if (msg.Tag == Tag.SERVER_TO_CLIENT_STOP)
			Disconnect();
		// someone timed out or disconnected, remove him locally
		else if (msg.Tag == Tag.SERVER_TO_CLIENT_TIMEOUT ||
			msg.Tag == Tag.SERVER_TO_CLIENT_DISCONNECT)
		{
			if (msg.Value != null && ipToNicknames.ContainsKey(msg.Value))
				ipToNicknames.Remove(msg.Value);
		}
	}
	private void SendConnectionMessage()
	{
		if (networkStream == null)
			return;

		var msg = new Message(IP, serverIP, Tag.CLIENT_TO_SERVER_CONNECTION, "");
		networkStream.Write(msg.Data);
	}
	#endregion
}