namespace Pure.LAN;

internal static class Tag
{
	public const byte
	SERVER_TO_CLIENT = 0, // regular server to client msg
	CLIENT_TO_SERVER = 1, // regular client to server msg
	CLIENT_TO_CLIENT = 2, // regular client to client msg
	CLIENT_TO_ALL = 3, // regular client to client msg

	SERVER_TO_CLIENT_STOP = 4, // server notifies all clients that it shuts down

	CLIENT_TO_SERVER_CONNECTION = 5, // client updates the server the connection exists
	SERVER_TO_CLIENT_CONNECTION = 6, // server updates the clinet the connection exists

	SERVER_TO_CLIENT_TIMEOUT = 7, // client lost connection, update other clients

	CLIENT_TO_SERVER_DISCONNECT = 8, // client notifies the server about their disconnect
	SERVER_TO_CLIENT_DISCONNECT = 9, // server notifies all clients that a client disconnected

	CLIENT_TO_SERVER_NICKNAME = 10, // client updated their nick, ask server if free
	SERVER_TO_CLIENT_NICKNAME = 11; // server sending back a free nick
}