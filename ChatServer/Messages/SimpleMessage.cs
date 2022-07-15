using System.Text.Json;

namespace ChatServer;

public class SimpleMessage : Message
{
    private string _message;
    private UserData _user;
    
    public SimpleMessage(Guid senderClientId, UserData user, string message) : base(senderClientId)
    {
        _user = user;
        _message = message;
    }

    public override string ToJson()
    {
        MessageData messageData = new MessageData()
        {
            Message = _message,
            MessageId = MessageOrdinalId,
            UtcTime = UtcTime,
            User = _user
        };
        string json = JsonSerializer.Serialize(messageData);
        return json;
    }
}