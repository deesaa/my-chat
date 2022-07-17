using System.Text.Json.Nodes;

namespace ChatServer;

public class SendUsersDataRout : IMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message)
    {
        var sendUsersData = message["sendUsersData"];
        if (sendUsersData == null)
            return false;
        
        server.SendUsersDataToClient(senderId);
        return true;
    }
}