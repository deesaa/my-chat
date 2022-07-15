using System.Text.Json;

namespace ChatServer.ServerCommands;

public class UserLeftCommand : Command
{
    private string _joinedUsername;

    public UserLeftCommand(Guid originId, string joinedUsername) : base(originId)
    {
        _joinedUsername = joinedUsername;
    }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            userLeftName = _joinedUsername,
            userLeftId = OriginId
        });
    }
}