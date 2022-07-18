using ChatServer.Database;
using JsonMessage.DTO;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatServer.ServerCommands;

public class UserJoinedCommand : Command
{
    private string _joinedUsername;
    private UserDto _user;

    public UserJoinedCommand(UserData user) : base(user.Id)
    {
        _user = new UserDto()
        {
            Color = user.Color,
            Id = user.Id,
            IsOnline = true,
            Username = user.Username
        };
    }
    public override string ToJson()
    {
        var users = JsonConvert.SerializeObject(new
        {
            userJoinedServer = true,
            user = _user
        });
        return users;
    }
}