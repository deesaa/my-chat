using System.Text.Json;

namespace ChatServer.ServerCommands;

public class LoginSuccessCommand : Command
{
    public LoginSuccessCommand(Guid originId) : base(originId) { }
    public override string ToJson()
    {
        return JsonSerializer.Serialize(new
        {
            loginSuccess = true
        });
    }
}