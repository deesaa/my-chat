using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Runtime.Serialization.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ChatServer.Database;
using JsonMessage.DTO;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatServer.ServerCommands;

public class SendUsersDataCommand : Command
{
    private UserDto[] _usersOnServer;

    public SendUsersDataCommand(Guid originId, ReadOnlyCollection<UserData> users) : base(originId)
    {
        _usersOnServer = users.Select(data => new UserDto()
        {
            Color = data.Color,
            Id = data.Id,
            Username = data.Username,
            IsOnline = data.IsOnline
        }).ToArray();
    }

    public override string ToJson()
    {
        var users = JsonConvert.SerializeObject(new
        {
            usersOnServer = _usersOnServer
        });
        return users;
    }
}