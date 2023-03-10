using System.Net;
using System.Net.Sockets;

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

	public Server(string name = "Server")
	{
		host = Dns.GetHostEntry(Dns.GetHostName());
		Name = name;
	}

	public string Start(int port = 13000)
	{
		if (server != null)
			Stop();

		try
		{
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			thread = new(Run);
			thread.Start();

			var ips = IPs;
			var ipsString = "";
			for (int i = 0; i < ips.Length; i++)
			{
				var sep = i != 0 ? ", " : "";
				ipsString += $"{sep}'{ips[i]}:{port}'";
			}
			return $"Server started on: {ipsString}";
		}
		catch (Exception e) { return $"Server error: {e.Message}"; }
	}
	public string Stop()
	{
		ipToNickname.Clear();
		isStopping = true;

		server = null;
		thread = null;

		return "Server stopped";
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
	private readonly Dictionary<string, string> ipToNickname = new();
	private TcpListener? server;
	private Thread? thread;
	private bool isStopping;
	private readonly IPHostEntry host;

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

			System.Console.WriteLine();

			var bytes = new byte[size];
			stream.Read(bytes, 0, size);

			ParseMessage(client, stream, new Message(bytes));
		}
	}
	private void ParseMessage(TcpClient client, NetworkStream responseStream, Message msg)
	{
		if (msg.Tag == (byte)Tag.ClientToServerNickname)
		{
			var ep = client?.Client.LocalEndPoint;
			var ip = ep == null ? null : ((IPEndPoint)ep).Address.ToString();
			var nick = GetFreeNickname(msg.Value);

			if (ip == null)
				return;

			ipToNickname[ip] = nick;

			var response = new Message(null, ip, (byte)Tag.ServerToClientNickname, nick);
			responseStream.Write(response.Data);
			return;
		}
	}
	private string GetFreeNickname(string? nickname)
	{
		if (nickname == null)
			nickname = "Player";

		while (ipToNickname.ContainsValue(nickname))
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
	#endregion
}
