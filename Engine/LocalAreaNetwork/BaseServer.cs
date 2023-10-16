namespace Pure.Engine.LocalAreaNetwork;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// A base class for a LocalAreaNetwork server.
/// </summary>
public abstract class BaseServer : Base
{
    /// <summary>
    /// Gets an array of nicknames of all connected clients.
    /// </summary>
    public string[] Nicknames
    {
        get
        {
            var result = new string[clients.Count];
            var i = 0;
            foreach (var kvp in clients)
            {
                result[i] = kvp.Value.Item2;
                i++;
            }

            return result;
        }
    }
    /// <summary>
    /// Gets an array of identifiers of all connected clients.
    /// </summary>
    public byte[] Ids
    {
        get
        {
            var result = new byte[clients.Count];
            var i = 0;
            foreach (var kvp in clients)
            {
                result[i] = kvp.Value.Item1;
                i++;
            }

            return result;
        }
    }

    /// <summary>
    /// Starts the server on the specified port.
    /// </summary>
    /// <param name="port">The port number to listen on.</param>
    public void Start(int port)
    {
        server?.Dispose();
        server = new(this, IPAddress.Any, port);
        server.Start();
    }
    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Stop()
    {
        server.Stop();
        server.Dispose();
        server = null;
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The contents of the message.</param>
    /// <param name="tag">The tag of the message.</param>
    public void SendToAll(string message, byte tag = 0)
    {
        var msg = new Message(0, 0, Tag.SERVER_TO_ALL, tag, message);
        server.Multicast(msg.Data);
    }
    /// <summary>
    /// Sends a message to a specific connected client.
    /// </summary>
    /// <param name="toNickname">The nickname of the client to send 
    /// the message to.</param>
    /// <param name="message">The contents of the message.</param>
    /// <param name="tag">The tag of the message.</param>
    public void SendToClient(string toNickname, string message, byte tag = 0)
    {
        var msg = new Message(0, GetId(toNickname), Tag.SERVER_TO_CLIENT, tag, message);
        server.Multicast(msg.Data);
    }

#region Backend
    private class Server : TcpServer
    {
        public Server(BaseServer parent, IPAddress address, int port)
            : base(address, port)
        {
            this.parent = parent;
        }

        protected override TcpSession CreateSession()
        {
            return new _Session(this);
        }
        protected override void OnError(SocketError error)
        {
            parent.OnError(error.ToString());
        }

#region Backend
        internal readonly BaseServer parent;
#endregion
    }

    private class _Session : TcpSession
    {
        public _Session(Server parent)
            : base(parent)
        {
            this.parent = parent;
        }

        protected override void OnConnected()
        {
            // client connecting isn't enough info to proceed;
            // wait for them to ask for their desired nickname
        }
        protected override void OnDisconnected()
        {
            // a client just disconnected
            var clientNickname = parent.parent.GetNickname(Id);
            var clientID = parent.parent.GetId(Id).ToString();

            // remove them from the local clients storage
            parent.parent.clients.TryRemove(Id, out _);

            // and notify game user
            parent.parent.OnClientDisconnect(clientNickname);

            // notify everybody that someone disconnected
            var msg = new Message(0, 0, Tag.DISCONNECT, 0, clientID);
            parent.Multicast(msg.Data);
        }
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //var msgStr = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            var bytes = buffer[(int)offset..((int)offset + (int)size)];
            Parse(bytes);
            return;

            void Parse(byte[] bytes)
            {
                var msg = new Message(bytes, out var remaining);
                parent.parent.ParseMessage(Id, msg);

                // some messages are received merged back to back;
                // keep reading since there are more messages
                if (remaining.Length > 0)
                    Parse(remaining);
            }
        }
        protected override void OnError(SocketError error)
        {
            parent.parent.OnError(error.ToString());
        }

#region Backend
        private readonly Server parent;
#endregion
    }

    private Server server;
    private readonly ConcurrentDictionary<Guid, (byte, string)> clients = new();

    internal bool HasNickname(string nickname)
    {
        foreach (var kvp in clients)
            if (kvp.Value.Item2 == nickname)
                return true;

        return false;
    }
    internal bool HasId(byte id)
    {
        foreach (var kvp in clients)
            if (kvp.Value.Item1 == id)
                return true;

        return false;
    }
    internal bool HasClient(Guid guid)
    {
        return clients.ContainsKey(guid);
    }

    internal string GetNickname(Guid guid)
    {
        return clients.TryGetValue(guid, out var client) ? client.Item2 : null;
    }
    internal byte GetId(string nickname)
    {
        foreach (var kvp in clients)
            if (kvp.Value.Item2 == nickname)
                return kvp.Value.Item1;

        return 0;
    }
    internal byte GetId(Guid guid)
    {
        return clients.TryGetValue(guid, out var client) ? client.Item1 : default;
    }
    internal Guid GetGuid(byte id)
    {
        foreach (var kvp in clients)
            if (kvp.Value.Item1 == id)
                return kvp.Key;

        return default;
    }

    private byte GetFreeId()
    {
        for (byte i = 1; i < byte.MaxValue; i++)
            if (HasId(i) == false)
                return i;

        return 0;
    }
    private string GetFreeNickname(string nickname)
    {
        nickname ??= "Player";

        for (byte i = 0; i < byte.MaxValue; i++)
        {
            if (HasNickname(nickname) == false)
                return nickname;

            var number = "";
            for (var j = nickname.Length - 1; j >= 0; j--)
                if (char.IsNumber(nickname[j]))
                    number = number.Insert(0, nickname[j].ToString());

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

    private void ParseMessage(Guid fromGuid, Message message)
    {
        // a newcomer client asks if this nickname is free
        if (message.TagSystem == Tag.NICKNAME_ASK)
        {
            // send them a free ID; this would also notify everyone else
            // about them joining and their new ID
            var freeID = GetFreeId();
            var newID = new Message(0, 0, Tag.ID, 0, freeID.ToString());
            server.Multicast(newID.Data);

            // send them a free, maybe modified, version of the nickname they asked for;
            // this would also notify everyone else about their new nickname;

            // note that i can use their new ID, this is possible because
            // IDs and nicknames are assigned only upon connection and also
            // the back to back messages are received in order thanks to TCP
            var freeNick = GetFreeNickname(message.Value);
            var newNick = new Message(0, freeID, Tag.NICKNAME, 0, freeNick);
            server.Multicast(newNick.Data);

            // now i can update the local storage of clients with the new info
            clients[fromGuid] = (freeID, freeNick);

            // and the game user
            OnClientConnect(freeNick);

            // just to stay in sync - send everyone their IDs and nicknames;
            foreach (var kvp in clients)
            {
                var clientMsg = new Message(0, kvp.Value.Item1, Tag.NICKNAME, 0, kvp.Value.Item2);
                server.Multicast(clientMsg.Data);
            }
        }
        // regular game user messages...
        else if (message.TagSystem == Tag.CLIENT_TO_SERVER)
            TriggerEvent();
        else if (message.TagSystem == Tag.CLIENT_TO_ALL ||
                 message.TagSystem == Tag.CLIENT_TO_CLIENT)
        {
            server.Multicast(message.Data);
            TriggerEvent();
        }

        return;

        void TriggerEvent()
        {
            OnMessageReceive(GetNickname(fromGuid), message.Tag, message.Value);
        }
    }
#endregion
}