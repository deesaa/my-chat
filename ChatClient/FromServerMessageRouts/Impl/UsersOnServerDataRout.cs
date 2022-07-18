using System.Text.Json.Nodes;
using JsonMessage.DTO;
using Newtonsoft.Json;

namespace ChatClient.FromServerMessageRouts;

public class UsersOnServerDataRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var usersOnServer = message["usersOnServer"];
        if (usersOnServer == null)
            return false;
        
        var jsonArray = usersOnServer.AsArray();
        if(jsonArray.Count <= 0)
            return false;

        var jsonString = usersOnServer.ToJsonString();
        UserDto[] users = JsonConvert.DeserializeObject<UserDto[]>(jsonString);
            
        _chatListener.OnUsersOnServerList(users);
        return true;
    }
}