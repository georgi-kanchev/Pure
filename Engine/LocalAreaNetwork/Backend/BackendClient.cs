namespace Pure.Engine.LocalAreaNetwork;

using System.Net.Sockets;

internal class BackendClient(Client parent, string address, int port) : TcpClient(address, port)
{
    public void DisconnectAndStop()
    {
        shouldDisconnect = true;
        DisconnectAsync();

        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        if (Id != parent.backendClient.Id)
            return;

        // this is me connecting
        parent.IsConnected = true;

        // other clients are ignored since just a connection
        // doesn't have enough information yet, the server will send
        // their new nick & id at a later point

        // sending my nickname
        var msg = new Message(0, 0, Tag.NICKNAME_ASK, 0, parent.Nickname, []);
        parent.backendClient.SendAsync(msg.Total);
    }
    protected override void OnDisconnected()
    {
        var wasConnected = parent.IsConnected;

        // i just disconnected;
        // clear local clients storage
        parent.clients.Clear();

        // and notify game user (once)
        if (parent.IsConnected)
            parent.onClientDisconnect?.Invoke(parent.Nickname);

        parent.IsConnected = false;

        // was it intended? bail
        if (shouldDisconnect)
            return;

        // notify game user that it's a lost connection (not intended)
        if (wasConnected && parent.IsConnected == false)
            parent.onLostConnection?.Invoke();

        // or i just lost connection? wait for awhile and try reconnecting
        Thread.Sleep(1000);

        // still should try reconnect?
        if (shouldDisconnect)
            return;

        // notify game user about it
        parent.onReconnectionAttempt?.Invoke();
        ConnectAsync();
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var bytes = buffer[(int)offset..((int)offset + (int)size)];
        while (true)
        {
            var msg = new Message(bytes, out var remaining);
            parent.ParseMessage(msg);

            // some messages are received merged back to back;
            // keep reading since there are more messages
            if (remaining.Length > 0)
            {
                bytes = remaining;
                continue;
            }

            break;
        }
    }
    protected override void OnError(SocketError error)
    {
        parent.onError?.Invoke(error.ToString());
    }

#region Backend
    private bool shouldDisconnect;
#endregion
}