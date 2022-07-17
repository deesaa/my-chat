using System.Text.Json.Nodes;

namespace ChatServer;

public class DefaultMessageRout : IMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message)
    {
        var messageTextNode = message["message"];
        if (messageTextNode == null)
            return false;
        
        var messageText = messageTextNode.GetValue<string>();
        server.OnMessageFromClient(senderId, messageText);
        return true;
    }
}