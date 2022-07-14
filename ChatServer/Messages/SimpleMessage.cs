using System.Text.Json;

namespace ChatServer;

public class SimpleMessage : Message
{
    private string _username;
    private string _message;
    
    public SimpleMessage(Guid senderClientId, string username, string message) : base(senderClientId)
    {
        _username = username;
        _message = message;
    }

    public override string ToJson()
    {
        string json = JsonSerializer.Serialize(new
        {
            utcTime = UtcTime,
            messageId = MessageOrdinalId,
            message = _message,
            username = _username
        });
        return json;
    }
}