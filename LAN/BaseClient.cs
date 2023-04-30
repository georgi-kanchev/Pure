namespace Pure.LAN;

using System.Collections.Concurrent;
using System.Net.Sockets;

/// <summary>
/// Represents a base class for LAN client communication.
/// </summary>
public class BaseClient : Base
{
	/// <summary>
	/// Gets an array of nicknames of all connected clients.
	/// </summary>
	public string[] Nicknames => clients.Values.ToArray();
	/// <summary>
	/// Gets an array of identifiers of all connected clients.
	/// </summary>
	public byte[] IDs => clients.Keys.ToArray();

	/// <summary>
	/// Gets a value indicating whether the client is currently connected to the network.
	/// </summary>
	public bool IsConnected { get; private set; }
	/// <summary>
	/// Gets or sets the identifier of the client.
	/// </summary>
	public byte ID { get; internal set; }
	/// <summary>
	/// Gets the nickname of the client.
	/// </summary>
	public string Nickname { get; private set; }

	/// <summary>
	/// Initializes a new instance with the specified nickname.
	/// </summary>
	/// <param name="nickname">The nickname of the client. If null or whitespace, 
	/// the default value "Player" will be used.</param>
	public BaseClient(string nickname)
	{
		Nickname = string.IsNullOrWhiteSpace(nickname) ? "Player" : nickname.Trim();
	}

	/// <summary>
	/// Connects the client to a server at the specified ip
	/// address and port.
	/// </summary>
	/// <param name="ip">The IP address of the server.</param>
	/// <param name="port">The port of the server.</param>
	public void Connect(string ip, int port)
	{
		client?.Dispose();
		client = new(this, ip, port);
		client.ConnectAsync();
	}
	/// <summary>
	/// Disconnects the client from the network.
	/// </summary>
	public void Disconnect()
	{
		client.DisconnectAndStop();
		client.Disconnect();
		client = null;
	}

	/// <summary>
	/// Sends a message to the server.
	/// </summary>
	/// <param name="message">The content of the message.</param>
	/// <param name="tag">The tag of the message.</param>
	public void SendToServer(string message, byte tag = default)
	{
		var msg = new Message(ID, 0, Tag.CLIENT_TO_SERVER, tag, message);
		client.SendAsync(msg.Data);
	}
	/// <summary>
	/// Sends a message to the server and all clients.
	/// </summary>
	/// <param name="message">The content of the message.</param>
	/// <param name="tag">The tag of the message.</param>
	public void SendToAll(string message, byte tag = default)
	{
		var msg = new Message(ID, 0, Tag.CLIENT_TO_ALL, tag, message);
		client.SendAsync(msg.Data);
	}
	/// <summary>
	/// Sends a message to a specific client with the specified nickname.
	/// </summary>
	/// <param name="toNickname">The nickname of the client to send the 
	/// message to.</param>
	/// <param name="message">The content of the message.</param>
	/// <param name="tag">The tag of the message.</param>
	public void SendToClient(string toNickname, string message, byte tag = default)
	{
		// self message is possible / goes through server
		var msg = new Message(ID, GetID(toNickname), Tag.CLIENT_TO_CLIENT, tag, message);
		client.SendAsync(msg.Data);
	}

	/// <summary>
	/// Occurs when the client has lost connection to the server.
	/// </summary>
	protected virtual void OnLostConnection() { }
	/// <summary>
	/// Occurs when the client is attempting to reconnect to the server after losing connection.
	/// </summary>
	protected virtual void OnReconnectionAttempt() { }

	#region Backend
	private class _Client : TcpClient
	{
		public _Client(BaseClient parent, string address, int port) : base(address, port)
		{
			this.parent = parent;
		}

		public void DisconnectAndStop()
		{
			shouldDisconnect = true;
			DisconnectAsync();

			while (IsConnected)
				Thread.Yield();
		}

