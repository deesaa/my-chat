using System.Text.Json.Nodes;
using JsonMessage.DTO;
using Newtonsoft.Json;

namespace ChatClient.FromServerMessageRouts;

public class UserJoinedRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var userJoined = message["userJoinedServer"];
        if (userJoined == null)
            return false;

        var jsonString = userJoined["user"].ToJsonString();
        UserDto user = JsonConvert.DeserializeObject<UserDto>(jsonString);
        _chatListener.OnOtherClientConnected(user.Username);
        return true;
    }
}