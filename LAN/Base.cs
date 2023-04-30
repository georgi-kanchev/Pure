namespace Pure.LAN;

/// <summary>
/// Represents a base class for LAN communication.
/// </summary>
public abstract class Base
{
	/// <summary>
	/// Occurs when an error is encountered during communication.
	/// </summary>
	/// <param name="error">The error message.</param>
	protected virtual void OnError(string error) { }
	/// <summary>
	/// Occurs when a message is received from a client.
	/// </summary>
	/// <param name="fromNickname">The nickname of the client sending the 
	/// message.</param>
	/// <param name="tag">The tag of the message.</param>
	/// <param name="message">The content of the message.</param>
	protected virtual void OnMessageReceive(string fromNickname, byte tag, string message) { }
	/// <summary>
	/// Occurs when a client disconnects from the network.
	/// </summary>
	/// <param name="clientNickname">The nickname of the client that disconnected.</param>
	protected virtual void OnClientDisconnect(string clientNickname) { }
	/// <summary>
	/// Occurs when a client connects to the network.
	/// </summary>
	/// <param name="clientNickname">The nickname of the client that connected.</param>
	protected virtual void OnClientConnect(string clientNickname) { }
}