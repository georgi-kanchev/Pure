namespace Pure.Engine.LocalAreaNetwork;

using System.Net.Sockets;

internal class Session : TcpSession
{
    public Session(BackendServer parent)
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
        var clientId = parent.parent.GetId(Id).ToString();

        // remove them from the local clients storage
        parent.parent.clients.TryRemove(Id, out _);

        // and notify game user
        parent.parent.onClientDisconnect?.Invoke(clientNickname);

        // notify everybody that someone disconnected
        var msg = new Message(0, 0, Tag.DISCONNECT, 0, clientId, Array.Empty<byte>());
        parent.Multicast(msg.Total);
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        //var msgStr = System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        var bytes = buffer[(int)offset..((int)offset + (int)size)];
        Parse(bytes);
        return;

        void Parse(byte[] currentBytes)
        {
            var msg = new Message(currentBytes, out var remaining);
            parent.parent.ParseMessage(Id, msg);

            // some messages are received merged back to back;
            // keep reading since there are more messages
            if (remaining.Length > 0)
                Parse(remaining);
        }
    }
    protected override void OnError(SocketError error)
    {
        parent.parent.onError?.Invoke(error.ToString());
    }

#region Backend
    private readonly BackendServer parent;
#endregion
}