using System.Buffers.Text;
using System.Net.Sockets;
using System.Text;

namespace Pure.LAN;

public class Client
{
	public string Nickname { get; set; }

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
			networkStream.Write(new byte[] { 0, (byte)Tag.ClientToServerConnect });

			thread = new(Run);
			thread.Start();

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

	public string SendToServer(string text)
	{
		if (networkStream == null)
			return $"'{Nickname}' error: No connection!";

		var textBytes = Utils.Compress(text);
		var bytes = new byte[2 + textBytes.Length];

		bytes[1] = (byte)Tag.ClientToServerNickname;
		Array.Copy(textBytes, 0, bytes, 2, textBytes.Length);
		networkStream.Write(bytes);

		return $"'{Nickname}' sent text to server: '{text}'";
	}

	#region Backend
	private Thread? thread;
	private TcpClient? client;
	private Stream? networkStream;
	private byte id = 0;

	private void Run()
	{
		while (true)
		{
			if (client == null || networkStream == null)
				continue;

			var bytes = new byte[client.ReceiveBufferSize];
			networkStream.Read(bytes, 0, bytes.Length);

			var id = default(byte);
			var tag = default(byte);
			if (bytes.Length > 0)
				id = bytes[0];
			if (bytes.Length > 1)
				tag = bytes[1];

			var bMessage = new byte[bytes.Length - 2];
			Array.Copy(bytes, 2, bMessage, 0, bMessage.Length);

			if (tag == (byte)Tag.ServerToClientID)
				id = bMessage[0];
		}
	}
	#endregion
}