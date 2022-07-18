using System.Text.Json.Nodes;

namespace ChatServer;

public class SetUserTextColorRout : IClientMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message)
    {
        
        var setTextColor = message["setTextColor"];
        if (setTextColor == null)
            return false;
        
        string color = setTextColor.GetValue<string>();
        server.SetTextColorForUser(senderId, color);
        return true;
    }
}