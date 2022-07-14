using System.Text.Json;

namespace ChatServer;

public class UserJoinedMessage : Message
{
    private string _joinedUsername;

    public UserJoinedMessage(Guid senderClientId, string joinedUsername) : base(senderClientId)
    {
        _joinedUsername = joinedUsername;
    }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            userJoinedName = _joinedUsername
        });
    }
}