using System.Text.Json.Nodes;

namespace ChatClient.FromServerMessageRouts;

public class LoginSuccessRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var loginSuccess = message["loginSuccess"];
        if (loginSuccess == null)
            return false;

        _chatListener.OnLoginSuccess();
        return true;
    }
}