		protected override void OnConnected()
		{
			if (Id != parent.client.Id)
				return;

			// this is me connecting
			parent.IsConnected = true;

			// other clients are ignored since just a connection
			// doesn't have enough information yet, the server will send
			// their new nick & id at a later point

			// sending my nickname
			var msg = new Message(0, 0, Tag.NICKNAME_ASK, 0, parent.Nickname);
			parent.client.SendAsync(msg.Data);
		}
		protected override void OnDisconnected()
		{
			var wasConnected = parent.IsConnected;

			// i just disconnected;
			// clear local clients storage
			parent.clients.Clear();

			// and notify game user (once)
			if (parent.IsConnected)
				parent.OnClientDisconnect(parent.Nickname);

			parent.IsConnected = false;

			// was it intended? bail
			if (shouldDisconnect)
				return;

			// notify game user that it's a lost connection
			if (wasConnected && parent.IsConnected == false)
				parent.OnLostConnection();

			// or i just lost connection?
			// wait for awhile and try reconnecting
			Thread.Sleep(1000);

			// still should try reconnect?
			if (shouldDisconnect == false)
			{
				// notify game user about it
				parent.OnReconnectionAttempt();
				ConnectAsync();
			}
		}
		protected override void OnReceived(byte[] buffer, long offset, long size)
		{
			var bytes = buffer[(int)offset..((int)offset + (int)size)];
			Parse(bytes);

			void Parse(byte[] bytes)
			{
				var msg = new Message(bytes, out var remaining);
				parent.ParseMessage(msg);

				// some messages are received merged back to back;
				// keep reading since there are more messages
				if (remaining.Length > 0)
					Parse(remaining);
			}
		}
		protected override void OnError(SocketError error)
		{
			parent.OnError(error.ToString());
		}

		#region Backend
		private BaseClient parent;
		private bool shouldDisconnect;

		#endregion
	}

	private _Client client;
	private readonly ConcurrentDictionary<byte, string> clients = new();

	private void ParseMessage(Message message)
	{
		var tag = message.TagSystem;
		var fromID = message.FromID;

		// a client has joined and server sent their new id;
		// is it for me? since i don't have an ID yet
		if (tag == Tag.ID && ID == 0)
		{
			byte.TryParse(message.Value, out var myNewID);
			ID = myNewID;
		}
		// the server sends a follow up of that newcomer's nickname
		else if (tag == Tag.NICKNAME)
		{
			// is it for me?
			if (ID == message.ToID)
				Nickname = message.Value;

			var isNewcomer = clients.ContainsKey(message.ToID) == false;

			// update the local storage of clients with the new ID and new nick
			clients[message.ToID] = message.Value;

			// and the game user if it's a newcomer
			if (isNewcomer)
				OnClientConnect(message.Value);
		}
		// the server said someone disconnected
		else if (tag == Tag.DISCONNECT)
		{
			// cannot be me since i received the message => i'm connected
			byte.TryParse(message.Value, out var clientID);
			var clientNickname = clients[clientID];

			// remove the disconnected client from the local clients storage
			clients.TryRemove(clientID, out _);

			// and notify the game user
			OnClientDisconnect(clientNickname);
		}
		// regular game user messages...
		else if ((tag == Tag.SERVER_TO_ALL || tag == Tag.CLIENT_TO_ALL) &&
			ID != fromID) // not my own message?
			TriggerEvent();
		else if ((tag == Tag.SERVER_TO_CLIENT || tag == Tag.CLIENT_TO_CLIENT) &&
			ID == message.ToID) // private msg for me? self msg is possible - goes through server
			TriggerEvent();

		void TriggerEvent()
			=> OnMessageReceive(fromID == 0 ? "" : clients[fromID], message.Tag, message.Value);
	}
	private byte GetID(string nickname)
	{
		foreach (var kvp in clients)
			if (kvp.Value == nickname)
				return kvp.Key;

		return 0;
	}
	#endregion
}