using System.Text.Json.Nodes;

namespace ChatServer;

public class EnterNamePasswordRout : IMessageRout
{
    public bool TryRout(Guid senderId, Server server, JsonNode message)
    {
        var enterName = message["enterName"];
        var enterPassword = message["enterPassword"];
        if (enterName == null || enterPassword == null)
            return false;
        
        string name = enterName.GetValue<string>();
        string password = enterPassword.GetValue<string>();
        server.OnClientTryEnterNamePassword(senderId, name, password);
        return true;
    }
}