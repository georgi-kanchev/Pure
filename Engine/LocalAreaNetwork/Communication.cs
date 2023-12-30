namespace Pure.Engine.LocalAreaNetwork;

public abstract class Communication
{
    public void OnError(Action<string> method)
    {
        onError += method;
    }
    public void OnClientConnect(Action<string> method)
    {
        onClientConnect += method;
    }
    public void OnClientDisconnect(Action<string> method)
    {
        onClientDisconnect += method;
    }
    public void OnReceive(Action<(string fromNickname, byte tag, string message)> method)
    {
        onReceive += method;
    }

    #region Backend
    internal Action<string> onError, onClientConnect, onClientDisconnect;
    internal Action<(string fromNickname, byte tag, string message)> onReceive;
    #endregion
}