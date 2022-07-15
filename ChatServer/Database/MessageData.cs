namespace ChatServer.Database;

public struct MessageData
{
    public Guid SenderUserId;
    public string MessageText;
    public DateTime UtcSendDate;
}