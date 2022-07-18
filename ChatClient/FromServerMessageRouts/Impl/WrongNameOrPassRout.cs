using System.Text.Json.Nodes;

namespace ChatClient.FromServerMessageRouts;

public class WrongNameOrPassRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var wrongNamePass = message["wrongNamePass"];
        if (wrongNamePass == null)
            return false;

        _chatListener.OnLoginFail();
        return true;
    }
}