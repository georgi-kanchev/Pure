namespace Pure.LAN;

public enum Tag : byte
{
	ClientToServerConnect, // a client just connected
	ServerToClientID, // a response to a client connection, their new ID
	ClientToServerNickname, // a client updated their nick
}