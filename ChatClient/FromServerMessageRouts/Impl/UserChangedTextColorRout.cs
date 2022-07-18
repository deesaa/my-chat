using System.Text.Json.Nodes;

namespace ChatClient.FromServerMessageRouts;

public class UserChangedTextColorRout : IServerMessageRout
{
    public bool TryRout(IChatListener _chatListener, JsonNode message)
    {
        var userChangedTextColor = message["userChangedTextColor"];
        if (userChangedTextColor == null)
            return false;
        
        var newTextColor = userChangedTextColor.GetValue<string>();
        var userName = message["userName"].GetValue<string>();
        _chatListener.OnUserChangedTextColor(userName, newTextColor);
        return true;
    }
}