using System.Text.Json.Nodes;
using JsonMessage.DTO;
using Newtonsoft.Json;

namespace ChatClient.FromServerMessageRouts;

public class DefaultMessageRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var userMessage = message["userMessage"];
        if (userMessage == null)
            return false;
        
        var json = userMessage.ToJsonString();
        var messageData = JsonConvert.DeserializeObject<MessageDto>(json);
        _chatListener.OnMessageFromServer(messageData);
        return true;
    }
}