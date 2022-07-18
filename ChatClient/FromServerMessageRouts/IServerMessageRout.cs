using System.Text.Json.Nodes;

namespace ChatClient.FromServerMessageRouts;

public interface IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message);
}