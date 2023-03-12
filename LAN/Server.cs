using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Pure.LAN;

public class Server
{
	public string Name { get; set; }
	public string[] IPs
	{
		get
		{
			var result = new List<string>();
			foreach (var ip in host.AddressList)
				if (ip.AddressFamily == AddressFamily.InterNetwork)
					result.Add(ip.ToString());

			return result.ToArray();
		}
	}
	public bool IsRunning { get; private set; }

	public Server(string name = "Server")
	{
		host = Dns.GetHostEntry(Dns.GetHostName());
		Name = name;
	}

	public bool Start(int port = 13000)
	{
		if (server != null)
			Stop();

		try
		{
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			thread = new(Run);
			thread.Start();

			connectionTimer = new() { AutoReset = true, Interval = 2000 };
			connectionTimer.Elapsed += (s, e) => SendConnectionMessage();
			connectionTimer.Start();

			var ips = IPs;
			var ipsString = "";
			for (int i = 0; i < ips.Length; i++)
			{
				var sep = i != 0 ? ", " : "";
				ipsString += $"{sep}'{ips[i]}:{port}'";
			}
			IsRunning = true;

			return true;
		}
		catch (Exception) { return false; }
	}
	public void Stop()
	{
		IsRunning = false;

		Thread.Sleep(1);
		thread = null;

		foreach (var kvp in connections)
			kvp.Value.stream.Dispose();
		connections.Clear();

		server = null;
	}

	public void WhenReceive(Action<Message> method)
	{
		methods.Add(method);
	}

	public void SendToAllClients(string message)
	{
		foreach (var kvp in connections)
		{
			var msg = new Message(IPs[^1], kvp.Key, Tag.SERVER_TO_CLIENT, message);
			kvp.Value.stream.Write(msg.Data);
		}
	}
	public void SendToClient(string nickname, string message)
	{
		if (connections.ContainsKey(nickname) == false)
			return;

		foreach (var kvp in connections)
			if (nickname == kvp.Value.nickname)
			{
				var msg = new Message(IPs[^1], kvp.Key, Tag.SERVER_TO_CLIENT, message);
				connections[kvp.Key].stream.Write(msg.Data);
			}
	}

	#region Backend
	private class Connection
	{
		public Server parent;
		public string ip, nickname;
		public NetworkStream stream;
		public System.Timers.Timer timeout;

		public Connection(Server parent, string ip, string nickname, NetworkStream stream)
		{
			this.ip = ip;
			this.parent = parent;
			this.nickname = nickname;
			this.stream = stream;
			timeout = new() { AutoReset = true, Interval = 5000 };
			timeout.Elapsed += (s, e) => parent.ConnectionTimeout(this);
			timeout.Start();
		}
	}

	private readonly Dictionary<string, Connection> connections = new();

	private System.Timers.Timer? connectionTimer;
	private readonly List<Action<Message>> methods = new();
	private TcpListener? server;
	private Thread? thread;
	private readonly IPHostEntry host;

	private void Run()
	{
		while (IsRunning)
		{
			if (server == null)
				continue;

			using var client = server.AcceptTcpClient();
			using var stream = client.GetStream();
			var bSize = new byte[4];
			stream.Read(bSize, 0, 4);
			var size = BitConverter.ToInt32(bSize);

			var bytes = new byte[size];
			stream.Read(bytes, 0, size);

			ParseMessage(client, stream, new Message(bytes));
		}
	}
	private void ParseMessage(TcpClient client, NetworkStream responseStream, Message msg)
	{
		Console.WriteLine(msg);

		var clientIP = msg.FromIP;

		if (clientIP == null || connections.ContainsKey(clientIP) == false)
			return;

		msg.FromNickname = connections[clientIP].nickname;

		// client wants to claim a nickname (if free)
		// this is also the first message they send, so add them to the connections
		if (msg.Tag == Tag.CLIENT_TO_SERVER_NICKNAME)
		{
			var nick = GetFreeNickname(msg.Value);

			if (connections.ContainsKey(clientIP) == false)
			{
				var connection = new Connection(this, clientIP, nick, responseStream);
				connections[clientIP] = connection;
			}
			connections[clientIP].nickname = nick;

			System.Console.WriteLine("woooo");

			var response = new Message(IPs[^1], clientIP, Tag.SERVER_TO_CLIENT_NICKNAME, nick);
			responseStream.Write(response.Data);
			return;
		}
		// client notifies me they are connected, don't timeout
		else if (msg.Tag == Tag.CLIENT_TO_SERVER_CONNECTION)
		{
			// not recognizing client? ignore
			if (connections.ContainsKey(clientIP) == false)
				return;

			var t = connections[clientIP].timeout;
			t.Stop();
			t.Start();
		}
		// a client wants to disconnect and does so, notify everyone else
		else if (msg.Tag == Tag.CLIENT_TO_SERVER_DISCONNECT)
		{
			if (msg.FromIP == null || connections.ContainsKey(msg.FromIP) == false)
				return;

			var c = connections[msg.FromIP];
			c.stream.Dispose();
			c.timeout.Dispose();
			connections.Remove(c.ip);

			foreach (var kvp in connections)
			{
				var respone = new Message(IPs[^1], kvp.Value.ip, Tag.SERVER_TO_CLIENT_DISCONNECT, c.ip);
				kvp.Value.stream.Write(respone.Data);
			}
		}
	}
	private string GetFreeNickname(string? nickname)
	{
		if (nickname == null)
			nickname = "Player";

		while (NicknameExists(nickname))
		{
			var number = "";
			for (int i = nickname.Length - 1; i >= 0; i--)
				if (char.IsNumber(nickname[i]))
					number = number.Insert(0, nickname[i].ToString());

			if (number == "")
			{
				nickname += "1";
				continue;
			}

			int.TryParse(nickname, out var n);
			n++;
			var nIndex = nickname.Length - number.Length;
			nickname = nickname[..nIndex] + n.ToString();
		}
		return nickname;
	}
	private bool NicknameExists(string nickname)
	{
		foreach (var kvp in connections)
			if (nickname == kvp.Value.nickname)
				return true;

		return false;
	}

	private void SendConnectionMessage()
	{
		foreach (var kvp in connections)
		{
			var msg = new Message(IPs[^1], kvp.Key, Tag.SERVER_TO_CLIENT_CONNECTION, "");
			kvp.Value.stream.Write(msg.Data);
		}
	}
	private void ConnectionTimeout(Connection connection)
	{
		// this connection hasn't reach out to me in about 5 seconds
		// so drop it and notify everyone else to do the same - timeout
		var c = (Connection?)connection;

		if (c == null)
			return;

		c.stream.Dispose();
		c.timeout.Dispose();
		connections.Remove(c.ip);

		foreach (var kvp in connections)
		{
			var msg = new Message(IPs[^1], kvp.Value.ip, Tag.SERVER_TO_CLIENT_TIMEOUT, c.ip);
			kvp.Value.stream.Write(msg.Data);
		}
	}
	#endregion
}
