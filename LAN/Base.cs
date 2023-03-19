namespace Pure.LAN;

public abstract class Base
{
	protected virtual void OnError(string error) { }
	protected virtual void OnMessageReceive(string fromNickname, byte tag, string message) { }
	protected virtual void OnClientDisconnect(string clientNickname) { }
	protected virtual void OnClientConnect(string clientNickname) { }
}