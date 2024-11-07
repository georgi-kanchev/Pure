namespace Pure.Engine.LocalAreaNetwork;

using System.Net;
using System.Net.Sockets;

internal class BackendServer(Server parent, IPAddress address, int port) : TcpServer(address, port)
{
    protected override TcpSession CreateSession()
    {
        return new Session(this);
    }
    protected override void OnError(SocketError error)
    {
        parent.onError?.Invoke(error.ToString());
    }

#region Backend
    internal readonly Server parent = parent;
#endregion
}