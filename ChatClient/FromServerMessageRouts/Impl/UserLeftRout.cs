using System.Text.Json.Nodes;

namespace ChatClient.FromServerMessageRouts;

public class UserLeftRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var userLeft = message["userLeftName"];
        if (userLeft == null)
            return false;

        var name = userLeft.GetValue<string>();
        _chatListener.OnOtherClientDisconnected(name);
        return true;
    }
}