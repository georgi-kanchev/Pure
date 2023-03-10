namespace Pure.LAN;

public enum Tag : byte
{
	// this is also used as a connection message
	ClientToServerNickname, // client updated their nick, ask server if free
	ServerToClientNickname, // server sending back a free nick

	ClientToServer, // regular msg
	ClientToClient, // regular msg
	ServerToClient, // regular msg
}