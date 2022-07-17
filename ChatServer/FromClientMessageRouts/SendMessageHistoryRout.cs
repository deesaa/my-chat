using System.Text.Json.Nodes;

namespace ChatServer;

public class SendMessageHistoryRout : IMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message)
    {
        var sendMessageHistoryCount = message["sendMessageHistoryCount"];
        if (sendMessageHistoryCount == null)
            return false;
        
        server.SendMessageHistoryToClient(senderId, sendMessageHistoryCount.GetValue<int>());
        return true;
    }
}