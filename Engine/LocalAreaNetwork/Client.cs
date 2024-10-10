namespace Pure.Engine.LocalAreaNetwork;

using System.Collections.Concurrent;

/// <summary>
/// Represents a base class for LocalAreaNetwork client communication.
/// </summary>
public class Client : Communication
{
    /// <summary>
    /// Gets an array of nicknames of all connected clients.
    /// </summary>
    public string[] Nicknames
    {
        get => clients.Values.ToArray();
    }
    /// <summary>
    /// Gets an array of identifiers of all connected clients.
    /// </summary>
    public byte[] Ids
    {
        get => clients.Keys.ToArray();
    }

    /// <summary>
    /// Gets a value indicating whether the client is currently connected to the network.
    /// </summary>
    public bool IsConnected { get; internal set; }
    /// <summary>
    /// Gets or sets the identifier of the client.
    /// </summary>
    public byte Id { get; internal set; }
    /// <summary>
    /// Gets the nickname of the client.
    /// </summary>
    public string Nickname { get; private set; }

    /// <summary>
    /// Initializes a new instance with the specified nickname.
    /// </summary>
    /// <param name="nickname">The nickname of the client. If null or whitespace, 
    /// the default value "Player" will be used.</param>
    public Client(string nickname)
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
        try
        {
            backendClient?.Dispose();
            backendClient = new(this, ip, port);
            backendClient.ConnectAsync();
        }
        catch (Exception e)
        {
            onError?.Invoke(e.Message);
            Thread.Sleep(100);
            Disconnect();
        }
    }
    /// <summary>
    /// Disconnects the client from the network.
    /// </summary>
    public void Disconnect()
    {
        backendClient?.DisconnectAndStop();
        backendClient?.Disconnect();
        backendClient = null;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The content of the message.</param>
    /// <param name="tag">The tag of the message.</param>
    public void SendToServer(string message, byte tag = default)
    {
        var msg = new Message(Id, 0, Tag.CLIENT_TO_SERVER, tag, message, []);
        backendClient?.SendAsync(msg.Total);
    }
    public void SendToServer(byte[] data, byte tag = default)
    {
        var msg = new Message(Id, 0, Tag.CLIENT_TO_SERVER, tag, string.Empty, data);
        backendClient?.SendAsync(msg.Total);
    }
    /// <summary>
    /// Sends a message to the server and all clients.
    /// </summary>
    /// <param name="message">The content of the message.</param>
    /// <param name="tag">The tag of the message.</param>
    public void SendToAll(string message, byte tag = default)
    {
        var msg = new Message(Id, 0, Tag.CLIENT_TO_ALL, tag, message, []);
        backendClient?.SendAsync(msg.Total);
    }
    public void SendToAll(byte[] data, byte tag = default)
    {
        var msg = new Message(Id, 0, Tag.CLIENT_TO_ALL, tag, string.Empty, data);
        backendClient?.SendAsync(msg.Total);
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
        var msg = new Message(Id, GetId(toNickname), Tag.CLIENT_TO_CLIENT, tag, message,
            []);
        backendClient?.SendAsync(msg.Total);
    }
    public void SendToClient(string toNickname, byte[] data, byte tag = default)
    {
        // self message is possible / goes through server
        var msg = new Message(Id, GetId(toNickname), Tag.CLIENT_TO_CLIENT, tag, string.Empty, data);
        backendClient?.SendAsync(msg.Total);
    }

    public void OnLostConnection(Action method)
    {
        onLostConnection += method;
    }
    public void OnReconnectionAttempt(Action method)
    {
        onReconnectionAttempt += method;
    }

#region Backend
    internal Action onReconnectionAttempt, onLostConnection;
    internal BackendClient backendClient;
    internal readonly ConcurrentDictionary<byte, string> clients = new();

    internal void ParseMessage(Message message)
    {
        var tag = message.TagSystem;
        var fromId = message.FromId;

        // a client has joined and server sent their new id;
        // is it for me? since i don't have an ID yet
        if (tag == Tag.ID && Id == 0)
        {
            byte.TryParse(message.Value, out var myNewId);
            Id = myNewId;
        }
        // the server sends a follow up of that newcomer's nickname
        else if (tag == Tag.NICKNAME)
        {
            // is it for me?
            if (Id == message.ToId)
                Nickname = message.Value;

            var isNewcomer = clients.ContainsKey(message.ToId) == false;

            // update the local storage of clients with the new ID and new nick
            clients[message.ToId] = message.Value;

            // and the game user if it's a newcomer
            if (isNewcomer)
                onClientConnect?.Invoke(message.Value);
        }
        // the server said someone disconnected
        else if (tag == Tag.DISCONNECT)
        {
            // cannot be me since i received the message => i'm connected
            byte.TryParse(message.Value, out var clientId);
            var clientNickname = clients[clientId];

            // remove the disconnected client from the local clients storage
            clients.TryRemove(clientId, out _);

            // and notify the game user
            onClientDisconnect?.Invoke(clientNickname);
        }
        // regular game user messages...
        else if (tag is Tag.SERVER_TO_ALL or Tag.CLIENT_TO_ALL &&
                 Id != fromId) // not my own message?
            TriggerEvent();
        else if (tag is Tag.SERVER_TO_CLIENT or Tag.CLIENT_TO_CLIENT &&
                 Id == message.ToId) // private msg for me? self msg is possible - goes through server
            TriggerEvent();

        return;

        void TriggerEvent()
        {
            var from = fromId == 0 ? string.Empty : clients[fromId];
            if (message.Data == null || message.Data.Length == 0)
                onReceiveMsg?.Invoke((from, message.Tag, message.Value));
            else
                onReceiveData?.Invoke((from, message.Tag, message.Data));
        }
    }
    private byte GetId(string nickname)
    {
        foreach (var kvp in clients)
            if (kvp.Value == nickname)
                return kvp.Key;

        return 0;
    }
#endregion
}