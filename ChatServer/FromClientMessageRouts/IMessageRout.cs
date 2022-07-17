using System.Text.Json.Nodes;

namespace ChatServer;

public interface IMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message);
}