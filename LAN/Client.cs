using System.Net.Sockets;

namespace Pure.LAN;

public class Client
{
	public string Nickname
	{
		get => nicknames[ID];
		set => nicknames[ID] = string.IsNullOrWhiteSpace(value) ? "Player" : value;
	}
	public byte ID { get; private set; }

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

			thread = new(Run);
			thread.Start();

			var msg = new Message(0, 0, (byte)Tag.ClientToServerConnect, Nickname);
			networkStream.Write(msg.Data);

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

		var msg = new Message(ID, 0, (byte)Tag.ClientToServer, message);
		networkStream.Write(msg.Data);

		return $"'{Nickname}' sent message to server: '{message}'";
	}
	public string SendToClient(byte clientID, string message)
	{
		if (networkStream == null)
			return ErrorNoConnection;

		if (nicknames.ContainsKey(clientID) == false)
			return $"'{Nickname}' tried to send message to '{clientID}' but could not find them";

		var msg = new Message(ID, clientID, (byte)Tag.ClientToClient, message);
		networkStream.Write(msg.Data);

		return $"'{Nickname}' sent message to '{nicknames[clientID]}': '{message}'";
	}

	#region Backend
	private string ErrorNoConnection => $"'{Nickname}' error: No connection!";

	private readonly List<Action<Message>> methods = new();
	private readonly Dictionary<byte, string> nicknames = new();
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
			networkStream.Read(bSize, 0, 4);
			var size = BitConverter.ToInt32(bSize);

			var bytes = new byte[size];
			networkStream.Read(bytes, 0, size);

			var msg = new Message(bytes);

			if (msg.ToID != ID) // not for me?
				return;

			msg.FromNickname = nicknames.ContainsKey(msg.FromID) ? null : nicknames[msg.FromID];
			msg.ToNickname = nicknames[ID];

			if (msg.Tag == (byte)Tag.ServerToClientID)
			{
				byte.TryParse(msg.Value, out var myNewID);
				ID = myNewID;
			}

			if (msg.Tag == (byte)Tag.ServerToClient ||
				msg.Tag == (byte)Tag.ClientToClient)
			{
				for (int i = 0; i < methods.Count; i++)
					methods[i].Invoke(msg);

				return;
			}
		}
	}
	#endregion
}