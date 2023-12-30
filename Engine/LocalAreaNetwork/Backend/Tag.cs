namespace Pure.Engine.LocalAreaNetwork;

internal static class Tag
{
    public const byte SERVER_TO_ALL = 0;
    public const byte SERVER_TO_CLIENT = 1;
    public const byte CLIENT_TO_SERVER = 2;
    public const byte CLIENT_TO_ALL = 3;
    public const byte CLIENT_TO_CLIENT = 4;
    /// <summary>
    /// Server notifying all clients that a client disconnected
    /// </summary>
    public const byte DISCONNECT = 5;
    /// <summary>
    /// Server sends all clients someone's new ID
    /// </summary>
    public const byte ID = 6;
    /// <summary>
    /// Client updates their nickname, asks server if it is free
    /// </summary>
    public const byte NICKNAME_ASK = 7;
    /// <summary>
    /// Server sends all clients someone's new nickname
    /// </summary>
    public const byte NICKNAME = 8;
}