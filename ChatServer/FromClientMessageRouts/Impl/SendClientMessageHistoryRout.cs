using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatServer;

public class SendClientMessageHistoryRout : IClientMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message)
    {
        var sendMessageHistoryCount = message["sendMessageHistoryCount"];
        if (sendMessageHistoryCount == null)
            return false;
        
        server.SendMessageHistoryToClient(senderId, sendMessageHistoryCount.GetValue<int>());
        return true;
    }

    public static string BuildRequest(int messagesCount)
    {
        return JsonSerializer.Serialize(new
        {
            sendMessageHistoryCount = messagesCount
        });
    }
}