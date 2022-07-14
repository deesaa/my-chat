using System.Text.Json;

namespace ChatServer;

public class UserLeftMessage : Message
{
    private string _joinedUsername;

    public UserLeftMessage(Guid senderClientId, string joinedUsername) : base(senderClientId)
    {
        _joinedUsername = joinedUsername;
    }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            userLeftName = _joinedUsername
        });
    }
}