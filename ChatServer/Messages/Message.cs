using System.Net.Sockets;

namespace ChatServer;

public abstract class Message
{
    private static ulong _lastMessageId;
    public static ulong NextMessageOrdinalId => _lastMessageId++;

    private Guid _senderClientId;
    public Guid SenderClientId
    {
        get => _senderClientId;
        private set => _senderClientId = value;
    }

    protected readonly DateTime UtcTime;
    protected readonly ulong MessageOrdinalId;
    
    public Message(Guid senderClientId)
    {
        SenderClientId = senderClientId;
        UtcTime = DateTime.UtcNow;
        MessageOrdinalId = NextMessageOrdinalId;
    }
    
    public abstract string ToJson();
}