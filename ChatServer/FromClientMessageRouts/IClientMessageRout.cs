using System.Text.Json.Nodes;

namespace ChatServer;

public interface IClientMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message);
}