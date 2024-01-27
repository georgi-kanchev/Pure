namespace Pure.Engine.LocalAreaNetwork;

using System.Collections.Concurrent;
using System.Net;

/// <summary>
/// A base class for a LocalAreaNetwork server.
/// </summary>
public class Server : Communication
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
        try
        {
            backendServer?.Dispose();
            backendServer = new(this, IPAddress.Any, port);
            backendServer.Start();
        }
        catch (Exception e)
        {
            onError?.Invoke(e.Message);
        }
    }
    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Stop()
    {
        backendServer?.Stop();
        backendServer?.Dispose();
        backendServer = null;
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The contents of the message.</param>
    /// <param name="tag">The tag of the message.</param>
    public void SendToAll(string message, byte tag = 0)
    {
        var msg = new Message(0, 0, Tag.SERVER_TO_ALL, tag, message, Array.Empty<byte>());
        backendServer?.Multicast(msg.Total);
    }
    public void SendToAll(byte[] data, byte tag = 0)
    {
        var msg = new Message(0, 0, Tag.SERVER_TO_ALL, tag, "", data);
        backendServer?.Multicast(msg.Total);
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
        var msg = new Message(0, GetId(toNickname), Tag.SERVER_TO_CLIENT, tag, message,
            Array.Empty<byte>());
        backendServer?.Multicast(msg.Total);
    }
    public void SendToClient(string toNickname, byte[] data, byte tag = 0)
    {
        var msg = new Message(0, GetId(toNickname), Tag.SERVER_TO_CLIENT, tag, "", data);
        backendServer?.Multicast(msg.Total);
    }

#region Backend
    private BackendServer backendServer;
    internal readonly ConcurrentDictionary<Guid, (byte, string)> clients = new();

    private bool HasNickname(string nickname)
    {
        foreach (var kvp in clients)
            if (kvp.Value.Item2 == nickname)
                return true;

        return false;
    }
    private bool HasId(byte id)
    {
        foreach (var kvp in clients)
            if (kvp.Value.Item1 == id)
                return true;

        return false;
    }

    internal string GetNickname(Guid guid)
    {
        return clients.TryGetValue(guid, out var client) ? client.Item2 : null;
    }
    private byte GetId(string nickname)
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
            nickname = nickname[..nIndex] + n;
        }

        return nickname;
    }

    internal void ParseMessage(Guid fromGuid, Message message)
    {
        // a newcomer client asks if this nickname is free
        if (message.TagSystem == Tag.NICKNAME_ASK)
        {
            // send them a free ID; this would also notify everyone else
            // about them joining and their new ID
            var freeId = GetFreeId();
            var newId = new Message(0, 0, Tag.ID, 0, freeId.ToString(), Array.Empty<byte>());
            backendServer.Multicast(newId.Total);

            // send them a free, maybe modified, version of the nickname they asked for
            // this would also notify everyone else about their new nickname

            // note that i can use their new id, this is possible because
            // Ids and nicknames are assigned only upon connection and also
            // the back to back messages are received in order thanks to TCP
            var freeNick = GetFreeNickname(message.Value);
            var newNick = new Message(0, freeId, Tag.NICKNAME, 0, freeNick, Array.Empty<byte>());
            backendServer.Multicast(newNick.Total);

            // now i can update the local storage of clients with the new info
            clients[fromGuid] = (freeId, freeNick);

            // and the game user
            onClientConnect?.Invoke(freeNick);

            // just to stay in sync - send everyone their IDs and nicknames;
            foreach (var kvp in clients)
            {
                var clientMsg = new Message(0, kvp.Value.Item1, Tag.NICKNAME, 0, kvp.Value.Item2,
                    Array.Empty<byte>());
                backendServer.Multicast(clientMsg.Total);
            }
        }
        // regular game user messages...
        else if (message.TagSystem == Tag.CLIENT_TO_SERVER)
            TriggerEvent();
        else if (message.TagSystem is Tag.CLIENT_TO_ALL or Tag.CLIENT_TO_CLIENT)
        {
            backendServer.Multicast(message.Total);
            TriggerEvent();
        }

        return;

        void TriggerEvent()
        {
            var nick = GetNickname(fromGuid);
            if (message.Data == null || message.Data.Length == 0)
                onReceiveMsg?.Invoke((nick, message.Tag, message.Value));
            else
                onReceiveData?.Invoke((nick, message.Tag, message.Data));
        }
    }
#endregion
}