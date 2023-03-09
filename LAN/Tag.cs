namespace Pure.LAN;

public enum Tag : byte
{
	ClientToServerConnect, // client just connected
	ServerToClientID, // response to a client connection, their new ID
	ClientToServerNickname, // client updated their nick

	ClientToServer, // regular msg
	ClientToClient, // regular msg
	ServerToClient, // regular msg
}